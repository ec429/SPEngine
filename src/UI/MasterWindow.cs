using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class MasterWindow : AbstractWindow
	{
		Vector2 familyScroll, designScroll;
		char showingFamily = '\0';
		public MasterWindow() :
			base(new Guid("4607f309-0fc8-4c8a-bd7b-d214d0174bc8"),
			     "SPEngine", new Rect(100, 100, 515, 320))
		{
			familyScroll = new Vector2();
			designScroll = new Vector2();
		}

		private void familyList()
		{
			foreach (Family f in Core.Instance.families.Values) {
				GUILayout.BeginHorizontal();
				try {
					if (GUILayout.Button(f.letter.ToString(), boldBtnStyle))
						showingFamily = f.letter;
					GUILayout.Label(f.description);
				} finally {
					GUILayout.EndHorizontal();
				}
			}
		}

		private void designList()
		{
			foreach (Design d in Core.Instance.library.designs.Values)
				if (d.family.letter == showingFamily)
					GUILayout.Label(String.Format("{0}: {1}kN, {2} ignitions.  Mass {3}, cost {4}", d.name, d.thrust, d.ignitions, d.mass, d.cost));
		}

		public override void Window(int id)
		{
			GUILayout.BeginVertical(GUILayout.Width(495));
			try {
				if (showingFamily == '\0') {
					GUILayout.Label("Engine Families", headingStyle);
					familyScroll = GUILayout.BeginScrollView(familyScroll, GUILayout.Width(495), GUILayout.Height(160));
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
					} finally {
						GUILayout.EndHorizontal();
					}
					designScroll = GUILayout.BeginScrollView(designScroll, GUILayout.Width(495), GUILayout.Height(160));
					try {
						designList();
					} finally {
						GUILayout.EndScrollView();
					}
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

