using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using SinglePlayer;
using MultiPlayer;
using SinglePlayer.UI;


public class SingleHost : NetworkManager {
	public CanvasGroup attributePanelGroup;
	public CanvasGroup pauseMenuGroup;
	public bool notReady = false;
	public bool enablePauseGameMenu = false;
	public GameObject AIPlayer;
	public GameObject AIUnits;
	public GameObject HumanPlayer;
	public GameObject HumanUnits;
	public GameObject AIUnitPrefab;
	public GameObject playerUmbrellaObject;

	private GameObject playerObject;
	
	public void Start() {
		this.notReady = true;
		this.enablePauseGameMenu = false;
		this.StartHost();
	}

	public void Update() {
		//TODO: Create a menu to back out into the main menu.
		if (this.enablePauseGameMenu && Input.GetKeyUp(KeyCode.Escape)) {
			TogglePauseMenu();
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
					this.playerObject.SetActive(false);
				}
			}
		}
	}

	public void SingleStartHost() {
		DropdownFix[] fixes = GameObject.FindObjectsOfType<DropdownFix>();
		int values = 0;
		for (int i = 0; i < fixes.Length; i++) {
			values += fixes[i].value;
		}
		if (values <= 0) {
			//Start Game button is now interactable only when player hasn't chosen a preset.
			return;
		}
		//Note(Thompson): Player is always index 1.
		//Note(Thompson): Obtain the A.I. level difficulty.
				//Note(Thompson): Check if the Player dropdown value is 5. If not, and is above 0, return difficulty equation used. Else, return a full range of custom equations used.
				

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
					AImanager.UpdateDifficulty(group.GetDifficulty());
				}

				unit.SetTeam(AImanager.teamFaction);
				AImanager.Activate();
			}
		}

		this.playerUmbrellaObject = GameObject.FindGameObjectWithTag("Player");
		if (this.playerUmbrellaObject != null) {
			this.HumanPlayer = this.playerUmbrellaObject;
			Transform unitUmbrellaTransform = this.playerUmbrellaObject.transform.GetChild(0);
			this.HumanUnits = unitUmbrellaTransform.gameObject;
		}

		SetupPlayerUnits();

		if (!this.isNetworkActive) {
			this.StartHost();
		}

		this.enablePauseGameMenu = true;
		this.notReady = false;

		if (Taskbar.Instance != null) {
			Taskbar.Instance.ShowTaskbar(true);
		}

							}

	public void ResetAttributePanelUI() {
		if (this.pauseMenuGroup != null) {
			if (this.pauseMenuGroup.gameObject.activeInHierarchy || this.pauseMenuGroup.gameObject.activeSelf) {
				this.pauseMenuGroup.gameObject.SetActive(false);
			}
		}

		if (this.attributePanelGroup != null) {
			this.attributePanelGroup.alpha = 1f;
			this.attributePanelGroup.blocksRaycasts = true;
			this.attributePanelGroup.interactable = true;
		}

		GameObject minimapCameraObject = GameObject.FindGameObjectWithTag("Minimap");
		Camera minimapCamera = minimapCameraObject.GetComponent<Camera>();
		if (minimapCamera != null && minimapCamera.enabled) {
			minimapCamera.enabled = false;
		}

		if (this.playerObject != null) {
			this.playerObject.SetActive(false);
		}

		AIManager AImanager = this.AIPlayer.GetComponentInChildren<AIManager>();
		if (AImanager != null) {
			AImanager.Deactivate();
		}

		foreach (Transform child in this.AIUnits.transform) {
			MonoBehaviour.Destroy(child.gameObject);
		}

		GameObject spawner = GameObject.FindGameObjectWithTag("Spawner");
		if (spawner != null) {
			MonoBehaviour.Destroy(spawner);
			MonoBehaviour.Destroy(this.HumanPlayer);
			this.HumanPlayer = null;
			this.HumanUnits = null;
		}

		this.StopHost();
		this.StopServer();

		this.enablePauseGameMenu = false;
		this.notReady = true;
	}

	public void TogglePauseMenu() {
		if (this.pauseMenuGroup != null) {
			if (this.pauseMenuGroup.gameObject.activeInHierarchy || this.pauseMenuGroup.gameObject.activeSelf) {
				this.pauseMenuGroup.gameObject.SetActive(false);
			}
			else {
				this.pauseMenuGroup.gameObject.SetActive(true);
			}
		}
	}

	public string ConvertCustomToEquations(int value, string text) {
		StringBuilder sB = new StringBuilder();
		return sB.ToString();
	}

	public void SetupPlayerUnits() {
		if (this.HumanUnits != null) {
			foreach (Transform child in this.HumanUnits.transform) {
				GameUnit unit = child.GetComponent<GameUnit>();
				if (unit != null) {
					unit.UpdateUnitAttributes();
				}
				else {
					Debug.LogError("Didn't see any GameUnit objects.");
				}
			}
		}
		else {
			Debug.LogError("Transform object, Units, is not set.");
		}
	}
}
