using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using Common;
using MultiPlayer;
using Analytics;

public class NetworkManagerActions : MonoBehaviour {
	public NetworkManager networkManager;
	public GameObject initialMenu;
	public GameObject LANHost;
	public GameObject optionsMenu;
	public GameObject unitAttributeEditor;
	public GameObject temporaryUnitAttributesObject;
	public InputField playerNameField;
	public Dropdown difficultyDropdown;

	public GameObject LANClientNotConnected;
	public GameObject LANClientNotReady;
	public GameObject LANClientReady;

	public void Awake() {
		if (this.networkManager == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("NetworkManager");
			this.networkManager = obj.GetComponent<NetworkManager>();
			if (this.networkManager == null) {
				Debug.LogError("Cannot find Network Manager.");
			}
		}
		if (this.initialMenu == null) {
			Debug.LogError("Unassigned context menu for the first menu shown to the player.");
		}
		//if (this.endGameMenu == null) {
		//	Debug.LogError("Unassigned context menu shown to the player when the game has ended.");
		//}
		if (this.LANHost == null) {
			Debug.LogError("Unassigned context menu shown to the player when the game sets up a LAN server.");
		}
		if (this.optionsMenu == null) {
			Debug.LogError("Unassigned context menu for changing unit attributes before connection.");
		}
		if (this.unitAttributeEditor == null) {
			Debug.LogError("Unassigned context menu for unit attributes editor.");
		}
		if (this.LANClientNotConnected == null) {
			Debug.LogError("Unassigned context menu shown to the player when the game sets up a LAN client. (Not connected state)");
		}
		if (this.LANClientNotReady == null) {
			Debug.LogError("Unassigned context menu shown to the player when the game sets up a LAN client. (Not ready state)");
		}
		if (this.LANClientReady == null) {
			Debug.LogError("Unassigned context menu shown to the player when the game sets up a LAN client. (Ready state)");
		}
		if (this.playerNameField == null || this.difficultyDropdown == null) {
			Debug.LogError("Can't even initialize the most basic metric ever.");
		}
	}

	public void Start() {
		if (!NetworkClient.active && !NetworkServer.active && this.networkManager.matchMaker == null) {
			this.initialMenu.SetActive(true);
			this.optionsMenu.SetActive(true);
			this.LANHost.SetActive(false);
			this.LANClientReady.SetActive(false);
			this.LANClientNotReady.SetActive(false);
			this.LANClientNotConnected.SetActive(false);
			//this.endGameMenu.SetActive(false);
			EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
			if (enableEditorObj != null) {
				enableEditorObj.TurnOffCanvasGroup();
			}

		}
		else {
			if (NetworkServer.active) {
				this.LANClientNotConnected.SetActive(false);
				this.initialMenu.SetActive(false);
				this.optionsMenu.SetActive(false);
				this.LANClientNotReady.SetActive(false);
				this.LANClientReady.SetActive(false);
				this.LANHost.SetActive(true);
				//this.endGameMenu.SetActive(false);
				EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
				if (enableEditorObj != null) {
					enableEditorObj.TurnOffCanvasGroup();
				}
			}
			else if (NetworkClient.active) {
				this.initialMenu.SetActive(false);
				this.optionsMenu.SetActive(false);
				this.LANHost.SetActive(false);
				this.unitAttributeEditor.SetActive(false);
				this.LANClientNotConnected.SetActive(false);
				//this.endGameMenu.SetActive(false);
				EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
				if (enableEditorObj != null) {
					enableEditorObj.TurnOffCanvasGroup();
				}
				if (ClientScene.ready) {
					this.LANClientReady.SetActive(true);
				}
				else {
					this.LANClientNotReady.SetActive(true);
				}
			}
		}
	}

	public void Update() {
		if (Input.GetKeyUp(KeyCode.Escape)) {
			if (NetworkClient.active && ClientScene.ready && !NetworkServer.active) {
				if (this.LANClientReady.activeSelf || this.LANClientReady.activeInHierarchy) {
					this.LANClientReady.SetActive(false);
				}
				else {
					this.LANClientReady.SetActive(true);
				}
			}
			else if (NetworkServer.active) {
				if (this.LANHost.activeSelf || this.LANHost.activeInHierarchy) {
					this.LANHost.SetActive(false);
				}
				else {
					this.LANHost.SetActive(true);
				}
			}
			else {
				SceneManager.LoadScene("new_multiplayer");
			}
		}
	}


