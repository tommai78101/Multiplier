using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[Serializable]
public struct MergeGroup {
	public NetworkIdentity owner;
	public NetworkIdentity merging;
	public GameUnit ownerUnit;
	public GameUnit mergingUnit;
	public float elaspedTime;

	public MergeGroup(GameObject ownerObject, GameObject mergingObject) {
		this.owner = ownerObject.GetComponent<NetworkIdentity>();
		this.merging = mergingObject.GetComponent<NetworkIdentity>();
		this.ownerUnit = ownerObject.GetComponent<GameUnit>();
		this.mergingUnit = mergingObject.GetComponent<GameUnit>();
		this.elaspedTime = 0f;
	}

	public void Update() {
		Debug.Log("Merging group : TODO");
	}
};

public class MergeManager : NetworkBehaviour {
	[SerializeField]
	public List<MergeGroup> mergeList;
	[SerializeField]
	public List<MergeGroup> removeList;
	[SerializeField]
	public SelectionManager selectionManager;


	void Start() {
		if (!this.hasAuthority) {
			return;
		}

		if (this.mergeList == null) {
			this.mergeList = new List<MergeGroup>();
		}
		if (this.removeList == null) {
			this.removeList = new List<MergeGroup>();
		}
		if (this.selectionManager == null) {
			GameObject[] managers = GameObject.FindGameObjectsWithTag("SelectionManager");
			foreach (GameObject select in managers) {
				SelectionManager selectManager = select.GetComponent<SelectionManager>();
				if (selectManager != null && selectManager.hasAuthority) {
					this.selectionManager = selectManager;
					break;
				}
			}
			if (this.selectionManager == null) {
				Debug.LogError("Merge Manager: Selection Manager is null. Please check.");
			}
		}
	}

	void Update() {
		if (!this.hasAuthority) {
			return;
		}

		if (this.mergeList.Count > 0) {
			for (int i = 0; i < this.mergeList.Count; i++) {
				MergeGroup group = this.mergeList[i];
				if (group.elaspedTime > 1f) {
					if (!this.removeList.Contains(group)) {
						this.removeList.Add(group);
					}
				}
				else {
					group.Update();
					group.elaspedTime += Time.deltaTime / 3f;
					this.mergeList[i] = group;
				}
			}
		}

		if (this.removeList.Count > 0) {
			foreach (MergeGroup group in this.removeList) {
				if (this.mergeList.Contains(group)) {
					this.mergeList.Remove(group);
				}
			}
			this.removeList.Clear();
		}
	}
}
