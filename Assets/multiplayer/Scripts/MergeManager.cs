using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using Analytics;

namespace MultiPlayer {
	[Serializable]
	public struct MergeGroup {
		public GameUnit ownerUnit;
		public GameUnit mergingUnit;
		public Vector3 origin;
		public Vector3 ownerPosition;
		public Vector3 mergingPosition;
		public Vector3 ownerScale;
		public Vector3 mergingScale;
		public float elapsedTime;
		public float mergeSpeedFactor;

		public MergeGroup(GameUnit ownerUnit, GameUnit mergingUnit, float mergeFactor) {
			this.ownerUnit = ownerUnit;
			this.mergingUnit = mergingUnit;
			this.elapsedTime = 0f;

			this.ownerPosition = ownerUnit.gameObject.transform.position;
			this.mergingPosition = mergingUnit.gameObject.transform.position;
			this.origin = Vector3.Lerp(this.ownerPosition, this.mergingPosition, 0.5f);
			this.ownerScale = ownerUnit.gameObject.transform.localScale;
			this.mergingScale = mergingUnit.gameObject.transform.localScale;
			this.mergeSpeedFactor = mergeFactor;

			if (ownerUnit.attributes == null) {
				Debug.LogError("Owner unit attributes are null.");
			}
			if (mergingUnit.attributes == null) {
				Debug.LogError("Merging unit attributes are null.");
			}

			this.Stop();
		}

		public void SetMergeSpeed(float value) {
			this.mergeSpeedFactor = value;
		}

		public void Update(float scaling) {
			this.ownerUnit.isSelected = false;
			this.mergingUnit.isSelected = false;

			this.Stop();

			//Merging animation. Most likely another known bug that I cannot fix.
			this.ownerUnit.gameObject.transform.position = Vector3.Lerp(this.ownerPosition, this.origin, this.elapsedTime);
			this.mergingUnit.gameObject.transform.position = Vector3.Lerp(this.mergingPosition, this.origin, this.elapsedTime);

			//Scaling animation. Same persistent bug? It might be another mysterious bug.
			Vector3 scale = Vector3.Lerp(this.ownerScale, this.ownerScale * scaling, this.elapsedTime);
			scale.y = this.ownerScale.y;
			this.ownerUnit.gameObject.transform.localScale = scale;
			scale = Vector3.Lerp(this.mergingScale, this.mergingScale * scaling, this.elapsedTime);
			scale.y = this.mergingScale.y;
			this.mergingUnit.gameObject.transform.localScale = scale;
		}

		public void Resume() {
			NavMeshAgent agent = this.ownerUnit.GetComponent<NavMeshAgent>();
			if (agent != null) {
				agent.Resume();
			}
			if (this.mergingUnit != null) {
				//Meaning that this merging unit is still merging.
				//If null, it means this is already destroyed, therefore no need to reference it anymore.
				agent = this.mergingUnit.GetComponent<NavMeshAgent>();
				if (agent != null) {
					agent.Resume();
				}
			}

			Collider collider = this.ownerUnit.GetComponent<Collider>();
			if (collider != null) {
				collider.enabled = true;
			}

			NetworkTransform transform = this.ownerUnit.GetComponent<NetworkTransform>();
			if (transform != null) {
				transform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
			}
			if (this.mergingUnit != null) {
				transform = this.mergingUnit.GetComponent<NetworkTransform>();
				if (transform != null) {
					transform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
				}
			}
		}

