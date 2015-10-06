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

	//This variable keeps track of any changes made for the NavMeshAgent's destination Vector3.
	//Doesn't even need to use [SyncVar]. Nothing is needed for tracking this on the server at all. 
	//Just let the clients (local and remote) handle the pathfinding calculations and not pass updated current transform position
	//through the network. It's not pretty when you do this.
	public Vector3 oldTargetPosition;
	public Vector3 oldEnemyTargetPosition;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();

		//Initialization code for local player (local client on the host, and remote clients).
		this.oldTargetPosition = Vector3.one * -9999f;
		this.oldEnemyTargetPosition = Vector3.one * -9999f;
		this.targetEnemy = null;
		this.isSelected = false;
		this.isDirected = false;
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
		if (!this.isDirected) {
			//Line of Sight. Detects if there are nearby enemy game units, and if so, follow them to engage in battle.
			LineOfSight sight = this.GetComponentInChildren<LineOfSight>();
			if (sight != null) {
				if (sight.enemiesInRange.Count > 0) {
					this.targetEnemy = sight.enemiesInRange[0];
				}
				else {
					this.targetEnemy = null;
				}
			}

			if (this.targetEnemy == null) {
				CmdSelfDefense(null, this.oldEnemyTargetPosition, this.oldTargetPosition);
			}
			else {
				CmdSelfDefense(this.targetEnemy.gameObject, this.targetEnemy.transform.position, this.oldTargetPosition);
			}

			////Attack Reach. If a nearby enemy game unit is within attack range, engage and attack.
			//AttackArea area = this.GetComponentInChildren<AttackArea>();
			//if (area != null) {
			//	if (area.enemies.Count > 0) {
			//		if (this.targetEnemy != null && this.targetEnemy.Equals(area.enemies[0])) {
			//			Debug.Log("Attacking!!");
			//		}
			//		else {
			//			this.targetEnemy = area.enemies[0];
			//		}
			//	}
			//}
		}

		//Keeping track of whether the game unit is carrying out a player's command, or is carrying out self-defense.
		if (agent != null && agent.ReachedDestination()) {
			this.isDirected = false;
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

	[Command]
	public void CmdSelfDefense(GameObject target, Vector3 enemyPosition, Vector3 movePosition) {
		NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
		if (agent != null) {
			if (target == null) {
				agent.SetDestination(movePosition);
			}
			else {
				if (this.oldEnemyTargetPosition != enemyPosition) {
					agent.SetDestination(enemyPosition);
					this.oldEnemyTargetPosition = enemyPosition;
				}
			}
		}
		RpcSelfDefense(target, enemyPosition, movePosition);
	}

	[ClientRpc]
	private void RpcSelfDefense(GameObject target, Vector3 enemyPosition, Vector3 movePosition) {
		if (!this.hasAuthority) {
			return;
		}

		NavMeshAgent agent = this.GetComponent<NavMeshAgent>();
		if (agent != null) {
			if (target == null) {
				agent.SetDestination(movePosition);
			}
			else {
				if (this.oldEnemyTargetPosition != enemyPosition) {
					agent.SetDestination(enemyPosition);
					this.oldEnemyTargetPosition = enemyPosition;
				}
			}
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

	public static void Copy(GameUnit original, GameUnit copy) {
		copy.isSelected = original.isSelected;
		copy.transform.position = original.transform.position;
		copy.transform.rotation = original.transform.rotation;
		copy.transform.localScale = original.transform.localScale;
	}
}
