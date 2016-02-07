using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Extension;
using Common;
using SinglePlayer;

namespace MultiPlayer {
	[System.Serializable]
	public class GameUnit : BaseUnit {
		//Properties of a Game Unit
		[SyncVar]
		public bool isSelected;
		[SyncVar]
		public bool isDirected;
		[SyncVar]
		public bool isSplitting;
		[SyncVar]
		public bool isMerging;
		[SyncVar]
		public int currentHealth;
		[Range(3, 100)]
		[SyncVar]
		public int maxHealth;
		[Range(1f, 100f)]
		[SyncVar]
		public float attackPower;
		[Range(0.001f, 10f)]
		[SyncVar]
		public float attackCooldown;
		[SyncVar]
		public float speed;
		//[Range(0.001f, 10f)]
		//[SyncVar]
		//public float recoverCooldown;
		[SyncVar]
		public int level;
		[SyncVar]
		public int previousLevel;
		[SyncVar]
		public int teamColorValue;

		public UnitAttributes attributes;
		public float attackCooldownCounter;
		public float recoverCounter;
		public Color initialColor;
		public Color takeDamageColor;
		public BaseUnit targetAIEnemy;
		public GameObject selectionRing;
		public EnumTeam teamFaction;

		public static bool once = false;

		//This variable keeps track of any changes made for the NavMeshAgent's destination Vector3.
		//Doesn't even need to use [SyncVar]. Nothing is needed for tracking this on the server at all. 
		//Just let the clients (local and remote) handle the pathfinding calculations and not pass updated current transform position
		//through the network. It's not pretty when you do this.
		public Vector3 oldTargetPosition;
		public Vector3 oldEnemyTargetPosition;

		public override void OnStartClient() {
			base.OnStartAuthority();

			//Initialization code for local player (local client on the host, and remote clients).
			this.oldTargetPosition = Vector3.one * -9999f;
			this.oldEnemyTargetPosition = Vector3.one * -9999f;
			this.targetEnemy = null;
			this.targetAIEnemy = null;
			this.isSelected = false;
			this.isDirected = false;
			this.currentHealth = this.maxHealth;
			this.recoverCounter = 1f;
			//this.recoverCooldown = this.attackCooldown + 3.5f;
			this.attackCooldownCounter = this.attackCooldown;
			this.teamFaction = EnumTeam.Player;
			if (this.attackPower <= 1f) {
				this.attackPower = 1f;
			}
			this.level = 1;
			this.previousLevel = 1;

			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				this.initialColor = renderer.material.color;
			}
		}

		public void Start() {
			this.SetTeamColor(this.teamColorValue);
		}

