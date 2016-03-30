using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace SinglePlayer.UI {
	public class PresetDefault : MonoBehaviour {
		public Toggle defaultMode;
		public Toggle customMode;
		public ToggleGroup presetOptionGroup;

		//Disabling/Enabling UI elements only
		public DropdownFix presetSelection;
		public Transform categoryContentParent;
		public InputField equationField;

		public void Start() {
			bool flag = (this.presetSelection == null) || (this.categoryContentParent == null) || (this.equationField == null);
			if (flag) {
				Debug.LogError("Something is missing or is null. Please check.");
			}

			this.presetOptionGroup = this.GetComponent<ToggleGroup>();
			if (this.presetOptionGroup == null) {
				Debug.Log("Something is wrong. Please check.");
			}
			this.defaultMode.isOn = true;
			this.customMode.isOn = false;

			SetPresetUIEnabledFlag(false);
		}

		public void Update() {
			foreach (Toggle tog in this.presetOptionGroup.ActiveToggles()) {
				if (tog.isOn) {
					if (tog.Equals(this.defaultMode)) {
						SetPresetUIEnabledFlag(false);
					}
					else if (tog.Equals(this.customMode)) {
						SetPresetUIEnabledFlag(true);
					}
					break;
				}
			}
		}

		public void SetPresetUIEnabledFlag(bool value) {
			this.presetSelection.interactable = value;
			foreach (Transform child in this.categoryContentParent) {
				Toggle tog = child.GetComponent<Toggle>();
				if (tog != null) {
					tog.interactable = value;
				}
			}
			if (this.presetSelection.value >= 5) {
				this.equationField.interactable = value;
			}
		}
	}
}
