using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct SplitGroup {
	public NetworkIdentity owner;
	public NetworkIdentity split;
	public NetworkConnection authorityOwner;
	public float elapsedTime;

	public SplitGroup(NetworkIdentity owner, NetworkIdentity split, NetworkConnection clientAuthorityOwner) {
		this.owner = owner;
		this.split = split;
		this.authorityOwner = clientAuthorityOwner;
		this.elapsedTime = 0f;

		if (this.owner.clientAuthorityOwner != null) {
			this.owner.RemoveClientAuthority(this.authorityOwner);
		}
		if (this.split.clientAuthorityOwner != null) {
			this.split.RemoveClientAuthority(this.authorityOwner);
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
	[SerializeField]
	public Spawner spawner;
	public GameObject gameUnitPrefab;

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
				if (spawner.hasAuthority) {
					this.spawner = spawner;
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
			CmdSplit(obj);
		}
		return;
	}

	[Command]
	public void CmdSplit(GameObject obj) {
		//Place the RPC call here.
		Debug.Log("CMD split was called. Calling RPC.");
		GameObject split = MonoBehaviour.Instantiate(this.gameUnitPrefab) as GameObject;
		NetworkIdentity managerIdentity = this.GetComponent<NetworkIdentity>();
		NetworkServer.SpawnWithClientAuthority(split, managerIdentity.clientAuthorityOwner);
		RpcSplit(obj, split);
	}

	[ClientRpc]
	public void RpcSplit(GameObject obj, GameObject split) {
		if (!this.hasAuthority) {
			return;
		}
		GameUnit original = obj.GetComponent<GameUnit>();
		GameUnit copy = split.GetComponent<GameUnit>();
		GameUnit.Copy(original, copy);
		this.selectionManager.allObjects.Add(split);
	}
}
