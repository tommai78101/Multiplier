using UnityEngine;
using System.Collections;

namespace SinglePlayer.UI {
	public class StopHost : MonoBehaviour {
		public SingleHost host;

		public void StopServerHost() {
			host.StopHost();
			GameObject.Destroy(host);
		}
	}
}
