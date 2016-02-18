using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {
	public class NewAtkRange : NetworkBehaviour {
		public UnitsSyncList detectedUnits = new UnitsSyncList();
		public SphereCollider attackRange;
		public Rigidbody colliderBody;
		public NewGameUnit parent;

		public void Awake() {
			NetworkIdentity id = this.gameObject.AddComponent<NetworkIdentity>();
			id.localPlayerAuthority = true;
			ClientScene.RegisterPrefab(this.gameObject);
		}

		public void Start() {
			this.colliderBody = this.GetComponent<Rigidbody>();
			if (this.colliderBody == null) {
				Debug.LogError("Attack Range: Cannot detect Rigidbody.");
			}
			this.attackRange = this.GetComponent<SphereCollider>();
			if (this.attackRange == null) {
				Debug.LogError("Attack Range: Cannot assign sphere collider.");
			}
		}

		public void OnTriggerEnter(Collider other) {
			NewGameUnit unit = other.gameObject.GetComponent<NewGameUnit>();
			if (unit != null && !unit.hasAuthority && !unit.gameObject.Equals(this.transform.parent.gameObject)) {
				NewUnitStruct temp = new NewUnitStruct(unit.gameObject);
				if (!this.detectedUnits.Contains(temp)) {
					this.detectedUnits.Add(temp);
				}
			}
		}

		public void OnTriggerExit(Collider other) {
			NewGameUnit unit = other.GetComponent<NewGameUnit>();
			if (unit != null && !unit.hasAuthority && !unit.gameObject.Equals(this.transform.parent.gameObject)) {
				NewUnitStruct temp = new NewUnitStruct(unit.gameObject);
				if (this.detectedUnits.Contains(temp)) {
					this.detectedUnits.Remove(temp);
				}
			}
		}

		public void FixedUpdate() {
			this.colliderBody.WakeUp();
			if (this.detectedUnits.Count > 0) {
				NewChanges changes = this.parent.CurrentProperty();
				changes.targetUnit = this.detectedUnits[0].unit;
				this.parent.NewProperty(changes);
			}
		}
	}
}
