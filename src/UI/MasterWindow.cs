using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class MasterWindow : DesignListWindow
	{
		Vector2 familyScroll, designScroll;
		char showingFamily = '\0';
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
						confirmTool = Guid.Empty;
					}
					GUILayout.Label(f.description);
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
					string details = String.Format("Isp(vac)={0:0.#}s; Isp(SL)={1:0.#}s, SLT={2:0.##}kN; mass={3:0.###}t; cost={4:0.#}f", f.getIspVac(tl), f.getIspAtmo(tl), f.getMaxThrustAtmo(tl), f.getMaxMass(tl), f.getMaxCost(tl));
					GUILayout.Label(new GUIContent(String.Format("{0}: {1:0.##}kN, {2} ignition{3}. ", tl + 1, f.getMaxThrust(tl), ignitionString, ignitionString.Equals("1") ? "" : "s"), details));
					if (f.getUllage(tl))
						GUILayout.Label(ullageContent);
					if (f.getPressureFed(tl))
						GUILayout.Label(pressureFedContent);
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
					showAll = GUILayout.Toggle(showAll, "Show deleted designs");
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
						designList(showingFamily);
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