		public void Stop() {
			NavMeshAgent agent = this.ownerUnit.GetComponent<NavMeshAgent>();
			if (agent != null) {
				agent.Stop();
				agent.ResetPath();
			}
			agent = this.mergingUnit.GetComponent<NavMeshAgent>();
			if (agent != null) {
				agent.Stop();
				agent.ResetPath();
			}

			Collider collider = this.ownerUnit.GetComponent<Collider>();
			if (collider != null) {
				collider.enabled = false;
			}

			NetworkTransform transform = this.ownerUnit.GetComponent<NetworkTransform>();
			if (transform != null) {
				transform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncNone;
			}
			transform = this.mergingUnit.GetComponent<NetworkTransform>();
			if (transform != null) {
				transform.transformSyncMode = NetworkTransform.TransformSyncMode.SyncNone;
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
		[SerializeField]
		public UnitAttributes unitAttributes;

		[Range(0.1f, 100f)]
		public float scalingValue = 2f;
		//[Range(0.00001f, 10f)]
		//public float healthFactor = 2f;
		//[Range(0.00001f, 10f)]
		//public float speedFactor = 1f;
		//[Range(0.00001f, 10f)]
		//public float attackFactor = 2f;
		[Range(0.00001f, 10f)]
		public float attackCooldownFactor = 1f;
		//[Range(1f, 10f)]
		//public float mergeSpeedFactor = 3f;

		private bool doNotAllowMerging;


		void Start() {
			if (!this.hasAuthority) {
				return;
			}

			this.doNotAllowMerging = false;

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
			if (this.unitAttributes == null) {
				GameObject[] attributes = GameObject.FindGameObjectsWithTag("UnitAttributes");
				foreach (GameObject attribute in attributes) {
					UnitAttributes attr = attribute.GetComponent<UnitAttributes>();
					if (attr != null && attr.hasAuthority) {
						this.unitAttributes = attr;
						break;
					}
				}
				if (this.unitAttributes == null) {
					Debug.LogError("Merge Manager: Unit Attributes Tracker is null. Please check.");
				}
			}
		}

		void Update() {
			if (this.mergeList.Count > 0 || this.removeList.Count > 0) {
				UpdateMergeGroups();
			}

			//Reordering this so the common codes are at the top, and segregated codes are at the bottom.
			if (!this.hasAuthority) {
				return;
			}

			if (Input.GetKeyDown(KeyCode.D)) {
				AddMergeGroup();
			}
		}

		private void AddMergeGroup() {
			//Since merging units require the selected units count to be a multiple of 2, we need to check to make sure they are a multiple of 2.
			//Else, ignore the final selected unit.
			//Going to change this into network code, to sync up merging.
			GameObject ownerObject = null, mergerObject = null;
			List<GameObject> used = new List<GameObject>();
			for (int i = this.selectionManager.selectedObjects.Count - 1; i > 0; i--) {
				if (used.Contains(this.selectionManager.selectedObjects[i])) {
					continue;
				}
				ownerObject = this.selectionManager.selectedObjects[i];
				GameUnit ownerUnit = ownerObject.GetComponent<GameUnit>();
				for (int j = i - 1; j > 0; j--) {
					if (used.Contains(this.selectionManager.selectedObjects[j])) {
						continue;
					}
					mergerObject = this.selectionManager.selectedObjects[j];
					GameUnit mergerUnit = mergerObject.GetComponent<GameUnit>();
					this.doNotAllowMerging = false;
					if (ownerUnit.level == 1 && mergerUnit.level == 1) {
						CheckAvailableResource();
					}
					if (ownerUnit.level == mergerUnit.level && !ownerUnit.isMerging && !mergerUnit.isMerging && !this.doNotAllowMerging) {
						used.Add(this.selectionManager.selectedObjects[i]);
						used.Add(this.selectionManager.selectedObjects[j]);

						NetworkIdentity identity = this.GetComponent<NetworkIdentity>();
						if (identity != null) {
							CmdAddMerge(ownerObject, mergerObject, ownerUnit.hasAuthority, (this.isServer ? NetworkServer.FindLocalObject(identity.netId) : ClientScene.FindLocalObject(identity.netId)));
						}

						this.selectionManager.selectedObjects.RemoveAt(i);
						i--;
						this.selectionManager.selectedObjects.RemoveAt(j);
						j--;
						break;
					}
				}
			}
		}

		private void UpdateMergeGroups() {
			//This follows the same code design pattern used in Split Manager. It's a very stable way of cleaning/updating the lists
			//this manager manages.
			if (this.mergeList.Count > 0) {
				for (int i = 0; i < this.mergeList.Count; i++) {
					MergeGroup group = this.mergeList[i];
					if (group.elapsedTime > 1f) {
						group.Resume();
						if (!this.removeList.Contains(group)) {
							FinishMergeGroup(group);
							this.removeList.Add(group);

							GameMetricLogger.Increment(GameMetricOptions.Merges);
						}
					}
					else {
						group.Update(this.scalingValue);
						group.elapsedTime += Time.deltaTime / group.mergeSpeedFactor;
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
			if (group.mergingUnit == null) {
				CmdEndMerge(group.ownerUnit.gameObject, null);
			}
			else {
				CmdEndMerge(group.ownerUnit.gameObject, group.mergingUnit.gameObject);
			}
		}

		//This is not used at the moment.
		private void UpdateGroup(MergeGroup group) {
			if (group.ownerUnit.level > this.unitAttributes.healthPrefabList.Count) {
				return;
			}
			int level = group.ownerUnit.level;
			float healthFactor = this.unitAttributes.healthPrefabList[level];
			float attackFactor = this.unitAttributes.attackPrefabList[level];
			float speedFactor = this.unitAttributes.speedPrefabList[level];

			//This is where we modify the unit attributes when the merge is happening.
			//However, this should really be placed in a function call at the end of the merge, meaning
			//this should be run only once.
			group.ownerUnit.attackCooldown *= this.attackCooldownFactor;
			group.ownerUnit.maxHealth = Mathf.FloorToInt((float) group.ownerUnit.maxHealth * healthFactor);
			group.ownerUnit.currentHealth = Mathf.FloorToInt((float) group.ownerUnit.currentHealth * healthFactor);
			group.ownerUnit.attackPower *= attackFactor;

			NavMeshAgent agent = group.ownerUnit.GetComponent<NavMeshAgent>();
			if (agent != null) {
				agent.speed *= speedFactor;
			}
		}

		private void CheckAvailableResource() {
			int selectedLevelOneUnitCount = 0;
			int totalLevelOneUnitCount = 0;

			for (int i = 0; i < this.selectionManager.selectedObjects.Count; i++) {
				GameUnit unit = this.selectionManager.selectedObjects[i].GetComponent<GameUnit>();
				if (unit != null && unit.level == 1) {
					selectedLevelOneUnitCount++;
				}
			}
			for (int i = 0; i < this.selectionManager.allObjects.Count; i++) {
				GameUnit unit = this.selectionManager.allObjects[i].GetComponent<GameUnit>();
				if (unit != null && unit.level == 1 && unit.previousLevel == 1) {
					totalLevelOneUnitCount++;
				}
			}

			
			if (selectedLevelOneUnitCount <= 2) {
				if (selectedLevelOneUnitCount == totalLevelOneUnitCount || totalLevelOneUnitCount <= 2) {
					this.doNotAllowMerging = true;
				}
			}
		}

		[Command]
		public void CmdEndMerge(GameObject ownerObject, GameObject mergingObject) {
			if (mergingObject != null) {
				NetworkServer.Destroy(mergingObject);
				mergingObject = null;
			}
			RpcEndMerge(ownerObject);
		}

		[ClientRpc]
		public void RpcEndMerge(GameObject ownerObject) {
			Debug.Log("RpcEndMerge: Testing something when the merge is ending.");


			//Updating merged unit attributes.
			GameUnit unit = ownerObject.GetComponent<GameUnit>();
			if (unit != null) {
				if (unit.previousLevel == unit.level) {
					unit.level++;
					Debug.Log("MergeManager: Unit.level: " + unit.level.ToString());
					if (unit.attributes != null) {
						if (unit.attributes.healthPrefabList[unit.level] != 0f) {
							Debug.Log("MergeManager: unit.maxHealth = " + unit.attributes.healthPrefabList[unit.level] * unit.maxHealth);
							unit.maxHealth = Mathf.FloorToInt(unit.attributes.healthPrefabList[unit.level] * unit.maxHealth);
						}
						if (unit.attributes.healthPrefabList[unit.level] != 0f) {
							Debug.Log("MergeManager: unit.currentHealth = " + unit.attributes.healthPrefabList[unit.level] * unit.currentHealth);
							unit.currentHealth = Mathf.FloorToInt(unit.attributes.healthPrefabList[unit.level] * unit.currentHealth);
						}
						if (unit.attributes.attackPrefabList[unit.level] != 0f) {
							unit.attackPower *= unit.attributes.attackPrefabList[unit.level];
							Debug.Log("MergeManager: unit.attackPower = " + unit.attackPower);
						}

						if (unit.currentHealth > unit.maxHealth) {
							unit.currentHealth = unit.maxHealth;
							Debug.Log("Current Health is too big.");
						}

						NavMeshAgent agent = ownerObject.GetComponent<NavMeshAgent>();
						if (agent != null) {
							if (unit.attributes.speedPrefabList[unit.level] != 0f) {
								agent.speed *= unit.attributes.speedPrefabList[unit.level];
								agent.Resume();
								agent.ResetPath();
								unit.speed = agent.speed;
								Debug.Log("MergeManager: unit.speed = " + unit.speed);
							}
						}
					}
					else {
						Debug.LogWarning("Unit attributes should not be null before end of merging.");
					}
				}
				else {
					Debug.Log("Unit levels (Previous level): " + unit.level + " " + unit.previousLevel);
				}
				unit.isMerging = false;
			}
		}

		[Command]
		public void CmdAddMerge(GameObject ownerObject, GameObject mergingObject, bool hasAuthority, GameObject manager) {
			RpcAddMerge(ownerObject, mergingObject, hasAuthority, manager);
		}

		[ClientRpc]
		public void RpcAddMerge(GameObject ownerObject, GameObject mergingObject, bool hasAuthority, GameObject manager) {
			Debug.Log("This is triggered: " + this.hasAuthority + " " + hasAuthority);

			GameUnit ownerUnit = ownerObject.GetComponent<GameUnit>();
			GameUnit mergingUnit = mergingObject.GetComponent<GameUnit>();

			if (ownerUnit != null && mergingUnit != null) {
				//Update the previous level to be the current unit's level before the merge.
				ownerUnit.previousLevel = ownerUnit.level;
				ownerUnit.isMerging = mergingUnit.isMerging = true;
				if (ownerUnit.attributes != null) {
					float mergeSpeedFactor = ownerUnit.attributes.mergePrefabList[ownerUnit.level];
					NavMeshAgent ownerAgent = ownerObject.GetComponent<NavMeshAgent>();
					ownerAgent.Stop();
					NavMeshAgent mergingAgent = mergingObject.GetComponent<NavMeshAgent>();
					mergingAgent.Stop();

					if (manager != null) {
						Debug.Log("Manager has been found.");
						MergeManager mergeManager = manager.GetComponent<MergeManager>();
						if (mergeManager != null) {
							Debug.Log("Adding new merge group...");
							mergeManager.mergeList.Add(new MergeGroup(ownerUnit, mergingUnit, mergeSpeedFactor));
						}
					}
				}
			}
		}
	}
}