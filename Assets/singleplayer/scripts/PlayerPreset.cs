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

		public void SetAttribute() {
			GameObject obj = GameObject.FindGameObjectWithTag("UnitAttributes");
			if (obj != null) {
				this.unitAttributes = obj.GetComponent<UnitAttributes>();
				if (this.unitAttributes != null && this.dropdown != null && this.attributePanelUI != null) {
					//TODO: Make complex presets.
					int itemValue = this.dropdown.value;
                    switch (itemValue) {
						default:
						case 0:
						case 1:
						case 2:
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
						case 3:
							unitAttributes.SetHealthAttributes("y=2*x");
							string otherExpression = "y=1.414*x";
							unitAttributes.SetAttackAttributes(otherExpression);
							unitAttributes.SetSpeedAttributes(otherExpression);
							unitAttributes.SetSplitAttributes(otherExpression);
							unitAttributes.SetMergeAttributes(otherExpression);
							unitAttributes.SetAttackCooldownAttributes(otherExpression);
							this.attributePanelUI.DisableCustomEquations();
							break;
						case 4:
							string zero = "y=0";
							unitAttributes.SetHealthAttributes(zero);
							unitAttributes.SetAttackAttributes(zero);
							unitAttributes.SetSpeedAttributes(zero);
							unitAttributes.SetSplitAttributes(zero);
							unitAttributes.SetMergeAttributes(zero);
							unitAttributes.SetAttackCooldownAttributes(zero);
							this.attributePanelUI.EnableCustomEquations();
							break;
					}
					this.attributePanelUI.RefreshAttributes(this.unitAttributes);
				}
			}
		}
	}
}
