using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer.UI {
	public class DifficultyGroup : MonoBehaviour {
		public ToggleGroup difficultyModeGroup;

		// Use this for initialization
		void Start() {
			if (this.difficultyModeGroup == null) {
				GameObject[] menu = GameObject.FindGameObjectsWithTag("Menu");
				if (menu != null && menu.Length > 0) {
					foreach (GameObject obj in menu) {
						if (obj.GetComponent<DifficultyGroup>() != null) {
							this.difficultyModeGroup = obj.GetComponent<ToggleGroup>();
							break;
						}
					}
				}
				if (this.difficultyModeGroup == null) {
					Debug.LogError("Toggle group isn't set. Please check.");
				}
			}
		}

		public Difficulty GetDifficulty() {
			try {
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
			}
			catch {
				return Difficulty.Normal;
			}
			return Difficulty.Normal;
		}
	}
}
