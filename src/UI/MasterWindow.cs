using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class MasterWindow : AbstractWindow
	{
		Vector2 familyScroll, designScroll;
		char showingFamily = '\0';
		string confirmTool = null;
		public MasterWindow() :
			base(new Guid("4607f309-0fc8-4c8a-bd7b-d214d0174bc8"),
			     "SPEngine", new Rect(100, 100, 615, 320))
		{
			familyScroll = new Vector2();
			designScroll = new Vector2();
		}

		private void familyList()
		{
			foreach (Family f in Core.Instance.families.Values) {
				GUILayout.BeginHorizontal();
				try {
					if (GUILayout.Button(f.letter.ToString(), boldBtnStyle, GUILayout.ExpandWidth(false))) {
						showingFamily = f.letter;
						confirmTool = null;
					}
					GUILayout.Label(f.description);
				} finally {
					GUILayout.EndHorizontal();
				}
			}
		}

		private void designList()
		{
			foreach (Design d in Core.Instance.library.designs.Values)
				if (d.family.letter == showingFamily) {
					GUILayout.BeginHorizontal();
					try {
						GUILayout.Label(String.Format("{0}: {1:0.##}kN, {2} ignitions; TL {3}.  Mass {4:0.###}t, cost {5:0.#}f {6}{7}", d.name, d.thrust, d.ignitions, d.tl + 1, d.mass, d.cost, d.ullage ? "[U]" : "", d.pressureFed ? "[P]" : ""));
						switch (d.check) {
						case Design.Constraint.OK:
							if (!d.tooled) {
								if (confirmTool == d.name) {
									GUILayout.Label("Tool:");
									if (GUILayout.Button("OK"))
										d.Tool();
									else if (GUILayout.Button("CANCEL"))
										confirmTool = null;
								} else {
									if (GUILayout.Button("TOOL"))
										confirmTool = d.name;
								}
								GUILayout.Label(String.Format("{0:0.#}f", d.toolCost));
							}
							break;
						case Design.Constraint.UNLOCK:
							if (d.unlockCost == 0f) {
								GUILayout.Label("Unlocking");
								d.Unlock();
								break;
							}
							if (GUILayout.Button("Unlock")) {
								d.Unlock();
							}
							GUILayout.Label(String.Format("{0:F0}f", d.unlockCost));
							break;
						case Design.Constraint.TECH:
							GUILayout.Label(String.Format("Requires {0}", d.techRequired));
							break;
						default:
							GUILayout.Label(d.check.ToString());
							break;
						}
						GUILayout.FlexibleSpace();
					} finally {
						GUILayout.EndHorizontal();
					}
				}
		}

		public void techLevelList()
		{
			Family f = Core.Instance.families[showingFamily];
			for (int tl = 0; tl < f.techLevels.Count; tl++) {
				GUILayout.BeginHorizontal();
				try {
					string ignitionString;
					if (f.minIgnitions == f.getMaxIgnitions(tl))
						ignitionString = f.minIgnitions.ToString();
					else
						ignitionString = String.Format("{0}-{1}", f.minIgnitions, f.getMaxIgnitions(tl));
					GUILayout.Label(String.Format("{0}: {1:0.##}kN, {2} ignitions, {3:0.#}s Isp(vac), {4:0.#}s ISP(atm), mass {5:0.###}t, cost {6:0.#}f {7}{8}", tl, f.getMaxThrust(tl), ignitionString, f.getIspVac(tl), f.getIspAtmo(tl), f.getMaxMass(tl), f.getMaxCost(tl), f.getUllage(tl) ? "[U]" : "", f.getPressureFed(tl) ? "[P]" : ""));
					if (!f.haveTechRequired(tl)) {
						GUILayout.Label(String.Format("Requires {0}", f.getTechRequired(tl)));
					} else if (tl >= f.unlocked) {
						if (f.unlockCost(tl) == 0f) {
							GUILayout.Label("Unlocking");
							f.Unlock(tl);
							break;
						}
						if (GUILayout.Button("Unlock")) {
							f.Unlock(tl);
						}
						GUILayout.Label(String.Format("{0:F0}f", f.unlockCost(tl)));
					}
					GUILayout.FlexibleSpace();
				} finally {
					GUILayout.EndHorizontal();
				}
			}
		}

		public override void Window(int id)
		{
			GUILayout.BeginVertical(GUILayout.Width(595));
			try {
				if (showingFamily == '\0') {
					GUILayout.Label("Engine Families", headingStyle);
					familyScroll = GUILayout.BeginScrollView(familyScroll, GUILayout.Width(595), GUILayout.Height(160));
					try {
						familyList();
					} finally {
						GUILayout.EndScrollView();
					}
				} else {
					GUILayout.BeginHorizontal();
					try {
						Family f = Core.Instance.families[showingFamily];
						GUILayout.Label("Family ");
						GUILayout.Label(f.letter.ToString(), boldLblStyle);
						GUILayout.Label(": ");
						GUILayout.Label(f.description);
						GUILayout.FlexibleSpace();
					} finally {
						GUILayout.EndHorizontal();
					}
					designScroll = GUILayout.BeginScrollView(designScroll, GUILayout.Width(595), GUILayout.Height(160));
					try {
						designList();
					} finally {
						GUILayout.EndScrollView();
					}
					techLevelList();
					if (GUILayout.Button("Back to families"))
						showingFamily = '\0';
				}
			} finally {
				GUILayout.EndVertical();
				base.Window(id);
			}
		}
	}
}

