using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class UnitAttributes : NetworkBehaviour {
	public List<float> healthPrefabList;
	public List<float> attackPrefabList;
	public List<float> speedPrefabList;
	public List<float> mergePrefabList;
	public List<float> splitPrefabList;

	//public SyncListFloat healthPrefabSyncList;
	//public SyncListFloat attackPrefabSyncList;
	//public SyncListFloat speedPrefabSyncList;
	//public SyncListFloat mergePrefabSyncList;
	//public SyncListFloat splitPrefabSyncList;


	public void Awake() {
		this.healthPrefabList = new List<float>(10);
		this.attackPrefabList = new List<float>(10);
		this.speedPrefabList = new List<float>(10);
		this.mergePrefabList = new List<float>(10);
		this.splitPrefabList = new List<float>(10);

		//this.healthPrefabSyncList = new SyncListFloat();
		//this.attackPrefabSyncList = new SyncListFloat();
		//this.speedPrefabSyncList = new SyncListFloat();
		//this.mergePrefabSyncList = new SyncListFloat();
		//this.splitPrefabSyncList = new SyncListFloat();


		GameObject content = GameObject.FindGameObjectWithTag("Content");
		if (content != null) {
			Attributes attr = content.GetComponent<Attributes>();
			if (attr != null) {
				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < Attributes.MAX_NUM_OF_LEVELS; j++) {
						switch (i) {
							case 0:
								this.healthPrefabList.Add(1f);
								break;
							case 1:
								this.attackPrefabList.Add(1f);
								break;
							case 2:
								this.speedPrefabList.Add(1f);
								break;
							case 3:
								this.mergePrefabList.Add(3f);
								break;
							case 4:
								this.splitPrefabList.Add(3f);
								break;
						}
					}
				}
			}
		}
	}

	public void UpdateValues() {
		//this.healthPrefabSyncList.Clear();
		//foreach (float value in this.healthPrefabList) {
		//	this.healthPrefabSyncList.Add(value);
		//}

		//this.attackPrefabSyncList.Clear();
		//foreach (float value in this.healthPrefabList) {
		//	this.attackPrefabSyncList.Add(value);
		//}

		//this.speedPrefabSyncList.Clear();
		//foreach (float value in this.healthPrefabList) {
		//	this.speedPrefabSyncList.Add(value);
		//}

		//this.mergePrefabSyncList.Clear();
		//foreach (float value in this.healthPrefabList) {
		//	this.mergePrefabSyncList.Add(value);
		//}

		//this.splitPrefabSyncList.Clear();
		//foreach (float value in this.healthPrefabList) {
		//	this.splitPrefabSyncList.Add(value);
		//}
	}

	[Command]
	public void CmdUpdateAnswer(float answer, int level, int propertyValue) {
		Debug.Log("Sending to server to update values.");

		switch (propertyValue) {
			default:
			case 0:
				this.healthPrefabList[level] = answer;
				break;
			case 1:
				this.attackPrefabList[level] = answer;
				break;
			case 2:
				this.speedPrefabList[level] = answer;
				break;
			case 3:
				this.mergePrefabList[level] = answer;
				break;
			case 4:
				this.splitPrefabList[level] = answer;
				break;
		}

		RpcUpdateAnswer(answer, level, propertyValue);
	}

	[ClientRpc]
	public void RpcUpdateAnswer(float answer, int level, int propertyValue) {
		if (!this.hasAuthority) {
			return;
		}

		Debug.Log("I'm updating answers.");

		switch (propertyValue) {
			default:
			case 0:
				this.healthPrefabList[level] = answer;
				break;
			case 1:
				this.attackPrefabList[level] = answer;
				break;
			case 2:
				this.speedPrefabList[level] = answer;
				break;
			case 3:
				this.mergePrefabList[level] = answer;
				break;
			case 4:
				this.splitPrefabList[level] = answer;
				break;
		}
	}
}


