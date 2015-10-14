using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using Extension;

public class GameUnit : NetworkBehaviour {
	//Properties of a Game Unit
	[SyncVar]
	public bool isSelected;
	[SyncVar]
	public bool isDirected;
	public GameUnit targetEnemy;
	public GameObject selectionRing;
	[SyncVar]
	public int currentHealth;
	[Range(3, 100)]
	[SyncVar]
	public int maxHealth;
	[Range(1f, 100f)]
	[SyncVar]
	public float attackPower;
	[Range(0.1f, 10f)]
	[SyncVar]
	public float attackCooldown;
	[SyncVar]
	public float attackCooldownCounter;
	[Range(0.001f, 10f)]
	[SyncVar]
	public float recoverCooldown;
	[SyncVar]
	public float recoverCounter;
	[SyncVar]
	public int level;
	[SyncVar]
	public Color initialColor;
	[SyncVar]
	public Color takeDamageColor;

	public static bool once = false;

	//This variable keeps track of any changes made for the NavMeshAgent's destination Vector3.
	//Doesn't even need to use [SyncVar]. Nothing is needed for tracking this on the server at all. 
	//Just let the clients (local and remote) handle the pathfinding calculations and not pass updated current transform position
	//through the network. It's not pretty when you do this.
	public Vector3 oldTargetPosition;
	public Vector3 oldEnemyTargetPosition;