	/*
		██╗      █████╗ ███╗   ██╗    ██╗  ██╗ ██████╗ ███████╗████████╗
		██║     ██╔══██╗████╗  ██║    ██║  ██║██╔═══██╗██╔════╝╚══██╔══╝
		██║     ███████║██╔██╗ ██║    ███████║██║   ██║███████╗   ██║   
		██║     ██╔══██║██║╚██╗██║    ██╔══██║██║   ██║╚════██║   ██║   
		███████╗██║  ██║██║ ╚████║    ██║  ██║╚██████╔╝███████║   ██║   
		╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝    ╚═╝  ╚═╝ ╚═════╝ ╚══════╝   ╚═╝   
	*/

	public void TurnOffLANHost() {
		this.LANHost.SetActive(false);
	}

	public void StartLANHost() {
		this.PreInitialization();

		this.networkManager.StartHost();
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.LANClientReady.SetActive(false);
		this.LANClientNotReady.SetActive(false);
		this.LANHost.SetActive(true);
		this.LANClientNotConnected.SetActive(false);
		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null) {
			enableEditorObj.TurnOffCanvasGroup();
		}

		this.PreUnitAttributesInitialization();

		MinimapStuffs stuffs = GameObject.FindObjectOfType<MinimapStuffs>();
		if (stuffs != null) {
			Camera minimapCamera = stuffs.GetComponent<Camera>();
			minimapCamera.enabled = true;
			stuffs.playerCameraPanning = Camera.main.GetComponent<CameraPanning>();
		}

		string name = this.playerNameField.text;
		if (name.Length > 0) {
			GameMetricLogger.instance.playerName = name;
		}
		else {
			GameMetricLogger.instance.playerName = "Player";
		}

		if (Taskbar.Instance != null) {
			Taskbar.Instance.ShowTaskbar(true);
		}

