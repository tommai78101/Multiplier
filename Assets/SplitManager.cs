using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct SplitGroup {
	public NetworkIdentity owner;
	public NetworkIdentity split;
	public NetworkConnection clientAuthorityOwner;
	public float elapsedTime;

	public SplitGroup(NetworkIdentity owner, NetworkIdentity split, NetworkConnection clientAuthorityOwner) {
		this.owner = owner;
		this.split = split;
		this.clientAuthorityOwner = clientAuthorityOwner;
		this.elapsedTime = 0f;

		if (this.owner.clientAuthorityOwner != null) {
			this.owner.RemoveClientAuthority(this.clientAuthorityOwner);
		}
		if (this.split.clientAuthorityOwner != null) {
			this.split.RemoveClientAuthority(this.clientAuthorityOwner);
		}
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


	public void Start() {
		if (!this.hasAuthority) {
			return;
		}

		if (this.splitGroupList == null) {
			this.splitGroupList = new List<SplitGroup>();
		}
		if (this.selectionManager == null) {
			Debug.LogError("Cannot find Selection Manager. Searching for one.");
			GameObject[] managers = GameObject.FindGameObjectsWithTag("SelectionManager");
			foreach (GameObject manager in managers) {
				SelectionManager select = manager.GetComponent<SelectionManager>();
				if (select != null && select.hasAuthority) {
					this.selectionManager = select;
				}
			}
			if (this.selectionManager == null) {
				Debug.LogError("Cannot find Selection Manager. Aborting");
			}
		}
	}

	public void Update () {
		if (!this.hasAuthority) {
			return;
		}

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
					group.elapsedTime += Time.deltaTime / 3f;
					this.splitGroupList[i] = group;
				}
			}
		}

		if (this.removeList != null && this.removeList.Count > 0) {
			foreach (SplitGroup group in this.removeList) {
				Debug.Log("Removing group " + group.ToString());
				this.splitGroupList.Remove(group);
			}
			this.removeList.Clear();
		}
	}

	private void AddingNewSplitGroup() {
		foreach (GameObject obj in this.selectionManager.selectedObjects) {
			NetworkIdentity identity = obj.GetComponent<NetworkIdentity>();

			if (identity.clientAuthorityOwner != null) {
				Debug.Log("Object identity has client authority owner.");
			}
			GameUnit unit = obj.GetComponent<GameUnit>();
			if (unit.hasAuthority) {
				Debug.Log("GameUnit however has authority.");
			}

			//Debug.Log("Calling on CmdSplit()");
			//CmdSplit(identity);
		}
		return;
	}

	[Command]
	public void CmdSplit(NetworkIdentity obj) {
		Debug.Log("It's being called...");

		//Just double-checking.
		if (!this.localPlayerAuthority) {
			return;
		}
		RpcSplit(obj.GetComponent<NetworkIdentity>());
	}

	[ClientRpc]
	public void RpcSplit(NetworkIdentity owner) {
		Debug.Log("Calling on RpcSplit()");

		if (!this.localPlayerAuthority) {
			return;
		}

		if (!this.hasAuthority) {
			Debug.Log("No authority");
			return;
		}

		//When spawning non-player objects from non-player objects, the spawned objects must be assigned a client authority owner
		//through NetworkIdentity. I don't think the order matters here...
		if (owner.clientAuthorityOwner != null) {
			GameObject mainObj = owner.gameObject;
			GameObject splitObj = MonoBehaviour.Instantiate(mainObj) as GameObject;
			NetworkIdentity splitIdentity = splitObj.GetComponent<NetworkIdentity>();
			NetworkServer.SpawnWithClientAuthority(splitObj, owner.clientAuthorityOwner);
			owner.AssignClientAuthority(owner.clientAuthorityOwner);
			splitIdentity.AssignClientAuthority(owner.clientAuthorityOwner);
		}

		//if (splitIdentity != null && owner != null) {
		//	Debug.Log("Adding new split group. Owner: " + owner.ToString() + " Split: " + splitIdentity.ToString());
		//	this.splitGroupList.Add(new SplitGroup(owner, splitIdentity, owner.clientAuthorityOwner));
		//}
	}
}
