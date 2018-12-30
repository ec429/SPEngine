using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPEngine
{
	public class DesignLibrary
	{
		public Dictionary<string, Design> designs = new Dictionary<string, Design>();

		public void Load(ConfigNode node)
		{
			designs.Clear();
			foreach (ConfigNode cn in node.GetNodes("Design")) {
				Design d = new Design(cn);
				designs.Add(d.name, d);
			}
		}

		public void Save(ConfigNode node)
		{
			foreach (KeyValuePair<string, Design> kvp in designs) {
				ConfigNode cn = new ConfigNode();
				kvp.Value.Save(cn);
				node.AddNode("Design", cn);
			}
		}
	}
}