		GameMetricLogger.instance.difficultyEquations = this.difficultyDropdown.options[this.difficultyDropdown.value].text;
		GameMetricLogger.SetGameLogger(GameLoggerOptions.StartGameMetrics);
	}

	public void StopLANHost() {
		//NOTE(Thompson): This is the official method of resetting the LAN session if the
		//player wants to play a new LAN session after ending a previous LAN session.
		SceneManager.LoadScene("new_multiplayer");

		//NOTE(Thompson): When reloading a scene, the GUIs are not reset to its initial states. Therefore
		//the following reinitialization codes are necessary.
		this.networkManager.StopHost();
		this.LANHost.SetActive(false);
		this.LANClientReady.SetActive(false);
		this.LANClientNotReady.SetActive(false);
		this.LANClientNotConnected.SetActive(false);
		this.initialMenu.SetActive(true);
		this.optionsMenu.SetActive(true);
		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null && enableEditorObj.isCustomOptionSelected) {
			enableEditorObj.TurnOnCanvasGroup();
		}

		MinimapStuffs stuffs = GameObject.FindObjectOfType<MinimapStuffs>();
		if (stuffs != null) {
			stuffs.playerCameraPanning = Camera.main.GetComponent<CameraPanning>();
			Camera minimapCamera = stuffs.GetComponent<Camera>();
			minimapCamera.enabled = false;
		}

		if (Taskbar.Instance != null) {
			Taskbar.Instance.ShowTaskbar(false);
		}

		GameMetricLogger.DisableLoggerHotkey();
		//GameMetricLogger.SetGameLogger(GameLoggerOptions.StopGameMetrics);
	}

	//NOTE(Thompson): This is equivalent to SetClientReady().
	public void SetLANHostReady() {
		this.LANHost.SetActive(false);

		GameMetricLogger.EnableLoggerHotkey();

		PostRenderer renderer = Camera.main.GetComponent<PostRenderer>();
		if (renderer != null) {
			renderer.enabled = true;
		}

		Canvas canvas = GameObject.FindObjectOfType<Canvas>();
		TooltipLocator loc = canvas.GetComponent<TooltipLocator>();
		if (loc != null) {
			loc.tooltip.SetToolTipHidden(true);
		}
	}


	/*
		███╗   ███╗ █████╗ ████████╗ ██████╗██╗  ██╗███╗   ███╗ █████╗ ██╗  ██╗███████╗██████╗ 
		████╗ ████║██╔══██╗╚══██╔══╝██╔════╝██║  ██║████╗ ████║██╔══██╗██║ ██╔╝██╔════╝██╔══██╗
		██╔████╔██║███████║   ██║   ██║     ███████║██╔████╔██║███████║█████╔╝ █████╗  ██████╔╝
		██║╚██╔╝██║██╔══██║   ██║   ██║     ██╔══██║██║╚██╔╝██║██╔══██║██╔═██╗ ██╔══╝  ██╔══██╗
		██║ ╚═╝ ██║██║  ██║   ██║   ╚██████╗██║  ██║██║ ╚═╝ ██║██║  ██║██║  ██╗███████╗██║  ██║
		╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝    ╚═════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝
	 */



	public void StartMatchMaker() {
	}




	/*
		██╗      █████╗ ███╗   ██╗     ██████╗██╗     ██╗███████╗███╗   ██╗████████╗
		██║     ██╔══██╗████╗  ██║    ██╔════╝██║     ██║██╔════╝████╗  ██║╚══██╔══╝
		██║     ███████║██╔██╗ ██║    ██║     ██║     ██║█████╗  ██╔██╗ ██║   ██║   
		██║     ██╔══██║██║╚██╗██║    ██║     ██║     ██║██╔══╝  ██║╚██╗██║   ██║   
		███████╗██║  ██║██║ ╚████║    ╚██████╗███████╗██║███████╗██║ ╚████║   ██║   
		╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝     ╚═════╝╚══════╝╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
	*/


	//Turning on/off menu
	public void TurnOnLANClientMenu() {
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.unitAttributeEditor.SetActive(false);
		this.LANHost.SetActive(false);
		this.LANClientNotConnected.SetActive(true);

		GameMetricLogger.SetGameLogger(GameLoggerOptions.GameIsOver);
	}

	public void TurnOffLANClientMenu() {
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.unitAttributeEditor.SetActive(false);
		this.LANHost.SetActive(false);
		this.LANClientReady.SetActive(false);
		this.LANClientNotReady.SetActive(false);
		this.LANClientNotConnected.SetActive(false);
		//EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		//if (enableEditorObj != null) {
		//	enableEditorObj.TurnOffCanvasGroup();
		//}

		GameMetricLogger.SetGameLogger(GameLoggerOptions.GameIsPlaying);
	}

	public void StartLANClient(InputField inputField) {
		this.PreInitialization();

		this.networkManager.networkAddress = "localhost";
		if (inputField.text.Length > 0) {
			this.networkManager.networkAddress = inputField.text;
		}
		this.networkManager.StartClient();
		this.LANClientReady.SetActive(false);
		this.LANClientNotConnected.SetActive(false);

		//NOTE(Thompson): If you want to know why we don't' check the main camera for PostRenderer,
		//and disable it here, it is because only when the connection is established between server
		//and client, the PostRenderer component will then be added to the main camera.
		//That is when PostRenderer is available for us to enable/disable. So, for now, we do not do
		//anything here, and disable the PostRenderer in the NewSpawner.Start().

		if (!ClientScene.ready) {
			this.LANClientNotReady.SetActive(true);
		}

		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null) {
			enableEditorObj.TurnOffCanvasGroup();
		}

		this.Invoke("PreUnitAttributesInitialization", 1f);

		if (Taskbar.Instance != null) {
			Taskbar.Instance.ShowTaskbar(true);
		}
	}

	public void StopLANClient() {
		if (NetworkClient.active) {
			this.networkManager.StopClient();
		}
		this.LANClientReady.SetActive(false);
		this.LANClientNotReady.SetActive(false);
		this.LANClientNotConnected.SetActive(false);
		this.initialMenu.SetActive(true);
		this.optionsMenu.SetActive(true);
		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null && enableEditorObj.isCustomOptionSelected) {
			enableEditorObj.TurnOnCanvasGroup();
		}

		GameMetricLogger.DisableLoggerHotkey();
		GameMetricLogger.SetGameLogger(GameLoggerOptions.GameIsOver);
	}

	public void SetClientReady() {
		if (!ClientScene.ready) {
			if (ClientScene.Ready(this.networkManager.client.connection)) {
				Camera cam = Camera.main.GetComponent<Camera>();
				PostRenderer renderer = cam.GetComponent<PostRenderer>();
				if (renderer != null) {
					renderer.enabled = true;
				}
				MinimapStuffs stuffs = GameObject.FindObjectOfType<MinimapStuffs>();
				if (stuffs != null) {
					Camera minimapCamera = stuffs.GetComponent<Camera>();
					minimapCamera.enabled = true;
					stuffs.playerCameraPanning = Camera.main.GetComponent<CameraPanning>();
				}
				this.LANClientReady.SetActive(false);
				this.LANClientNotReady.SetActive(false);

				GameMetricLogger.EnableLoggerHotkey();

				NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
				foreach (NewSpawner spawn in spawners) {
					if (spawn != null && spawn.hasAuthority) {
						NewSpawner.Instance.CmdSetReadyFlag(ClientScene.ready && !NetworkServer.active);
					}
				}

				GameMetricLogger.SetGameLogger(GameLoggerOptions.StartGameMetrics);
			}
			else {
				this.LANClientReady.SetActive(false);
				this.LANClientNotReady.SetActive(true);
			}
			if (ClientScene.localPlayers.Count == 0) {
				ClientScene.AddPlayer(0);
			}
		}
		else {
			this.LANClientReady.SetActive(false);
			this.LANClientNotReady.SetActive(false);

			Camera cam = Camera.main.GetComponent<Camera>();
			PostRenderer renderer = cam.GetComponent<PostRenderer>();
			if (renderer != null) {
				renderer.enabled = true;
			}
			MinimapStuffs stuffs = GameObject.FindObjectOfType<MinimapStuffs>();
			if (stuffs != null) {
				Camera minimapCamera = stuffs.GetComponent<Camera>();
				minimapCamera.enabled = true;
				stuffs.playerCameraPanning = Camera.main.GetComponent<CameraPanning>();
			}

			this.LANClientReady.SetActive(false);
			this.LANClientNotReady.SetActive(false);

			string name = this.playerNameField.text;
			if (name.Length > 0) {
				GameMetricLogger.instance.playerName = name;
			}
			else {
				GameMetricLogger.instance.playerName = "Player";
			}
			GameMetricLogger.instance.difficultyEquations = this.difficultyDropdown.options[this.difficultyDropdown.value].text;
			GameMetricLogger.EnableLoggerHotkey();

			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			foreach (NewSpawner spawn in spawners) {
				if (spawn != null && spawn.hasAuthority) {
					NewSpawner.Instance.CmdSetReadyFlag(ClientScene.ready && !NetworkServer.active);
				}
			}

			GameMetricLogger.SetGameLogger(GameLoggerOptions.StartGameMetrics);
		}

		Canvas canvas = GameObject.FindObjectOfType<Canvas>();
		TooltipLocator loc = canvas.GetComponent<TooltipLocator>();
		if (loc != null) {
			loc.tooltip.SetToolTipHidden(true);
		}
	}

