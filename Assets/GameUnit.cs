using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameUnit : NetworkBehaviour {
	//Properties of a Game Unit
	[SyncVar]
	public bool isSelected;
	public GameObject selectionRing;

	//This variable keeps track of any changes made for the NavMeshAgent's destination Vector3.
	//Doesn't even need to use [SyncVar]. Nothing is needed for tracking this on the server at all. 
	//Just let the clients (local and remote) handle the pathfinding calculations and not pass updated current transform position
	//through the network. It's not pretty when you do this.
	private Vector3 oldTargetPosition;

	public override void OnStartLocalPlayer() {
		base.OnStartLocalPlayer();

		//Initialization code for local player (local client on the host, and remote clients).
		this.oldTargetPosition = Vector3.one * -1f;
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

	void CastRay() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray);
		foreach (RaycastHit hit in hits) {
			if (hit.collider.gameObject.tag.Equals("Floor")) {
				//Call on the client->server method to start the action.
				CmdSetTarget(hit.point);
				break;
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
