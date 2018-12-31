using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPEngine
{
	public class TechLevel
	{
		public string techRequired;
		public float maxThrust;
		public FloatCurve isp;
		public int maxIgnitions;
		public float mass;
		public float cost;
		public float burnTime;

		public TechLevel(ConfigNode node)
		{
			techRequired = node.GetValue("techRequired");
			maxThrust = float.Parse(node.GetValue("maxThrust"));
			isp = new FloatCurve();
			isp.Load(node.GetNode("isp"));
			maxIgnitions = int.Parse(node.GetValue("maxIgnitions"));
			mass = float.Parse(node.GetValue("mass"));
			cost = float.Parse(node.GetValue("cost"));
			burnTime = float.Parse(node.GetValue("burnTime"));
			/* TODO rest of reliability numbers */
		}

		public float getMass(float thrust)
		{
			return (float)Math.Pow(thrust / maxThrust, 0.8f) * mass;
		}
		public float getCost(float thrust, int ignitions)
		{
			return (float)Math.Pow(thrust / maxThrust, 1.2f) * (float)Math.Pow((ignitions + 1.0f) / (maxIgnitions + 1.0f), 0.2) * cost;
		}
		public float getScaleFactor(float thrust)
		{
			return (float)Math.Sqrt(thrust / maxThrust);
		}
	}

	public class Family
	{
		public char letter = '?';
		public string description;
		public Dictionary<string, float> propellants = new Dictionary<string, float>();
		public float minTf = 0.2f;
		public enum Flags {
			NONE = 0,
			NO_GIMBAL = 0x1,
		};
		public Flags flags = 0;
		public List<TechLevel> techLevels = new List<TechLevel>();
		public Design baseDesign;

		public Family(ConfigNode node)
		{
			letter = node.GetValue("letter")[0];
			description = node.GetValue("description");
			ConfigNode pn = node.GetNode("Propellants");
			foreach (ConfigNode.Value v in pn.values)
				propellants.Add(v.name, float.Parse(v.value));
			if (node.HasValue("minTf"))
				minTf = float.Parse(node.GetValue("minTf"));
			if (node.HasValue("noGimbal"))
				flags |= Flags.NO_GIMBAL;
			foreach (ConfigNode tn in node.GetNodes("TechLevel"))
				techLevels.Add(new TechLevel(tn));
			baseDesign = new Design(this, 0);
		}

		public bool check(int tl)
		{
			return tl >= 0 && tl < techLevels.Count;
		}

		public float getMaxThrust(int tl)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].maxThrust;
		}
		public float getMinThrust(int tl)
		{
			return getMaxThrust(tl) * minTf;
		}
		public int getMaxIgnitions(int tl)
		{
			if (!check(tl))
				return 0;
			return techLevels[tl].maxIgnitions;
		}
		public float getMass(int tl, float thrust)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getMass(thrust);
		}
		public float getCost(int tl, float thrust, int ignitions)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getCost(thrust, ignitions);
		}
		public FloatCurve getIsp(int tl)
		{
			if (!check(tl))
				return null;
			return techLevels[tl].isp;
		}
		public float getIspAtmo(int tl)
		{
			return getIsp(tl).Evaluate(1.0f);
		}
		public float getIspVac (int tl)
		{
			return getIsp(tl).Evaluate(0.0f);
		}
		public float getBurnTime(int tl)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].burnTime;
		}
		public string getTechRequired(int tl)
		{
			if (!check(tl))
				return null;
			return techLevels[tl].techRequired;
		}
		public float getScaleFactor(int tl, float thrust)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getScaleFactor(thrust);
		}
	}
}