/***
 *    ███████╗███╗   ██╗██████╗      ██████╗  █████╗ ███╗   ███╗███████╗
 *    ██╔════╝████╗  ██║██╔══██╗    ██╔════╝ ██╔══██╗████╗ ████║██╔════╝
 *    █████╗  ██╔██╗ ██║██║  ██║    ██║  ███╗███████║██╔████╔██║█████╗  
 *    ██╔══╝  ██║╚██╗██║██║  ██║    ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝  
 *    ███████╗██║ ╚████║██████╔╝    ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗
 *    ╚══════╝╚═╝  ╚═══╝╚═════╝      ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝
 *                                                                      
 */

	public void SetEndGameSession() {
		//if (!(this.endGameMenu.activeSelf || this.endGameMenu.activeInHierarchy)) {
		//}

		//this.endGameMenu.SetActive(true);
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.LANClientNotConnected.SetActive(false);
		this.LANClientNotReady.SetActive(false);
		this.LANClientReady.SetActive(false);
		this.LANHost.SetActive(false);
		this.unitAttributeEditor.SetActive(false);

		GameMetricLogger.SetGameLogger(GameLoggerOptions.StopGameMetrics);
		GameMetricLogger.ShowPrintLog();
	}

	public void EndGameSessionAction() {
		SceneManager.LoadScene("new_multiplayer");
	}

