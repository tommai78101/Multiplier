using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Extension;

namespace MultiPlayer {
	[System.Serializable]
	public struct UnitProperties {
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public bool isCommanded;
		public int currentHealth;
		public int maxHealth;
		public int level;
		public float scalingFactor;
		public Vector3 targetPosition;
		public GameObject targetUnit;
	}

	[System.Serializable]
	public struct NewChanges {
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public bool isCommanded;
		public int damage;
		public int newLevel;
		public Vector3 position;
		public GameObject targetUnit;

		public NewChanges Clear() {
			this.isMerging = false;
			this.isSplitting = false;
			this.isSelected = false;
			this.isCommanded = false;
			this.damage = -1;
			this.newLevel = 1;
			this.position = Vector3.one * -9999;
			this.targetUnit = null;
			return this;
		}
	}


	[System.Serializable]
	public class NewGameUnit : NetworkBehaviour {
		[SyncVar(hook = "OnPropertiesChanged")]
		public UnitProperties properties;

		public delegate void UpdateProperties(NewChanges changes);
		public UpdateProperties updateProperties;
		public NavMeshAgent agent;

		public GameObject selectionRing;

		public void Start() {
			this.properties = new UnitProperties();
			this.properties.currentHealth = 3;
			this.properties.maxHealth = 3;
			this.properties.targetPosition = -9999 * Vector3.one;
			this.properties.isSelected = false;
			this.properties.scalingFactor = 1.4f;
			this.properties.level = 1;
			this.updateProperties += NewProperty;
			NewSelectionRing[] selectionRings = this.GetComponentsInChildren<NewSelectionRing>(true);
			foreach (NewSelectionRing ring in selectionRings) {
				if (ring != null) {
					this.selectionRing = ring.gameObject;
					break;
				}
				else {
					Debug.LogError("Cannot find mesh filter and/or mesh renderer for unit's selection ring.");
				}
			}
			this.agent = this.GetComponent<NavMeshAgent>();
			if (this.agent == null) {
				Debug.LogError("Cannot obtain nav mesh agent from game unit.");
			}
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
				NewChanges changes = new NewChanges().Clear();
				changes.damage = attackDamage;
				updateProperties(changes);
			}
			else {
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

			if (this.agent.ReachedDestination()) {
				if (this.properties.isCommanded) {
					Debug.Log("Game Unit is at destination.");
					NewChanges changes = this.CurrentProperty();
					changes.isCommanded = false;
					this.CmdUpdateProperty(this.gameObject, changes);
				}
			}
			else {
				Debug.Log("Agent is moving? + " + this.agent.ReachedDestination().ToString());
			}

			if (this.properties.isSelected) {
				this.selectionRing.SetActive(true);
			}
			else {
				this.selectionRing.SetActive(false);
			}
		}

		public void NewProperty(NewChanges changes) {
			UnitProperties pro = new UnitProperties();
			pro = this.properties;
			if (changes.damage > 0) {
				pro.currentHealth -= changes.damage;
			}
			if (changes.position != Vector3.one * -9999) {
				pro.targetPosition = changes.position;
			}
			pro.isSelected = changes.isSelected;
			pro.isSplitting = changes.isSplitting;
			pro.isMerging = changes.isMerging;
			pro.level = changes.newLevel;
			pro.isCommanded = changes.isCommanded;
			OnPropertiesChanged(pro);
		}

		public NewChanges CurrentProperty() {
			NewChanges changes = new NewChanges().Clear();
			changes.isSelected = this.properties.isSelected;
			changes.isMerging = this.properties.isMerging;
			changes.isSplitting = this.properties.isSplitting;
			changes.isCommanded = this.properties.isCommanded;
			changes.newLevel = this.properties.level;
			changes.position = this.properties.targetPosition;
			changes.damage = 0;
			return changes;
		}

		public void OnPropertiesChanged(UnitProperties pro) {
			this.properties = pro;
		}

		[Command]
		public void CmdDestroy(GameObject obj) {
			NetworkServer.Destroy(obj);
		}

		[Command]
		public void CmdUpdateProperty(GameObject obj, NewChanges changes) {
			NewGameUnit unit = obj.GetComponent<NewGameUnit>();
			if (unit != null) {
				unit.NewProperty(changes);
			}
		}
	}
}
