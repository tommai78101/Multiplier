using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MultiPlayer;

namespace SinglePlayer {
	public class AIPreset : MonoBehaviour {
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
	}
}
