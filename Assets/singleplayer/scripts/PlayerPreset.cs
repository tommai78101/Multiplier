using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MultiPlayer;
using Common;

namespace SinglePlayer {
	public class PlayerPreset : MonoBehaviour {
		public Dropdown dropdown;
		public UnitAttributes unitAttributes;

		public void SetAttribute() {
			GameObject obj = GameObject.FindGameObjectWithTag("UnitAttributes");
			if (obj != null) {
				this.unitAttributes = obj.GetComponent<UnitAttributes>();
				if (this.unitAttributes != null && this.dropdown != null) {
					//TODO: Set attribute presets here...
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
							break;
					}
				}
			}
		}

		public void SetHealthAttributes(string mathExpression) {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Health, i);
				this.unitAttributes.healthPrefabList.Add(answer);
			}
		}

		public void SetSpeedAttributes(string mathExpression) {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Speed, i);
				this.unitAttributes.speedPrefabList.Add(answer);
			}
		}

		public void SetAttackAttributes(string mathExpression) {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Attack, i);
				this.unitAttributes.attackPrefabList.Add(answer);
			}
		}

		public void SetSplitAttributes(string mathExpression) {
			float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Split, 1);
			this.unitAttributes.splitPrefabFactor = answer;
		}

		public void SetMergeAttributes(string mathExpression) {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float) MathParser.ProcessEquation(mathExpression, AttributeProperty.Merge, i);
				this.unitAttributes.mergePrefabList.Add(answer);
			}
		}
	}
}
