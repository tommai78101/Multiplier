using UnityEngine;
using System;
using System.Collections.Generic;
using SinglePlayer.UI;
using Common;

namespace SinglePlayer {
	[Serializable]
	public struct TierUpgrade {
		public int level;
		public float health;
		public float attack;
		public float attackCooldown;
		public float speed;
		public float split;
		public float merge;
	}

	public class AIAttributeManager : MonoBehaviour {
		public List<TierUpgrade> tiers;
		public int maxTiersLimit;
		public AttributePanelUI attributePanelUI;
		public LevelRateHandler aiLevelRateHandler;

		public void Awake() {
			Debug.Log("Initializing AI attribute manager.");
			this.maxTiersLimit = Attributes.MAX_NUM_OF_LEVELS;
			this.tiers = new List<TierUpgrade>();
			for (int i = 0; i < this.maxTiersLimit; i++) {
				TierUpgrade tier = new TierUpgrade();
				if (i == 0) {
					tier.level = i + 1;
					tier.health = 1f;
					tier.attack = 1f;
					tier.speed = 1.2f;
					tier.split = 2f;
					tier.merge = 2f;
					tier.attackCooldown = 3f;
				}
				else {
					TierUpgrade previous = this.tiers[i - 1];
					tier.level = i + 1;
					tier.health = previous.health * 2f;
					tier.attack = previous.attack * 2f;
					tier.speed = previous.speed * 1.2f;
					tier.split = previous.split * 2f;
					tier.merge = previous.merge * 2f;
					tier.attackCooldown = previous.attackCooldown * 2f;
				}
				this.tiers.Add(tier);
			}
			GameObject tempAttribute = GameObject.FindGameObjectWithTag("AIAttributePanel");
			if (tempAttribute != null) {
				this.attributePanelUI = tempAttribute.GetComponent<AttributePanelUI>();
				if (this.attributePanelUI == null) {
					Debug.LogError("Something is not right.");
				}
			}
			//If tempAttribute is null, then it means the game does not allow the player to do customizations.
			//This means, the only scenario that doesn't allow the player to make customizations would be in
			//the menu screen, where the AI players are duking out without player's interventions.
		}

		public void Start() {
			if (this.attributePanelUI != null) {
				//This is only set if the game allows the player to do customizations to the AI players.
				//Else, the game is in the menu, with the AI players duking out in the background.
				this.aiLevelRateHandler = this.attributePanelUI.aiLevelingRatesObject;
				if (this.aiLevelRateHandler != null && this.aiLevelRateHandler.allAttributes != null) {
					List<LevelRate> healthList = this.aiLevelRateHandler.allAttributes[Category.Health.value];
					List<LevelRate> attackList = this.aiLevelRateHandler.allAttributes[Category.Attack.value];
					List<LevelRate> attackCooldownList = this.aiLevelRateHandler.allAttributes[Category.AttackCooldown.value];
					List<LevelRate> speedList = this.aiLevelRateHandler.allAttributes[Category.Speed.value];
					List<LevelRate> splitList = this.aiLevelRateHandler.allAttributes[Category.Split.value];
					List<LevelRate> mergeList = this.aiLevelRateHandler.allAttributes[Category.Merge.value];
					for (int i = 0; i < this.maxTiersLimit; i++) {
						TierUpgrade tierUpgrade = this.tiers[i];
						tierUpgrade.health = healthList[i].rate;
						tierUpgrade.attack = attackList[i].rate;
						tierUpgrade.attackCooldown = attackCooldownList[i].rate;
						tierUpgrade.speed = speedList[i].rate;
						tierUpgrade.split = splitList[i].rate;
						tierUpgrade.merge = mergeList[i].rate;
						this.tiers[i] = tierUpgrade;
					}
				}
				else {
					Debug.LogError("Something is wrong. Please check.");
				}
			}
		}

		public void SetHealthAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> healthList = this.aiLevelRateHandler.allAttributes[Category.Health.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < healthList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Health, i);
					LevelRate rate = healthList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - healthList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					healthList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.health = healthList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}

		public void SetAttackAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> attackList = this.aiLevelRateHandler.allAttributes[Category.Attack.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < attackList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Attack, i);
					LevelRate rate = attackList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - attackList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					attackList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.attack = attackList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}

		public void SetAttackCooldownAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> attackCooldownList = this.aiLevelRateHandler.allAttributes[Category.AttackCooldown.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < attackCooldownList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.AttackCooldown, i);
					LevelRate rate = attackCooldownList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - attackCooldownList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					attackCooldownList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.attackCooldown = attackCooldownList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}

		public void SetSpeedAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> speedList = this.aiLevelRateHandler.allAttributes[Category.Speed.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < speedList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Speed, i);
					LevelRate rate = speedList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - speedList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					speedList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.speed = speedList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}

		public void SetSplitAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> splitList = this.aiLevelRateHandler.allAttributes[Category.Split.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < splitList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Split, i);
					LevelRate rate = splitList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - splitList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					splitList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.split = splitList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}

		public void SetMergeAttribute(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			List<LevelRate> mergeList = this.aiLevelRateHandler.allAttributes[Category.Merge.value];
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				if (i < mergeList.Count) {
					float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Merge, i);
					LevelRate rate = mergeList[i];
					rate.rate = answer;
					rate.level = i + 1;
					if (i > 0) {
						float rateDiff = rate.rate - mergeList[i - 1].rate;
						if (rateDiff < 0) {
							rate.isIncreasing = -1;
						}
						else if (rateDiff > 0) {
							rate.isIncreasing = 1;
						}
						else {
							rate.isIncreasing = 0;
						}
					}
					mergeList[i] = rate;
				}
				if (i < this.tiers.Count) {
					TierUpgrade tier = this.tiers[i];
					tier.merge = mergeList[i].rate;
					this.tiers[i] = tier;
				}
			}
		}
	}
}
