﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using Common;

namespace MultiPlayer {
	public class UnitAttributes : NetworkBehaviour {
		public List<float> healthPrefabList = new List<float>(10);
		public List<float> attackPrefabList = new List<float>(10);
		public List<float> speedPrefabList = new List<float>(10);
		public List<float> mergePrefabList = new List<float>(10);
		public List<float> attackCooldownPrefabList = new List<float>(10);
		public float splitPrefabFactor;
		public int maxLevelCount;

		public void Awake() {
			Initialize();
		}

		//public override void OnStartClient() {
		//	base.OnStartClient();
		//	Initialize();
		//}

		public void Initialize() {
			for (int i = 0; i < 6; i++) {
				for (int j = 0; j < Attributes.MAX_NUM_OF_LEVELS; j++) {
					switch (i) {
						case 0:
							this.healthPrefabList.Add((float)(1 + j));
							break;
						case 1:
							this.attackPrefabList.Add((float)(1 + j));
							break;
						case 2:
							this.speedPrefabList.Add((float)3f / (1 + j));
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


		public void SetHealthAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			if (this.healthPrefabList.Count > 0) {
				this.healthPrefabList.Clear();
			}
			float previousAnswer = 0f;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.Health, i + 1, i, previousAnswer);
				this.healthPrefabList.Add(answer);
				previousAnswer = answer;
			}
		}

		public void SetSpeedAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			if (this.speedPrefabList.Count > 0) {
				this.speedPrefabList.Clear();
			}
			float previousAnswer = 0f;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.Speed, i + 1, i, previousAnswer);
				this.speedPrefabList.Add(answer);
				previousAnswer = answer;
			}
		}

		public void SetAttackAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			if (this.attackPrefabList.Count > 0) {
				this.attackPrefabList.Clear();
			}
			float previousAnswer = 0f;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.Attack, i + 1, i, previousAnswer);
				this.attackPrefabList.Add(answer);
				previousAnswer = answer;
			}
		}

		public void SetSplitAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.Split, 1, 0, 0f);
			this.splitPrefabFactor = answer;
		}

		public void SetMergeAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			if (this.mergePrefabList.Count > 0) {
				this.mergePrefabList.Clear();
			}
			float previousAnswer = 0f;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.Merge, i + 1, i, previousAnswer);
				this.mergePrefabList.Add(answer);
				previousAnswer = answer;
			}
		}

		public void SetAttackCooldownAttributes(string mathExpression) {
			if (mathExpression.Equals("") || mathExpression.Length <= 0) {
				return;
			}
			if (this.attackCooldownPrefabList.Count > 0) {
				this.attackCooldownPrefabList.Clear();
			}
			float previousAnswer = 0f;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				float answer = (float)MathParser.ProcessEquation(mathExpression, AttributeProperty.AttackCooldown, i + 1, i, previousAnswer);
				this.attackCooldownPrefabList.Add(answer);
				previousAnswer = answer;
			}
		}

		public void CopyFrom(UnitAttributes tempAttr) {
			this.maxLevelCount = tempAttr.maxLevelCount;
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				this.healthPrefabList.Add(tempAttr.healthPrefabList[i]);
				this.attackPrefabList.Add(tempAttr.attackPrefabList[i]);
				this.attackCooldownPrefabList.Add(tempAttr.attackCooldownPrefabList[i]);
				this.speedPrefabList.Add(tempAttr.speedPrefabList[i]);
				this.mergePrefabList.Add(tempAttr.mergePrefabList[i]);
			}
			this.splitPrefabFactor = tempAttr.splitPrefabFactor;
		}
	}
}


