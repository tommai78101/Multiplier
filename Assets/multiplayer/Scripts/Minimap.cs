using UnityEngine;
using System.Collections;


public class Minimap : MonoBehaviour {
	public Camera minimapCamera;

	public void Awake() {
		if (this.minimapCamera == null) {
			Debug.LogError("Minimap camera has not been set.");
		}
	}

	public void FixedUpdate() {
		float aspectRatio = Camera.main.aspect;
		if (aspectRatio >= 1.7f) {
			this.minimapCamera.rect = new Rect(0.8f, -0.64f, 0.5f, 1f);
		}
		else if (aspectRatio >= 1.6f) {
			this.minimapCamera.rect = new Rect(0.775f, -0.64f, 0.5f, 1f);
		}
		else if (aspectRatio >= 1.5f) {
			this.minimapCamera.rect = new Rect(0.76f, -0.64f, 0.5f, 1f);
		}
		else if (aspectRatio >= 1.3f) {
			this.minimapCamera.rect = new Rect(0.725f, -0.64f, 0.5f, 1f);
		}
		else if (aspectRatio >= 1.24f) {
			this.minimapCamera.rect = new Rect(0.71f, -0.64f, 0.5f, 1f);
		}
	}
}

