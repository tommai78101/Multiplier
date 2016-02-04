using UnityEngine;
using System.Collections;

namespace SinglePlayer {
	public class GlobalManager : MonoBehaviour {
		public int playerMaxUnitCount;

		public void Start() {
			this.playerMaxUnitCount = 1;
		}
	}
}
