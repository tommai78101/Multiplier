using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct SplitGroup {
	public NetworkIdentity owner;
	public NetworkIdentity split;
	public GameUnit ownerUnit;
	public GameUnit splitUnit;
	public float elapsedTime;

	public SplitGroup(GameUnit ownerUnit, GameUnit splitUnit) {
		this.ownerUnit = ownerUnit;
		this.splitUnit = splitUnit;
		this.owner = ownerUnit.gameObject.GetComponent<NetworkIdentity>();
		this.split = splitUnit.gameObject.GetComponent<NetworkIdentity>();
		this.elapsedTime = 0f;
	}

	public void Update() {
		Debug.Log("Updating... from Split Group ");
	}

	public override string ToString() {
		return "Group something.";
	}
};


public class SplitManager : NetworkBehaviour {
	[SerializeField]
	public List<SplitGroup> splitGroupList;
	[SerializeField]
	public List<SplitGroup> removeList;
	[SerializeField]
	public SelectionManager selectionManager;
	[SerializeField]
	public Spawner spawner;
	public GameObject gameUnitPrefab;

	//Split Manager is designed to streamline the creation of new game units.
	//To achieve this, there needs to be two different array list that keeps track of all the creations, called Split Groups.
	//One keeps track of the Split Groups, the other removes them from the tracking list.

	public void Start() {
		if (!this.hasAuthority) {
			return;
		}

		if (this.splitGroupList == null) {
			this.splitGroupList = new List<SplitGroup>();
		}
		if (this.selectionManager == null) {
			GameObject[] managers = GameObject.FindGameObjectsWithTag("SelectionManager");
			foreach (GameObject manager in managers) {
				SelectionManager select = manager.GetComponent<SelectionManager>();
				if (select != null && select.hasAuthority) {
					this.selectionManager = select;
					break;
				}
			}
			if (this.selectionManager == null) {
				Debug.LogError("Cannot find Selection Manager. Aborting");
			}
		}
		if (this.spawner == null) {
			GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
			foreach (GameObject obj in spawners) {
				Spawner spawner = obj.GetComponent<Spawner>();
				if (spawner != null && spawner.hasAuthority) {
					this.spawner = spawner;
					break;
				}
			}
			if (this.spawner == null) {
				Debug.LogError("Spawner is never set. Please check.");
			}
		}
	}

	public void Update() {
		if (!this.hasAuthority) {
			return;
		}

		//When the player starts the action to split a game unit into two, it takes in all the selected game units
		//one by one, and splits them individually.
		if (Input.GetKeyDown(KeyCode.S)) {
			if (this.selectionManager != null) {
				AddingNewSplitGroup();
			}
		}
		UpdateSplitGroup();
	}

	private void UpdateSplitGroup() {
		if (this.splitGroupList != null && this.splitGroupList.Count > 0) {
			for (int i = 0; i < this.splitGroupList.Count; i++) {
				Debug.Log("Updating split group...." + i);
				SplitGroup group = this.splitGroupList[i];
				if (group.elapsedTime > 1f) {
					if (!this.selectionManager.allObjects.Contains(group.split.gameObject)) {
						this.selectionManager.allObjects.Add(group.split.gameObject);
					}
					if (!this.selectionManager.allObjects.Contains(group.owner.gameObject)) {
						this.selectionManager.allObjects.Add(group.owner.gameObject);
					}
					this.removeList.Add(group);
				}
				else {
					//Some weird C# language design...
					group.Update();
					group.elapsedTime += Time.deltaTime / 3f;
					this.splitGroupList[i] = group;
				}
			}
		}

		if (this.removeList != null && this.removeList.Count > 0) {
			foreach (SplitGroup group in this.removeList) {
				this.splitGroupList.Remove(group);
			}
			this.removeList.Clear();
		}
	}

	private void AddingNewSplitGroup() {
		foreach (GameObject obj in this.selectionManager.selectedObjects) {
			CmdSplit(obj);
		}
		return;
	}

	[Command]
	public void CmdSplit(GameObject obj) {
		//This is profoundly one of the hardest puzzles I had tackled. Non-player object spawning non-player object.
		//Instead of the usual spawning design used in the Spawner script, the spawning codes here are swapped around.
		//In Spawner, you would called on NetworkServer.SpawnWithClientAuthority() in the [ClientRpc]. Here, it's in [Command].
		//I am guessing it has to do with how player objects and non-player objects interact with UNET.
		GameObject split = MonoBehaviour.Instantiate(this.gameUnitPrefab) as GameObject;
		split.transform.position = obj.transform.position;
		NetworkIdentity managerIdentity = this.GetComponent<NetworkIdentity>();
		NetworkServer.SpawnWithClientAuthority(split, managerIdentity.clientAuthorityOwner);
		RpcSplit(obj, split);
	}

	[ClientRpc]
	public void RpcSplit(GameObject obj, GameObject split) {
		//We do not call on NetworkServer methods here. This is used only to sync up with the original game unit for all clients.
		//This includes adding the newly spawned game unit into the Selection Manager that handles keeping track of all game units.
		if (!this.hasAuthority) {
			return;
		}
		GameUnit original = obj.GetComponent<GameUnit>();
		GameUnit copy = split.GetComponent<GameUnit>();
		GameUnit.Copy(original, copy);
		this.splitGroupList.Add(new SplitGroup(original, copy));
		this.selectionManager.allObjects.Add(split);
	}
}
