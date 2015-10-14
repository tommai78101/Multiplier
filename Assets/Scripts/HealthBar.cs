using UnityEngine;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour {
	public GameUnit unit;

	public void OnGUI() {
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.black;
		style.alignment = TextAnchor.MiddleCenter;
		Vector3 healthPosition = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
		Rect healthRect = new Rect(healthPosition.x - 50f, (Screen.height - healthPosition.y) - 45f, 100f, 25f);
		GUI.Label(healthRect, unit.currentHealth.ToString() + "/" + unit.maxHealth.ToString(), style);
	}
}
