using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer.UI {
	public class UIEnableDisable : MonoBehaviour {
		public InputField equationInputField;
		public bool enableCustomEquations = false;

		public void Start() {
			this.DisableCustomEquations();
		}

		public void EnableCustomEquations() {
			if (this.equationInputField != null) {
				this.equationInputField.interactable = true;
				this.enableCustomEquations = true;
			}
		}

		public void DisableCustomEquations() {
			if (this.equationInputField != null) {
				this.equationInputField.interactable = false;
				this.enableCustomEquations = false;
			}
		}
	}
}
