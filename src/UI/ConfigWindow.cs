using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class ConfigWindow : AbstractWindow
	{
		Vector2 designScroll;
		ModuleSPEngine module;
		string inputThrust, inputIgnitions, inputName;
		int inputTL = 1;
		Design currentDesign;
		Guid confirmTool = Guid.Empty;
		Guid renaming = Guid.Empty;
		bool showAll = false;
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
			inputName = module.DesignName;
			if (module.design != null) {
				inputThrust = module.design.thrust.ToString();
				inputIgnitions = module.design.ignitions.ToString();
				inputTL = module.design.tl + 1;
			}
		}

		private void designList()
		{
			foreach (Design d in Core.Instance.library.designs.Values)
				if (d.family.letter == module.familyLetter[0]) {
					if (d.hidden && !showAll)
						continue;
					GUILayout.BeginHorizontal();
					try {
						if (renaming == d.guid) {
							if (!GUILayout.Toggle(true, "*"))
								renaming = Guid.Empty;
							d.name = GUILayout.TextField(d.name, GUILayout.Width(90));
						} else {
							if (GUILayout.Toggle(false, "*"))
								renaming = d.guid;
							if (GUILayout.Button(d.name)) {
								module.DesignGuid = d.guid;
								module.applyConfig();
								fetchDesign();
							}
						}
						GUILayout.Label(String.Format(": {0:0.##}kN, {1} ignitions; TL {2}.  Mass {3:0.###}, cost {4:0.#} {5}{6}", d.thrust, d.ignitions, d.tl + 1, d.mass, d.cost, d.ullage ? "[U]" : "", d.pressureFed ? "[P]" : ""));
						if (!d.tooled) {
							if (confirmTool == d.guid) {
								GUILayout.Label("Tool:");
								if (GUILayout.Button("OK"))
									d.Tool();
								else if (GUILayout.Button("CANCEL"))
									confirmTool = Guid.Empty;
							} else {
								if (GUILayout.Button("TOOL"))
									confirmTool = d.guid;
							}
							GUILayout.Label(String.Format("{0:0.#}f", d.toolCost));
						}
						if (d.tl + 1 < d.family.techLevels.Count) {
							if (d.upgradeTo != Guid.Empty) {
								GUILayout.Label(String.Format("Upgraded: {0}", d.upgradeToName));
							} else if (!d.family.haveTechRequired(d.tl + 1)) {
								GUILayout.Label(String.Format("Upgrade: requires {0}", d.family.getTechRequired(d.tl + 1)));
							} else if (d.tl + 1 >= d.family.unlocked) {
								GUILayout.Label(String.Format("Upgrade: unlock TL {0}", d.tl + 2));
							} else {
								if (GUILayout.Button("Upgrade")) {
									Design up = new Design(d, d.tl + 1);
									Design updup = up.checkDuplicate();
									if (updup != null) {
										/* Link the existing upgrade to this design.
										 * It shouldn't be possible for updup to already have an upgradeFrom,
										 * because otherwise we'd be a duplicate of that so how did we get
										 * created in the first place?  But check anyway, and don't overwrite
										 * an existing upgradeFrom.
										 */
										d.upgradeTo = updup.guid;
										if (updup.upgradeFrom == Guid.Empty)
											updup.upgradeFrom = d.guid;
									} else {
										up.name = inputName;
										if (!Core.Instance.library.designs.ContainsKey(currentDesign.guid)) {
											Core.Instance.library.AddDesign(up);
											d.upgradeTo = up.guid;
											renaming = up.guid; // select this design for renaming
											module.DesignGuid = up.guid;
											module.applyConfig();
											/* Notify the editor that we changed the module's Design */
											if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && HighLogic.LoadedSceneIsEditor)
												GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
											currentDesign = new Design(up);
											fetchDesign();
										}
									}
								}
							}
						}
						d.hidden = GUILayout.Toggle(d.hidden, "X");
						GUILayout.FlexibleSpace();
					} finally {
						GUILayout.EndHorizontal();
					}
				}
			GUILayout.BeginHorizontal();
			try {
				inputName = GUILayout.TextField(inputName, GUILayout.Width(90));
				inputThrust = GUILayout.TextField(inputThrust, GUILayout.Width(50));
				GUILayout.Label("kN, ");
				inputIgnitions = GUILayout.TextField(inputIgnitions, GUILayout.Width(30));
				currentDesign.name = inputName;
				float.TryParse(inputThrust, out currentDesign.thrust);
				int.TryParse(inputIgnitions, out currentDesign.ignitions);
				GUILayout.Label("ignitions.  TL ");
				if (GUILayout.Button("-") && inputTL > 1)
					inputTL -= 1;
				GUILayout.Label(inputTL.ToString());
				if (GUILayout.Button("+") && inputTL < currentDesign.family.techLevels.Count)
					inputTL += 1;
				currentDesign.tl = inputTL - 1;
				GUILayout.Label(String.Format("  Mass {0:0.###}, cost {1:0.#} {2}{3}", currentDesign.mass, currentDesign.tooledCost, currentDesign.ullage ? "[U]" : "", currentDesign.pressureFed ? "[P]" : ""));
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
						currentDesign = new Design(currentDesign);
						/* Notify the editor that we changed the module's Design */
						if (EditorLogic.fetch != null && EditorLogic.fetch.ship != null && HighLogic.LoadedSceneIsEditor)
							GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
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
					designList();
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
