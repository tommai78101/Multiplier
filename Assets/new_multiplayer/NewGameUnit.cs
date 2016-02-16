using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {
	[System.Serializable]
	public struct UnitProperties {
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public int currentHealth;
		public int maxHealth;
		public Vector3 targetPosition;
	}

	[System.Serializable]
	public struct NewChanges {
		public int damage;
		public Vector3 position;
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;

		public NewChanges Clear() {
			this.damage = -1;
			this.position = Vector3.one * -9999;
			this.isMerging = false;
			this.isSplitting = false;
			this.isSelected = false;
			return this;
		}
	}


	[System.Serializable]
	public class NewGameUnit : NetworkBehaviour {
		[SyncVar(hook = "OnPropertiesChanged")]
		public UnitProperties properties;

		public delegate void UpdateProperties(NewChanges changes);
		public UpdateProperties updateProperties;
		//public event UpdateProperties updateProperties;


		public void Start() {
			Debug.Log("Setting up properties.");
			this.properties = new UnitProperties();
			this.properties.currentHealth = 3;
			this.properties.maxHealth = 3;
			this.properties.targetPosition = -9999 * Vector3.one;
			this.updateProperties += NewProperty;
		}

		[Command]
		public void CmdTakeDamage(int attackDamage) {
			RpcTakeDamage(attackDamage);
		}

		[ClientRpc]
		public void RpcTakeDamage(int attackDamage) {
			if (!this.hasAuthority) {
				return;
			}
			if (this.properties.currentHealth > 1) {
				Debug.Log("Unit is taking damage.");
				NewChanges changes = new NewChanges().Clear();
				changes.damage = attackDamage;
				updateProperties(changes);
			}
			else {
				Debug.Log("Destroying myself.");
				CmdDestroy(this.gameObject);
			}
		}

		public void Update() {
			if (!this.hasAuthority) {
				return;
			}

			//if (Input.GetKeyUp(KeyCode.L)) {
			//	Debug.Log("Damage time!");
			//	CmdTakeDamage(1);
			//}

			//if (Input.GetMouseButtonUp(0)) {
			//	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//	RaycastHit hit;
			//	if (Physics.Raycast(ray, out hit)) {
			//		Debug.Log("Moving time!");
			//		NewChanges changes = new NewChanges().Clear();
			//		changes.position = hit.point;
			//		updateProperties(changes);
			//	}
			//}

			if (this.properties.targetPosition != -9999 * Vector3.one) {
				CmdSetDestination(this.properties.targetPosition);
			}
		}

		public void NewProperty(NewChanges changes) {
			Debug.Log("Updating properties with new values.");
			UnitProperties pro = new UnitProperties();
			pro = this.properties;
			if (changes.damage > 0) {
				pro.currentHealth -= changes.damage;
			}
			if (changes.position != Vector3.one * -9999) {
				pro.targetPosition = changes.position;
			}
			pro.isSelected = changes.isSelected;
			OnPropertiesChanged(pro);
		}

		public void OnPropertiesChanged(UnitProperties pro) {
			Debug.Log("Unit properties have changed.");
			this.properties = pro;
		}

		[Command]
		public void CmdDestroy(GameObject obj) {
			Debug.Log("Destroying object.");
			NetworkServer.Destroy(obj);
		}

		[Command]
		public void CmdSetDestination(Vector3 target) {
			RpcSetDestination(target);
		}

		[ClientRpc]
		public void RpcSetDestination(Vector3 target) {
			NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
			agent.SetDestination(target);
		}
	}
}
