using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using SinglePlayer;

public class SingleHost : NetworkManager {
	public CanvasGroup attributePanelGroup;
	public bool notReady = false;
	public GameObject AIPlayer;
	public GameObject AIUnits;
	public GameObject AIUnitPrefab;

	private GameObject playerObject;

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

			if (this.playerObject == null) {
				this.playerObject = GameObject.FindGameObjectWithTag("Unit");
				if (this.playerObject != null && (playerObject.activeSelf || playerObject.activeInHierarchy)) {
					Debug.Log("Deactivating the player's game unit.");
					this.playerObject.SetActive(false);
				}
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

		if (this.playerObject != null) {
			this.playerObject.SetActive(true);
		}

		if (this.AIPlayer != null) {
			//AI Unit spawning.
			GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
			obj.transform.SetParent(this.AIUnits.transform);
			obj.transform.position = this.GetStartPosition().position;
			AIUnit unit = obj.GetComponent<AIUnit>();
			//NetworkServer.Spawn(obj);

			//AI manager spawning.
			AIManager AIManager = this.AIPlayer.GetComponentInChildren<AIManager>();
			if (AIManager != null) {
				unit.unitManager = AIManager;
				AIManager.Activate();
			}
		}

		this.notReady = false;
	}

}
