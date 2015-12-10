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
				if (this.unitAttributes != null && this.dropdown != null) {
					//TODO: Make complex presets.
					int itemValue = this.dropdown.value;
                    switch (itemValue) {
						default:
						case 0:
						case 1:
						case 2:
							Debug.Log("Setting expression: " + this.dropdown.options[itemValue].text);
							string expression = this.dropdown.options[itemValue].text;
							this.SetHealthAttributes(expression);
							this.SetAttackAttributes(expression);
							this.SetSpeedAttributes(expression);
							this.SetSplitAttributes(expression);
							this.SetMergeAttributes(expression);
							break;
						case 3:
							this.SetHealthAttributes("y=2*x");
							string otherExpression = "y=1.414*x";
							this.SetAttackAttributes(otherExpression);
							this.SetSpeedAttributes(otherExpression);
							this.SetSplitAttributes(otherExpression);
							this.SetMergeAttributes(otherExpression);
							break;
					}
					if (this.attributePanelUI != null) {
						this.attributePanelUI.RefreshAttributes(this.unitAttributes);
					}
				}
			}
		}

		public void SetHealthAttributes(string mathExpression) {
			if (this.unitAttributes.healthPrefabList.Count > 0) {
				this.unitAttributes.healthPrefabList.Clear();
			}
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Health, i+1);
				this.unitAttributes.healthPrefabList.Add(answer);
			}
		}

		public void SetSpeedAttributes(string mathExpression) {
			if (this.unitAttributes.speedPrefabList.Count > 0) {
				this.unitAttributes.speedPrefabList.Clear();
			}
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Speed, i+1);
				this.unitAttributes.speedPrefabList.Add(answer);
			}
		}

		public void SetAttackAttributes(string mathExpression) {
			if (this.unitAttributes.attackPrefabList.Count > 0) {
				this.unitAttributes.attackPrefabList.Clear();
			}
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Attack, i+1);
				this.unitAttributes.attackPrefabList.Add(answer);
			}
		}

		public void SetSplitAttributes(string mathExpression) {
			float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Split, 1+1);
			this.unitAttributes.splitPrefabFactor = answer;
		}

		public void SetMergeAttributes(string mathExpression) {
			if (this.unitAttributes.mergePrefabList.Count > 0) {
				this.unitAttributes.mergePrefabList.Clear();
			}
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Merge, i+1);
				this.unitAttributes.mergePrefabList.Add(answer);
			}
		}
	}
}
