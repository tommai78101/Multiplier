using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Extension;

namespace SinglePlayer.UI {

	public class CategoryHandler : MonoBehaviour {
		public GameObject categoryItemPrefab;
		public List<string> items;
		public ToggleGroup categoriesToggleGroup;

		public void Start() {
			this.items = new List<string>() { "Health", "Attack", "Speed", "AtkCooldown", "Split", "Merge" };
			if (this.categoriesToggleGroup == null) {
				Debug.LogError("Something is wrong with the toggle group. Please check.");
			}
			if (this.transform.childCount > 0) {
				for (int i = 0; i < this.transform.childCount; i++) {
					GameObject.Destroy(this.transform.GetChild(i).gameObject);
				}
			}
			if (this.categoryItemPrefab != null) {
				for (int i = 0; i < this.items.Count; i++) {
					GameObject obj = GameObject.Instantiate(this.categoryItemPrefab) as GameObject;
					obj.transform.SetParent(this.transform);
					RectTransform rectTransform = obj.GetComponent<RectTransform>();
					if (rectTransform != null) {
						rectTransform.localScale = Vector3.one;
						rectTransform.offsetMax = new Vector2(rectTransform.GetWidth() + 20f, rectTransform.offsetMax.y);
					}
					Toggle toggle = obj.GetComponent<Toggle>();
					if (toggle != null) {
						toggle.group = this.categoriesToggleGroup;
						if (i == 0) {
							toggle.isOn = true;
						}
						else {
							toggle.isOn = false;
						}
					}
					Text text = obj.GetComponentInChildren<Text>();
					if (text != null) {
						text.text = this.items[i];
						RectTransform textTransform = text.GetComponent<RectTransform>();
						if (textTransform != null) {
							textTransform.offsetMin = new Vector2(-textTransform.GetWidth()+17f, textTransform.offsetMin.y);
						}
					}
				}
			}
		}
	}
}
