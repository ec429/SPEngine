using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPEngine
{
	public class DesignLibrary
	{
		public Dictionary<Guid, Design> designs = new Dictionary<Guid, Design>();
		public int generation = 0;

		public void AddDesign(Design d)
		{
			if (designs.ContainsKey(d.guid))
			{
				/* Implies we've messed up somewhere and tried to add the same Design twice. */
				Logging.LogFormat("Tried to add design with duplicate guid {0}", d.guid);
				return;
			}
			designs[d.guid] = d;
			generation++;
		}

		public Design findDesign(Design d)
		{
			foreach (Design r in designs.Values) {
				if (r.Equals(d))
					return r;
			}
			return null;
		}

		public void Load(ConfigNode node)
		{
			designs.Clear();
			foreach (ConfigNode cn in node.GetNodes("Design")) {
				Design d = new Design(cn);
				designs.Add(d.guid, d);
			}
		}

		public void Save(ConfigNode node)
		{
			foreach (KeyValuePair<Guid, Design> kvp in designs) {
				ConfigNode cn = new ConfigNode();
				kvp.Value.Save(cn);
				node.AddNode("Design", cn);
			}
		}
	}
}
