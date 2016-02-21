using UnityEngine;
using System.Collections.Generic;

namespace MultiPlayer {
	public class NewAtkRange : MonoBehaviour {
		public List<NewGameUnit> detectedUnits = new List<NewGameUnit>();
		public SphereCollider attackRange;
		public Rigidbody colliderBody;
		public NewGameUnit parent;

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
				this.detectedUnits.Add(unit);
			}
		}

		public void OnTriggerExit(Collider other) {
			NewGameUnit unit = other.GetComponent<NewGameUnit>();
			if (unit != null && !unit.hasAuthority && !unit.gameObject.Equals(this.transform.parent.gameObject)) {
				this.detectedUnits.Remove(unit);
			}
		}

		public void FixedUpdate() {
			this.colliderBody.WakeUp();
			if (this.detectedUnits.Count > 0) {
				NewChanges changes = this.parent.CurrentProperty();
				changes.targetUnit = this.detectedUnits[0].gameObject;
				changes.position = this.detectedUnits[0].transform.position;
				this.parent.NewProperty(changes);
			}
		}
	}
}
