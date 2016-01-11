using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using SinglePlayer;
using SinglePlayer.UI;

public class SingleHost : NetworkManager {
	public CanvasGroup attributePanelGroup;
	public CanvasGroup pauseMenuGroup;
	public bool notReady = false;
	public bool enablePauseGameMenu = false;
	public GameObject AIPlayer;
	public GameObject AIUnits;
	public GameObject AIUnitPrefab;

	private GameObject playerObject;

	public void Start() {
		this.notReady = true;
		this.enablePauseGameMenu = false;
		this.StartHost();
	}

	public void Update() {
		//TODO: Create a menu to back out into the main menu.
		if (this.enablePauseGameMenu && Input.GetKeyUp(KeyCode.Escape)) {
			if (this.pauseMenuGroup != null) {
				if (this.pauseMenuGroup.gameObject.activeInHierarchy || this.pauseMenuGroup.gameObject.activeSelf) {
					this.pauseMenuGroup.gameObject.SetActive(false);
				}
				else {
					this.pauseMenuGroup.gameObject.SetActive(true);
				}
			}
		}

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
			//AI attribute manager.
			AIAttributeManager attributeManager = this.AIPlayer.GetComponentInChildren<AIAttributeManager>();
			if (attributeManager != null) {
				Debug.Log("It works!");
			}

			//AI Unit spawning.
			GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
			obj.transform.SetParent(this.AIUnits.transform);
			obj.transform.position = this.GetStartPosition().position;
			AIUnit unit = obj.GetComponent<AIUnit>();

			//AI manager spawning.
			AIManager AImanager = this.AIPlayer.GetComponentInChildren<AIManager>();
			if (AImanager != null) {
				unit.unitManager = AImanager;

				if (attributeManager != null && attributeManager.attributePanelUI != null) {
					DifficultyGroup group = attributeManager.attributePanelUI.aiCalibrationDifficulty;
					AImanager.difficulty = group.GetDifficulty();
				}

				unit.SetTeam(AImanager.teamFaction);
				AImanager.Activate();
			}
		}
		this.enablePauseGameMenu = true;
		this.notReady = false;
	}

}
