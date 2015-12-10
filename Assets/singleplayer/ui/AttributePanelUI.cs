using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

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
	}
}
