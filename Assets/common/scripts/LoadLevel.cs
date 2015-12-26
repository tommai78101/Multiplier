using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Common {
	public class LoadLevel : MonoBehaviour {
		public void Load(string value) {
			GameObject networkManagerObj = GameObject.FindGameObjectWithTag("NetworkManager");
			if (networkManagerObj != null) {
				NetworkManager manager = networkManagerObj.GetComponent<NetworkManager>();
				if (manager != null) {
					manager = networkManagerObj.GetComponent<SingleHost>();
				}
				if (manager != null) {
					Debug.Log("Stopping host and client.");
					manager.StopHost();
				}
			}
			SceneManager.LoadScene(value);
		}
	}
}
