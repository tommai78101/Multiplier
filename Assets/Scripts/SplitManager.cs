using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct SplitGroup {
	public GameUnit ownerUnit;
	public GameUnit splitUnit;
	public float elapsedTime;
	public Vector3 rotationVector;
	public Vector3 origin;

	public SplitGroup(GameUnit ownerUnit, GameUnit splitUnit, float angle) {
		this.ownerUnit = ownerUnit;
		this.splitUnit = splitUnit;
		this.elapsedTime = 0f;
		this.origin = ownerUnit.gameObject.transform.position;

		//TODO: Add a radius where the unit will always go towards.
		SpawnRange range = this.ownerUnit.GetComponentInChildren<SpawnRange>();
		this.rotationVector = Quaternion.Euler(0f, angle, 0f) * (Vector3.one * range.radius);

		NavMeshAgent agent = this.ownerUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.ResetPath();
			agent.Stop();
		}

		agent = this.splitUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.ResetPath();
			agent.Stop();
		}
	}

	public void Update() {
		this.ownerUnit.isSelected = false;
		this.splitUnit.isSelected = false;

		//Known Bug: When splitting, the local client will have smooth transitions, but the remote clients will see stuttering animations caused by
		//constant updates to the NavMeshAgent.
		//If it's a bug that I couldn't fix, then make it a feature!
		//Making splitting animations an obvious cue for the players to see.
		Vector3 pos = Vector3.Lerp(this.origin, this.origin + this.rotationVector, this.elapsedTime);
		if (this.ownerUnit == null || this.ownerUnit.gameObject == null) {
			this.elapsedTime = 1f;
			return;
		}
		this.ownerUnit.gameObject.transform.position = pos;
		pos = Vector3.Lerp(this.origin, this.origin - this.rotationVector, this.elapsedTime);
		if (this.splitUnit == null || this.splitUnit.gameObject == null) {
			this.elapsedTime = 1f;
			return;
		}
		this.splitUnit.gameObject.transform.position = pos;
	}

	public void Stop() {
		NavMeshAgent agent = this.ownerUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.Resume();
		}

		agent = this.splitUnit.GetComponent<NavMeshAgent>();
		if (agent != null) {
			agent.Resume();
		}
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

	public void UpdateSplitGroup() {
		if (this.splitGroupList != null && this.splitGroupList.Count > 0) {
			for (int i = 0; i < this.splitGroupList.Count; i++) {
				Debug.Log("Updating split group...." + i);
				SplitGroup group = this.splitGroupList[i];
				if (group.elapsedTime > 1f) {
					group.Stop();
					if (!this.selectionManager.allObjects.Contains(group.splitUnit.gameObject)) {
						this.selectionManager.allObjects.Add(group.splitUnit.gameObject);
					}
					if (!this.selectionManager.allObjects.Contains(group.ownerUnit.gameObject)) {
						this.selectionManager.allObjects.Add(group.ownerUnit.gameObject);
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
			if (obj == null) {
				this.selectionManager.removeList.Add(obj);
				continue;
			}
			GameUnit objUnit = obj.GetComponent<GameUnit>();
			if (objUnit.level == 1) {
				CmdSplit(obj);
			}
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
		float angle = UnityEngine.Random.Range(-180f, 180f);
		RpcSplit(obj, split, angle);
	}

	[ClientRpc]
	public void RpcSplit(GameObject obj, GameObject split, float angle) {
		//We do not call on NetworkServer methods here. This is used only to sync up with the original game unit for all clients.
		//This includes adding the newly spawned game unit into the Selection Manager that handles keeping track of all game units.
		GameUnit original = obj.GetComponent<GameUnit>();
		GameUnit copy = split.GetComponent<GameUnit>();
		Copy(original, copy);
		NavMeshAgent originalAgent = obj.GetComponent<NavMeshAgent>();
		originalAgent.ResetPath();
		NavMeshAgent copyAgent = copy.GetComponent<NavMeshAgent>();
		copyAgent.ResetPath();

		this.splitGroupList.Add(new SplitGroup(original, copy, angle));
		this.selectionManager.allObjects.Add(split);
	}

	private void Copy(GameUnit original, GameUnit copy) {
		copy.isSelected = original.isSelected;
		copy.transform.position = original.transform.position;
		copy.transform.rotation = original.transform.rotation;
		copy.transform.localScale = original.transform.localScale;
		copy.oldTargetPosition = original.oldTargetPosition;
		copy.isDirected = original.isDirected = false;
		copy.level = original.level;
	}
}
