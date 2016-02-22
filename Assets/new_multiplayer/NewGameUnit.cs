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
		public bool isAttackCooldownEnabled;
		public bool isRecoveryEnabled;
		public int currentHealth;
		public int maxHealth;
		public int level;
		public float scalingFactor;
		public Vector3 mouseTargetPosition;
		public Vector3 oldMouseTargetPosition;
		public Vector3 enemyTargetPosition;
		public Vector3 oldEnemyTargetPosition;
		public GameObject targetUnit;
	}

	[System.Serializable]
	public struct NewChanges {
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public bool isCommanded;
		public bool isAttackCooldownEnabled;
		public bool isRecoveryEnabled;
		public int damage;
		public int newLevel;
		public Vector3 mousePosition;
		public Vector3 enemyPosition;
		public GameObject targetUnit;

		public NewChanges Clear() {
			this.isMerging = false;
			this.isSplitting = false;
			this.isSelected = false;
			this.isCommanded = false;
			this.damage = -1;
			this.newLevel = 1;
			this.mousePosition = Vector3.one * -9999;
			this.enemyPosition = this.mousePosition;
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

		private float recoveryCounter;
		private float attackCooldownCounter;

		public void Start() {
			this.properties = new UnitProperties();
			this.properties.currentHealth = 3;
			this.properties.maxHealth = 3;
			this.properties.mouseTargetPosition = -9999 * Vector3.one;
			this.properties.oldMouseTargetPosition = this.properties.mouseTargetPosition;
			this.properties.enemyTargetPosition = this.properties.mouseTargetPosition;
			this.properties.oldEnemyTargetPosition = this.properties.mouseTargetPosition;
			this.properties.isSelected = false;
			this.properties.scalingFactor = 1.4f;
			this.properties.level = 1;
			this.properties.isCommanded = false;
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
			this.recoveryCounter = 0f;
			this.attackCooldownCounter = 0f;
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

			HandleMovement();
			HandleSelectionRing();
			HandleAttacking();
			HandleRecovering();
			HandleStatus();
		}

		public void NewProperty(NewChanges changes) {
			UnitProperties pro = new UnitProperties();
			pro = this.properties;
			if (changes.damage > 0 && this.attackCooldownCounter <= 0f) {
				pro.currentHealth -= changes.damage;
				pro.isAttackCooldownEnabled = true;
				this.attackCooldownCounter = 1f;
			}
			if (pro.mouseTargetPosition != changes.mousePosition) {
				pro.oldMouseTargetPosition = pro.mouseTargetPosition;
				pro.mouseTargetPosition = changes.mousePosition;
			}
			if (pro.enemyTargetPosition != changes.enemyPosition) {
				pro.oldEnemyTargetPosition = pro.enemyTargetPosition;
				pro.enemyTargetPosition = changes.enemyPosition;
			}
			pro.isSelected = changes.isSelected;
			pro.isSplitting = changes.isSplitting;
			pro.isMerging = changes.isMerging;
			pro.level = changes.newLevel;
			pro.isCommanded = changes.isCommanded;
			if (changes.isRecoveryEnabled) {
				pro.isRecoveryEnabled = changes.isRecoveryEnabled;
				if (this.recoveryCounter <= 0f) {
					this.recoveryCounter = 1f;
				}
			}
			OnPropertiesChanged(pro);
		}

		public NewChanges CurrentProperty() {
			NewChanges changes = new NewChanges().Clear();
			changes.isSelected = this.properties.isSelected;
			changes.isMerging = this.properties.isMerging;
			changes.isSplitting = this.properties.isSplitting;
			changes.isCommanded = this.properties.isCommanded;
			changes.newLevel = this.properties.level;
			changes.mousePosition = this.properties.mouseTargetPosition;
			changes.enemyPosition = this.properties.enemyTargetPosition;
			changes.damage = 0;
			return changes;
		}

		public void OnPropertiesChanged(UnitProperties pro) {
			this.properties = pro;
		}

		public void CallCmdupdateProperty(NewChanges changes) {
			this.CmdUpdateProperty(this.gameObject, changes);
		}

		//*** ----------------------------   PRIVATE METHODS  -------------------------

		private void HandleMovement() {
			if (this.properties.isCommanded) {
				if (this.properties.mouseTargetPosition != this.properties.oldMouseTargetPosition) {
					CmdSetDestination(this.gameObject, this.properties.mouseTargetPosition);
				}
			}
			else {
				if (this.agent.ReachedDestination()) {
					if (this.properties.isCommanded) {
						NewChanges changes = this.CurrentProperty();
						changes.isCommanded = false;
						this.CmdUpdateProperty(this.gameObject, changes);
					}
					if (!this.properties.isCommanded) {
						CmdSetDestination(this.gameObject, this.properties.enemyTargetPosition);
					}
				}
			}
		}

		private void HandleSelectionRing() {
			if (this.properties.isSelected) {
				this.selectionRing.SetActive(true);
			}
			else {
				this.selectionRing.SetActive(false);
			}
		}

		private void HandleAttacking() {
			if (this.properties.targetUnit != null && this.attackCooldownCounter <= 0f) {
				CmdAttack(this.gameObject, this.properties.targetUnit, 1);
			}
			else if (this.attackCooldownCounter > 0f) {
				this.attackCooldownCounter -= Time.deltaTime;
			}
		}

		private void HandleRecovering() {
			if (this.properties.isRecoveryEnabled) {
				if (recoveryCounter > 0f) {
					recoveryCounter -= Time.deltaTime;
				}
				else {
					NewChanges changes = this.CurrentProperty();
					changes.isRecoveryEnabled = false;
					this.NewProperty(changes);
				}
			}
		}

		private void HandleStatus() {
			if (this.properties.currentHealth <= 0) {
				CmdDestroy(this.gameObject);
			}
		}

		//----------------------------  COMMANDS and CLIENTRPCS  ----------------------------

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

		[Command]
		public void CmdAttack(GameObject attacker, GameObject victim, int damage) {
			NewGameUnit victimUnit = victim.GetComponent<NewGameUnit>();
			if (victimUnit != null) {
				NewChanges changes = victimUnit.CurrentProperty();
				changes.damage = damage;
				changes.isRecoveryEnabled = true;
				victimUnit.NewProperty(changes);
			}
			NewGameUnit attackerUnit = attacker.GetComponent<NewGameUnit>();
			if (attackerUnit != null) {
				NewChanges changes = attackerUnit.CurrentProperty();
				changes.isAttackCooldownEnabled = true;
				attackerUnit.NewProperty(changes);
			}
		}

		//Do not remove. This is required.
		[Command]
		public void CmdSetDestination(GameObject obj, Vector3 pos) {
			RpcSetDestination(obj, pos);
		}

		[ClientRpc]
		public void RpcSetDestination(GameObject obj, Vector3 pos) {
			NewGameUnit unit = obj.GetComponent<NewGameUnit>();
			if (unit != null && unit.agent != null) {
				unit.agent.SetDestination(pos); 
			}
		}
	}
}
