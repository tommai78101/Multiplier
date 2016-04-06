using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SinglePlayer.UI;

namespace SinglePlayer {
	public class StartGameButton : MonoBehaviour {
		public DropdownFix playerDropdown;
		public DropdownFix aiPlayerDropdown;
		public Toggle defaultToggle;
		public Toggle customToggle;
		public Button startButton;

		public void Start() {
			bool flag = this.playerDropdown == null;
			flag |= this.aiPlayerDropdown == null;
			flag |= this.defaultToggle == null;
			flag |= this.customToggle == null;
			flag |= this.startButton == null;

			if (flag) {
				Debug.LogError("One of the objects is missing.");
			}
		}

		public void Update() {
			bool flag = this.playerDropdown.value == 0;
			flag |= this.aiPlayerDropdown.value == 0 && this.customToggle.isOn && !this.defaultToggle.isOn;

			if (flag) {
				this.startButton.interactable = false;
			}
			else {
				this.startButton.interactable = true;
			}
		}
	}
}