		public void Update() {
			//Because the game is now spawning objects from the player-owned objects (spawning from NetworkManager-spawned objects), don't check for 
			//isLocalPlayer, but instead check to see if the clients have authority over the non-player owned objects spawned from the NetworkManager-spawned objects.
			//Wordy, I know...
			if (!this.hasAuthority) {
				return;
			}

			if (this.isSplitting || this.isMerging) {
				return;
			}


			//Simple, "quick," MOBA-style controls. Hence, the class name.
			if (this.isSelected) {
				this.selectionRing.SetActive(true);
				Vector3 screenPoint = Camera.main.ScreenToViewportPoint(Input.mousePosition);
				if (!MinimapStuffs.Instance.minimapCamera.rect.Contains(screenPoint)) {
					if (Input.GetMouseButtonDown(1)) {
						CastRay(false, Input.mousePosition, null);
					}
				}
			}
			else {
				this.selectionRing.SetActive(false);
			}

			//Obtaining the nav mesh agent here for future uses.
			NavMeshAgent agent = this.GetComponent<NavMeshAgent>();

			//Non-directed, self-defense
			LineOfSight sight = this.GetComponentInChildren<LineOfSight>();
			AttackArea area = this.GetComponentInChildren<AttackArea>();
			if (!this.isDirected || agent.remainingDistance < 0.5f) {
				//Line of Sight. Detects if there are nearby enemy game units, and if so, follow them to engage in battle.
				if (sight != null && area != null) {
					//There are 4 cases when detecting an enemy in both areas, line of sight and attack range. I had to consider each of the cases 
					//in order to ease the Console error gods...
					// (12/5/2015) Now there are 8 cases in total. Consider AI players.
					GameUnit sightGameUnit = null;
					if (sight.enemiesInRange.Count > 0) {
						foreach (GameUnit temp in sight.enemiesInRange) {
							if (temp != null) {
								sightGameUnit = temp;
								break;
							}
						}
					}
					GameUnit attackGameUnit = null;
					if (sight.enemiesInRange.Count > 0) {
						foreach (GameUnit temp in sight.enemiesInRange) {
							if (temp != null) {
								attackGameUnit = temp;
								break;
							}
						}
					}
					AIUnit sightAiUnit = null;
					if (sight.otherEnemies.Count > 0) {
						foreach (AIUnit temp in sight.otherEnemies) {
							if (temp != null) {
								sightAiUnit = temp;
								break;
							}
						}
					}
					AIUnit attackAiUnit = null;
					if (sight.otherEnemies.Count > 0) {
						foreach (AIUnit temp in sight.otherEnemies) {
							if (temp != null) {
								attackAiUnit = temp;
								break;
							}
						}
					}

					if (sightGameUnit != null && attackGameUnit != null) {
						SetTargetEnemy(this.gameObject, sightGameUnit.gameObject, attackGameUnit.gameObject);
                        CmdSetTargetEnemy(this.gameObject, sightGameUnit.gameObject, attackGameUnit.gameObject);
					}
					else if (sightGameUnit != null && attackGameUnit == null) {
						SetTargetEnemy(this.gameObject, sightGameUnit.gameObject, sightGameUnit.gameObject);
                        CmdSetTargetEnemy(this.gameObject, sightGameUnit.gameObject, sightGameUnit.gameObject);
					}
					else if (sightGameUnit == null && attackGameUnit != null) {
						SetTargetEnemy(this.gameObject, attackGameUnit.gameObject, attackGameUnit.gameObject);
						CmdSetTargetEnemy(this.gameObject, attackGameUnit.gameObject, attackGameUnit.gameObject);
					}
					else {
						SetTargetEnemy(this.gameObject, this.gameObject, this.gameObject);
						CmdSetTargetEnemy(this.gameObject, this.gameObject, this.gameObject);
					}

					if (sightAiUnit != null && attackAiUnit != null) {
						SetTargetAIEnemy(this.gameObject, sightAiUnit.gameObject, attackAiUnit.gameObject);
					}
					else if (sightAiUnit != null && attackAiUnit == null) {
						SetTargetAIEnemy(this.gameObject, sightAiUnit.gameObject, sightAiUnit.gameObject);
					}
					else if (sightAiUnit == null && attackAiUnit != null) {
						SetTargetAIEnemy(this.gameObject, attackAiUnit.gameObject, attackAiUnit.gameObject);
					}
					else {
						SetTargetAIEnemy(this.gameObject, this.gameObject, this.gameObject);
					}
				}
			}

			//Start attacking.
			Attack();
			//Updating status.
			UpdateStatus();

			//Keeping track of whether the game unit is carrying out a player's command, or is carrying out self-defense.
			if (agent != null && agent.ReachedDestination()) {
				this.isDirected = false;
			}
		}

		public void FixedUpdate() {
			if (this.level == 1) {
				if (this.currentHealth != 3 || this.maxHealth != 3 || this.currentHealth != 3 || this.maxHealth != 3) {
					this.SetDirtyBit(int.MaxValue);
				}
			}
		}

		//Sets the team color.
		public void SetTeamColor(int colorValue) {
			this.teamColorValue = colorValue;

			Renderer playerRenderer = this.GetComponent<Renderer>();
			if (playerRenderer != null) {
				Material customMaterial = playerRenderer.material;
				switch (colorValue) {
					default:
						customMaterial.SetColor("_TeamColor", Color.gray);
						break;
					case 0:
						customMaterial.SetColor("_TeamColor", Color.yellow);
						break;
					case 1:
						customMaterial.SetColor("_TeamColor", Color.blue);
						break;
					case 2:
						customMaterial.SetColor("_TeamColor", Color.green);
						break;
				}
			}
		}

		//Tells the unit to at least move to an enemy opponent. 
		public void MoveToTarget(GameObject obj) {
			//LineOfSight sight = this.GetComponentInChildren<LineOfSight>();
			//AttackArea area = this.GetComponentInChildren<AttackArea>();
			//bool checkEnemyNearby = false;
			//if (sight != null && area != null) {
			//	if (sight.enemiesInRange.Count > 0 || area.enemiesInAttackRange.Count > 0) {
			//		checkEnemyNearby = true;
			//	}
			//}
			NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (agent != null && unit != null) {
				Renderer unitRenderer = unit.targetEnemy.GetComponent<Renderer>();
				if (unit.targetEnemy != null && unitRenderer.enabled) {  //&& checkEnemyNearby
					agent.stoppingDistance = 0.5f;
					agent.SetDestination(unit.targetEnemy.transform.position);
				}
				else {
					agent.stoppingDistance = 0f;
					agent.SetDestination(unit.oldTargetPosition);
				}
			}
		}

