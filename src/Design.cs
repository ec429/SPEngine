﻿using System;
using UnityEngine;

namespace SPEngine
{
	public class Design
	{
		public string name; // user-provided name of this point-design
		public char familyLetter;
		public Family family;
		public int tl;
		public float thrust;
		public int ignitions;
		public bool tooled = false;
		public string upgradeFrom = null;
		public string upgradeTo = null;
		private Design existingDesign = null;
		private int generation = -1;
		private Design checkedDesign = null;

		public Design(ConfigNode node)
		{
			Load(node);
		}

		public Design(string _name, Family _family, int _tl, float _thrust, int _ignitions)
		{
			name = _name;
			family = _family;
			tl = _tl;
			thrust = _thrust;
			ignitions = _ignitions;
			familyLetter = family.letter;
		}
		public Design(Family _family, int _tl)
		{
			name = "";
			family = _family;
			tl = _tl;
			thrust = family.getMaxThrust(tl);
			ignitions = family.getMaxIgnitions(tl);
			familyLetter = family.letter;
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
			familyLetter = family.letter;
			upgradeFrom = old.name;
		}

		public Design(Design old) : this(old.name, old.family, old.tl, old.thrust, old.ignitions) // clone
		{
		}

		public bool Equals(Design other)
		{
			return other.familyLetter == familyLetter &&
			       other.tl == tl &&
			       other.thrust == thrust &&
			       other.ignitions == ignitions;
		}

		public Design checkDuplicate()
		{
			if (generation != Core.Instance.library.generation || !checkedDesign.Equals(this)) {
				existingDesign = Core.Instance.library.findDesign(this);
				generation = Core.Instance.library.generation;
				checkedDesign = new Design(this);
			}
			return existingDesign;
		}

		public enum Constraint { OK, BROKEN, TECHLEVEL, MINTHRUST, MAXTHRUST, IGNITIONS, UNLOCK, TECH };

		public Constraint check { // is this Design within the Family's constraints?
			get {
				if (broken)
					return Constraint.BROKEN;
				if (!family.check(tl))
					return Constraint.TECHLEVEL;
				if (!family.haveTechRequired(tl))
					return Constraint.TECH;
				if (tl >= family.unlocked)
					return Constraint.UNLOCK;
				if (thrust < family.getMinThrust(tl))
					return Constraint.MINTHRUST;
				if (thrust > family.getMaxThrust(tl))
					return Constraint.MAXTHRUST;
				if (ignitions < 0 || ignitions > family.getMaxIgnitions(tl))
					return Constraint.IGNITIONS;
				return Constraint.OK;
			}
		}

		public void Tool()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
				Funding.Instance.AddFunds(-toolCost, TransactionReasons.RnDPartPurchase);
			tooled = true;
		}

		public void Unlock()
		{
			family.Unlock(tl);
		}

		public float unlockCost {
			get {
				if (broken)
					return float.NaN;
				return family.unlockCost(tl);
			}
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
		private Design upgradeFromDesign {
			get {
				if (upgradeFrom == null)
					return null;
				if (!Core.Instance.library.designs.ContainsKey(upgradeFrom))
					return null;
				return Core.Instance.library.designs[upgradeFrom];
			}
		}
		public float tooledCost {
			get {
				if (broken)
					return float.NaN;
				return family.getCost(tl, thrust, ignitions);
			}
		}
		public float cost {
			get {
				if (broken)
					return float.NaN;
				return tooledCost * ((tooled && tl < family.unlocked) ? 1f : 1000f);
			}
		}
		public float origToolCost {
			get {
				if (broken)
					return float.NaN;
				return family.getToolCost(tl, thrust, ignitions);
			}
		}
		public float toolCost {
			get {
				if (broken)
					return float.NaN;
				if (tooled)
					return 0f;
				/* Upgrades get to use 80% of the old version's engineering */
				if (upgradeFromDesign != null && upgradeFromDesign.tooled)
					return origToolCost - upgradeFromDesign.origToolCost * 0.8f;
				return origToolCost;
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
			if (Core.Instance.families.ContainsKey(familyLetter)) {
				family = Core.Instance.families[familyLetter];
			} else {
				Logging.LogFormat("Failed to load family '{0}'", familyLetter);
				family = null;
			}
			tl = int.Parse(node.GetValue("tl"));
			thrust = float.Parse(node.GetValue("thrust"));
			ignitions = int.Parse(node.GetValue("ignitions"));
			tooled = bool.Parse(node.GetValue("tooled"));
			if (node.HasValue("upgradeFrom"))
				upgradeFrom = node.GetValue("upgradeFrom");
			if (node.HasValue("upgradeTo"))
				upgradeTo = node.GetValue("upgradeTo");
		}

		public void Save(ConfigNode node)
		{
			node.AddValue("name", name);
			node.AddValue("family", familyLetter.ToString());
			node.AddValue("tl", tl.ToString());
			node.AddValue("thrust", thrust.ToString());
			node.AddValue("ignitions", ignitions.ToString());
			node.AddValue("tooled", tooled.ToString());
			if (upgradeFrom != null)
				node.AddValue("upgradeFrom", upgradeFrom);
			if (upgradeTo != null)
				node.AddValue("upgradeTo", upgradeTo);
		}
	}
}