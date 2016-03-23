using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Common;
using Extension;
using MultiPlayer;
using Simulation;
using Analytics;

namespace SinglePlayer {
	public enum State {
		Idle, Move, Attack, Split, Merge, Scout, Random
	};

	public class AIUnit : BaseUnit {
		public bool isSplitting;
		public float splitFactor;
		public float mergeFactor;
		public float attackFactor;
		public float speedFactor;
		public float attackCooldownFactor;
		public float attackCooldownCounter;
		public float recoveryCounter;
		[Range(0, 100)]
		public int currentHealth;
		[Range(0, 100)]
		public int maxHealth;
		public int teamColorValue;
		public int level;
		public int previousLevel;
		public State currentState;
		public EnumTeam teamFaction;
		public AIManager unitManager;
		public AILineOfSight lineOfSight;
		public AIAttackRange attackRange;
		public SimulationManager simulationManager;

		private float splitCounter;
		private float mergeCounter;
		private NavMeshAgent agent;
		private Rect minimapCameraRect;
		private Vector3 healthViewportPosition;
		private Color takeDamageColor = Color.red;
		private Color initialColor = Color.white;

		public void Start() {
			bool initialStateFlag = false;
			if (this.unitManager != null) {
				AIAttributeManager aiAttributeManager = this.unitManager.aiAttributeManager;
				if (aiAttributeManager.tiers != null) {
					TierUpgrade firstTier = aiAttributeManager.tiers[0];
					this.maxHealth = (int) (firstTier.health);
					this.currentHealth = this.maxHealth;
					this.attackFactor = firstTier.attack;
					this.attackCooldownFactor = firstTier.attackCooldown;
					this.speedFactor = firstTier.speed;
					this.splitFactor = firstTier.split;
					this.mergeFactor = firstTier.merge;

					initialStateFlag = true;
				}
			}

			this.currentState = State.Idle;
			this.level = 1;
			this.previousLevel = 1;
			this.isSplitting = false;
			this.splitCounter = 0f;
			this.mergeCounter = 0f;
			this.attackCooldownCounter = 1f;
			this.recoveryCounter = 1f;

			if (!initialStateFlag) {
				this.speedFactor = 1f;
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
			}
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
				if (!initialStateFlag) {
					this.agent.speed = this.speedFactor;
				}
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

			//Check if targetEnemy is within sight range.
			if (this.targetEnemy != null) {
				if (this.lineOfSight != null) {
					this.lineOfSight.sphereColliderRigidBody.WakeUp();
					if (!this.lineOfSight.enemies.Contains(this.targetEnemy.gameObject)) {
						this.targetEnemy = null;
						this.currentState = State.Idle;
						this.lineOfSight.Clean();
					}
				}
				else if (this.attackRange != null) {
					this.attackRange.sphereColliderRigidBody.WakeUp();
					if (!this.attackRange.enemies.Contains(this.targetEnemy.gameObject)) {
						this.targetEnemy = null;
						this.currentState = State.Idle;
						this.attackRange.Clean();
					}
				}
			}

			if (this.targetEnemy == null && this.currentState != State.Split && this.currentState != State.Merge) {
				if (this.lineOfSight != null) {
					this.lineOfSight.sphereColliderRigidBody.WakeUp();
					if (this.lineOfSight.enemies.Count > 0) {
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
			}

			switch (this.currentState) {
				default:
				case State.Idle:
					if (this.targetEnemy != null) {
						this.currentState = State.Attack;
						break;
					}
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
						//EnumTeam index value is constant: Player = 0, Computer = 1
						SimulationMetricsLogger.Increment(GameMetricOptions.Splits, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
					}
					break;
				case State.Scout:
					if (this.targetEnemy != null) {
						this.currentState = State.Attack;
						break;
					}
					if (this.agent != null) {
						if (this.agent.ReachedDestination()) {
							this.targetEnemy = null;
							this.currentState = State.Idle;
						}
					}
					break;
				case State.Merge:
					if (this.targetEnemy != null) {
						this.currentState = State.Attack;
						break;
					}
					if (this.mergeCounter > 0f) {
						this.mergeCounter -= Time.deltaTime / this.mergeFactor;
					}
					else {
						this.currentState = State.Idle;
						//EnumTeam index value is constant: Player = 0, Computer = 1
						SimulationMetricsLogger.Increment(GameMetricOptions.Merges, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
					}
					break;
				case State.Attack:
					//Attack logic.
					if (this.attackCooldownCounter < 0f) {
						if (this.lineOfSight != null && this.lineOfSight.enemies.Count > 0 && this.attackRange != null && this.attackRange.enemies.Count > 0) {
							for (int i = 0; i < this.attackRange.enemies.Count; i++) {
								GameObject enemy = this.attackRange.enemies[i];
								if (enemy != null) {
									AIUnit aiUnit = enemy.GetComponentInParent<AIUnit>() as AIUnit;
									GameUnit playerUnit = enemy.GetComponent<GameUnit>() as GameUnit;
									if (aiUnit != null && aiUnit.teamFaction != this.teamFaction) {
										this.attackCooldownCounter = 1f;
										this.targetEnemy = aiUnit;
										aiUnit.TakeDamage(this.attackFactor);
										break;
									}
									if (playerUnit != null && playerUnit.teamFaction != this.teamFaction) {
										this.attackCooldownCounter = 1f;
										this.targetEnemy = playerUnit;
										playerUnit.CmdTakeDamage(playerUnit.gameObject, playerUnit.currentHealth - 1);
										break;
									}

									//EnumTeam index value is constant: Player = 0, Computer = 1
									SimulationMetricsLogger.Increment(GameMetricOptions.Attacks, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
									SimulationMetricsLogger.Increment(GameMetricOptions.AttackTime, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
								}
							}
						}
					}
					//EnumTeam index value is constant: Player = 0, Computer = 1
					SimulationMetricsLogger.Increment(GameMetricOptions.BattleEngagementTime, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
					break;
			}

			if (this.recoveryCounter < 1f) {
				this.recoveryCounter += Time.deltaTime;
			}
			Renderer renderer = this.GetComponent<Renderer>();
			if (renderer != null) {
				renderer.material.color = Color.Lerp(this.takeDamageColor, this.initialColor, this.recoveryCounter);
			}
		}

		public void OnGUI() {
			if (SimulationMetricsLogger.instance != null) {
				if (SimulationMetricsLogger.instance.isShownToScreen) {
					return;
				}
			}

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
			this.recoveryCounter = 0f;
			if (this.currentHealth <= 0) {
				MonoBehaviour.Destroy(this.gameObject);
				//EnumTeam index value is constant: Player = 0, Computer = 1
				SimulationMetricsLogger.Increment(GameMetricOptions.Death, (this.teamFaction == EnumTeam.Player) ? 0 : 1);
				SimulationMetricsLogger.Increment(GameMetricOptions.Kills, (this.teamFaction != EnumTeam.Player) ? 0 : 1);
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
						customMaterial.SetColor("_TeamColor", new Color(220f/255f, 220f/255f, 0f));
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