		public void MoveToAITarget(GameObject attacker) {
			NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
			GameUnit unit = attacker.GetComponent<GameUnit>();
			if (agent != null) {
				if (unit.targetAIEnemy != null) {
					agent.stoppingDistance = 0.5f;
					agent.SetDestination(unit.targetAIEnemy.transform.position);
				}
				else {
					agent.stoppingDistance = 0f;
					agent.SetDestination(unit.oldTargetPosition);
				}
			}
		}

		public void Attack() {
			if (this.targetEnemy != null) {
				//Attack Reach. If a nearby enemy game unit is within attack range, engage and attack.
				AttackArea area = this.GetComponentInChildren<AttackArea>();
				if (area != null) {
					if (area.enemiesInAttackRange.Contains(this.targetEnemy)) {
						if (this.attackCooldownCounter <= 0f) {
							CmdAttack(this.targetEnemy.gameObject, this.attributes.attackCooldownPrefabList[this.level]);
						}
					}
					else if (area.enemiesInAttackRange.Count > 0) {
						this.targetEnemy = area.enemiesInAttackRange[0];
					}
				}
			}
			else if (this.targetAIEnemy != null) {
				AttackArea area = this.GetComponentInChildren<AttackArea>();
				if (area != null) {
					if (area.otherEnemies.Contains(this.targetAIEnemy)) {
						if (this.attackCooldownCounter <= 0f) {
							if (this.attributes.attackCooldownPrefabList.Count > 0) {
								AttackAI(this.targetAIEnemy.gameObject, this.attributes.attackCooldownPrefabList[this.level]);
							}
							else {
								AttackAI(this.targetAIEnemy.gameObject, 1f);
							}
						}
					}
					else if (area.otherEnemies.Count > 0) {
						this.targetAIEnemy = area.otherEnemies[0];
					}
				}
			}
		}

		public void UpdateStatus() {
			if (this.attackCooldownCounter > 0) {
				this.attackCooldownCounter -= Time.deltaTime;
			}

			if (this.recoverCounter < 1f) {
				this.recoverCounter += Time.deltaTime;
			}

			if (this.currentHealth <= 0) {
				CmdUnitDestroy(this.gameObject);
			}


			//TODO: Work on this logic. Make sure updates are handled locally, and only significant data is transferred
			//across the network.
			Renderer renderer = this.GetComponent<Renderer>();
			Color color = this.initialColor;
			if (renderer != null) {
				color = Color.Lerp(this.takeDamageColor, this.initialColor, this.recoverCounter);
				renderer.material.color = color;
			}

			if (this.attackCooldownCounter > 0f) {
				this.attackCooldownCounter -= Time.deltaTime;
			}

			bool targetEnemyIsGone = false;
			if (this.targetEnemy != null) {
				Renderer targetRenderer = this.targetEnemy.GetComponent<Renderer>();
				if (!targetRenderer.enabled) {
					targetEnemyIsGone = true;
				}
			}
			else {
				targetEnemyIsGone = true;
			}

			//This is used for syncing up with the non-authoritative game unit. It is used with [SyncVar].
			CmdUpdateStatus(targetEnemyIsGone, color);
		}

		public void OnPlayerDisconnected(NetworkPlayer player) {
			//Destroying this client's game object on the server when the client has disconnected. This game object, the object with Quick
			//script attached.
			CmdDestroy();
		}

		public void CastRay(bool isMinimap, Vector3 mousePosition, Camera minimapCamera) {
			if (isMinimap) {
				Ray ray = minimapCamera.ViewportPointToRay(mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
				foreach (RaycastHit hit in hits) {
					if (hit.collider.gameObject.tag.Equals("Floor")) {
						CmdSetTarget(this.gameObject, hit.point);
						break;
					}
				}
			}
			else {
				Ray ray = Camera.main.ScreenPointToRay(mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray);
				foreach (RaycastHit hit in hits) {
					if (hit.collider.gameObject.tag.Equals("Floor")) {
						//Call on the client->server method to start the action.
						CmdSetTarget(this.gameObject, hit.point);
						break;
					}
				}
			}
		}

		[Command]
		public void CmdTakeDamage(GameObject playerUnitObject, int newHealth) {
			GameUnit playerUnit = playerUnitObject.GetComponent<GameUnit>();
			if (playerUnit != null) {
				//ONLY USED FOR SINGLE PLAYER.
				if (!playerUnit.hasAuthority) {
					return;
				}
				//CmdHealth(this.gameObject, this.currentHealth - Mathf.FloorToInt(attacker.attackPower));
				playerUnit.currentHealth = newHealth;
				playerUnit.recoverCounter = 0f;
			}
		}

		public void ToggleVisibility() {
			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				renderer.enabled = !renderer.enabled;
			}
		}

