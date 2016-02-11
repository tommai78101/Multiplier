using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManagerActions : MonoBehaviour {
	public NetworkManager networkManager;
	public GameObject initialMenu;
	public GameObject LANHost;

	public void Awake() {
		if (this.networkManager == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("NetworkManager");
			this.networkManager = obj.GetComponent<NetworkManager>();
			if (this.networkManager == null) {
				Debug.LogError("Cannot find Network Manager.");
			}
		}
	}

	public void Start() {
		this.initialMenu.SetActive(true);
		this.LANHost.SetActive(false);
	}

	//Start functions

	public void StartLANHost() {
		this.networkManager.StartHost();
		this.LANHost.SetActive(true);
		this.initialMenu.SetActive(false);

	}
	public void StartLANClient() {
		this.networkManager.StartHost();
	}
	public void StartMatchMaker() {
		this.networkManager.StartHost();
	}

	//Stop functions

	public void StopLANHost() {
		this.networkManager.StopHost();
		this.LANHost.SetActive(false);
		this.initialMenu.SetActive(true);
	}


}
