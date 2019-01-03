using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public abstract class DesignListWindow : AbstractWindow
	{
		protected Guid confirmTool = Guid.Empty;
		protected Guid renaming = Guid.Empty;
		protected bool showAll = false;
		private GUIContent renameBtnContent = new GUIContent("", "Rename this design");
		private GUIContent deleteBtnContent = new GUIContent("X", "Delete this design");
		private GUIContent undeleteBtnContent = new GUIContent("#", "Un-delete this design");
		protected GUIContent ullageContent = new GUIContent("[U]", "Engine is subject to ullage");
		protected GUIContent pressureFedContent = new GUIContent("[P]", "Engine is pressure-fed");

		public DesignListWindow(Guid id, String title, Rect position) : base(id, title, position)
		{
		}

		protected Design designList(char familyLetter)
		{
			Design select = null;
			foreach (Design d in Core.Instance.library.designs.Values)
				if (d.family.letter == familyLetter) {
					if (d.hidden && !showAll)
						continue;
					GUILayout.BeginHorizontal();
					try {
						if (renaming == d.guid) {
							if (!GUILayout.Toggle(true, renameBtnContent))
								renaming = Guid.Empty;
							d.name = GUILayout.TextField(d.name, GUILayout.Width(90));
						} else {
							if (GUILayout.Toggle(false, renameBtnContent))
								renaming = d.guid;
							if (GUILayout.Button(d.name))
								select = d;
						}
						string details = String.Format("Isp(vac)={0:0.#}s; Isp(SL)={1:0.#}s, SLT={2:0.##kN}; mass={3:0.###}t; cost={4:0.#}f", d.ispVac, d.ispAtmo, d.thrustAtmo, d.mass, d.cost);
						GUILayout.Label(new GUIContent(String.Format(": {0:0.##}kN, {1} ignition{2}; TL {3}. ", d.thrust, d.ignitions, d.ignitions == 1 ? "" : "s", d.tl + 1), details));
						if (d.ullage)
							GUILayout.Label(ullageContent);
						if (d.pressureFed)
							GUILayout.Label(pressureFedContent);
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
										Core.Instance.library.AddDesign(up);
										d.upgradeTo = up.guid;
										renaming = up.guid; // select this design for renaming
										select = up; // apply this design to the module if any
									}
								}
							}
						}
						if (d.hidden) {
							if (GUILayout.Button(undeleteBtnContent))
								d.hidden = false;
						} else {
							if (GUILayout.Button(deleteBtnContent))
								d.hidden = true;
						}
						GUILayout.FlexibleSpace();
					} finally {
						GUILayout.EndHorizontal();
					}
				}
			return select;
		}
	}
}
