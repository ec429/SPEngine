using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

namespace SPEngine
{
	public class ModuleSPEngine : PartModule, IPartMassModifier
	{
		public Guid DesignGuid;
		[KSPField(guiActive=true, guiActiveEditor=true, guiName="Design")]
		public string DesignName = "";
		private Design cacheDesign = null;

		[KSPField()]
		public string familyLetter;

		[KSPField()]
		public float scaleReference = 0.7f; /* thrust factor of the reference visual model */

		private float oldScale = 1.0f;

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

		private float scaleFactor {
			get {
				return design.getScaleFactor(scaleReference);
			}
		}

		private void moveNode(AttachNode node, AttachNode baseNode, bool movePart)
		{
			var oldPosition = node.position;

			node.position = baseNode.position * scaleFactor;

			if (movePart && node.attachedPart == part.parent)
				part.transform.Translate(oldPosition - node.position, part.transform);
		}

		private void fixNodes(bool movePart)
		{
			var prefab = PartLoader.getPartInfoByName(part.partInfo.name).partPrefab;

			foreach (var node in part.attachNodes) {
				var nodesWithSameId = part.attachNodes.Where(a => a.id == node.id).ToArray();
				var idIdx = Array.FindIndex(nodesWithSameId, a => a == node);
				var baseNodesWithSameId = prefab.attachNodes.Where(a => a.id == node.id).ToArray();
				if (idIdx < baseNodesWithSameId.Length) {
					moveNode(node, baseNodesWithSameId[idIdx], movePart);
				} else {
					Logging.LogFormat("Error scaling part. Node {0} does not have counterpart in base part.", node.id);
				}
			}

			if (part.srfAttachNode != null)
				moveNode(part.srfAttachNode, prefab.srfAttachNode, movePart);
		}

		private void scaleDragCubes()
		{
			/* This causes SIGSEGV crashes, so for now it's not called.
			 * DRVeyl says to do what https://github.com/KSP-RO/ProceduralParts/blob/3466a39/Source/ProceduralPart.cs#L698-L725 does instead, maybe we'll try that later.
			 * For now, we just live with potentially-incorrect drag cubes.
			 */
			float factor = scaleFactor / oldScale;
			int len = part.DragCubes.Cubes.Count;
			for (int ic = 0; ic < len; ic++) {
				DragCube dragCube = part.DragCubes.Cubes[ic];
				dragCube.Size *= factor;
				for (int i = 0; i < dragCube.Area.Length; i++)
					dragCube.Area[i] *= factor * factor;
				for (int i = 0; i < dragCube.Depth.Length; i++)
					dragCube.Depth[i] *= factor;
			}
			part.DragCubes.ForceUpdate(true, true);
		}

		public void applyConfig(bool propagate, bool movePart)
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
			node.AddValue("cost", design.cost.ToString());
			/* Setting this to 0 forces RF to calculate a proper throttle up time.
			 * Previously it was taking (assumingly large) numbers and setting instant throttle
			 */
			node.AddValue("throttleResponseRate", 0);
			for (int i = 0; i < design.propellants.Count; i++)
				node.AddNode(design.propellants[i]);
			for (int i = 0; i < design.ignitorResources.Count; i++)
				node.AddNode(design.ignitorResources[i]);
			engine.configs.Add(node);
			engine.SetConfiguration(configName);
			/* Scale the visual model, nodes and drag cubes */
			part.partTransform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
			part.partTransform.hasChanged = true;
			fixNodes(movePart);
			//scaleDragCubes(); // This causes crashes.  Evidently we're doing it wrong.
			oldScale = scaleFactor;
			if (propagate && design != design.family.baseDesign)
				UpdateSymmetryCounterparts();
		}

		public void applyConfig()
		{
			applyConfig(true, true);
		}

		public override void OnAwake()
		{
			engine = part.FindModuleImplementing<RealFuels.ModuleEngineConfigs>();
			applyConfig(false, false);
		}

		public override void OnStart(StartState state)
		{
			base.OnStart(state);
			this.OnAwake();
		}

		public override void OnCopy(PartModule fromModule)
		{
			ModuleSPEngine from = fromModule as ModuleSPEngine;
			DesignGuid = from.DesignGuid;
			base.OnCopy(fromModule);
			OnAwake();
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			if (node.HasValue("DesignGuid")) {
				try {
					DesignGuid = new Guid(node.GetValue("DesignGuid"));
				} catch (Exception ex) {
					// we failed to load it, so we're a broken part now.  Nothing we can do about it, so let's log and swallow the exception :(
					Logging.LogException(ex);
				}
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
				engine.applyConfig(false, true);
			}
		}

		[KSPEvent(name = "EventEdit", guiName = "Configure", guiActiveEditor = true)]
		public void EventEdit()
		{
			Core.Instance.EditPart(this);
		}
	}
}

