﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Common;
using Extension;
using MultiPlayer;

namespace SinglePlayer {
	public enum State {
		Idle, Move, Attack, Split, Merge, Scout, Random
	};

	public class AIUnit : BaseUnit {
		public BaseUnit targetUnit;
		public State currentState;
		public float splitFactor;
		public float mergeFactor;
		public float attackFactor;
		[Range(3, 100)]
		public float attackCooldownFactor;
		public float attackCooldownCounter;
		public int currentHealth;
		[Range(3, 100)]
		public int maxHealth;
		public bool isSplitting;
		public int teamColorValue;
		public EnumTeam teamFaction;
		public AIManager unitManager;
		public AILineOfSight lineOfSight;
		public AIAttackRange attackRange;
		public int level;
		public int previousLevel;

		private float splitCounter;
		private float mergeCounter;
		private NavMeshAgent agent;
		private Rect minimapCameraRect;
		private Vector3 healthViewportPosition;

		public void Start() {
			this.currentState = State.Idle;
			this.level = 1;
			this.previousLevel = 1;
			this.isSplitting = false;
			this.splitCounter = 0f;
			this.mergeCounter = 0f;
			this.attackCooldownCounter = 1f;
			if (this.attackFactor == 0f) {
				this.attackFactor = 1f;
			}
			if (this.splitFactor == 0f) {
				this.splitFactor = 1f;
			}
			if (this.mergeFactor == 0f) {
				this.mergeFactor = 1f;
			}
			if (this.attackCooldownFactor == 0f) {
				this.attackCooldownFactor = 1f;
			}
			this.currentHealth = this.maxHealth;
			if (this.lineOfSight == null) {
				this.lineOfSight = this.GetComponentInChildren<AILineOfSight>();
				if (this.lineOfSight == null) {
					Debug.LogError("Cannot find Line of Sight component for the AI unit.");
				}
			}
			if (this.attackRange == null) {
				this.attackRange = this.GetComponentInChildren<AIAttackRange>();
				if (this.attackRange == null) {
					Debug.LogError("Cannot find Attack Range component for the AI unit.");
				}
			}
			this.agent = this.GetComponent<NavMeshAgent>();
			if (this.agent != null) {
				this.agent.stoppingDistance = 1.5f;
			}

			GameObject minimapObject = GameObject.FindGameObjectWithTag("Minimap");
			if (minimapObject != null) {
				Camera camera = minimapObject.GetComponent<Camera>();
				this.minimapCameraRect = camera.rect;
			}
			else {
				minimapObject = GameObject.FindGameObjectWithTag("Menu");
				if (minimapObject != null) {
					Image image = minimapObject.GetComponent<Image>();
					this.minimapCameraRect = image.rectTransform.rect;
				}
			}
		}

		public void Update() {
			if (this.attackCooldownCounter >= 0f) {
				this.attackCooldownCounter -= Time.deltaTime / this.attackCooldownFactor;
			}

			if (this.targetUnit == null && this.currentState != State.Split && this.currentState != State.Merge) {
				this.lineOfSight.sphereColliderRigidBody.WakeUp();
				if (this.lineOfSight != null && this.lineOfSight.enemies.Count > 0) {
					for (int i = 0; i < this.lineOfSight.enemies.Count; i++) {
						GameObject enemy = this.lineOfSight.enemies[i];
						if (enemy != null) {
							AIUnit aiUnit = enemy.GetComponentInParent<AIUnit>();
							GameUnit playerUnit = enemy.GetComponent<GameUnit>();
							if (aiUnit != null || playerUnit != null) {
								if (aiUnit != null && aiUnit.teamFaction != this.teamFaction) {
									if (this.agent != null) {
										this.agent.SetDestination(aiUnit.transform.position);
									}
									this.currentState = State.Attack;
									break;
								}
								else if (playerUnit != null && playerUnit.teamFaction != this.teamFaction) {
									if (this.agent != null) {
										this.agent.SetDestination(playerUnit.transform.position);
									}
									this.currentState = State.Attack;
									break;
								}
							}
						}
					}
				}
			}

			switch (this.currentState) {
				default:
				case State.Idle:
					if (this.agent != null) {
						if (!this.agent.ReachedDestination()) {
							this.agent.ResetPath();
						}
					}
					break;
				case State.Split:
					if (this.splitCounter > 0f) {
						this.splitCounter -= Time.deltaTime / this.splitFactor;
					}
					else {
						this.currentState = State.Idle;
						if (this.agent != null) {
							this.agent.ResetPath();
						}
					}
					break;
				case State.Scout:
					if (this.agent != null) {
						if (this.agent.ReachedDestination()) {
							this.targetUnit = null;
							this.currentState = State.Idle;
						}
					}
					break;
				case State.Merge:
					if (this.mergeCounter > 0f) {
						this.mergeCounter -= Time.deltaTime / this.mergeFactor;
					}
					else {
						this.currentState = State.Idle;
					}
					break;
				case State.Attack:
					//Attack logic.
					if (this.attackCooldownCounter < 0f) {
						if (this.lineOfSight != null && this.lineOfSight.enemies.Count > 0 && this.attackRange != null && this.attackRange.enemies.Count > 0) {
							for (int i = 0; i < this.attackRange.enemies.Count; i++) {
								GameObject enemy = this.attackRange.enemies[i];
								if (enemy != null) {
									AIUnit aiUnit = enemy.GetComponentInParent<AIUnit>();
									GameUnit playerUnit = enemy.GetComponent<GameUnit>();
									if (aiUnit != null || playerUnit != null) {
										if (aiUnit != null && aiUnit.teamFaction != this.teamFaction) {
											this.targetUnit = aiUnit;
											aiUnit.TakeDamage(this.attackFactor);
											this.attackCooldownCounter = 1f;
											break;
										}
										else if (playerUnit != null && playerUnit.teamFaction != this.teamFaction) {
											this.targetUnit = playerUnit;
											playerUnit.CmdTakeDamage(playerUnit.currentHealth - 1);
											this.attackCooldownCounter = 1f;
											break;
										}
									}
								}
							}
						}
					}
					//if (this.targetUnit != null) {
					//	AIUnit aiUnit = this.targetUnit as AIUnit;
					//	if (aiUnit != null && aiUnit.teamFaction != this.teamFaction) {
					//		aiUnit.TakeDamage(this.attackFactor);
					//		this.attackCooldownCounter = 1f;
					//	}
					//	else {
					//		GameUnit unit = this.targetUnit as GameUnit;
					//		if (unit != null && unit.teamFaction != this.teamFaction) {
					//			unit.CmdTakeDamage(unit.currentHealth - 1);
					//			this.attackCooldownCounter = 1f;
					//		}
					//		else {
					//			this.targetUnit = null;
					//		}
					//	}
					//	if (this.agent != null) {
					//		this.agent.SetDestination(this.targetUnit.transform.position);
					//	}
					//}
					break;
			}
		}

