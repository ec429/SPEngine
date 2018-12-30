using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

namespace SPEngine
{
	public class ModuleSPEngine : PartModule, IPartCostModifier, IPartMassModifier
	{
		[KSPField(isPersistant=true)]
		public string DesignName = "";
		private Design cacheDesign = null;

		[KSPField()]
		public string familyLetter;

		private ModuleEngines engine;
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
					if (familyLetter == null)
						return null;
					if (!Core.Instance.families.ContainsKey(familyLetter[0]))
						return null;
					Family f = Core.Instance.families[familyLetter[0]];
					return f == null ? null : f.baseDesign;
				}
				return Core.Instance.library.designs[DesignName];
			}
		}

		#region IPartCostModifier implementation

		public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
		{
			return design == null ? defaultCost : design.cost;
		}

		public ModifierChangeWhen GetModuleCostChangeWhen()
		{
			return ModifierChangeWhen.FIXED;
		}

		#endregion

		#region IPartMassModifier implementation

		public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
		{
			return design == null ? defaultMass : design.mass;
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
			cacheDesign = design;
			Logging.LogFormat("Applying design '{2}': thrust {0}, scale {1}", design.thrust, design.scaleFactor, design.name);
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

