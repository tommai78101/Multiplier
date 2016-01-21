using UnityEngine;
using System.Collections;

namespace SinglePlayer {
	public class GlobalManager : MonoBehaviour {
		public int playerMaxUnitCount;
		public int aiMaxUnitCount;

		public void Start() {
			this.playerMaxUnitCount = 1;
			this.aiMaxUnitCount = 1;
		}
	}
}
