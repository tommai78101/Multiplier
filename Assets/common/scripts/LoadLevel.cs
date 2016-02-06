using UnityEngine;
using UnityEngine.Networking;
#if UNITY_5_2_3 || UNITY_5_2_4 || UNITY_5_2_4
#else
using UnityEngine.SceneManagement;
#endif
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
#if UNITY_5_2_3 || UNITY_5_2_4
			Application.LoadLevel(value);
#else
			SceneManager.LoadScene(value);
#endif
		}
	}
}
