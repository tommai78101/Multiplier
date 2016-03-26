using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Extension;
using Analytics;

namespace MultiPlayer {
	[System.Serializable]
	public struct UnitProperties {
		public bool isInitialized;
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public bool isCommanded;
		public bool isAttackCooldownEnabled;
		public bool isRecoveryEnabled;
		public int currentHealth;
		public int maxHealth;
		public int level;
		public int teamFactionID;
		public float scalingFactor;
		public float attackFactor;
		public float speedFactor;
		public float splitFactor;
		public float mergeFactor;
		public float attackCooldownFactor;
		public Color teamColor;
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
		public bool isInitialized;
		public bool isSplitting;
		public bool isMerging;
		public bool isSelected;
		public bool isCommanded;
		public bool isAttackCooldownEnabled;
		public bool isRecoveryEnabled;
		public int damage;
		public int heal;
		public int newLevel;
		public int teamFactionID;
		public float newMaxHealth;
		public float newCurrentHealth;
		public float newAttack;
		public float newSpeed;
		public float newSplit;
		public float newMerge;
		public float newAttackCooldown;
		public Vector3 mousePosition;
		public Vector3 enemySeenPosition;
		public Vector3 enemyHitPosition;
		public Color teamColor;
		public GameObject targetUnit;

		public NewChanges Clear() {
			this.isInitialized = false;
			this.isMerging = false;
			this.isSplitting = false;
			this.isSelected = false;
			this.isCommanded = false;
			this.isAttackCooldownEnabled = false;
			this.isRecoveryEnabled = false;
			this.damage = 0;
			this.heal = 0;
			this.newLevel = 1;
			this.newMaxHealth = 3;
			this.newCurrentHealth = 3;
			this.newAttack = 1f;
			this.newSpeed = 1f;
			this.newSplit = 1f;
			this.newMerge = 1f;
			this.newAttackCooldown = 1f;
			this.mousePosition = Vector3.one * -9999;
			this.teamColor = Color.gray;
			this.enemySeenPosition = this.mousePosition;
			this.enemyHitPosition = this.enemySeenPosition;
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
		public Camera minimapCamera;
		public Vector3 viewportPosition;

		[SerializeField]
		private float recoveryCounter;
		[SerializeField]
		private float attackCooldownCounter;
		[SerializeField]
		private float takeDamageCounter;
		[SerializeField]
		private bool isDead;

		public void Start() {
			if (!this.properties.isInitialized) {
				this.properties.isInitialized = true;
				this.properties.maxHealth = 3;
				this.properties.currentHealth = this.properties.maxHealth;
				this.properties.mouseTargetPosition = -9999 * Vector3.one;
				this.properties.oldMouseTargetPosition = this.properties.mouseTargetPosition;
				this.properties.enemyHitTargetPosition = this.properties.mouseTargetPosition;
				this.properties.oldEnemyHitTargetPosition = this.properties.enemyHitTargetPosition;
				this.properties.enemySeenTargetPosition = this.properties.mouseTargetPosition;
				this.properties.oldEnemySeenTargetPosition = this.properties.enemySeenTargetPosition;
				this.properties.scalingFactor = 1.4f;
				this.properties.level = 1;
				this.properties.isSelected = false;
				this.properties.isCommanded = false;
				this.properties.isAttackCooldownEnabled = false;
				this.properties.isRecoveryEnabled = false;
				this.properties.isMerging = false;
				this.properties.isSplitting = false;
				this.properties.targetUnit = null;
			}
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
			this.takeDamageCounter = 0f;
			this.isDead = false;

			//NOTE(Thompson): Changing the name here, so I can really get rid of (Clone).
			this.name = "NewGameUnit";
		}

		public void Update() {
			//NOTE(Thompson): Common update codes for authoritative and non-authoritative objects goes here.
			HandleNonAuthorityStatus();

			if (!this.hasAuthority) {
				return;
			}
			if (!this.properties.isInitialized) {
				//NOTE(Thompson): Rare chance the game unit is not initialized correctly.
				//Therefore the best way to fix this is to re-initialize the game unit again.
				//Not sure if this fixes that rare issue.
				this.Start();
				return;
			}

			if (HandleStatus()) {
				return;
			}
			HandleMovement();
			HandleSelectionRing();
			HandleAttacking();
			HandleRecovering();
		}

		public void NewProperty(NewChanges changes) {
			UnitProperties pro = new UnitProperties();
			pro = this.properties;
			pro.isInitialized = changes.isInitialized;
			pro.teamFactionID = changes.teamFactionID;
			if (changes.heal > 0) {
				pro.currentHealth += changes.heal;
			}
			if (changes.damage > 0) {
				pro.currentHealth -= changes.damage;
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
			pro.isRecoveryEnabled = changes.isRecoveryEnabled;
			pro.targetUnit = changes.targetUnit;

			//pro.currentHealth = (int) changes.newCurrentHealth;
			pro.maxHealth = (int) changes.newMaxHealth;
			pro.attackFactor = changes.newAttack;
			pro.speedFactor = changes.newSpeed;
			pro.splitFactor = changes.newSplit;
			pro.mergeFactor = changes.newMerge;
			pro.attackCooldownFactor = changes.newAttackCooldown;


			pro.teamColor = changes.teamColor;
			Renderer renderer = this.GetComponent<Renderer>();
			renderer.material.SetColor("_TeamColor", changes.teamColor);

			pro.isAttackCooldownEnabled = changes.isAttackCooldownEnabled;
			OnPropertiesChanged(pro);
		}

		public NewChanges CurrentProperty() {
			NewChanges changes = new NewChanges().Clear();
			changes.isInitialized = this.properties.isInitialized;
			changes.isSelected = this.properties.isSelected;
			changes.isMerging = this.properties.isMerging;
			changes.isSplitting = this.properties.isSplitting;
			changes.isCommanded = this.properties.isCommanded;
			changes.isAttackCooldownEnabled = this.properties.isAttackCooldownEnabled;
			changes.isRecoveryEnabled = this.properties.isRecoveryEnabled;
			changes.newLevel = this.properties.level;
			changes.newCurrentHealth = this.properties.currentHealth;
			changes.newMaxHealth = this.properties.maxHealth;
			changes.newAttack = this.properties.attackFactor;
			changes.newSpeed = this.properties.speedFactor;
			changes.newSplit = this.properties.splitFactor;
			changes.newMerge = this.properties.mergeFactor;
			changes.newAttackCooldown = this.properties.attackCooldownFactor;
			changes.mousePosition = this.properties.mouseTargetPosition;
			changes.enemySeenPosition = this.properties.enemySeenTargetPosition;
			changes.targetUnit = this.properties.targetUnit;
			changes.damage = 0;
			changes.heal = 0;
			changes.teamFactionID = this.properties.teamFactionID;
			changes.teamColor = this.properties.teamColor;
			return changes;
		}

		public void OnPropertiesChanged(UnitProperties pro) {
			this.properties = pro;
		}

		public void CallCmdupdateProperty(NewChanges changes) {
			this.CmdUpdateProperty(this.gameObject, changes);
		}

		public Color SetTeamColor(Color color) {
			NewChanges changes = this.CurrentProperty();
			changes.teamColor = color;
			this.NewProperty(changes);

			Renderer renderer = this.GetComponent<Renderer>();
			renderer.material.SetColor("_TeamColor", color);
			return color;
		}

		public Color GetTeamColor() {
			return this.properties.teamColor;
		}

		public void OnGUI() {
			if (this.minimapCamera != null) {
				GUIStyle style = new GUIStyle();
				style.normal.textColor = Color.black;
				style.alignment = TextAnchor.MiddleCenter;
				Vector3 healthPosition = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
				this.viewportPosition = Camera.main.ScreenToViewportPoint(new Vector3(healthPosition.x, healthPosition.y + 30f));
				if (!this.minimapCamera.rect.Contains(this.viewportPosition)) {
					Rect healthRect = new Rect(healthPosition.x - 50f, (Screen.height - healthPosition.y) - 45f, 100f, 25f);
					GUI.Label(healthRect, this.properties.currentHealth.ToString() + "/" + this.properties.maxHealth.ToString(), style);
				}
			}
			else {
				GameObject obj = GameObject.FindGameObjectWithTag("Minimap");
				if (obj != null) {
					if (obj.activeSelf || obj.activeInHierarchy) {
						this.minimapCamera = obj.GetComponent<Camera>();
						if (this.minimapCamera == null) {
							return;
						}
						if (!this.minimapCamera.isActiveAndEnabled || !this.minimapCamera.enabled) {
							this.minimapCamera.enabled = true;
						}
					}
				}
			}
		}

		//*** ----------------------------   PRIVATE METHODS  -------------------------

		private void HandleMovement() {
			if (this.properties.isCommanded) {
				if (this.properties.mouseTargetPosition != this.properties.oldMouseTargetPosition) {
					//Client side nav mesh agent
					CmdSetDestination(this.gameObject, this.properties.mouseTargetPosition);
				}
			}
			if (this.agent.ReachedDestination()) {
				if (this.properties.isCommanded) {
					NewChanges changes = this.CurrentProperty();
					changes.isCommanded = false;
					this.CmdUpdateProperty(this.gameObject, changes);
					return;
				}
			}
			if (!this.properties.isCommanded) {
				if (this.properties.targetUnit == null) {
					if (this.properties.mouseTargetPosition != this.properties.oldMouseTargetPosition) {
						CmdSetDestination(this.gameObject, this.properties.mouseTargetPosition);
					}
					else {
						CmdSetDestination(this.gameObject, this.properties.enemySeenTargetPosition);
					}
				}
				else {
					Vector3 agentDestination = this.properties.targetUnit.transform.position;
					agentDestination.y = 0f; //May or may not need this.
					CmdSetDestination(this.gameObject, agentDestination);
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
			if (this.properties.targetUnit != null && this.attackCooldownCounter <= 0 && !this.properties.isAttackCooldownEnabled) {
				CmdAttack(this.hasAuthority, this.gameObject, this.properties.targetUnit, 1);
				this.attackCooldownCounter = 1f;
				NewChanges changes = this.CurrentProperty();
				changes.isAttackCooldownEnabled = true;
				this.NewProperty(changes);

				GameMetricLogger.Increment(GameMetricOptions.Attacks);
			}
			else if (this.attackCooldownCounter > 0f) {
				NewChanges changes = this.CurrentProperty();
				changes.isAttackCooldownEnabled = true;
				this.NewProperty(changes);

				GameMetricLogger.Increment(GameMetricOptions.AttackTime);
			}
		}

		private void HandleRecovering() {
			if (this.properties.isRecoveryEnabled && this.recoveryCounter <= 0) {
				CmdRecover(this.gameObject, 1);
				this.recoveryCounter = 1f;
				if (this.properties.currentHealth >= this.properties.maxHealth) {
					this.properties.currentHealth = this.properties.maxHealth;
					NewChanges changes = this.CurrentProperty();
					changes.isRecoveryEnabled = false;
					this.NewProperty(changes);
				}
			}
		}

		private bool HandleStatus() {
			//Returns TRUE if the game unit is destroyed, preventing all other actions from executing.
			//Returns FALSE if game unit is alive.

			if (this.properties.currentHealth <= 0 && !this.isDead) {
				this.isDead = true;
				GameMetricLogger.Increment(GameMetricOptions.Death);
				CmdDestroy(this.gameObject);
				return true;
			}
			if (this.properties.isAttackCooldownEnabled) {
				if (this.attackCooldownCounter > 0) {
					this.attackCooldownCounter -= Time.deltaTime;

					GameMetricLogger.Increment(GameMetricOptions.BattleEngagementTime);
				}
				else {
					NewChanges changes = this.CurrentProperty();
					changes.isAttackCooldownEnabled = false;
					this.NewProperty(changes);
				}
			}
			if (this.properties.isRecoveryEnabled) {
				if (this.recoveryCounter > 0f) {
					this.recoveryCounter -= Time.deltaTime / 5f;
				}
				else {
					NewChanges changes = this.CurrentProperty();
					changes.isRecoveryEnabled = false;
					this.NewProperty(changes);
				}
			}
			return false;
		}

		private void HandleNonAuthorityStatus() {
			if (this.takeDamageCounter > 0) {
				this.takeDamageCounter -= Time.deltaTime * 1.4f;
				Renderer renderer = this.GetComponent<Renderer>();
				if (renderer != null) {
					renderer.material.color = Color.Lerp(Color.red, Color.white, 1f - this.takeDamageCounter);
				}
			}
		}

		private void LogKill() {
			GameMetricLogger.Increment(GameMetricOptions.Kills);
		}

		//----------------------------  COMMANDS and CLIENTRPCS  ----------------------------

		[Command]
		public void CmdTakeDamageColor() {
		}

		[ClientRpc]
		public void RpcTakeDamageColor(NetworkInstanceId victimNetID) {
			GameObject obj = ClientScene.FindLocalObject(victimNetID);
			if (obj != null) {
				NewGameUnit victimUnit = obj.GetComponent<NewGameUnit>();
				if (victimUnit != null) {
					//NOTE(Thompson): This makes the victim (this game unit) show a visual indicator
					//of it being attacked and taking damage.
					victimUnit.takeDamageCounter = 1f;
				}
			}
		}

		[Command]
		public void CmdRecover(GameObject recoveringObject, int healValue) {
			if (recoveringObject != null) {
				NewGameUnit unit = recoveringObject.GetComponent<NewGameUnit>();
				if (unit != null && NetworkServer.FindLocalObject(unit.netId)) {
					if (unit.properties.currentHealth < unit.properties.maxHealth && unit.properties.isRecoveryEnabled) {
						NewChanges changes = unit.CurrentProperty();
						changes.heal = healValue;
						unit.NewProperty(changes);
					}
				}
			}
		}

		[Command]
		public void CmdDestroy(GameObject targetUnit) {
			if (targetUnit != null) {
				NewGameUnit unit = targetUnit.GetComponent<NewGameUnit>();
				if (unit != null && NetworkServer.FindLocalObject(unit.netId)) {
					if (targetUnit != null) {
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
			}
		}

		[Command]
		public void CmdUpdateProperty(GameObject obj, NewChanges changes) {
			if (obj != null) {
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				if (unit != null) {
					unit.NewProperty(changes);
				}
			}
		}

		[Command]
		public void CmdAttack(bool hasAuthority, GameObject attacker, GameObject victim, int damage) {
			if (victim != null && attacker != null) {
				NewGameUnit victimUnit = victim.GetComponent<NewGameUnit>();
				NewGameUnit attackerUnit = attacker.GetComponent<NewGameUnit>();
				if (!(NetworkServer.FindLocalObject(victimUnit.netId) || NetworkServer.FindLocalObject(attackerUnit.netId))) {
					return;
				}
				if (victimUnit != null && attackerUnit != null && !attackerUnit.properties.isAttackCooldownEnabled && NetworkServer.FindLocalObject(victimUnit.netId) != null && NetworkServer.FindLocalObject(attackerUnit.netId) != null) {
					NewChanges changes = victimUnit.CurrentProperty();
					changes.damage = damage;
					changes.isRecoveryEnabled = true;
					victimUnit.NewProperty(changes);
					RpcTakeDamageColor(victimUnit.netId);

					if (victimUnit.properties.currentHealth == 0 && attackerUnit.hasAuthority == hasAuthority) {
						//TODO(Thompson): See if removing the authority check helps fixing the kill counter being 
						//only the server side can take kill counts, while client side sometimes not taking kill counts.
						attackerUnit.LogKill();
					}
				}
			}
		}


		//Do not remove. This is required.
		[Command]
		public void CmdSetDestination(GameObject obj, Vector3 pos) {
			if (obj != null) {
				NewGameUnit unit = obj.GetComponent<NewGameUnit>();
				if (unit != null) {
					RpcSetDestination(obj, pos);
				}
			}
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
