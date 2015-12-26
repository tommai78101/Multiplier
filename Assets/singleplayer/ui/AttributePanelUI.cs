using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using MultiPlayer;
using Common;

namespace SinglePlayer.UI {
	public class Category {
		public static readonly Category Health = new Category("Health", 0);
		public static readonly Category Attack = new Category("Attack", 1);
		public static readonly Category AttackCooldown = new Category("Atk Cooldown", 2);
		public static readonly Category Speed = new Category("Speed", 3);
		public static readonly Category Split = new Category("Split", 4);
		public static readonly Category Merge = new Category("Merge", 5);

		public static IEnumerable<Category> Values {
			get {
				yield return Health;
				yield return Attack;
				yield return AttackCooldown;
				yield return Speed;
				yield return Split;
				yield return Merge;
			}
		}
		public string name {
			get {
				return mName;
			}
		}

		public int value {
			get {
				return mValue;
			}
		}

		private readonly string mName;
		private readonly int mValue;

		Category(string name, int value) {
			this.mName = name;
			this.mValue = value;
		}

		public override string ToString() {
			return name;
		}
	}


	public class AttributePanelUI : MonoBehaviour {
		public readonly static int MAX_NUMBER_OF_LEVELS = 10;

		public DropdownFix selections;
		public CategoryHandler categoryContentObject;
		public LevelRateHandler levelingRatesObject;
		public Text equationTextObject;
		public InputField equationInputField;
		public CategoryHandler aiCategoryContentObject;
		public LevelRateHandler aiLevelingRatesObject;
		public DifficultyGroup aiCalibrationDifficulty;
		public PresetDefault aiCalibrationPresets;
		public Text aiEquationTextObject;
		public InputField aiEquationInputField;
		//public CustomFieldHandler aiCalibrationCustomFields;
		public bool enablePlayerCustomEquations = false;

		public void Start() {
			bool flag = (this.selections != null) && (this.categoryContentObject != null) && (this.levelingRatesObject != null) && (this.equationTextObject != null) && (this.aiCalibrationDifficulty != null)
				&& (this.aiCalibrationPresets != null) && (this.aiLevelingRatesObject != null); // && (this.aiCalibrationCustomFields != null);
			if (!flag) {
				Debug.LogError("One or many of the variables are null. Please check.");
			}
			this.enablePlayerCustomEquations = false;
			this.equationInputField = this.equationTextObject.GetComponentInParent<InputField>();
			if (this.equationInputField != null) {
				this.equationInputField.onEndEdit.AddListener(delegate {
					this.FinishedEditing();
				});
			}

			this.aiEquationInputField = this.aiEquationTextObject.GetComponentInParent<InputField>();
			if (this.aiEquationInputField != null) {
				this.aiEquationInputField.onEndEdit.AddListener(delegate {
					this.AIFinishedEditing();
				});
			}
			this.DisableCustomEquations();
		}

		public void Update() {
			if (Input.GetMouseButtonUp(0)) {
				//Category
				Category temp = null;
				foreach (Category cat in this.categoryContentObject.items) {
					if (cat.name.Equals(this.categoryContentObject.selectedToggle)) {
						temp = cat;
						break;
					}
				}
				if (temp != null) {
					this.levelingRatesObject.ChangeCategory(temp);
				}
				temp = null;

				foreach (Category cat in this.aiCategoryContentObject.items) {
					if (cat.name.Equals(this.aiCategoryContentObject.selectedToggle)) {
						temp = cat;
						break;
					}
				}
				if (temp != null) {
					this.aiLevelingRatesObject.ChangeCategory(temp);
				}
			}
		}

		public void RefreshAttributes(UnitAttributes unitAttributes) {
			foreach (Category cat in this.categoryContentObject.items) {
				List<LevelRate> tempList = this.levelingRatesObject.allAttributes[cat.value];
				for (int i = 0; i < tempList.Count; i++) {
					LevelRate rate = tempList[i];
					if (!this.enablePlayerCustomEquations) {
						rate.isIncreasing = 0;
					}
					switch (cat.value) {
						default:
							break;
						case 0:
							rate.rate = unitAttributes.healthPrefabList[i];
							break;
						case 1:
							rate.rate = unitAttributes.attackPrefabList[i];
							break;
						case 2:
							rate.rate = unitAttributes.attackCooldownPrefabList[i];
							break;
						case 3:
							rate.rate = unitAttributes.speedPrefabList[i];
							break;
						case 4:
							if (i == 0) {
								rate.rate = unitAttributes.splitPrefabFactor;
							}
							else {
								rate.rate = 0f;
							}
							break;
						case 5:
							rate.rate = unitAttributes.mergePrefabList[i];
							break;
					}
					tempList[i] = rate;
				}
				this.levelingRatesObject.allAttributes[cat.value] = tempList;
			}
			this.levelingRatesObject.UpdateAllPanelItems(this.categoryContentObject.selectedToggle);
		}

		public void EnableCustomEquations() {
			if (this.equationInputField != null) {
				this.equationInputField.interactable = true;
				this.enablePlayerCustomEquations = true;
			}
		}

		public void DisableCustomEquations() {
			if (this.equationInputField != null) {
				this.equationInputField.interactable = false;
				this.enablePlayerCustomEquations = false;
			}
		}

