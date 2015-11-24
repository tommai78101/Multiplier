using UnityEngine;
using System.Collections.Generic;
using MultiPlayer;

public class HealthBar : MonoBehaviour {
	public GameUnit unit;
	public Camera minimapCamera;
	public Vector3 viewportPosition;

	public void Start() {
		if (this.minimapCamera == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("Minimap");
			this.minimapCamera = obj.GetComponent<Camera>();
			if (this.minimapCamera == null) {
				Debug.LogError("HealthBar: This failed to initialize minimap camera.");
			}
		}
	}

	public void OnGUI() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.black;
		style.alignment = TextAnchor.MiddleCenter;
		Vector3 healthPosition = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
		this.viewportPosition = Camera.main.ScreenToViewportPoint(new Vector3(healthPosition.x, healthPosition.y + 30f));
		if (!this.minimapCamera.rect.Contains(this.viewportPosition)) {
			Rect healthRect = new Rect(healthPosition.x - 50f, (Screen.height - healthPosition.y) - 45f, 100f, 25f);
			GUI.Label(healthRect, unit.currentHealth.ToString() + "/" + unit.maxHealth.ToString(), style);
		}
	}
}
