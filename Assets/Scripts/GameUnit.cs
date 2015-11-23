using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Extension;


[System.Serializable]
public class GameUnit : NetworkBehaviour {
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
	[Range(0.001f, 10f)]
	[SyncVar]
	public float recoverCooldown;
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
	public GameUnit targetEnemy;
	public GameObject selectionRing;

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
		this.isSelected = false;
		this.isDirected = false;
		this.currentHealth = this.maxHealth;
		this.recoverCounter = this.recoverCooldown = 1f;
		this.attackCooldownCounter = this.attackCooldown;
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
				if (sight.enemiesInRange.Count > 0 && area.enemiesInAttackRange.Count > 0) {
					CmdSetTargetEnemy(this.gameObject, sight.enemiesInRange[0].gameObject, area.enemiesInAttackRange[0].gameObject);
				}
				else if (sight.enemiesInRange.Count > 0 && area.enemiesInAttackRange.Count <= 0) {
					CmdSetTargetEnemy(this.gameObject, sight.enemiesInRange[0].gameObject, sight.enemiesInRange[0].gameObject);
				}
				else if (sight.enemiesInRange.Count <= 0 && area.enemiesInAttackRange.Count > 0) {
					CmdSetTargetEnemy(this.gameObject, area.enemiesInAttackRange[0].gameObject, area.enemiesInAttackRange[0].gameObject);
				}
				else {
					CmdSetTargetEnemy(this.gameObject, this.gameObject, this.gameObject);
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
					customMaterial.SetColor("_TeamColor", Color.red);
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
			if (unit.targetEnemy != null && unit.targetEnemy.CheckIfVisible()) {  //&& checkEnemyNearby
				agent.stoppingDistance = 0.5f;
				agent.SetDestination(unit.targetEnemy.transform.position);
			}
			else {
				agent.stoppingDistance = 0f;
				agent.SetDestination(unit.oldTargetPosition);
			}
		}
	}

	public void Attack() {
		if (this.targetEnemy == null) {
			return;
		}
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

	public void UpdateStatus() {
		if (this.attackCooldownCounter > 0) {
			this.attackCooldownCounter -= Time.deltaTime;
		}
		if (this.recoverCounter < 1f) {
			this.recoverCounter += Time.deltaTime / this.recoverCooldown;
		}
		if (this.currentHealth <= 0) {
			CmdUnitDestroy(this.gameObject);
		}

		//This is used for syncing up with the non-authoritative game unit. It is used with [SyncVar].
		//CmdUpdateStatus();
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

	//public void TakeDamage(int newHealth) {
	//	if (!this.hasAuthority) {
	//		return;
	//	}
	//	//CmdHealth(this.gameObject, this.currentHealth - Mathf.FloorToInt(attacker.attackPower));
	//	this.recoverCounter = 0f;
	//}

	public bool CheckIfVisible() {
		Renderer renderer = this.GetComponent<Renderer>();
		if (renderer != null) {
			return renderer.enabled;
		}
		else {
			return false;
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
		//TODO: Cut down the amount of Cmd and Rpc calls.
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

	//[Command]
	//public void CmdUpdateStatus() {
	//	Renderer renderer = this.GetComponent<Renderer>();
	//	Color color = this.initialColor;
	//	if (renderer != null) {
	//		color = Color.Lerp(this.takeDamageColor, this.initialColor, this.recoverCounter);
	//		renderer.material.color = color;
	//       }

	//	if (this.attackCooldownCounter > 0f) {
	//		this.attackCooldownCounter -= Time.deltaTime;
	//	}
	//	if (this.recoverCounter < 1f) {
	//		this.recoverCounter += Time.deltaTime / this.recoverCooldown;
	//	}
	//	bool targetEnemyIsGone = false;
	//	if (this.targetEnemy != null) {
	//		if (!this.targetEnemy.CheckIfVisible()) {
	//			targetEnemyIsGone = true;
	//		}
	//	}

	//	if (this.currentHealth <= 0) {
	//		RpcUnitDestroy(this.gameObject);
	//	}
	//	else {
	//		RpcUpdateStatus(targetEnemyIsGone, color);
	//	}
	//}

	[Command]
	public void CmdSetTargetEnemy(GameObject obj, GameObject enemy, GameObject attackee) {
		RpcSetTargetEnemy(obj, enemy, attackee);
	}

	[ClientRpc]
	public void RpcSetTargetEnemy(GameObject obj, GameObject enemy, GameObject attackee) {
		if (obj != null) {
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (unit != null) {
				if (enemy != null && attackee != null && obj.Equals(enemy) && obj.Equals(attackee)) {
					unit.targetEnemy = null;
				}
				else {
					if (enemy != null) {
						unit.targetEnemy = enemy.GetComponent<GameUnit>();
						MoveToTarget(obj);
					}
					else if (attackee != null) {
						unit.targetEnemy = attackee.GetComponent<GameUnit>();
						MoveToTarget(obj);
					}
					else {
						unit.targetEnemy = null;
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
