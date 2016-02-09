using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NetworkManagerActions : MonoBehaviour {
	public NetworkManager networkManager;

	public void Start() {
		if (this.networkManager == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("NetworkManager");
			this.networkManager = obj.GetComponent<NetworkManager>();
			if (this.networkManager == null) {
				Debug.LogError("Cannot find Network Manager.");
			}
		}
	}

	public void StartLANHost() {
		this.networkManager.StartHost();
	}
	public void StartLANClient() {
		this.networkManager.StartHost();
	}
	public void StartMatchMaker() {
		this.networkManager.StartHost();
	}
}
