using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class ConfigWindow : DesignListWindow
	{
		Vector2 designScroll;
		ModuleSPEngine module;
		string inputThrust, inputIgnitions, inputName;
		int inputTL = 1;
		Design currentDesign;
		public ConfigWindow(ModuleSPEngine m) :
			base(new Guid("41f4fc6f-06b4-4d6c-9774-908f46beffc0"),
			     "SPEngine Config", new Rect(100, 100, 615, 320))
		{
			designScroll = new Vector2();
			module = m;
			inputThrust = "";
			inputIgnitions = "1";
			fetchDesign();
			Family f = Core.Instance.families[module.familyLetter[0]];
			currentDesign = new Design(f, 0);
		}

		private void fetchDesign()
		{
			if (module.design != null) {
				inputName = module.design.name;
				inputThrust = module.design.thrust.ToString();
				inputIgnitions = module.design.ignitions.ToString();
				inputTL = module.design.tl + 1;
				currentDesign = new Design(module.design);
			}
		}

		private void designInput()
		{
			GUILayout.BeginHorizontal();
			try {
				Family f = currentDesign.family;
				inputName = GUILayout.TextField(inputName, GUILayout.Width(90));
				inputThrust = GUILayout.TextField(inputThrust, GUILayout.Width(50));
				GUILayout.Label(new GUIContent("kN, ", String.Format("min {0:0.##}kN, max {1:0.##}kN; SLT={2:0.##}kN", f.getMinThrust(currentDesign.tl), f.getMaxThrust(currentDesign.tl), currentDesign.thrustAtmo)));
				inputIgnitions = GUILayout.TextField(inputIgnitions, GUILayout.Width(30));
				currentDesign.name = inputName;
				float.TryParse(inputThrust, out currentDesign.thrust);
				int.TryParse(inputIgnitions, out currentDesign.ignitions);
				GUILayout.Label(new GUIContent("ignitions.  ", String.Format("min {0}, max {1}", f.minIgnitions, f.getMaxIgnitions(currentDesign.tl))));
				if (GUILayout.Button("-") && inputTL > 1)
					inputTL -= 1;
				GUILayout.Label(String.Format("TL {0}", inputTL));
				if (GUILayout.Button("+") && inputTL < currentDesign.family.techLevels.Count)
					inputTL += 1;
				currentDesign.tl = inputTL - 1;
				GUILayout.Label(String.Format("  Mass {0:0.###}, cost {1:0.#}", currentDesign.mass, currentDesign.tooledCost));
				if (currentDesign.ullage)
					GUILayout.Label(ullageContent);
				if (currentDesign.pressureFed)
					GUILayout.Label(pressureFedContent);
				Design.Constraint check = currentDesign.check;
				switch (check) {
				case Design.Constraint.OK:
					Design dup = currentDesign.checkDuplicate();
					if (currentDesign.name.Equals("")) {
						GUILayout.Label("Choose name");
					} else if (dup != null) {
						if (dup.hidden) {
							if (GUILayout.Button(String.Format("Duplicates {0}", dup.name)))
								dup.hidden = false;
						} else {
							GUILayout.Label(String.Format("Duplicates {0}", dup.name));
						}
					} else if (GUILayout.Button("Apply")) {
						Core.Instance.library.AddDesign(currentDesign);
						module.DesignGuid = currentDesign.guid;
						module.applyConfig();
						/* Notify the editor that we changed the module's Design */
						if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && HighLogic.LoadedSceneIsEditor)
							GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
						currentDesign = new Design(currentDesign); // decouple & get fresh guid
					}
					break;
				case Design.Constraint.UNLOCK:
					if (currentDesign.unlockCost == 0f) {
						GUILayout.Label("Unlocking");
						currentDesign.Unlock();
						break;
					}
					if (GUILayout.Button("Unlock")) {
						currentDesign.Unlock();
					}
					GUILayout.Label(String.Format("{0:F0}f", currentDesign.unlockCost));
					break;
				case Design.Constraint.TECH:
					GUILayout.Label(String.Format("Requires {0}", currentDesign.techRequired));
					break;
				default:
					GUILayout.Label(check.ToString());
					break;
				}
				GUILayout.FlexibleSpace();
			} finally {
				GUILayout.EndHorizontal();
			}
		}

		public override void Window(int id)
		{
			GUILayout.BeginVertical(GUILayout.Width(595));
			showAll = GUILayout.Toggle(showAll, "Show deleted designs");
			try {
				GUILayout.BeginHorizontal();
				try {
					Family f = Core.Instance.families[module.familyLetter[0]];
					GUILayout.Label("Family ");
					GUILayout.Label(f.letter.ToString(), boldLblStyle);
					GUILayout.Label(": ");
					GUILayout.Label(f.description);
				} finally {
					GUILayout.EndHorizontal();
				}
				designScroll = GUILayout.BeginScrollView(designScroll, GUILayout.Width(595), GUILayout.Height(160));
				try {
					Design d = designList(module.familyLetter[0]);
					if (d != null) {
						module.DesignGuid = d.guid;
						module.applyConfig();
						/* Notify the editor that we changed the module's Design */
						if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && HighLogic.LoadedSceneIsEditor)
							GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
						fetchDesign();
					}
					designInput();
				} finally {
					GUILayout.EndScrollView();
				}
			} finally {
				GUILayout.EndVertical();
				base.Window(id);
			}
		}
	}
}
