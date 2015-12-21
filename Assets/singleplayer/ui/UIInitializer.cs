using UnityEngine;
using System.Collections;

public class UIInitializer : MonoBehaviour {
	void Start() {
		RectTransform trans = this.GetComponent<RectTransform>();
		if (trans != null) {
			trans.localPosition = new Vector2(0f, 0f);
		}
	}
}
