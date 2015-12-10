using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using Extension;

namespace SinglePlayer.UI {
	public class CategoryHandler : MonoBehaviour {
		public GameObject categoryItemPrefab;
		public IEnumerable<Category> items;
		public ToggleGroup categoriesToggleGroup;
		public string selectedToggle;
		private bool flipFlag;

		public void Start() {
			this.flipFlag = true;
			this.items = Category.Values;
			if (this.categoriesToggleGroup == null) {
				Debug.LogError("Something is wrong with the toggle group. Please check.");
			}
			if (this.transform.childCount > 0) {
				for (int i = 0; i < this.transform.childCount; i++) {
					GameObject.Destroy(this.transform.GetChild(i).gameObject);
				}
			}
			if (this.categoryItemPrefab != null) {
				bool first = true;
				foreach (Category cat in this.items) {
					GameObject obj = GameObject.Instantiate(this.categoryItemPrefab) as GameObject;
					obj.transform.SetParent(this.transform);
					RectTransform rectTransform = obj.GetComponent<RectTransform>();
					if (rectTransform != null) {
						rectTransform.localScale = Vector3.one;
						rectTransform.offsetMax = new Vector2(rectTransform.GetWidth() + 20f, rectTransform.offsetMax.y);
					}
					Text text = obj.GetComponentInChildren<Text>();
					if (text != null) {
						text.text = cat.name;
						RectTransform textTransform = text.GetComponent<RectTransform>();
						if (textTransform != null) {
							textTransform.offsetMin = new Vector2(-textTransform.GetWidth() + 17f, textTransform.offsetMin.y);
						}
					}
					Toggle toggle = obj.GetComponent<Toggle>();
					if (toggle != null) {
						toggle.group = this.categoriesToggleGroup;
						if (first) {
							toggle.isOn = true;
							this.flipFlag = false;
							this.OnChangeCategory(toggle);
							first = false;
						}
						else {
							toggle.isOn = false;
						}
						toggle.onValueChanged.AddListener(delegate {
							OnChangeCategory(toggle);
						});
					}
				}
			}
		}

		public void OnChangeCategory(Toggle toggle) {
			this.flipFlag = !this.flipFlag;
			if (this.flipFlag) {
				Text text = toggle.GetComponentInChildren<Text>();
				if (text != null) {
					this.selectedToggle = text.text;
				}
			}
		}
	}
}
