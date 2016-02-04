using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer {
	public class GlobalMaxUnitLimit : MonoBehaviour {
		public Text counter;
		public GameObject globalManagerObject;

		private GlobalManager globalManager;

		public void Start() {
			if (this.globalManagerObject == null) {
				Debug.LogError("Please check to see if global manager game object is set.");
			}
			if (this.counter == null) {
				Debug.LogError("Please check to see if the text counter is set.");
			}

		}

		public void OnSliderValueChange(Slider slider) {
			int value = (int) slider.value;
			this.counter.text = value.ToString();
			if (this.globalManager == null) {
				this.globalManager = this.globalManagerObject.GetComponent<GlobalManager>();
			}
			if (this.globalManager != null) {
				this.globalManager.playerMaxUnitCount = value;
			}
		}
	}
}
