using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManagerActions : MonoBehaviour {
	public NetworkManager networkManager;
	public GameObject initialMenu;
	public GameObject LANHost;
	public GameObject optionsMenu;
	public GameObject unitAttributeEditor;

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
	}

	public void Start() {
		this.initialMenu.SetActive(true);
		this.optionsMenu.SetActive(true);
		this.LANHost.SetActive(false);
		this.unitAttributeEditor.SetActive(false);
	}

	//Start functions

	public void StartLANHost() {
		Debug.Log("Starting LAN Host.");
		this.networkManager.StartHost();
		this.initialMenu.SetActive(false);
		this.optionsMenu.SetActive(false);
		this.unitAttributeEditor.SetActive(false);
		this.LANHost.SetActive(true);
	}
	public void StartLANClient() {
		this.networkManager.StartHost();
	}
	public void StartMatchMaker() {
		this.networkManager.StartHost();
	}

	//Stop functions

	public void StopLANHost() {
		Debug.Log("Stopping LAN Host.");
		this.networkManager.StopHost();
		this.initialMenu.SetActive(true);
		this.optionsMenu.SetActive(true);
		EnableAttributeEditor enableEditorObj = this.optionsMenu.GetComponentInChildren<EnableAttributeEditor>();
		if (enableEditorObj != null) {
			if (enableEditorObj.isCustomOptionSelected) {
				this.unitAttributeEditor.SetActive(true);
			}
			else {
				this.unitAttributeEditor.SetActive(false);
			}
		}
		this.LANHost.SetActive(false);
	}


}
