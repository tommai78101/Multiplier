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
		public Vector3 enemySeenTargetPosition;
		public Vector3 oldEnemySeenTargetPosition;
		public Vector3 enemyHitTargetPosition;
		public Vector3 oldEnemyHitTargetPosition;
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
		public Vector3 enemySeenPosition;
		public Vector3 enemyHitPosition;
		public GameObject targetUnit;

		public NewChanges Clear() {
			this.isMerging = false;
			this.isSplitting = false;
			this.isSelected = false;
			this.isCommanded = false;
			this.damage = -1;
			this.newLevel = 1;
			this.mousePosition = Vector3.one * -9999;
			this.enemySeenPosition = this.mousePosition;
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

		[SerializeField]
		private float recoveryCounter;
		[SerializeField]
		private float attackCooldownCounter;

		public void Start() {
			this.properties = new UnitProperties();
			this.properties.currentHealth = 3;
			this.properties.maxHealth = 3;
			this.properties.mouseTargetPosition = -9999 * Vector3.one;
			this.properties.oldMouseTargetPosition = this.properties.mouseTargetPosition;
			this.properties.enemySeenTargetPosition = this.properties.mouseTargetPosition;
			this.properties.oldEnemySeenTargetPosition = this.properties.mouseTargetPosition;
			this.properties.isSelected = false;
			this.properties.scalingFactor = 1.4f;
			this.properties.level = 1;
			this.properties.isCommanded = false;
			this.properties.isAttackCooldownEnabled = false;
			this.properties.isRecoveryEnabled = false;
			this.properties.targetUnit = null;
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
			if (pro.enemySeenTargetPosition != changes.enemySeenPosition) {
				pro.oldEnemySeenTargetPosition = pro.enemySeenTargetPosition;
				pro.enemySeenTargetPosition = changes.enemySeenPosition;
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
			pro.targetUnit = changes.targetUnit;
			OnPropertiesChanged(pro);
		}

		public NewChanges CurrentProperty() {
			NewChanges changes = new NewChanges().Clear();
			changes.isSelected = this.properties.isSelected;
			changes.isMerging = this.properties.isMerging;
			changes.isSplitting = this.properties.isSplitting;
			changes.isCommanded = this.properties.isCommanded;
			changes.isAttackCooldownEnabled = this.properties.isAttackCooldownEnabled;
			changes.isRecoveryEnabled = this.properties.isRecoveryEnabled;
			changes.newLevel = this.properties.level;
			changes.mousePosition = this.properties.mouseTargetPosition;
			changes.enemySeenPosition = this.properties.enemySeenTargetPosition;
			changes.targetUnit = this.properties.targetUnit;
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
				if (this.agent.ReachedDestination() && Vector3.Distance(this.properties.mouseTargetPosition, this.transform.position) < 3f) {
					NewChanges changes = this.CurrentProperty();
					changes.isCommanded = false;
					this.CmdUpdateProperty(this.gameObject, changes);
					return;
				}
				if (this.properties.mouseTargetPosition != this.properties.oldMouseTargetPosition) {
					//Client side nav mesh agent
					CmdSetDestination(this.gameObject, this.properties.mouseTargetPosition);
					return;
				}
			}
			if (this.agent.ReachedDestination()) {
				if (this.properties.isCommanded) {
					NewChanges changes = this.CurrentProperty();
					changes.isCommanded = false;
					this.CmdUpdateProperty(this.gameObject, changes);
				}
			}
			if (!this.properties.isCommanded) {
				if (this.properties.targetUnit == null) {
					CmdSetDestination(this.gameObject, this.properties.enemySeenTargetPosition);
				}
				else {
					CmdSetDestination(this.gameObject, this.properties.enemyHitTargetPosition);
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
				CmdDestroy(this.properties.targetUnit);
			}
		}

		//----------------------------  COMMANDS and CLIENTRPCS  ----------------------------

		[Command]
		public void CmdDestroy(GameObject targetUnit) {
			if (targetUnit != null) {
				NewGameUnit unit = targetUnit.GetComponent<NewGameUnit>();
				NewChanges changes = unit.CurrentProperty();
				if (changes.targetUnit != null && changes.targetUnit.Equals(this.gameObject)) {
					changes.targetUnit = null;
					unit.NewProperty(changes);
				}
			}
			NewLOS los = this.GetComponentInChildren<NewLOS>();
			if (los != null) {
				los.parent = null;
			}
			NewAtkRange range = this.GetComponentInChildren<NewAtkRange>();
			if (range != null) {
				range.parent = null;
			}
			NetworkServer.Destroy(this.gameObject);
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
			if (victim != null && attacker != null) {
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
