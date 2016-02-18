using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using MultiPlayer;

public class NetworkManagerActions : MonoBehaviour {
	public NetworkManager networkManager;
	public GameObject initialMenu;
	public GameObject LANHost;
	public GameObject optionsMenu;
	public GameObject unitAttributeEditor;
	public GameObject temporaryUnitAttributesObject;

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
	}

	public void Start() {
		if (!NetworkClient.active && !NetworkServer.active && this.networkManager.matchMaker == null) {
			this.initialMenu.SetActive(true);
			this.optionsMenu.SetActive(true);
			this.LANHost.SetActive(false);
			this.LANClientReady.SetActive(false);
			this.LANClientNotReady.SetActive(false);
			this.LANClientNotConnected.SetActive(false);
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
					if (this.LANClientReady.activeSelf || this.LANClientReady.activeInHierarchy) {
						Debug.Log("Closing LAN client Ready state menu.");
						this.LANClientReady.SetActive(false);
					}
					else {
						Debug.Log("Bringing up LAN client Ready state menu.");
						this.LANClientReady.SetActive(true);
					}
				}
			}
			else if (NetworkServer.active) {
				if (this.LANHost.activeSelf || this.LANHost.activeInHierarchy) {
					Debug.Log("Closing LAN Host menu.");
					this.LANHost.SetActive(false);
				}
				else {
					Debug.Log("Bringing up LAN Host menu.");
					this.LANHost.SetActive(true);
				}
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

	public void StartLANHost() {
		this.PreInitialization();

		Debug.Log("Starting LAN Host.");
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

	}

	public void StopLANHost() {
		Debug.Log("Stopping LAN Host.");
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
	}

	public void TurnOffLANHost() {
		Debug.Log("Turning off LAN Host menu.");
		this.LANHost.SetActive(false);
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
		Debug.Log("Starting match maker.");
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
		Debug.Log("Turning on LAN Client menu.");
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.unitAttributeEditor.SetActive(false);
		this.LANHost.SetActive(false);
		this.LANClientNotConnected.SetActive(true);
	}

	public void TurnOffLANClientMenu() {
		Debug.Log("Turning off LAN Client menu.");
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
	}

	public void StartLANClient(InputField inputField) {
		this.PreInitialization();

		Debug.Log("Starting LAN client.");
		this.networkManager.networkAddress = "localhost";
		if (inputField.text.Length > 0) {
			this.networkManager.networkAddress = inputField.text;
		}
		this.networkManager.StartClient();
		this.LANClientReady.SetActive(false);
		this.LANClientNotConnected.SetActive(false);
		this.LANClientNotReady.SetActive(true);
		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null) {
			enableEditorObj.TurnOffCanvasGroup();
		}

		this.Invoke("PreUnitAttributesInitialization", 1f);
	}

	public void StopLANClient() {
		Debug.Log("Stopping LAN client.");
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
	}

	public void SetClientReady() {
		Debug.Log("Setting LAN client as 'ready' state.");
		if (!ClientScene.ready) {
			if (ClientScene.Ready(this.networkManager.client.connection)) {
				this.LANClientReady.SetActive(false);
				this.LANClientNotReady.SetActive(false);
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
		}
	}

	private void PreInitialization() {
		Debug.Log("Fetching equations from options.");
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

		if (equation.Length <= 0) {
			Debug.Log("Fetching equations from unit attributes editor.");
		}
		else {
			Debug.Log("Setting new equations from options to unit attributes editor.");
			InputField[] equationsFields = this.unitAttributeEditor.GetComponentsInChildren<InputField>(true);
			foreach (InputField field in equationsFields) {
				Debug.Log("Setting the equations to the input field.");
				field.text = equation;
				field.textComponent.text = equation;
			}
		}

		Debug.Log("Adding camera panning when game starts.");
		CameraPanning panning = Camera.main.gameObject.AddComponent<CameraPanning>();
		panning.zoomLevel = 5;
	}

	private void PreUnitAttributesInitialization() {
		GameObject obj = GameObject.Find("Temporary Unit Attributes");
		if (obj == null) {
			Debug.Log("The object is inactive.");
			obj = this.temporaryUnitAttributesObject;
		}
		if (obj != null) {
			UnitAttributes myAttributes = null;
			Debug.Log("Fetching temporary unit attributes.");
			GameObject[] playerUnitAttributes = GameObject.FindGameObjectsWithTag("UnitAttributes");
			Debug.Log("Length: " + playerUnitAttributes.Length);
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
						gameUnit.attributes = myAttributes;
					}
				}
			}
		}

	}
}