/***
 *    ██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗    ███████╗██╗   ██╗███╗   ██╗ ██████╗████████╗██╗ ██████╗ ███╗   ██╗
 *    ██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝    ██╔════╝██║   ██║████╗  ██║██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
 *    ██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗      █████╗  ██║   ██║██╔██╗ ██║██║        ██║   ██║██║   ██║██╔██╗ ██║
 *    ██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝      ██╔══╝  ██║   ██║██║╚██╗██║██║        ██║   ██║██║   ██║██║╚██╗██║
 *    ██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗    ██║     ╚██████╔╝██║ ╚████║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
 *    ╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝    ╚═╝      ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
 *                                                                                                                               
 */

	private void PreInitialization() {
		string equation = "";
		Dropdown[] difficultyOptions = this.optionsMenu.GetComponentsInChildren<Dropdown>(true);
		foreach (Dropdown dropdown in difficultyOptions) {
			Dropdown.OptionData data;
			switch (dropdown.value) {
				default:
				case 0:
					data = dropdown.options[dropdown.value];
					equation = data.text.Substring(0, data.text.Length - "(Easy)".Length);
					break;
				case 1:
					data = dropdown.options[dropdown.value];
					equation = data.text.Substring(0, data.text.Length - "(Normal)".Length);
					break;
				case 2:
					data = dropdown.options[dropdown.value];
					equation = data.text.Substring(0, data.text.Length - "(Hard)".Length);
					break;
				case 3:
					break;
			}
		}

		if (equation.Length > 0) {
			InputField[] equationsFields = this.unitAttributeEditor.GetComponentsInChildren<InputField>(true);
			foreach (InputField field in equationsFields) {
				field.text = equation;
				field.textComponent.text = equation;
			}
		}

		//NOTE(Thompson): Check to make sure camera have camera panning, else do not add any more.
		if (Camera.main.gameObject.GetComponent<CameraPanning>() == null) {
			CameraPanning panning = Camera.main.gameObject.AddComponent<CameraPanning>();
			panning.zoomLevel = 5;
		}
	}

	private void PreUnitAttributesInitialization() {
		GameObject obj = GameObject.Find("Temporary Unit Attributes");
		if (obj == null) {
			obj = this.temporaryUnitAttributesObject;
		}
		if (obj != null) {
			UnitAttributes myAttributes = null;
			GameObject[] playerUnitAttributes = GameObject.FindGameObjectsWithTag("UnitAttributes");
			for (int i = 0; i < playerUnitAttributes.Length; i++) {
				NetworkIdentity identity = playerUnitAttributes[i].GetComponent<NetworkIdentity>();
				if (identity.hasAuthority) {
					myAttributes = playerUnitAttributes[i].GetComponent<UnitAttributes>();
					UnitAttributes tempAttr = obj.GetComponent<UnitAttributes>();
					myAttributes.CopyFrom(tempAttr);
					break;
				}
			}
			if (myAttributes != null) {
				GameObject[] gameUnits = GameObject.FindGameObjectsWithTag("Unit");
				foreach (GameObject unit in gameUnits) {
					NetworkIdentity identity = unit.GetComponent<NetworkIdentity>();
					if (identity != null && identity.hasAuthority) {
						GameUnit gameUnit = unit.GetComponent<GameUnit>();
						gameUnit.unitAttributes = myAttributes;
					}
				}
			}
			GameMetricLogger.SetGameLogger(GameLoggerOptions.GameIsPlaying);
		}
	}
}
