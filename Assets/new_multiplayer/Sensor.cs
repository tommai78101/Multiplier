using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {
	public class Sensor : NetworkBehaviour {
		[SyncVar(hook = "OnTarget")]
		public NewGameUnit targetUnit;

		public SphereCollider lineOfSight;
		public SphereCollider attackRange;

		public void Start() {
			this.targetUnit = null;
			if ((this.lineOfSight == null) || (this.attackRange == null)) {
				Debug.LogError("Unassigned sphere colliders. Please check either one or all of them.");
			}
		}

		public void NewTarget(NewGameUnit unit) {
			this.OnTarget(unit);
		}

		private void OnTarget(NewGameUnit unit) {
			this.targetUnit = unit;
		}
	}
}
