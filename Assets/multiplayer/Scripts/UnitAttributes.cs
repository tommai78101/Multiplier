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
								this.healthPrefabList.Add((float) (1 + j));
								break;
							case 1:
								this.attackPrefabList.Add((float) (1 + j));
								break;
							case 2:
								this.speedPrefabList.Add((float) 3f / (1 + j));
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
		//Debug.Log("Sending to server to update values.");
		NetworkIdentity identity = this.GetComponent<NetworkIdentity>();
		if (identity != null) {
			RpcUpdateAnswer(answer, level, propertyValue, identity.netId);
		}
		//Debug.Log("I finished sending answers.");
	}

	[ClientRpc]
	public void RpcUpdateAnswer(float answer, int level, int propertyValue, NetworkInstanceId id) {
		//Debug.Log("I'm updating answers.");
		GameObject obj = ClientScene.FindLocalObject(id);
		UnitAttributes attr = obj.GetComponent<UnitAttributes>();
		if (attr != null) {
			switch (propertyValue) {
				default:
				case 0:
					attr.healthPrefabList[level] = answer;
					break;
				case 1:
					attr.attackPrefabList[level] = answer;
					break;
				case 2:
					attr.speedPrefabList[level] = answer;
					break;
				case 3:
					attr.mergePrefabList[level] = answer;
					break;
				case 4:
					attr.attackCooldownPrefabList[level] = answer;
					break;
				case 5:
					attr.splitPrefabFactor = answer;
					break;
			}
		}
		//Debug.Log("I finished updating answers.");
	}
}


