using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SingleHost : NetworkManager {
	public CanvasGroup attributePanelGroup;
	public bool notReady = false;

	public void Start() {
		this.notReady = true;
		this.StartHost();
	}

	public void Update() {
		if (this.notReady) {
			GameObject minimapCameraObject = GameObject.FindGameObjectWithTag("Minimap");
			Camera minimapCamera = minimapCameraObject.GetComponent<Camera>();
			if (minimapCamera != null && minimapCamera.enabled) {
				minimapCamera.enabled = false;
			}
		}
	}

	public void SingleStartHost() {
		if (this.attributePanelGroup != null) {
			this.attributePanelGroup.alpha = 0f;
			this.attributePanelGroup.blocksRaycasts = false;
			this.attributePanelGroup.interactable = false;
		}

		GameObject minimapCameraObject = GameObject.FindGameObjectWithTag("Minimap");
		Camera minimapCamera = minimapCameraObject.GetComponent<Camera>();
		if (minimapCamera != null && !minimapCamera.enabled) {
			minimapCamera.enabled = true;
		}

		this.notReady = false;
	}

}
