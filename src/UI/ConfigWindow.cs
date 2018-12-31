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
				inputTL = module.design.tl;
			}
		}

		private void designList()
		{
			foreach (Design d in Core.Instance.library.designs.Values)
				if (d.family.letter == module.familyLetter[0]) {
					GUILayout.BeginHorizontal();
					try {
						if (GUILayout.Button(d.name)) {
							module.DesignName = d.name;
							module.applyConfig();
							fetchDesign();
						}
						GUILayout.Label(String.Format(": {0}kN, {1} ignitions; TL {4}.  Mass {2}, cost {3}.", d.thrust, d.ignitions, d.mass, d.cost, d.tl + 1));
						if (!d.tooled) {
							if (GUILayout.Button("TOOL")) {
								/* TODO entry costs.  And a confirmation window. */
								d.tooled = true;
							}
						}
						if (d.tl < d.family.techLevels.Count) {
							if (GUILayout.Button("Upgrade")) {
								inputName = String.Format("{0} @ {1}", d.name, d.tl + 2);
								Design up = new Design(d, d.tl + 1);
								inputThrust = up.thrust.ToString();
								inputIgnitions = up.ignitions.ToString();
								inputTL = d.tl + 2;
							}
						}
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
				GUILayout.Label(String.Format("  Mass {0}, cost {1}", currentDesign.mass, currentDesign.cost));
				Design.Constraint check = currentDesign.check;
				switch (check) {
				case Design.Constraint.OK:
					if (Core.Instance.library.designs.ContainsKey(currentDesign.name)) {
						GUILayout.Label("Name in use");
					} else if (GUILayout.Button("Apply")) {
						Core.Instance.library.designs.Add(currentDesign.name, currentDesign);
						module.DesignName = currentDesign.name;
						module.applyConfig();
					}
					break;
				default:
					GUILayout.Label(check.ToString());
					break;
				}
			} finally {
				GUILayout.EndHorizontal();
			}
		}

		public override void Window(int id)
		{
			GUILayout.BeginVertical(GUILayout.Width(595));
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
