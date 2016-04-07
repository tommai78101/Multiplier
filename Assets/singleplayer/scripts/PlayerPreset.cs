using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MultiPlayer;
using Common;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class PlayerPreset : MonoBehaviour {
		public Dropdown dropdown;
		public UnitAttributes unitAttributes;
		public AttributePanelUI attributePanelUI;

		public void SetAttributes() {
			DropdownFix[] fixes = GameObject.FindObjectsOfType<DropdownFix>();
			int values = 0;
			for (int i = 0; i < fixes.Length; i++) {
				values += fixes[i].value;
			}
			if (values <= 0) {
				//TODO(Thompson): Need to create some sort of message box alerting the player to set the player presets first.
				return;
			}

			if (this.unitAttributes == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("UnitAttributes");
				if (obj != null) {
					this.unitAttributes = obj.GetComponent<UnitAttributes>();
				}
			}
			if (this.unitAttributes != null && this.dropdown != null && this.attributePanelUI != null) {
				//TODO: Make complex presets.
				int itemValue = this.dropdown.value;
				switch (itemValue) {
					default:
					case 0:
						string zero1 = "y=0";
						unitAttributes.SetHealthAttributes(zero1);
						unitAttributes.SetAttackAttributes(zero1);
						unitAttributes.SetSpeedAttributes(zero1);
						unitAttributes.SetSplitAttributes(zero1);
						unitAttributes.SetMergeAttributes(zero1);
						unitAttributes.SetAttackCooldownAttributes(zero1);
						this.attributePanelUI.DisableCustomEquations();
						break;
					case 1:
					case 2:
					case 3:
						Debug.Log("Setting expression: " + this.dropdown.options[itemValue].text);
						string expression = this.dropdown.options[itemValue].text;
						unitAttributes.SetHealthAttributes(expression);
						unitAttributes.SetAttackAttributes(expression);
						unitAttributes.SetSpeedAttributes(expression);
						unitAttributes.SetSplitAttributes(expression);
						unitAttributes.SetMergeAttributes(expression);
						unitAttributes.SetAttackCooldownAttributes(expression);
						this.attributePanelUI.DisableCustomEquations();
						break;
					case 4:
						unitAttributes.SetHealthAttributes("y=2*x");
						string otherExpression = "y=1.414*x";
						unitAttributes.SetAttackAttributes(otherExpression);
						unitAttributes.SetSpeedAttributes(otherExpression);
						unitAttributes.SetSplitAttributes(otherExpression);
						unitAttributes.SetMergeAttributes(otherExpression);
						unitAttributes.SetAttackCooldownAttributes(otherExpression);
						this.attributePanelUI.DisableCustomEquations();
						break;
					case 5:
						string one = "y=1";
						unitAttributes.SetHealthAttributes(one);
						unitAttributes.SetAttackAttributes(one);
						unitAttributes.SetSpeedAttributes(one);
						unitAttributes.SetSplitAttributes(one);
						unitAttributes.SetMergeAttributes(one);
						unitAttributes.SetAttackCooldownAttributes(one);
						this.attributePanelUI.EnableCustomEquations();
						break;
				}
				this.attributePanelUI.RefreshAttributes(this.unitAttributes);
			}
		}

		public void ApplyAttributes() {

		}
	}
}
