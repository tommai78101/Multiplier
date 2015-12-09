using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer.UI {
	public class DifficultyGroup : MonoBehaviour {
		public Toggle easyDifficulty;
		public Toggle normalDifficulty;
		public Toggle hardDifficulty;
		private ToggleGroup difficultyModeGroup;

		// Use this for initialization
		void Start() {
			this.easyDifficulty.isOn = false;
			this.normalDifficulty.isOn = true;
			this.hardDifficulty.isOn = false;
		}

		public Difficulty GetDifficulty() {
			foreach (Toggle result in this.difficultyModeGroup.ActiveToggles()) {
				if (result.isOn) {
					Text text = result.GetComponentInChildren<Text>();
					if (text.text.Equals("Easy")) {
						return Difficulty.Easy;
					}
					else if (text.text.Equals("Normal")) {
						return Difficulty.Normal;
					}
					else if (text.text.Equals("Hard")) {
						return Difficulty.Hard;
					}
				}
			}
			return Difficulty.Normal;
		}
	}
}
