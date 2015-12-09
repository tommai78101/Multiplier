using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SinglePlayer.UI {

	public class CategoryHandler : MonoBehaviour {
		public GameObject categoryItemPrefab;
		public List<string> items = new List<string>() {
			"Health", "Attack", "Speed", "Attack Cooldown", "Split Time", "Merge Time"
		};

		public void Start() {
			if (this.transform.childCount > 0) {
				for (int i = 0; i < this.transform.childCount; i++) {
					GameObject.Destroy(this.transform.GetChild(i).gameObject);
				}
			}
			if (this.categoryItemPrefab != null) {
				for (int i = 0; i < this.items.Count; i++) {
					GameObject obj = GameObject.Instantiate(this.categoryItemPrefab) as GameObject;
					Text text = obj.GetComponentInChildren<Text>();
					text.text = this.items[i];
					Toggle toggle = obj.GetComponent<Toggle>(); asdasd
					if (toggle != null) {
						if (i == 0) {
							toggle.isOn = true;
						}
						else {
							toggle.isOn = false;
						}
					}
				}
			}
		}
	}
}
