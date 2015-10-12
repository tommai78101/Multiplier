using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[Serializable]
public struct MergeGroup {
	public GameUnit ownerUnit;
	public GameUnit mergingUnit;
	public Vector3 origin;
	public Vector3 ownerPosition;
	public Vector3 mergingPosition;
	public Vector3 ownerScale;
	public Vector3 mergingScale;
	public float elaspedTime;

	public MergeGroup(GameUnit ownerUnit, GameUnit mergingUnit) {
		this.ownerUnit = ownerUnit;
		this.mergingUnit = mergingUnit;
		this.elaspedTime = 0f;

		this.ownerPosition = ownerUnit.gameObject.transform.position;
		this.mergingPosition = mergingUnit.gameObject.transform.position;
        this.origin = Vector3.Lerp(this.ownerPosition, this.mergingPosition, 0.5f);
		this.ownerScale = ownerUnit.gameObject.transform.localScale;
		this.mergingScale = mergingUnit.gameObject.transform.localScale;
	}

	public void Update(float scaling) {
		this.ownerUnit.isSelected = false;
		this.mergingUnit.isSelected = false;

		//Merging animation. Most likely another known bug that I cannot fix.
		this.ownerUnit.gameObject.transform.position = Vector3.Lerp(this.ownerPosition, this.origin, this.elaspedTime);
		this.mergingUnit.gameObject.transform.position = Vector3.Lerp(this.mergingPosition, this.origin, this.elaspedTime);

		//Scaling animation. Same persistent bug? It might be another mysterious bug.
		Vector3 scale = Vector3.Lerp(this.ownerScale, this.ownerScale * scaling, this.elaspedTime);
		scale.y = this.ownerScale.y;
		this.ownerUnit.gameObject.transform.localScale = scale;
		scale = Vector3.Lerp(this.mergingScale, this.mergingScale * scaling, this.elaspedTime);
		scale.y = this.mergingScale.y;
		this.mergingUnit.gameObject.transform.localScale = scale;
	}

	public void Stop() {
		NavMeshAgent agent = this.ownerUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.ResetPath();
		}
		agent = this.mergingUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.ResetPath();
		}
	}
};

public class MergeManager : NetworkBehaviour {
	[SerializeField]
	public List<MergeGroup> mergeList;
	[SerializeField]
	public List<MergeGroup> removeList;
	[SerializeField]
	public SelectionManager selectionManager;

	[Range(0.1f, 100f)]
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
		//Since merging units require the selected units count to be a multiple of 2, we need to check to make sure they are a multiple of 2.
		//Else, ignore the final selected unit.
		//Going to change this into network code, to sync up merging.
		for (int i = 0; (i < this.selectionManager.selectedObjects.Count) && (i + 1 < this.selectionManager.selectedObjects.Count); i += 2) {
			GameObject ownerObject = this.selectionManager.selectedObjects[i];
			//GameUnit ownerUnit = ownerObject.GetComponent<GameUnit>();
			GameObject mergerObject = this.selectionManager.selectedObjects[i + 1];
			//GameUnit mergerUnit = mergerObject.GetComponent<GameUnit>();
			CmdAddMerge(ownerObject, mergerObject);
		}
	}

	private void UpdateMergeGroups() {
		//This follows the same code design pattern used in Split Manager. It's a very stable way of cleaning/updating the lists
		//this manager manages.
		if (this.mergeList.Count > 0) {
			for (int i = 0; i < this.mergeList.Count; i++) {
				MergeGroup group = this.mergeList[i];
				if (group.elaspedTime > 1f) {
					if (!this.removeList.Contains(group)) {
						group.Stop();
						FinishMergeGroup(group);
						this.removeList.Add(group);
					}
				}
				else {
					group.Update(this.scalingValue);
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
		CmdEndMerge(group.ownerUnit.gameObject, group.mergingUnit.gameObject);
	}

	[Command]
	public void CmdEndMerge(GameObject ownerObject, GameObject mergingObject) {
		if (mergingObject != null) {
			NetworkServer.Destroy(mergingObject);
		}
		RpcEndMerge(ownerObject);
	}

	[ClientRpc]
	public void RpcEndMerge(GameObject ownerObject) {
		NavMeshAgent agent = ownerObject.GetComponent<NavMeshAgent>();
		agent.Resume();
		agent.ResetPath();
	}

	[Command]
	public void CmdAddMerge(GameObject ownerObject, GameObject mergingObject) {
		//NetworkServer.Destroy(mergingUnit.gameObject);
		RpcAddMerge(ownerObject, mergingObject);
	}

	[ClientRpc]
	public void RpcAddMerge(GameObject ownerObject, GameObject mergingObject) {
		GameUnit ownerUnit = ownerObject.GetComponent<GameUnit>();
		GameUnit mergingUnit = mergingObject.GetComponent<GameUnit>();
		NavMeshAgent ownerAgent = ownerObject.GetComponent<NavMeshAgent>();
		ownerAgent.Stop();
		NavMeshAgent mergingAgent = mergingObject.GetComponent<NavMeshAgent>();
		mergingAgent.Stop();
		MergeGroup group = new MergeGroup(ownerUnit, mergingUnit);

		//this.mergeList.Add(new MergeGroup(ownerUnit, mergingUnit));
		GameObject[] managers = GameObject.FindGameObjectsWithTag("MergeManager");
		foreach (GameObject manager in managers) {
			MergeManager mergeManager = manager.GetComponent<MergeManager>();
			if (mergeManager != null) {
				mergeManager.mergeList.Add(group);
			}
		}
	}
}