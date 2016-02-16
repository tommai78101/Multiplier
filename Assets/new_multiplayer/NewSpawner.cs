using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace MultiPlayer {

	public class NewSpawner : NetworkBehaviour {
		public GameObject newGameUnitPrefab;
		public NetworkConnection owner;

		public void Start() {
			NetworkIdentity spawnerIdentity = this.GetComponent<NetworkIdentity>();
			this.owner = this.isServer ? spawnerIdentity.connectionToClient : spawnerIdentity.connectionToServer;
			Debug.Log("This is " + (this.isServer ? " Server." : " Client."));

			ServerInitialize();
		}

		[ServerCallback]
		public void ServerInitialize() {
			GameObject gameUnit = MonoBehaviour.Instantiate<GameObject>(this.newGameUnitPrefab);
			gameUnit.transform.SetParent(this.transform);
			gameUnit.transform.position = this.transform.position;
			NetworkIdentity unitIdentity = gameUnit.GetComponent<NetworkIdentity>();
			unitIdentity.localPlayerAuthority = true;
			NetworkServer.SpawnWithClientAuthority(gameUnit, this.owner);
			RpcOrganize();
		}

		[ClientRpc]
		public void RpcOrganize() {
			NewSpawner[] spawners = GameObject.FindObjectsOfType<NewSpawner>();
			NewGameUnit[] units = GameObject.FindObjectsOfType<NewGameUnit>();
			foreach (NewSpawner spawner in spawners) {
				if (spawner.hasAuthority) {
					foreach (NewGameUnit unit in units) {
						if (unit.hasAuthority) {
							unit.transform.SetParent(spawner.transform);
						}
					}
				} 
				else {
					foreach (NewGameUnit unit in units) {
						if (!unit.hasAuthority) {
							unit.transform.SetParent(spawner.transform);
						}
					}
				}
			}
		}
	}
}