		public void OnGUI() {
			GUIStyle style = new GUIStyle();
			style.normal.textColor = Color.black;
			style.alignment = TextAnchor.MiddleCenter;
			Vector3 healthPosition = Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
			this.healthViewportPosition = Camera.main.ScreenToViewportPoint(new Vector3(healthPosition.x, healthPosition.y + 30f));
			if (!this.minimapCameraRect.Contains(this.healthViewportPosition)) {
				Rect healthRect = new Rect(healthPosition.x - 50f, (Screen.height - healthPosition.y) - 45f, 100f, 25f);
				GUI.Label(healthRect, this.currentHealth.ToString() + "/" + this.maxHealth.ToString(), style);
			}
		}

		public void Tick() {
			switch (this.currentState) {
				default:
					break;
				case State.Attack:
					break;
			}
		}

		public void SetSplitFlag() {
			if (this.level != 1) {
				this.currentState = State.Idle;
				this.splitCounter = 0f;
				return;
			}
			if (this.currentState != State.Split) {
				//Setting up Split.
				this.currentState = State.Split;
				this.splitCounter = 1f;
			}
		}

		public void SetScoutFlag() {
			if (this.currentState != State.Scout) {
				if (this.agent != null) {
					GameObject floor = GameObject.FindGameObjectWithTag("Floor");
					if (floor != null) {
						Vector3 randomPosition = floor.GetRandomPosition();
						this.agent.SetDestination(randomPosition);
					}
				}
				this.currentState = State.Scout;
			}
		}

		public bool CheckMergeFlag() {
			return this.currentState == State.Idle;
		}

		public void SetMergeFlag() {
			this.currentState = State.Merge;
			this.mergeCounter = 1f;
		}

		public void Copy(AIUnit original) {
			this.currentState = original.currentState;
			this.splitFactor = original.splitFactor;
			this.mergeFactor = original.mergeFactor;
			this.attackCooldownFactor = original.attackCooldownFactor;
			this.attackFactor = original.attackFactor;
			this.unitManager = original.unitManager;
			this.currentHealth = original.currentHealth;
			this.maxHealth = original.maxHealth;
			this.splitCounter = original.splitCounter;
			this.minimapCameraRect = original.minimapCameraRect;
			this.teamFaction = original.teamFaction;

			this.SetTeamColor(original.teamColorValue);

			AILineOfSight myLOS = this.GetComponentInChildren<AILineOfSight>();
			AILineOfSight originalLOS = original.GetComponentInChildren<AILineOfSight>();
			AIAttackRange myAR = this.GetComponentInChildren<AIAttackRange>();
			AIAttackRange originalAR = original.GetComponentInChildren<AIAttackRange>();

			myLOS.teamFaction = originalLOS.teamFaction;
			myAR.teamFaction = originalAR.teamFaction;
		}

		public void TakeDamage(float damageAmount) {
			this.currentHealth -= Mathf.FloorToInt(damageAmount);
			if (this.currentHealth <= 0) {
				MonoBehaviour.Destroy(this.gameObject);
			}
		}

		//Sets the team color.
		public void SetTeamColor(int colorValue) {
			this.teamColorValue = colorValue;

			Renderer playerRenderer = this.GetComponent<Renderer>();
			if (playerRenderer != null) {
				Material customMaterial = playerRenderer.material;
				switch (colorValue) {
					default:
						customMaterial.SetColor("_TeamColor", Color.gray);
						break;
					case 0:
						customMaterial.SetColor("_TeamColor", Color.red);
						break;
					case 1:
						customMaterial.SetColor("_TeamColor", Color.blue);
						break;
					case 2:
						customMaterial.SetColor("_TeamColor", Color.green);
						break;
				}
			}
		}

		public void SetTeam(EnumTeam teamFaction) {
			this.teamFaction = teamFaction;
			switch (teamFaction) {
				case EnumTeam.Player:
					this.SetTeamColor(0);
					break;
				case EnumTeam.Computer:
					this.SetTeamColor(-1);
					break;
				case EnumTeam.Neutral:
					this.SetTeamColor((int) UnityEngine.Random.Range(0, 3));
					break;
			}
		}
	}
}