using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class StartNetwork : MonoBehaviour {
	public void Start() {
		NetworkManager manager = this.GetComponent<NetworkManager>();
		manager.StartHost();
	}
}
