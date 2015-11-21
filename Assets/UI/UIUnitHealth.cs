using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIUnitHealth : MonoBehaviour {
	public GameObject uiTextPrefab;
	public Text unitHealth;
	public Canvas mainCanvas;
	public GameUnit gameUnit;
	public Vector3 tweak;

	public void Start() {
		if (this.mainCanvas == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("Canvas");
			if (obj != null) {
				this.mainCanvas = obj.GetComponent<Canvas>();
				if (this.mainCanvas == null) {
					Debug.LogError("UIUnitHealth: Cannot find Canvas game object.");
				}
			}
		}
		if (this.unitHealth == null) {
			GameObject obj = MonoBehaviour.Instantiate<GameObject>(this.uiTextPrefab) as GameObject;
			this.unitHealth = obj.GetComponent<Text>();
			obj.transform.SetParent(this.mainCanvas.transform);
			RectTransform rectTransform = obj.GetComponent<RectTransform>();
			if (rectTransform != null) {
				rectTransform.localPosition = new Vector3(0f, 0f, 0f);
				rectTransform.localRotation = Quaternion.identity;
				rectTransform.localScale = new Vector3(1f, 1f, 1f);
			}
		}
		this.tweak = new Vector3(-405.1f, -248.8f, -13.5f);
	}

	public void Update() {
		if (this.unitHealth != null) {
			Vector3 pos = Camera.main.WorldToScreenPoint(this.gameUnit.transform.position);
			this.unitHealth.rectTransform.localPosition = pos + this.tweak;
			this.unitHealth.text = gameUnit.currentHealth.ToString() + "/" + gameUnit.maxHealth.ToString();
		}
	}
}
