using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CategoryInfo : MonoBehaviour {
	public Text level;
	public Text rate;
	public Text relation;

	public void Start() {
		if ((level == null) || (rate == null) || (relation == null)) {
			Debug.LogError("All text elements are not set.");
		}
	}

	public int Compare(CategoryInfo info) {
		if (this.Equals(info)) {
			return 0;
		}
		try {
			Text[] texts = info.GetComponentsInChildren<Text>();
			for (int i = 0; i < texts.Length; i++) {
				if (texts[i].name.Equals("Rate")) {
					float otherNumber = float.Parse(texts[i].text);
					float myNumber = float.Parse(this.rate.text);
					//There's no equal or approximate relations. Being thorough here.
					if (myNumber < otherNumber) {
						//Other number is larger, so we are smaller in comparison.
						return -1;
					}
					else if (myNumber > otherNumber) {
						//Other number is smaller, so we are larger in comparison.
						return 1;
					}
					break;
				}
			}
		}
		catch (System.Exception e) {
			Debug.LogError("Error parsing data from Category Info: " + e.ToString());
		}
		return -1;
	}
}
