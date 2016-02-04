using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer {
	public class RatioSlider : MonoBehaviour {
		public AIManager aiManager;
		public Text counter;

		private int previousMaxUnitLimit;
		private float previousMergeRatio;
		private float previousScoutRatio;
		private float previousSplitRatio;

		public void Start() {
			if (this.aiManager == null) {
				this.aiManager = GameObject.FindObjectOfType<AIManager>();
				if (this.aiManager == null) {
					Debug.LogError("There's something wrong with AI Manager. Please check.");
				}
			}
			if (this.counter == null) {
				Debug.LogError("Please check if the counter Text UI element has been set.");
			}
			this.previousMaxUnitLimit = 0;
			this.previousMergeRatio = 0f;
			this.previousScoutRatio = 0f;
			this.previousSplitRatio = 0f;
		}

		public void OnSliderChangeMergeRatioValue(Slider slider) {
			this.aiManager.mergeRatio = slider.value;
			this.counter.text = slider.value.ToString("0.000");
		}

		public void OnSliderChangeScoutRatioValue(Slider slider) {
			this.aiManager.scoutRatio = slider.value;
			this.counter.text = slider.value.ToString("0.000");
		}

		public void OnSliderChangeSplitRatioValue(Slider slider) {
			this.aiManager.splitRatio = slider.value;
			this.counter.text = slider.value.ToString("0.000");
		}

		public void OnSliderChangeMaxUnitLimit(Slider slider) {
			int value = (int) slider.value;
			this.aiManager.maxUnitCount = value;
			this.counter.text = value.ToString();
		}

		public void ToggleRatioSlider(Toggle toggle) {
			Slider slider = this.GetComponentInChildren<Slider>();
			if (slider != null) {
				slider.interactable = toggle.isOn;
			}
		}

		public void RevertToDefaultMaxUnitLimit(Slider slider) {
			this.previousMaxUnitLimit = this.aiManager.maxUnitCount;
			slider.value = 50;
			this.aiManager.maxUnitCount = 50;
			this.counter.text = "50";
		}

		public void RevertToDefaultMergeRatio(Slider slider) {
			this.previousMergeRatio = this.aiManager.mergeRatio;
			slider.value = 0.1532644f;
			this.aiManager.mergeRatio = 0.1532644f;
			this.counter.text = "0.153";
		}

		public void RevertToDefaultScoutRatio(Slider slider) {
			this.previousScoutRatio = this.aiManager.scoutRatio;
			slider.value = 0.3040901f;
			this.aiManager.scoutRatio = 0.3040901f;
			this.counter.text = "0.304";
		}

		public void RevertToDefaultSplitRatio(Slider slider) {
			this.previousSplitRatio = this.aiManager.splitRatio;
			slider.value = 0.5426455f;
			this.aiManager.splitRatio = 0.5426455f;
			this.counter.text = "0.543";
		}

		public void RevertToCustomMaxUnitLimit(Slider slider) {
			slider.value = this.previousMaxUnitLimit;
			this.aiManager.maxUnitCount = this.previousMaxUnitLimit;
			this.counter.text = this.previousMaxUnitLimit.ToString();
		}

		public void RevertToCustomMergeRatio(Slider slider) {
			slider.value = this.previousMergeRatio;
			this.aiManager.mergeRatio = this.previousMergeRatio;
			this.counter.text = this.previousMergeRatio.ToString("0.000");
		}

		public void RevertToCustomScoutRatio(Slider slider) {
			slider.value = this.previousScoutRatio;
			this.aiManager.scoutRatio = this.previousScoutRatio;
			this.counter.text = this.previousScoutRatio.ToString("0.000");
		}

		public void RevertToCustomSplitRatio(Slider slider) {
			slider.value = this.previousSplitRatio;
			this.aiManager.splitRatio = this.previousSplitRatio;
			this.counter.text = this.previousSplitRatio.ToString("0.000");
		}
	}
}
