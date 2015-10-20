using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Attributes : MonoBehaviour {
	public GameObject panelPrefab;

	public void Start() {
		if (this.panelPrefab == null) {
			Debug.LogError("Panels prefab has not been set.");
			return;
		}

		string[] attributesList = new string[] {
			"Health", "Attack", "Speed", "Merge", "Split"
		};

		for (int i = 0; i < attributesList.Length; i++) {
			GameObject obj = MonoBehaviour.Instantiate<GameObject>(this.panelPrefab);
			obj.transform.SetParent(this.transform);

			Title title = obj.GetComponentInChildren<Title>();
			if (title != null) {
				title.titleText.text = attributesList[i];
			}

			Number number = obj.GetComponentInChildren<Number>();
			if (number != null) {
				number.numberText.text = (1234.001f).ToString();
			}
		} 
	}
}
