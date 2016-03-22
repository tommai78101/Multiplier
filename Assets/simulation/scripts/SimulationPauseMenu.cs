using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimulationPauseMenu : MonoBehaviour {
	public KeyCode triggerKey;
	public CanvasGroup canvasGroup;

	public void Start() {
		if (this.canvasGroup == null) {
			Debug.LogError("Canvas group is not defined.");
			return;
		}
	}

	public void Update() {
		if (Input.GetKeyUp(triggerKey)) {
			Toggle();
		}
	}

	public void Toggle() {
		this.canvasGroup.alpha = (this.canvasGroup.alpha == 1) ? 0 : 1;
		this.canvasGroup.interactable = (this.canvasGroup.interactable) ? false : true;
		this.canvasGroup.blocksRaycasts = (this.canvasGroup.blocksRaycasts) ? false : true;
	}
}
