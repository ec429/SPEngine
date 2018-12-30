using System;
using UnityEngine;

namespace SPEngine
{
	public class Design
	{
		public string name; // user-provided name of this point-design
		private char familyLetter;
		public Family family;
		public int tl;
		public float thrust;
		public int ignitions;
		public bool tooled = false;

		public Design(ConfigNode node)
		{
			Load(node);
			tooled = true;
		}

		public Design(string _name, Family _family, int _tl, float _thrust, int _ignitions)
		{
			name = _name;
			family = _family;
			tl = _tl;
			thrust = _thrust;
			ignitions = _ignitions;
		}
		public Design(Family _family, int _tl)
		{
			name = "";
			family = _family;
			tl = _tl;
			thrust = family.getMaxThrust(tl);
			ignitions = family.getMaxIgnitions(tl);
		}

		public Design(Design old, int newTL) // upgrade of an existing design
		{
			name = "";
			family = old.family;
			tl = newTL;
			float tf = old.thrust / family.getMaxThrust(old.tl);
			thrust = family.getMaxThrust(tl) * tf;
			int ni = family.getMaxIgnitions(old.tl) - old.ignitions;
			ignitions = family.getMaxIgnitions(tl) - ni;
		}

		public bool broken {
			get {
				return family == null;
			}
		}

		public float mass {
			get {
				if (broken)
					return float.NaN;
				return family.getMass(tl, thrust);
			}
		}
		public float cost {
			get {
				if (broken)
					return float.NaN;
				return family.getCost(tl, thrust, ignitions) * (tooled ? 1f : 100f);
			}
		}
		public FloatCurve isp {
			get {
				if (broken)
					return null;
				return family.getIsp(tl);
			}
		}
		public float ispAtmo {
			get {
				if (broken)
					return float.NaN;
				return family.getIspAtmo(tl);
			}
		}
		public float ispVac {
			get {
				if (broken)
					return float.NaN;
				return family.getIspVac(tl);
			}
		}
		public float burnTime {
			get {
				if (broken)
					return 0;
				return family.getBurnTime(tl);
			}
		}
		public string techRequired {
			get {
				if (broken)
					return null;
				return family.getTechRequired(tl);
			}
		}
		public float scaleFactor {
			get {
				if (broken)
					return float.NaN;
				return family.getScaleFactor(tl, thrust);
			}
		}

		public void Load(ConfigNode node)
		{
			name = node.GetValue("name");
			familyLetter = node.GetValue("family")[0];
			if (Core.Instance.families.ContainsKey(familyLetter))
				family = Core.Instance.families[familyLetter];
			else
				family = null;
			tl = int.Parse(node.GetValue("tl"));
			thrust = float.Parse(node.GetValue("thrust"));
			ignitions = int.Parse(node.GetValue("ignitions"));
		}

		public void Save(ConfigNode node)
		{
			node.AddValue("name", name);
			node.AddValue("family", familyLetter.ToString());
			node.AddValue("tl", tl.ToString());
			node.AddValue("thrust", thrust.ToString());
			node.AddValue("ignitions", ignitions.ToString());
		}
	}
}
