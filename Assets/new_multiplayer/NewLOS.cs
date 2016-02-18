using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {
	public class NewLOS : NetworkBehaviour {
		public UnitsSyncList detectedUnits = new UnitsSyncList();
		public SphereCollider lineOfSight;
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
				Debug.LogError("Line Of Sight: Cannot detect Rigidbody.");
			}
			this.lineOfSight = this.GetComponent<SphereCollider>();
			if (this.lineOfSight == null) {
				Debug.LogError("Line Of Sight: Cannot assign sphere collider.");
			}
		}

		[Command]
		public void CmdAddUnit(GameObject obj) {
			NewUnitStruct temp = new NewUnitStruct(obj);
			if (!this.detectedUnits.Contains(temp)) {
				this.detectedUnits.Add(temp);
			}
		}

		public void OnTriggerEnter(Collider other) {
			NewGameUnit unit = other.gameObject.GetComponent<NewGameUnit>();
			if (unit != null && !unit.hasAuthority && !unit.gameObject.Equals(this.transform.parent.gameObject)) {
				CmdAddUnit(unit.gameObject);
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
				changes.position = this.detectedUnits[0].unit.transform.position;
				this.parent.NewProperty(changes);
			}
		}
	}
}
