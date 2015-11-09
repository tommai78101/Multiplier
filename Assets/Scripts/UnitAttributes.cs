using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class UnitAttributes : NetworkBehaviour {
	public List<float> healthPrefabList;
	public List<float> attackPrefabList;
	public List<float> speedPrefabList;
	public List<float> mergePrefabList;
	public List<float> attackCooldownPrefabList;
	public float splitPrefabFactor;

	public void Awake() {
		this.healthPrefabList = new List<float>(10);
		this.attackPrefabList = new List<float>(10);
		this.speedPrefabList = new List<float>(10);
		this.mergePrefabList = new List<float>(10);
		this.attackCooldownPrefabList = new List<float>(10);

		GameObject content = GameObject.FindGameObjectWithTag("Content");
		if (content != null) {
			Attributes attr = content.GetComponent<Attributes>();
			if (attr != null) {
				for (int i = 0; i < 6; i++) {
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
								this.attackCooldownPrefabList.Add(3f);
								break;
							case 5:
								if (j <= 0) {
									this.splitPrefabFactor = 3f;
								}
								break;
						}
					}
				}
			}
		}
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
				this.attackCooldownPrefabList[level] = answer;
				break;
			case 5:
				if (level <= 0) {
					this.splitPrefabFactor = answer;
				}
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
				this.attackCooldownPrefabList[level] = answer;
				break;
			case 5:
				if (level <= 0) {
					this.splitPrefabFactor = answer;
				}
				break;
		}
	}
}