		public void FinishedEditing() {
			GameObject obj = GameObject.FindGameObjectWithTag("UnitAttributes");
			if (obj != null) {
				UnitAttributes unitAttributes = obj.GetComponent<UnitAttributes>();
				if (unitAttributes != null) {
					foreach (Category cat in Category.Values) {
						if (cat.name.Equals(this.categoryContentObject.selectedToggle)) {
							int catValue = cat.value;
							switch (catValue) {
								default:
								case 0:
									unitAttributes.SetHealthAttributes(this.equationTextObject.text);
									break;
								case 1:
									unitAttributes.SetAttackAttributes(this.equationTextObject.text);
									break;
								case 2:
									unitAttributes.SetAttackCooldownAttributes(this.equationTextObject.text);
									break;
								case 3:
									unitAttributes.SetSpeedAttributes(this.equationTextObject.text);
									break;
								case 4:
									unitAttributes.SetSplitAttributes(this.equationTextObject.text);
									break;
								case 5:
									unitAttributes.SetMergeAttributes(this.equationTextObject.text);
									break;
							}
							List<LevelRate> tempList = this.levelingRatesObject.allAttributes[catValue];
							for (int i = 0; i < tempList.Count; i++) {
								bool flag = i > 0;
								LevelRate rate = tempList[i];
								switch (cat.value) {
									default:
									case 0:
										rate.rate = unitAttributes.healthPrefabList[i];
										if (flag) {
											rate.isIncreasing = rate.rate > unitAttributes.healthPrefabList[i - 1] ? 1 : -1;
										}
										break;
									case 1:
										rate.rate = unitAttributes.attackPrefabList[i];
										if (flag) {
											rate.isIncreasing = rate.rate > unitAttributes.attackPrefabList[i - 1] ? 1 : -1;
										}
										break;
									case 2:
										rate.rate = unitAttributes.attackCooldownPrefabList[i];
										if (flag) {
											rate.isIncreasing = rate.rate > unitAttributes.attackCooldownPrefabList[i - 1] ? 1 : -1;
										}
										break;
									case 3:
										rate.rate = unitAttributes.speedPrefabList[i];
										if (flag) {
											rate.isIncreasing = rate.rate > unitAttributes.speedPrefabList[i - 1] ? 1 : -1;
										}
										break;
									case 4:
										if (i == 0) {
											rate.rate = unitAttributes.splitPrefabFactor;
											rate.isIncreasing = 0;
										}
										else {
											rate.rate = 0f;
											rate.isIncreasing = 0;
										}
										break;
									case 5:
										rate.rate = unitAttributes.mergePrefabList[i];
										if (flag) {
											rate.isIncreasing = rate.rate > unitAttributes.mergePrefabList[i - 1] ? 1 : -1;
										}
										break;
								}
								tempList[i] = rate;
							}
							this.levelingRatesObject.allAttributes[cat.value] = tempList;
							break;
						}
					}
					this.levelingRatesObject.UpdateAllPanelItems(this.categoryContentObject.selectedToggle);
				}
			}
		}

		public void AIFinishedEditing() {
			GameObject obj = GameObject.FindGameObjectWithTag("AIAttributeManager");
			if (obj != null) {
				AIAttributeManager aiAttributeManager = obj.GetComponent<AIAttributeManager>();
				if (aiAttributeManager != null) {
					foreach (Category cat in Category.Values) {
						if (cat.name.Equals(this.categoryContentObject.selectedToggle)) {
							int catValue = cat.value;
							switch (catValue) {
								default:
								case 0:
									aiAttributeManager.SetHealthAttribute(this.aiEquationTextObject.text);
									break;
								case 1:
									aiAttributeManager.SetAttackAttribute(this.aiEquationTextObject.text);
									break;
								case 2:
									aiAttributeManager.SetAttackCooldownAttribute(this.aiEquationTextObject.text);
									break;
								case 3:
									aiAttributeManager.SetSpeedAttribute(this.aiEquationTextObject.text);
									break;
								case 4:
									aiAttributeManager.SetSplitAttribute(this.aiEquationTextObject.text);
									break;
								case 5:
									aiAttributeManager.SetMergeAttribute(this.aiEquationTextObject.text);
									break;
							}
							List<LevelRate> tempList = this.aiLevelingRatesObject.allAttributes[catValue];
							for (int i = 0; i < tempList.Count; i++) {
								bool flag = i > 0;
								LevelRate rate = tempList[i];
								switch (cat.value) {
									default:
									case 0:
										rate.rate = aiAttributeManager.tiers[i].health;
										if (flag) {
											rate.isIncreasing = rate.rate > aiAttributeManager.tiers[i - 1].health ? 1 : -1;
										}
										break;
									case 1:
										rate.rate = aiAttributeManager.tiers[i].attack;
										if (flag) {
											rate.isIncreasing = rate.rate > aiAttributeManager.tiers[i - 1].attack ? 1 : -1;
										}
										break;
									case 2:
										rate.rate = aiAttributeManager.tiers[i].attackCooldown;
										if (flag) {
											rate.isIncreasing = rate.rate > aiAttributeManager.tiers[i - 1].attackCooldown ? 1 : -1;
										}
										break;
									case 3:
										rate.rate = aiAttributeManager.tiers[i].speed;
										if (flag) {
											rate.isIncreasing = rate.rate > aiAttributeManager.tiers[i - 1].speed ? 1 : -1;
										}
										break;
									case 4:
										if (i == 0) {
											rate.rate = aiAttributeManager.tiers[i].split;
											rate.isIncreasing = 0;
										}
										else {
											rate.rate = 0f;
											rate.isIncreasing = 0;
										}
										break;
									case 5:
										rate.rate = aiAttributeManager.tiers[i].merge;
										if (flag) {
											rate.isIncreasing = rate.rate > aiAttributeManager.tiers[i - 1].merge ? 1 : -1;
										}
										break;
								}
								tempList[i] = rate;
							}
							this.aiLevelingRatesObject.allAttributes[cat.value] = tempList;
							break;
						}
					}
					this.aiLevelingRatesObject.UpdateAllPanelItems(this.categoryContentObject.selectedToggle);
				}
			}
		}
	}
}
