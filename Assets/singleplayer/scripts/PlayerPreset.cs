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
					switch (this.dropdown.value) {
						default:
						case 0:
							Debug.Log("Setting first value.");
							break;
						case 1:
							Debug.Log("Setting second value.");
							break;
						case 2:
							Debug.Log("Setting third value.");
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
		}

		public void SetAttackAttributes(string mathExpression) {
		}

		public void SetSplitAttributes(string mathExpression) {
		}

		public void SetMergeAttributes(string mathExpression) {
		}
	}
}
