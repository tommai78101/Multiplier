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


	public class AttributePanelUI : MonoBehaviour  {
		public readonly static int MAX_NUMBER_OF_LEVELS = 10;

		public DropdownFix selections;
		public CategoryHandler categoryContentObject;
		public LevelRateHandler levelingRatesObject;
		public Text equationTextObject;
		public DifficultyGroup aiCalibrationDifficulty;
		public PresetDefault aiCalibrationPresets;
		public CustomFieldHandler aiCalibrationCustomFields;

		public void Start() {
			bool flag = (this.selections != null) && (this.categoryContentObject != null) && (this.levelingRatesObject != null) && (this.equationTextObject != null) && (this.aiCalibrationDifficulty != null)
				&& (this.aiCalibrationPresets != null) && (this.aiCalibrationCustomFields != null);
			if (!flag) {
				Debug.LogError("One or many of the variables are null. Please check.");
			}
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
			}
		}

		public void RefreshAttributes(UnitAttributes unitAttributes) {
			foreach (Category cat in this.categoryContentObject.items) {
				List<LevelRate> tempList = this.levelingRatesObject.allAttributes[cat.value];
				for (int i = 0; i < tempList.Count; i++) {
					LevelRate rate = tempList[i];
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
			this.levelingRatesObject.UpdateAllPanelItems();
		}
	}
}
