using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Simulation {
	public class LevelInfo : MonoBehaviour {
		public int level;
		public float rate;
		public byte comparisonFlag;

		public Text levelText;
		public Text rateText;
		public Text signText;

		public void Start() {
			bool flag = levelText == null;
			flag |= rateText == null;
			flag |= signText == null;
			if (flag) {
				Debug.LogError("One of these Text UI components is not initialized.");
			}
		}

		public void UpdateText() {
			levelText.text = "Level " + level.ToString();
			rateText.text = rate.ToString();
			string value;
			switch (comparisonFlag) {
				default:
					value = "Error";
					break;
				case SimulationManager.INVALID:
					value = "N/A";
					break;
				case SimulationManager.LARGER:
					value = "++";
					break;
				case SimulationManager.APPROXIMATE:
					value = "+-";
					break;
				case SimulationManager.SMALLER:
					value = "--";
					break;
			}
			signText.text = value;
		}
	}
}
