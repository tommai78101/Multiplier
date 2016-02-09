using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FetchIPAddress : MonoBehaviour {
	public NetworkManager networkManager;
	public InputField inputField;

	public void Start() {
		if (this.networkManager == null) {
			GameObject obj = GameObject.FindGameObjectWithTag("NetworkManager");
			this.networkManager = obj.GetComponent<NetworkManager>();
			if (this.networkManager == null) {
				Debug.LogError("Cannot find Network Manager.");
			}
		}
		if (this.inputField == null) {
			Debug.LogError("Cannot find Input field.");
		}
		else {
			ObtainExternalIPAddress();
		}
	}

	public void ObtainExternalIPAddress() {
		inputField.text = this.networkManager.networkAddress;
		Text text = inputField.placeholder.GetComponent<Text>();
		if (text != null) {
			text.text = this.networkManager.networkAddress;
		}
		text = inputField.GetComponent<Text>();
		if (text != null) {
			text.text = this.networkManager.networkAddress;
		}
	}
}
