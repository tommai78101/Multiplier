using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {
	[System.Serializable]
	public struct UnitProperties {
		public int currentHealth;
		public int maxHealth;
		public Vector3 targetPosition;
	}

	public delegate void UpdateProperties(int damage, Vector3 position);

	public class NewGameUnit : NetworkBehaviour {
		[SyncVar(hook = "OnPropertiesChanged")]
		public UnitProperties properties;

		public event UpdateProperties updateProperties;

		public void Start() {
			Debug.Log("Setting up properties.");
			this.properties = new UnitProperties();
			this.properties.currentHealth = 3;
			this.properties.maxHealth = 3;
			this.properties.targetPosition = -9999 * Vector3.one;
			this.updateProperties += new UpdateProperties(NewProperty);
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
				updateProperties(attackDamage, Vector3.one * -9999);
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

			if (Input.GetKeyUp(KeyCode.L)) {
				Debug.Log("Damage time!");
				CmdTakeDamage(1);
			}

			if (Input.GetMouseButtonUp(0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit)) {
					Debug.Log("Moving time!");
					updateProperties(0, hit.point);
				}
			}

			if (this.properties.targetPosition != -9999 * Vector3.one) {
				NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
				agent.SetDestination(this.properties.targetPosition);
			}
		}

		public void NewProperty(int damage, Vector3 newPosition) {
			Debug.Log("Updating properties with new values.");
			UnitProperties pro = new UnitProperties();
			pro = this.properties;
			if (damage > 0) {
				pro.currentHealth -= damage;
			}
			if (newPosition != Vector3.one * -9999) {
				pro.targetPosition = newPosition;
			} 
			this.properties = pro;
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
	}
}