	public override void OnStartClient() {
		base.OnStartAuthority();

		Debug.LogWarning("Starting client");

		//if (!GameUnit.once) {
		//	GameUnit.once = true;

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

		Renderer renderer = this.GetComponent<Renderer>();
		if (renderer != null) {
			this.initialColor = renderer.material.color;
		}
		//}
	}

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();
		Debug.LogWarning("Starting local player");
	}

	public override void OnStartAuthority() {
		base.OnStartClient();
		Debug.LogWarning("Starting authority");
	}

	public void Update() {
		//Because the game is now spawning objects from the player-owned objects (spawning from NetworkManager-spawned objects), don't check for 
		//isLocalPlayer, but instead check to see if the clients have authority over the non-player owned objects spawned from the NetworkManager-spawned objects.
		//Wordy, I know...
		if (!this.hasAuthority) {
			return;
		}

		//Simple, "quick," MOBA-style controls. Hence, the class name.
		if (this.isSelected) {
			this.selectionRing.SetActive(true);
			if (Input.GetMouseButton(1)) {
				CastRay();
			}
		}
		else {
			this.selectionRing.SetActive(false);
		}

		NavMeshAgent agent = this.GetComponent<NavMeshAgent>();

		//Non-directed, self-defense
		LineOfSight sight = this.GetComponentInChildren<LineOfSight>();
		if (!this.isDirected || agent.remainingDistance < 0.5f) {
			//Line of Sight. Detects if there are nearby enemy game units, and if so, follow them to engage in battle.
			if (sight != null) {
				if (sight.enemiesInRange.Count > 0) {
					CmdSetTargetEnemy(this.gameObject, sight.enemiesInRange[0].gameObject);
				}
				else {
					this.targetEnemy = null;
				}
			}
		}

		Attack();
		UpdateStatus();

		//Keeping track of whether the game unit is carrying out a player's command, or is carrying out self-defense.
		if (agent != null && agent.ReachedDestination()) {
			this.isDirected = false;
		}
	}

	public void MoveToTarget() {
		NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
		if (agent != null) {
			if (this.targetEnemy != null) {
				agent.stoppingDistance = 0.5f;
				agent.SetDestination(this.targetEnemy.transform.position);
			}
			else {
				agent.stoppingDistance = 0f;
				agent.SetDestination(this.oldTargetPosition);
			}
		}
	}

	public void Attack() {
		//Attack Reach. If a nearby enemy game unit is within attack range, engage and attack.
		AttackArea area = this.GetComponentInChildren<AttackArea>();
		if (area != null) {
			if (this.targetEnemy != null && area.enemiesInAttackRange.Contains(this.targetEnemy)) {
				if (this.attackCooldownCounter <= 0f) {
					CmdAttack(this.targetEnemy.gameObject);
					this.attackCooldownCounter = this.attackCooldown;
				}
			}
			else if (area.enemiesInAttackRange.Count > 0) {
				this.targetEnemy = area.enemiesInAttackRange[0];
			}
			else {
				this.targetEnemy = null;
			}
		}
	}

	public void UpdateStatus() {
		Renderer renderer = this.GetComponent<Renderer>();
		if (renderer != null) {
			renderer.material.color = Color.Lerp(this.takeDamageColor, this.initialColor, this.recoverCounter);
		}

		if (this.attackCooldownCounter > 0f) {
			this.attackCooldownCounter -= Time.deltaTime;
		}
		if (this.recoverCounter < 1f) {
			this.recoverCounter += Time.deltaTime / this.recoverCooldown;
		}
		//This is used for syncing up with the non-authoritative game unit. It is used with [SyncVar].
		CmdUpdateStatus(this.attackCooldownCounter, this.recoverCounter, this.currentHealth, renderer.material.color);
	}

	[Command]
	public void CmdSetTargetEnemy(GameObject obj, GameObject enemy) {
		Debug.Log("This command is working.");
		RpcSetTargetEnemy(obj, enemy);
	}

	[ClientRpc]
	public void RpcSetTargetEnemy(GameObject obj, GameObject enemy) {
		Debug.Log("This rpc is working.");
		if (obj != null && enemy != null) {
			GameUnit unit = obj.GetComponent<GameUnit>();
			unit.targetEnemy = enemy.GetComponent<GameUnit>();
			MoveToTarget();
		}
		else {
			this.targetEnemy = null;
		}
	}

	[Command]
	public void CmdUpdateStatus(float attackCounter, float recoverCounter, int currentHealth, Color color) {
		this.attackCooldownCounter = attackCounter;
		this.recoverCounter = recoverCounter;
		this.currentHealth = currentHealth;
		RpcUpdateStatus(color);
	}

	[ClientRpc]
	public void RpcUpdateStatus(Color color) {
		Renderer renderer = this.GetComponent<Renderer>();
		if (renderer != null) {
			renderer.material.color = color;
		}
	}

	public void OnPlayerDisconnected(NetworkPlayer player) {
		//Destroy camera stuff
		GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
		if (camObj != null) {
			GameObject.Destroy(camObj.GetComponent<PostRenderer>());
		}

		//Destroying this client's game object on the server when the client has disconnected. This game object, the object with Quick
		//script attached.
		CmdDestroy();
	}

	private void CastRay() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray);
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.tag.Equals("Floor")) {
				//Confirm that the player has issued an order for the game unit to follow/move to.
				this.isDirected = true;
				//Call on the client->server method to start the action.
				CmdSetTarget(hit.point);
				break;
			}
		}
	}

	public void TakeDamage(GameUnit attacker) {
		CmdHealth(this.currentHealth - Mathf.FloorToInt(attacker.attackPower));
		this.recoverCounter = 0f;
		if (this.currentHealth <= 0) {
			CmdUnitDestroy();
		}
	}

	[Command]
	public void CmdHealth(int newHealth) {
		this.currentHealth = newHealth;
	}

	[Command]
	public void CmdAttack(GameObject victim) {
		Debug.Log("Calling on CmdAttack to server.");
		RpcAttack(victim);
	}

	[ClientRpc]
	public void RpcAttack(GameObject victim) {
		Debug.Log("Calling on RpcAttack to client.");
		if (victim == null) {
			return;
		}
		GameUnit victimUnit = victim.GetComponent<GameUnit>();
		if (victimUnit != null) {
			victimUnit.TakeDamage(this);
		}
	}

	[Command]
	public void CmdSetTarget(Vector3 target) {
		//Command call to tell the server to run the following code.
		RpcSetTarget(target);
	}

	//My guess is that this should be a [ClientCallback] instead of [ClientRpc]
	//Both can work.
	[ClientRpc]
	public void RpcSetTarget(Vector3 target) {
		//Server tells all clients to run the following codes.
		NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
		if (agent != null) {
			if (this.oldTargetPosition != target) {
				agent.SetDestination(target);
				//Making sure that we actually track the new NavMeshAgent destination. If it's different, it may cause
				//desync among local and remote clients. That's a hunch though, so don't take my word for word on this.
				this.oldTargetPosition = target;
			}
		}
	}


	//Destroy [Command] and [ClientRpc] code definition.
	//It seems like all future code design patterns must use [Command] and [ClientRpc] / [ClientCallback] combo to actually get
	//something to work across the network. Keeping this in mind.
	[Command]
	public void CmdDestroy() {
		RpcDestroy();
	}

	[ClientRpc]
	public void RpcDestroy() {
		GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cams) {
			CameraPanning camPan = cam.GetComponent<CameraPanning>();
			if (camPan != null) {
				Debug.Log("Destroying camPan. Check!");
				Destroy(cam.GetComponent<CameraPanning>());
			}
		}

		NetworkServer.Destroy(this.gameObject);
	}

	[Command]
	public void CmdUnitDestroy() {
		RpcUnitDestroy(this.gameObject);
		NetworkServer.Destroy(this.gameObject);
	}

	[ClientRpc]
	public void RpcUnitDestroy(GameObject obj) {
		GameObject[] selects = GameObject.FindGameObjectsWithTag("SelectionManager");
		foreach (GameObject selectObj in selects) {
			SelectionManager select = selectObj.GetComponent<SelectionManager>();
			if (select != null) {
				select.AddToRemoveList(obj);
			}
		}
	}
}
