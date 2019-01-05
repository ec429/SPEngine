using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SPEngine
{
	public class ModuleSPEngine : PartModule, IPartCostModifier, IPartMassModifier
	{
		public Guid DesignGuid;
		[KSPField(guiActive=true, guiActiveEditor=true, guiName="Design")]
		public string DesignName = "";
		private Design cacheDesign = null;

		[KSPField()]
		public string familyLetter;

		private RealFuels.ModuleEngineConfigs engine;

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
				if (!Core.Instance.library.designs.ContainsKey(DesignGuid)) {
					Logging.LogFormat("Design {0} not found; families {1}", DesignGuid, Core.Instance.families == null ? "missing" : "found");
					if (familyLetter == null || Core.Instance.families == null)
						return null;
					if (!Core.Instance.families.ContainsKey(familyLetter[0])) {
						Logging.Log("Family not found");
						return null;
					}
					Family f = Core.Instance.families[familyLetter[0]];
					return f.baseDesign;
				}
				return Core.Instance.library.designs[DesignGuid];
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

		public void applyConfig(bool propagate)
		{
			cacheDesign = null;
			if (design == null)
				return;
			if (engine == null) {
				Logging.LogFormat("No ModuleEngineConfigs found on part!");
				return;
			}
			if (design.check != Design.Constraint.OK) {
				Logging.LogFormat("Broken design {0}: letter={1}, problem={2}", design.name, design.familyLetter, design.check);
				return;
			}
			cacheDesign = design;
			DesignName = design.name;
			Logging.LogFormat("Applying design '{0}': thrust {1}", design.name, design.thrust);

			engine.configs.Clear();
			ConfigNode node = new ConfigNode();
			/* Name to match our TestFlight configs.
			 * Per TechLevel, not per Design, because the latter
			 * would mean constructing TestFlight configs at run-
			 * time which I'd rather not have to do.
			 */
			string configName = String.Format("SPEngine-{0}-{1}", design.familyLetter, design.tl);
			node.AddValue("name", configName);
			node.AddValue("maxThrust", design.thrust.ToString());
			node.AddValue("minThrust", (design.thrust * design.minThrottle).ToString());
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
			engine.configs.Add(node);
			engine.SetConfiguration(configName);
			if (propagate)
				UpdateSymmetryCounterparts();
		}

		public void applyConfig()
		{
			applyConfig(true);
		}

		public override void OnAwake()
		{
			engine = part.FindModuleImplementing<RealFuels.ModuleEngineConfigs>();
			applyConfig();
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			this.OnAwake();
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			try {
				DesignGuid = new Guid(node.GetValue("DesignGuid"));
			} catch (Exception ex) {
				// we failed to load it, so we're a broken part now.  Nothing we can do about it, so let's log and swallow the exception :(
				Logging.LogException(ex);
			}
			this.OnAwake();
		}
		public override void OnSave(ConfigNode node)
		{
			base.OnSave(node);
			node.AddValue("DesignGuid", DesignGuid.ToString());
		}

		virtual public void UpdateSymmetryCounterparts()
		{
			if (part.symmetryCounterparts == null)
				return;

			int pCount = part.symmetryCounterparts.Count;
			for (int j = 0; j < pCount; j++)
			{
				if (part.symmetryCounterparts[j] == part)
					continue;
				/* Assumes each part only has one ModuleSPEngine. */
				ModuleSPEngine engine = part.symmetryCounterparts[j].FindModuleImplementing<ModuleSPEngine>();
				engine.DesignGuid = DesignGuid;
				engine.applyConfig(false);
			}
		}

		[KSPEvent(name = "EventEdit", guiName = "Configure", guiActiveEditor = true)]
		public void EventEdit()
		{
			Core.Instance.EditPart(this);
		}
	}
}

