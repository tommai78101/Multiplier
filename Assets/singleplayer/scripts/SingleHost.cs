using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SingleHost : NetworkManager {
	public CanvasGroup attributePanelGroup;

	public void SingleStartHost() {
		if (this.attributePanelGroup != null) {
			this.attributePanelGroup.alpha = 0f;
			this.attributePanelGroup.blocksRaycasts = false;
			this.attributePanelGroup.interactable = false;
		}

		GameObject minimapCamera = GameObject.FindGameObjectWithTag("Minimap");
		if (minimapCamera != null && !(minimapCamera.activeSelf && minimapCamera.activeInHierarchy)) {
			minimapCamera.SetActive(true);
		}

		this.StartHost();
	}

}
