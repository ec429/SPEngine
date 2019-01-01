using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SPEngine
{
	public class ModuleSPEngine : PartModule, IPartCostModifier, IPartMassModifier
	{
		[KSPField(isPersistant=true, guiActive=true, guiActiveEditor=true, guiName="Design")]
		public string DesignName = "";
		private Design cacheDesign = null;

		[KSPField()]
		public string familyLetter;

		private ModuleEngines engine;
		private RealFuels.ModuleEngineConfigs engineConfig;

		private float initialScale;

		public override string GetInfo()
		{
			return "Using the Simple Procedural Engine system.";
		}

		public Design design {
			get {
				if (cacheDesign != null)
					return cacheDesign;
				if (Core.Instance == null)
					return null;
				if (!Core.Instance.library.designs.ContainsKey(DesignName)) {
					Logging.LogFormat("Design {0} not found; families {1}", DesignName, Core.Instance.families == null ? "missing" : "found");
					if (familyLetter == null || Core.Instance.families == null)
						return null;
					if (!Core.Instance.families.ContainsKey(familyLetter[0])) {
						Logging.Log("Family not found");
						return null;
					}
					Family f = Core.Instance.families[familyLetter[0]];
					return f.baseDesign;
				}
				return Core.Instance.library.designs[DesignName];
			}
		}

		#region IPartCostModifier implementation

		public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
		{
			return design == null || design.broken ? 0 : design.cost - defaultCost;
		}

		public ModifierChangeWhen GetModuleCostChangeWhen()
		{
			return ModifierChangeWhen.FIXED;
		}

		#endregion

		#region IPartMassModifier implementation

		public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
		{
			return design == null || design.broken ? 0 : design.mass - defaultMass;
		}

		public ModifierChangeWhen GetModuleMassChangeWhen()
		{
			return ModifierChangeWhen.FIXED;
		}

		#endregion

		public void applyConfig()
		{
			cacheDesign = null;
			if (design == null)
				return;
			if (design.check != Design.Constraint.OK) {
				Logging.LogFormat("Broken design {0}: letter={1}, problem={2}", design.name, design.familyLetter, design.check);
				return;
			}
			cacheDesign = design;
			Logging.LogFormat("Applying design '{2}': thrust {0}, scale {1}", design.thrust, design.scaleFactor, design.name);
			if (engineConfig == null)
				applyConfigStock();
			else
				applyConfigRF();
		}

		private void applyConfigRF()
		{
			engineConfig.configs.Clear();
			ConfigNode node = new ConfigNode();
			/* Name to match our TestFlight configs.
			 * Per TechLevel, not per Design, because the latter
			 * would mean constructing TestFlight configs at run-
			 * time which I'd rather not have to do.
			 */
			string configName = String.Format("SPEngine-{0}-{1}", design.familyLetter, design.tl);
			node.AddValue("name", configName);
			node.AddValue("maxThrust", design.thrust.ToString());
			node.AddValue("minThrust", design.thrust.ToString());
			node.AddValue("ignitions", design.ignitions.ToString());
			ConfigNode ispn = new ConfigNode();
			design.isp.Save(ispn);
			node.AddNode("atmosphereCurve", ispn);
			node.AddValue("ullage", design.ullage.ToString());
			node.AddValue("pressureFed", design.pressureFed.ToString());
			for (int i = 0; i < design.propellants.Count; i++)
				node.AddNode(design.propellants[i]);
			for (int i = 0; i < design.ignitorResources.Count; i++)
				node.AddNode(design.ignitorResources[i]);
			engineConfig.configs.Add(node);
			engineConfig.SetConfiguration(configName);
		}

		private void applyConfigStock()
		{
			engine.maxThrust = design.thrust;
			engine.minThrust = engine.maxThrust; // Assume no throttling for now.  Later it'll go in the Family
			engine.atmosphereCurve = design.isp;
			part.scaleFactor = initialScale * design.scaleFactor;
			/* Have to recalculate some values, because ModuleEngines is weird like that */
			engine.throttleMin = engine.minThrust / engine.maxThrust;
			engine.maxFuelFlow = engine.maxThrust / (engine.atmosphereCurve.Evaluate(0f) * engine.g);
			engine.minFuelFlow = engine.minThrust / (engine.atmosphereCurve.Evaluate(0f) * engine.g);
			/* TODO ignitions (needs MERF?) */
			/* TODO propellants (we can just use the existing config for now, but MERF might be different) */
		}

		public override void OnAwake()
		{
			initialScale = part.scaleFactor;
			engine = part.FindModuleImplementing<ModuleEngines>();
			engineConfig = part.FindModuleImplementing<RealFuels.ModuleEngineConfigs>();
			applyConfig();
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			applyConfig();
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			this.OnAwake();
		}

		[KSPEvent(name = "EventEdit", guiName = "Configure", guiActiveEditor = true)]
		public void EventEdit()
		{
			Core.Instance.EditPart(this);
		}
	}
}

