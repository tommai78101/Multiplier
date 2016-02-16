using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


public class FirstMenu : MonoBehaviour {
	public NetworkManager networkManager;
	public RectTransform multiplayerMenu_Main;

	public void StartHost() {
		this.networkManager.StartHost();
	}

	public void StartClient() {
		this.networkManager.StartClient();
	}

	public void EnableMatchMaker() {
		this.networkManager.StartMatchMaker();
	}
}
