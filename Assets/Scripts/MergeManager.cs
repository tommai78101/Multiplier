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
	public GameObject ownerObject;
	public GameObject mergingObject;
	public float elaspedTime;

	public MergeGroup(GameUnit ownerUnit, GameUnit mergingUnit) {
		this.owner = ownerUnit.gameObject.GetComponent<NetworkIdentity>();
		this.merging = mergingUnit.gameObject.GetComponent<NetworkIdentity>();
		this.ownerObject = ownerUnit.gameObject;
		this.mergingObject = mergingUnit.gameObject;
		this.ownerUnit = ownerUnit;
		this.mergingUnit = mergingUnit;
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

	public float scalingValue = 2f;


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

		if (Input.GetKeyDown(KeyCode.D)) {
			AddMergeGroup();
		}
		UpdateMergeGroups();
	}

	private void AddMergeGroup() {
		for (int i = 0; (i < this.selectionManager.selectedObjects.Count) && (i + 1 < this.selectionManager.selectedObjects.Count); i += 2) {
			GameObject ownerObject = this.selectionManager.selectedObjects[i];
			GameUnit ownerUnit = ownerObject.GetComponent<GameUnit>();
			GameObject mergerObject = this.selectionManager.selectedObjects[i + 1];
			GameUnit mergerUnit = mergerObject.GetComponent<GameUnit>();
			this.mergeList.Add(new MergeGroup(ownerUnit, mergerUnit));
		}
	}

	private void UpdateMergeGroups() {
		if (this.mergeList.Count > 0) {
			for (int i = 0; i < this.mergeList.Count; i++) {
				MergeGroup group = this.mergeList[i];
				if (group.elaspedTime > 1f) {
					if (!this.removeList.Contains(group)) {
						FinishMergeGroup(group);
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

	private void FinishMergeGroup(MergeGroup group) {
		CmdMerge(group.ownerObject, group.mergingObject);
	}

	[Command]
	public void CmdMerge(GameObject ownerObject, GameObject mergingObject) {
		NetworkServer.Destroy(mergingObject);
		RpcMerge(ownerObject, mergingObject);
	}

	[ClientRpc]
	public void RpcMerge(GameObject ownerObject, GameObject mergingObject) {
		Vector3 scale = ownerObject.transform.localScale;
		scale *= this.scalingValue;
		ownerObject.transform.localScale = scale;
	}
}