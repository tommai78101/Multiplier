using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MultiPlayer;
using Common;

public class InitializeCategoryContent : MonoBehaviour {
	public GameObject categoryContentPrefab;
	public Transform levelingRatesContent;

	public void Awake() {
		if (this.categoryContentPrefab == null) {
			Debug.LogError("Category content prefab is not initialized.");
		}
		for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
			GameObject obj = MonoBehaviour.Instantiate(this.categoryContentPrefab) as GameObject;
			Text[] texts = obj.GetComponentsInChildren<Text>();
			//Level
			texts[0].text = "Level " + (i+1).ToString();
			//Rate
			texts[1].text = "0.0";
			//Relation
			texts[2].text = "N/A";
			obj.transform.SetParent(this.levelingRatesContent);
			
			//Always set the local scale AFTER setting it as a child Transform.
			RectTransform rect = obj.GetComponent<RectTransform>();
			rect.localScale = Vector3.one;
		}
	}
}