		public void EnableVisibility() {
			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				renderer.enabled = true;
			}
		}

		public void DisableVisibility() {
			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				renderer.enabled = false;
			}
		}

		//[Command]
		//public void CmdHealth(GameObject victim, int newHealth) {
		//	GameUnit victimUnit = victim.GetComponent<GameUnit>();
		//	victimUnit.currentHealth = newHealth;
		//	//RpcHealth(victim, newHealth);
		//}

		[Command]
		public void CmdAttack(GameObject victim, float attackCounter) {
			//RpcAttack(victim);
			if (victim == null) {
				return;
			}
			GameUnit victimUnit = victim.GetComponent<GameUnit>();
			if (victimUnit != null) {
				victimUnit.currentHealth -= Mathf.FloorToInt(this.attackPower);
				this.attackCooldownCounter = attackCounter;
			}
		}

		public void AttackAI(GameObject victim, float attackCounter) {
			if (victim == null) {
				return;
			}
			AIUnit victimUnit = victim.GetComponent<AIUnit>();
			if (victimUnit != null) {
				victimUnit.TakeDamage(this.attackPower);
				this.attackCooldownCounter = attackCounter;
			}
		}

		[Command]
		public void CmdSetTarget(GameObject obj, Vector3 target) {
			//Command call to tell the server to run the following code.
			RpcSetTarget(obj, target);
		}

		//Destroy [Command] and [ClientRpc] code definition.
		//It seems like all future code design patterns must use [Command] and [ClientRpc] / [ClientCallback] combo to actually get
		//something to work across the network. Keeping this in mind.
		[Command]
		public void CmdDestroy() {
			RpcDestroy();
			NetworkServer.Destroy(this.gameObject);
		}

		[Command]
		public void CmdUnitDestroy(GameObject obj) {
			RpcUnitDestroy(this.gameObject);
			//NetworkServer.Destroy(this.gameObject);
		}

		[Command]
		public void CmdUpdateStatus(bool targetEnemyIsGone, Color recoveryColor) {
			if (this.currentHealth <= 0) {
				if (this.gameObject != null) {
					RpcUnitDestroy(this.gameObject);
				}
			}
			else {
				RpcUpdateStatus(targetEnemyIsGone, recoveryColor);
			}
		}

		private void SetTargetEnemy(GameObject attacker, GameObject enemy, GameObject victim) {
			if (attacker != null && (enemy != null || victim != null)) {
				GameUnit unit = attacker.GetComponent<GameUnit>();
				if (unit != null) {
					if (victim != null && attacker.Equals(enemy) && attacker.Equals(victim)) {
						unit.targetEnemy = null;
					}
					else {
						if (victim != null) {
							unit.targetEnemy = victim.GetComponent<GameUnit>();
							MoveToTarget(attacker);
						}
						else if (enemy != null) {
							unit.targetEnemy = enemy.GetComponent<GameUnit>();
							MoveToTarget(attacker);
						}
						else {
							unit.targetEnemy = null;
						}
					}
				}
			}
		}

		[Command]
		public void CmdSetTargetEnemy(GameObject obj, GameObject enemy, GameObject attackee) {
			RpcSetTargetEnemy(obj, enemy, attackee);
		}

		[ClientRpc]
		public void RpcSetTargetEnemy(GameObject attacker, GameObject enemyInLineOfSight, GameObject victimInAttackRange) {
			if (attacker != null && (enemyInLineOfSight != null || victimInAttackRange != null)) {
				GameUnit unit = attacker.GetComponent<GameUnit>();
				if (unit != null) {
					if (victimInAttackRange != null && attacker.Equals(enemyInLineOfSight) && attacker.Equals(victimInAttackRange)) {
						unit.targetEnemy = null;
					}
					else {
						if (victimInAttackRange != null) {
							unit.targetEnemy = victimInAttackRange.GetComponent<GameUnit>();
							MoveToTarget(attacker);
						}
						else if (enemyInLineOfSight != null) {
							unit.targetEnemy = enemyInLineOfSight.GetComponent<GameUnit>();
							MoveToTarget(attacker);
						}
						else {
							unit.targetEnemy = null;
						}
					}
				}
			}
		}

		private void SetTargetAIEnemy(GameObject attacker, GameObject enemyInLineOfSight, GameObject victimInAttackRange) {
			if (enemyInLineOfSight == null && victimInAttackRange == null) {
				return;
			}
			GameUnit unit = attacker.GetComponent<GameUnit>();
			if (unit != null) {
				AIUnit AIunit = enemyInLineOfSight != null ? enemyInLineOfSight.GetComponent<AIUnit>() : victimInAttackRange.GetComponent<AIUnit>();
				if (AIunit != null) {
					if (victimInAttackRange != null && attacker.Equals(enemyInLineOfSight) && attacker.Equals(victimInAttackRange)) {
						unit.targetAIEnemy = null;
					}
					else {
						if (victimInAttackRange != null) {
							unit.targetAIEnemy = victimInAttackRange.GetComponent<AIUnit>();
							MoveToAITarget(attacker);
						}
						else if (enemyInLineOfSight != null) {
							unit.targetAIEnemy = enemyInLineOfSight.GetComponent<AIUnit>();
							MoveToAITarget(attacker);
						}
						else {
							unit.targetAIEnemy = null;
						}
					}
				}
			}
		}


		[ClientRpc]
		public void RpcUpdateStatus(bool targetEnemyIsGone, Color color) {
			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				renderer.material.color = color;
			}
			if (targetEnemyIsGone) {
				this.targetEnemy = null;
			}
		}


		//[ClientRpc]
		//public void RpcHealth(GameObject victim, int newHealth) {
		//	GameUnit victimUnit = victim.GetComponent<GameUnit>();
		//	victimUnit.currentHealth = newHealth;
		//}

		//[ClientRpc]
		//public void RpcAttack(GameObject victim) {
		//	if (victim == null) {
		//		return;
		//	}
		//	GameUnit victimUnit = victim.GetComponent<GameUnit>();
		//	if (victimUnit != null) {
		//		victimUnit.TakeDamage(this);
		//	}
		//}

		//My guess is that this should be a [ClientCallback] instead of [ClientRpc]
		//Both can work.
		[ClientRpc]
		public void RpcSetTarget(GameObject obj, Vector3 target) {
			//Server tells all clients to run the following codes.
			GameUnit unit = obj.GetComponent<GameUnit>();
			NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
			if (agent != null) {
				if (unit.oldTargetPosition != target) {
					agent.SetDestination(target);
					//Making sure that we actually track the new NavMeshAgent destination. If it's different, it may cause
					//desync among local and remote clients. That's a hunch though, so don't take my word for word on this.
					unit.oldTargetPosition = target;
					//Confirm that the player has issued an order for the game unit to follow/move to.
					//Syncing the isDirected flag.
					unit.isDirected = true;
					unit.targetEnemy = null;
				}
			}
		}

		[ClientRpc]
		public void RpcDestroy() {
			GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
			foreach (GameObject cam in cams) {
				CameraPanning camPan = cam.GetComponent<CameraPanning>();
				if (camPan != null) {
					Debug.Log("Destroying camPan. Check!");
					GameObject.Destroy(camPan);
					GameObject.Destroy(cam.GetComponent<PostRenderer>());
				}
			}
		}

		[ClientRpc]
		public void RpcUnitDestroy(GameObject obj) {

			GameUnit unit = obj.GetComponent<GameUnit>();
			if (unit != null) {
				if (unit.currentHealth > 0) {
					return;
				}
			}

			if (unit != null) {
				unit.DisableVisibility();
			}

			GameObject[] selects = GameObject.FindGameObjectsWithTag("SelectionManager");
			foreach (GameObject selectObj in selects) {
				SelectionManager select = selectObj.GetComponent<SelectionManager>();
				if (select != null && select.hasAuthority) {
					foreach (GameObject unitObj in select.allObjects) {
						GameUnit someUnit = unitObj.GetComponent<GameUnit>();
						if (someUnit != null) {
							if (unit != null && someUnit.targetEnemy != null) {
								if (someUnit.targetEnemy.Equals(unit)) {
									someUnit.targetEnemy = null;
								}
							}
							else if (unit == null) {
								someUnit.targetEnemy = null;
							}
						}
					}
					select.AddToRemoveList(obj);
					break;
				}
			}
		}
	}
}