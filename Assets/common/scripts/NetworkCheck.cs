using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace SinglePlayer {
	public class NetworkCheck : MonoBehaviour {
		public SingleHost singleHost;

		public void TurnOffNetwork() {
			SingleHost host = this.GetComponent<SingleHost>();
			if (host != null && host.isNetworkActive) {
				Debug.Log("Turing off network.");
				host.StopHost();
				host.StopClient();
			}
		}
	}
}
