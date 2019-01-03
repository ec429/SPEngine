using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SPEngine
{
	public class TechLevel
	{
		public string techRequired;
		public List<string> entryCosts;
		public float maxThrust;
		public float minThrottle = 1f;
		public FloatCurve isp;
		public int maxIgnitions;
		public float mass;
		public float cost;
		public float toolCost;
		public bool ullage = true;
		public bool pressureFed = false;
		public List<ConfigNode> propellants;
		public List<ConfigNode> ignitorResources;

		public TechLevel(ConfigNode node)
		{
			techRequired = node.GetValue("techRequired");
			entryCosts = node.GetValues("entryCost").ToList();
			try {
				maxThrust = float.Parse(node.GetValue("maxThrust"));
			} catch {
				Logging.LogFormat("Bad maxThrust {0}", node.GetValue("maxThrust"));
				throw;
			}
			if (node.HasValue("minThrottle")) {
				try {
					minThrottle = float.Parse(node.GetValue("minThrottle"));
				} catch {
					Logging.LogFormat("Bad minThrottle {0}", node.GetValue("minThrottle"));
					throw;
				}
			}
			try {
				isp = new FloatCurve();
				isp.Load(node.GetNode("isp"));
			} catch {
				Logging.LogFormat("Bad isp {0}", node.GetNode("isp"));
				throw;
			}
			try {
				maxIgnitions = int.Parse(node.GetValue("maxIgnitions"));
			} catch {
				Logging.LogFormat("Bad maxIgnitions {0}", node.GetValue("maxIgnitions"));
				throw;
			}
			try {
				mass = float.Parse(node.GetValue("mass"));
			} catch {
				Logging.LogFormat("Bad mass {0}", node.GetValue("mass"));
				throw;
			}
			try {
				cost = float.Parse(node.GetValue("cost"));
			} catch {
				Logging.LogFormat("Bad cost {0}", node.GetValue("cost"));
				throw;
			}
			try {
				toolCost = float.Parse(node.GetValue("toolCost"));
			} catch {
				Logging.LogFormat("Bad toolCost {0}", node.GetValue("toolCost"));
				throw;
			}
			if (node.HasValue("ullage")) {
				try {
					ullage = bool.Parse(node.GetValue("ullage"));
				} catch {
					Logging.LogFormat("Bad ullage {0}", node.GetValue("ullage"));
					throw;
				}
			}
			if (node.HasValue("pressureFed")) {
				try {
					pressureFed = bool.Parse(node.GetValue("pressureFed"));
				} catch {
					Logging.LogFormat("Bad pressureFed {0}", node.GetValue("pressureFed"));
					throw;
				}
			}
			propellants = node.GetNodes("PROPELLANT").ToList();
			ignitorResources = node.GetNodes("IGNITOR_RESOURCE").ToList();
		}

		public float getMass(float thrust)
		{
			return (float)Math.Pow(thrust / maxThrust, 0.8f) * mass;
		}
		private float costFactor(float thrust, int ignitions)
		{
			return (float)Math.Pow(thrust / maxThrust, 1.2f) * (float)Math.Pow((ignitions + 1.0f) / (maxIgnitions + 1.0f), 0.2);
		}
		public float getCost(float thrust, int ignitions)
		{
			return costFactor(thrust, ignitions) * cost;
		}
		public float getToolCost(float thrust, int ignitions)
		{
			return costFactor(thrust, ignitions) * toolCost;
		}
		public float entryCost {
			get {
				RealFuels.EntryCostManager ecm = RealFuels.EntryCostManager.Instance;
				float sum = 0;
				for (int i = 0; i < entryCosts.Count; i++) {
					float val;
					if (float.TryParse(entryCosts[i], out val)) {
						sum += val;
					} else {
						if (!ecm.ConfigUnlocked(entryCosts[i]))
							sum += (float)ecm.ConfigEntryCost(entryCosts[i]);
					}
				}
				return sum;
			}
		}
		public bool Unlock() {
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
				return true;
			if (HighLogic.CurrentGame.Parameters.Difficulty.BypassEntryPurchaseAfterResearch)
				return true;
			if (entryCost > Funding.Instance.Funds)
				return false;
			RealFuels.EntryCostManager ecm = RealFuels.EntryCostManager.Instance;
			for (int i = 0; i < entryCosts.Count; i++) {
				float val;
				if (float.TryParse(entryCosts[i], out val)) {
					Funding.Instance.AddFunds(-val, TransactionReasons.RnDPartPurchase);
				} else {
					ecm.PurchaseConfig(entryCosts[i]);
				}
			}
			return true;
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
		public float minTf = 0.2f;
		public int minIgnitions = 0;
		public List<TechLevel> techLevels = new List<TechLevel>();
		public int unlocked = 0;
		public Design baseDesign;

		public Family(ConfigNode node)
		{
			letter = node.GetValue("letter")[0];
			try {
				description = node.GetValue("description");
				if (node.HasValue("minTf"))
					minTf = float.Parse(node.GetValue("minTf"));
				if (node.HasValue("minIgnitions"))
					minIgnitions = int.Parse(node.GetValue("minIgnitions"));
				foreach (ConfigNode tn in node.GetNodes("TechLevel"))
					techLevels.Add(new TechLevel(tn));
				baseDesign = new Design(this, 0);
			} catch (Exception ex) {
				Logging.LogFormat("Error occurred in family {0}, TL {1}", letter, techLevels.Count);
				throw ex;
			}
		}

		public void Unlock(int tl)
		{
			if (!check(tl))
				return;
			while (unlocked <= tl) {
				if (!haveTechRequired(unlocked))
					return;
				TechLevel next = techLevels[unlocked];
				if (!next.Unlock())
					return;
				unlocked += 1;
			}
		}

		public float unlockCost(int tl)
		{
			int t = unlocked;
			float result = 0f;
			if (!check(tl))
				return float.NaN;
			while (t <= tl) {
				TechLevel next = techLevels[t];
				result += next.entryCost;
				t += 1;
			}
			return result;
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
		/* This is the minimum maxThrust allowed for a Design.
		 * The actual minThrust of the Design will be the Design's own
		 * maxThrust multipled by our getMinThrottle(tl), below.
		 */
		public float getMinThrust(int tl)
		{
			return getMaxThrust(tl) * minTf;
		}
		public float getMinThrottle(int tl)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].minThrottle;
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
		public float getMaxMass(int tl)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].mass;
		}
		public float getCost(int tl, float thrust, int ignitions)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getCost(thrust, ignitions);
		}
		public float getMaxCost(int tl)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].cost;
		}
		public float getToolCost(int tl, float thrust, int ignitions)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getToolCost(thrust, ignitions);
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
		public string getTechRequired(int tl)
		{
			if (!check(tl))
				return null;
			return techLevels[tl].techRequired;
		}
		public bool haveTechRequired(int tl)
		{
			string tech = getTechRequired(tl);
			if (tech == null || tech.Equals(""))
				return true;
			if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
				return true;
			return ResearchAndDevelopment.GetTechnologyState(tech) == RDTech.State.Available;
		}
		public bool getUllage(int tl)
		{
			if (!check(tl))
				return true;
			return techLevels[tl].ullage;
		}
		public bool getPressureFed(int tl)
		{
			if (!check(tl))
				return true;
			return techLevels[tl].pressureFed;
		}
		public List<ConfigNode> getPropellants(int tl)
		{
			if (!check(tl))
				return null;
			return techLevels[tl].propellants;
		}
		public List<ConfigNode> getIgnitorResources(int tl)
		{
			if (!check(tl))
				return null;
			return techLevels[tl].ignitorResources;
		}
		public float getScaleFactor(int tl, float thrust)
		{
			if (!check(tl))
				return float.NaN;
			return techLevels[tl].getScaleFactor(thrust);
		}
	}
}
