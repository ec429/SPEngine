using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine.UI
{
	public class MasterWindow : AbstractWindow
	{
		public MasterWindow(SPEngine.DesignLibrary l) :
			base(new Guid("4607f309-0fc8-4c8a-bd7b-d214d0174bc8"),
			     "SPEngine", new Rect(100, 100, 515, 320))
		{
			//
		}

		public override void Window(int id)
		{
			GUILayout.BeginVertical(GUILayout.Width(495));
			try {
				GUILayout.Label("There is nothing here yet.", headingStyle);
			} finally {
				GUILayout.EndVertical();
				base.Window(id);
			}
		}
	}
}

