using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SinglePlayer.UI {
	public class CustomFieldHandler : MonoBehaviour {
		public GameObject customFieldPrefab;
		public AttributePanelUI attributePanelUI;
		public GameObject levelTier;

		public void Start() {
			foreach (Transform child in this.transform) {
				MonoBehaviour.Destroy(child.gameObject);
			}
			if (this.attributePanelUI == null) {
				GameObject tempObj = GameObject.FindGameObjectWithTag("AIAttributePanel");
				this.attributePanelUI = tempObj.GetComponent<AttributePanelUI>();
				if (this.attributePanelUI == null) {
					Debug.LogError("Something is wrong. Please check.");
				}
			}

			//this.levelTier = MonoBehaviour.Instantiate(this.customFieldPrefab) as GameObject;
			//Text title = this.levelTier.GetComponent<Text>();
			//title.text = "Level Tier";
			//InputField inputField = this.levelTier.GetComponent<InputField>();
			//inputField.text = "1";
			//this.levelTier.transform.SetParent(this.transform);
			foreach (Category cat in Category.Values) {
				GameObject temp = MonoBehaviour.Instantiate(this.customFieldPrefab) as GameObject;
				Text text = temp.GetComponentInChildren<Text>();
				if (text != null) {
					text.text = cat.name;
				}
				InputField field = temp.GetComponentInChildren<InputField>();
				if (field != null) {
					field.text = cat.name;
				}
				temp.transform.SetParent(this.transform);
				RectTransform tempRect = temp.GetComponent<RectTransform>();
				if (tempRect != null) {
					tempRect.localScale = Vector3.one;
				}
			}

			LevelRateHandler aiRateHandler = this.attributePanelUI.levelingRatesObject;
			if (aiRateHandler != null && aiRateHandler.allAttributes != null) {

			}
		}
	}
}
