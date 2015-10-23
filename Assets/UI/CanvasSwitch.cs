using UnityEngine;
using System.Collections;

public class CanvasSwitch : MonoBehaviour {
	public CanvasGroup canvasGroup;
	public bool showCanvas;

	public void Start() {
		if (this.canvasGroup == null) {
			Debug.LogError("Canvas group has not been set. Please check.");
		}
		this.showCanvas = true;
		this.ToggleCanvas();
	}

	public void Update() {
		if (Input.GetKeyUp(KeyCode.M)) {
			this.showCanvas = !this.showCanvas;
			this.ToggleCanvas();
		}
	}

	public void ToggleCanvas() {
		this.canvasGroup.alpha = this.showCanvas ? 1f : 0f;
		this.canvasGroup.interactable = this.showCanvas;
		this.canvasGroup.blocksRaycasts = this.showCanvas;
	}
}
