using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Spawner : NetworkBehaviour {
	public GameObject spawnPrefab;
	public GameObject selectionManagerPrefab;
	public GameObject splitManagerPrefab;
	public GameObject mergeManagerPrefab;
	public GameObject unitAttributesPrefab;
	[SerializeField]
	public NetworkConnection owner;

	public static int colorCode = 0;

	public override void OnStartLocalPlayer() {
		//I kept this part in, because I don't know if this is the function that sets isLocalPlayer to true, 
		//or this function triggers after isLocalPlayer is set to true.
		base.OnStartLocalPlayer();

		NetworkIdentity identity = this.GetComponent<NetworkIdentity>();
		if (this.isServer) {
			this.owner = identity.connectionToClient;
		}
		else {
			this.owner = identity.connectionToServer;
		}

		//On initialization, make the client (local client and remote clients) tell the server to call on an [ClientRpc] method.
		CmdCall();
	}

	[Command]
	public void CmdCall() {
		//Calling [ClientRpc] on the server.
		RpcLog();
	}

	[ClientRpc]
	public void RpcLog() {
		//First, checks to see what type of recipient is receiving this message. If it's server, the output message should tell the user what the type is.
		Debug.Log("RPC: This is " + (this.isServer ? " Server" : " Client"));

		//Second, initialize everything that is common for both server and client. 
		//Camera stuff
		GameObject[] cameraObjects = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cameraObjects) {
			GameObject minimap = GameObject.FindGameObjectWithTag("Minimap");
			if (minimap != null) {
				Material borderMaterial = Resources.Load<Material>("Border");
				if (borderMaterial != null) {
					if (cam.GetComponent<PostRenderer>() == null) {
						cam.AddComponent<PostRenderer>();
					}
					PostRenderer postRenderer = cam.GetComponent<PostRenderer>();
					if (postRenderer != null) {
						postRenderer.borderMaterial = borderMaterial;
						postRenderer.minimapCamera = minimap.GetComponent<Camera>();
						if (postRenderer.minimapCamera == null) {
							Debug.LogError("Unable to assign minimap camera to post renderer.");
						}
					}
				}

				if (cam.GetComponent<CameraPanning>() == null) {
					Debug.Log("Camera Panning is added to camera. Please check.");
					CameraPanning panning = cam.AddComponent<CameraPanning>();
					MinimapStuffs stuffs = minimap.GetComponent<MinimapStuffs>();
					if (stuffs != null) {
						stuffs.playerCameraPanning = panning;
					}
					panning.cameraPanning = true;
				}
				else {
					if (!cam.GetComponent<CameraPanning>().cameraPanning) {
						cam.GetComponent<CameraPanning>().cameraPanning = true;
					}
				}


			}
		}

		//Finally, initialize server only stuff or client only stuff.
		//Also, finally found a use-case for [Server] / [ServerCallback]. Simplifies things a bit.
		ServerInitialize();
	}

	[ServerCallback]
	public void ServerInitialize() {
		//Server code
		//This is run for spawning new non-player objects. Since it is a server calling to all clients (local and remote), it needs to pass in a
		//NetworkConnection that connects from server to THAT PARTICULAR client, who is going to own client authority on the spawned object.

		//Player unit
		GameObject playerObject = MonoBehaviour.Instantiate(this.spawnPrefab) as GameObject;
		playerObject.transform.position = this.transform.position;
		NetworkIdentity objIdentity = playerObject.GetComponent<NetworkIdentity>();
		NetworkServer.SpawnWithClientAuthority(playerObject, this.connectionToClient);

		//Player selection manager
		GameObject manager = MonoBehaviour.Instantiate(this.selectionManagerPrefab) as GameObject;
		SelectionManager selectionManager = manager.GetComponent<SelectionManager>();
		if (selectionManager != null) {
			selectionManager.allObjects.Add(playerObject);
			selectionManager.authorityOwner = objIdentity.clientAuthorityOwner;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		//Player split manager
		manager = MonoBehaviour.Instantiate(this.splitManagerPrefab) as GameObject;
		SplitManager splitManager = manager.GetComponent<SplitManager>();
		if (splitManager != null) {
			splitManager.selectionManager = selectionManager;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		//Player merge manager
		manager = MonoBehaviour.Instantiate(this.mergeManagerPrefab) as GameObject;
		MergeManager mergeManager = manager.GetComponent<MergeManager>();
		if (mergeManager != null) {
			mergeManager.selectionManager = selectionManager;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		//Unit Attributes Tracker
		manager = MonoBehaviour.Instantiate(this.unitAttributesPrefab) as GameObject;
		UnitAttributes attributes = manager.GetComponent<UnitAttributes>();
		if (attributes != null) {
			splitManager.unitAttributes = attributes;
			mergeManager.unitAttributes = attributes;
		}
		NetworkServer.SpawnWithClientAuthority(manager, this.connectionToClient);

		RpcCameraSetup(playerObject);

		int colorValue;
		switch (Spawner.colorCode) {
			default:
				colorValue = -1;
				break;
			case 0:
				colorValue = 0;
				break;
			case 1:
				colorValue = 1;
				break;
			case 2:
				colorValue = 2;
				break;
		}

		Spawner.colorCode++;
		if (Spawner.colorCode > 2) {
			Spawner.colorCode = 0;
		}
		RpcUnitAttributesSetup(manager, playerObject, colorValue);
	}

	[ClientRpc]
	public void RpcUnitAttributesSetup(GameObject manager, GameObject playerObject, int colorValue) {
		Debug.Log("Setting up unit attributes.");
		GameUnit playerUnit = playerObject.GetComponent<GameUnit>();
		if (playerUnit != null) {
			playerUnit.SetTeamColor(colorValue);
		}

		UnitAttributes attributes = manager.GetComponent<UnitAttributes>();
		if (attributes != null && attributes.hasAuthority) {
			GameObject console = GameObject.FindGameObjectWithTag("Console");
			if (console != null) {
				CanvasSwitch canvasSwitch = console.GetComponent<CanvasSwitch>();
				if (canvasSwitch != null) {
					canvasSwitch.unitAttributes = attributes;
				}
			}

			GameObject content = GameObject.FindGameObjectWithTag("Content");
			if (content != null) {
				Attributes attr = content.GetComponent<Attributes>();
				if (attr != null) {
					attr.unitAttributes = attributes;
				}
			}

			if (playerUnit != null) {
				playerUnit.attributes = attributes;
			}
		}
	}

	[ClientRpc]
	public void RpcCameraSetup(GameObject obj) {
		if (!this.isLocalPlayer) {
			return;
		}

		Vector3 pos = obj.transform.position;
		pos.y = Camera.main.transform.position.y;
		Camera.main.transform.position = pos;
	}

	public void OnDestroy() {
		//By default, NetworkManager destroys game objects that were spawned into the game via NetworkServer.Spawn() or NetworkServer.SpawnWithClientAuthority().
		//This is why NetworkBehaviours and MonoBehaviours could not fire OnPlayerDisconnected() and OnDisconnectedFromServer() event methods. The
		//game objects the NetworkBehaviours and MonoBehaviours had been attached to are destroyed before they have the chance to fire the event methods.

		//This hasAuthority flag checking is still required, just like any other event methods from NetworkBehaviours.
		if (!this.hasAuthority) {
			return;
		}

		//This is called to destroy the camera panning. When the game ends, the player shouldn't be moving around.
		GameObject[] cams = GameObject.FindGameObjectsWithTag("MainCamera");
		foreach (GameObject cam in cams) {
			CameraPanning panning = cam.GetComponent<CameraPanning>();
			if (panning != null) {
				GameObject minimap = GameObject.FindGameObjectWithTag("Minimap");
				if (minimap != null) {
					MinimapStuffs stuffs = minimap.GetComponent<MinimapStuffs>();
					if (stuffs != null) {
						stuffs.playerCameraPanning = null;
					}
				}

				Debug.Log("Destroying camera panning.");
				Destroy(panning);
			}
		}
	}
}