using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Common;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class EnableStartGameButton : MonoBehaviour {
		public Button startGameButton;
		public DropdownFix dropdownFix;

		public Toggle aiDefaultMode;
		public Toggle aiCustomMode;
		public DropdownFix aiDropdownFix;

		public void Start() {
			if (this.startGameButton == null) {
				Debug.LogError("Start button has not been set. Please check.");
			}
			if (this.dropdownFix == null) {
				Debug.LogError("DropdownFix has not been set. Please check.");
			}
			if (this.aiDefaultMode == null) {
				Debug.LogError("Toggle (Default) for AI has not been set. Please check.");
			}
			if (this.aiCustomMode == null) {
				Debug.LogError("Toggle (Custom) for AI has not been set. Please check.");
			}
			if (this.aiDropdownFix == null) {
				Debug.LogError("DropdownFix for AI has not been set. Please check.");
			}
			this.startGameButton.interactable = false;
		}

		// Update is called once per frame
		public void Update() {
			bool flag = (this.aiDefaultMode.isOn && !this.aiCustomMode.isOn && this.dropdownFix.value > 0) || (!this.aiDefaultMode.isOn && this.aiCustomMode.isOn && this.aiDropdownFix.value > 0);
			this.startGameButton.interactable = flag;
		}

		public void TurnOff() {
			this.enabled = false;
		}

		public void TurnOn() {
			this.enabled = true;
		}
	}
}
