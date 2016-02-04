using UnityEngine;
using System.Collections.Generic;
using Common;
using Extension;

namespace SinglePlayer {
	public enum Difficulty {
		Easy, Normal, Hard
	}

	public enum FSMState {
		Wait, Scout, Split, Merge, Retreat
	}

	public struct SplitGroup {
		public GameObject owner;
		public GameObject split;
		public Vector3 origin;
		public float elapsedTime;
		public Vector3 rotationVector;

		public SplitGroup(GameObject owner, GameObject split) {
			this.owner = owner;
			this.split = split;
			this.origin = (owner.transform.position + split.transform.position) / 2f;
			this.elapsedTime = 0f;

			SpawnRange range = owner.GetComponentInChildren<SpawnRange>();
			this.rotationVector = Quaternion.Euler(new Vector3(0f, Random.Range(-180f, 180f), 0f)) * (Vector3.one * range.radius);

			AIUnit ownerUnit = owner.GetComponent<AIUnit>();
			AIUnit splitUnit = split.GetComponent<AIUnit>();
			if (ownerUnit != null && splitUnit != null && ownerUnit.targetEnemy != null) {
				splitUnit.targetEnemy = ownerUnit.targetEnemy;
			}
		}

		public void Update() {
			Vector3 pos = Vector3.Lerp(this.origin, this.origin + this.rotationVector, this.elapsedTime);
			if (this.owner == null || this.owner == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.owner.transform.position = pos;
			pos = Vector3.Lerp(this.origin, this.origin - this.rotationVector, this.elapsedTime);
			if (this.split == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.split.transform.position = pos;
		}
	};

	//TODO: Continue work on the merging.
	public struct MergeGroup {
		public GameObject owner;
		public GameObject merge;
		public Vector3 origin;
		public Vector3 ownerPosition;
		public Vector3 mergePosition;
		public Vector3 ownerScale;
		public Vector3 mergeScale;
		public float elapsedTime;
		public float scaling;

		public MergeGroup(GameObject owner, GameObject merge, float scaleFactor) {
			this.owner = owner;
			this.merge = merge;
			this.ownerPosition = owner.transform.position;
			this.mergePosition = merge.transform.position;
			this.origin = (owner.transform.position + merge.transform.position) / 2f;
			this.elapsedTime = 0f;
			this.ownerScale = owner.transform.localScale;
			this.mergeScale = merge.transform.localScale;
			this.scaling = scaleFactor;
		}

		public void Update() {
			Vector3 pos = Vector3.Lerp(this.ownerPosition, this.origin, this.elapsedTime);
			if (this.owner == null || this.owner == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.owner.transform.position = pos;
			pos = Vector3.Lerp(this.mergePosition, this.origin, this.elapsedTime);
			if (this.merge == null) {
				this.elapsedTime = 1f;
				return;
			}
			this.merge.transform.position = pos;

			//Scaling animation. Same persistent bug? It might be another mysterious bug.
			Vector3 scale = Vector3.Lerp(this.ownerScale, this.ownerScale * scaling, this.elapsedTime);
			scale.y = this.ownerScale.y;
			this.owner.transform.localScale = scale;
			scale = Vector3.Lerp(this.mergeScale, this.mergeScale * scaling, this.elapsedTime);
			scale.y = this.mergeScale.y;
			this.merge.transform.localScale = scale;
		}
	};

	public class AIManager : MonoBehaviour {
		public List<AIUnit> removeUnitList;
		public List<AIUnit> selectedUnits;
		public List<AIUnit> spawnList;
		public List<AIUnit> mergeList;
		public List<SplitGroup> splitGroupList;
		public List<MergeGroup> mergeGroupList;
		public bool startAIFlag;
		public float tickIntervals;
		public int unitCount;
		public GameObject unitContainer;
		public AIAttributeManager aiAttributeManager;
		public Difficulty difficulty;
		public FSMState currentFiniteState;
		public GameObject AIUnitPrefab;
		[Range(1, 50)]
		public int maxUnitCount;
		[Range(0f, 1f)]
		public float mergeRatio;
		[Range(0f, 1f)]
		public float scoutRatio;
		[Range(0f, 1f)]
		public float splitRatio;
		public EnumTeam teamFaction;


		private float tickIntervalCounter;
		private float splitPercentage = 0f;
		private float mergePercentage = 0f;
		private float scoutPercentage = 0f;

		public void Start() {
			this.startAIFlag = false;
			this.unitCount = 0;
			if (this.maxUnitCount <= 0) {
				this.maxUnitCount = 50;
			}

			switch (this.difficulty) {
				case Difficulty.Easy:
					tickIntervals = 2f;
					break;
				case Difficulty.Normal:
					tickIntervals = 1f;
					break;
				case Difficulty.Hard:
					tickIntervals = 0.5f;
					break;
			}
			this.tickIntervalCounter = UnityEngine.Random.Range(0f, this.tickIntervals + 1f);
			this.currentFiniteState = FSMState.Wait;
			this.removeUnitList = new List<AIUnit>();
			this.selectedUnits = new List<AIUnit>();
			this.spawnList = new List<AIUnit>();
			this.splitGroupList = new List<SplitGroup>();
			this.mergeGroupList = new List<MergeGroup>();
			this.splitPercentage = 0f;
			this.mergePercentage = 0f;
			this.scoutPercentage = 0f;
		}

		public void Update() {
			if (this.splitGroupList.Count > 0) {
				for (int i = 0; i < this.splitGroupList.Count; i++) {
					SplitGroup splitGroup = this.splitGroupList[i];
					if (splitGroup.elapsedTime > 1f) {
						this.splitGroupList.RemoveAt(i);
						i--;
					}
					else {
						splitGroup.Update();
						splitGroup.elapsedTime += Time.deltaTime;
						this.splitGroupList[i] = splitGroup;
					}
				}
			}
			if (this.mergeGroupList.Count > 0) {
				for (int i = 0; i < this.mergeGroupList.Count; i++) {
					MergeGroup mergeGroup = this.mergeGroupList[i];
					if (mergeGroup.elapsedTime > 1f) {
						if (mergeGroup.owner != null) {
							AIUnit unit = mergeGroup.owner.GetComponent<AIUnit>();
							unit.previousLevel = unit.level;
							unit.level++;

							//TODO: Use the attribute manager to manage the attributes after merging.
							TierUpgrade tier = this.aiAttributeManager.tiers[unit.level];

							float temp = unit.currentHealth;
							unit.currentHealth = (int) (temp * tier.health);
							temp = unit.maxHealth;
							unit.maxHealth = (int) (temp * tier.health);

							unit.attackFactor *= tier.attack;
							unit.mergeFactor *= tier.merge;
							unit.attackCooldownFactor = tier.attackCooldown;
							unit.splitFactor = tier.split;
							unit.mergeFactor = tier.merge;
							unit.speedFactor = tier.speed;
						}
						if (mergeGroup.merge != null) {
							this.removeUnitList.Add(mergeGroup.merge.GetComponent<AIUnit>());
						}
						//MonoBehaviour.Destroy(mergeGroup.merge);
						this.mergeGroupList.RemoveAt(i);
						i--;
					}
					else {
						mergeGroup.Update();
						mergeGroup.elapsedTime += Time.deltaTime;
						this.mergeGroupList[i] = mergeGroup;
					}
				}
			}
			if (this.removeUnitList.Count > 0) {
				foreach (AIUnit unit in this.removeUnitList) {
					if (unit != null) {
						MonoBehaviour.Destroy(unit.gameObject);
					}
				}
				this.removeUnitList.Clear();
			}
		}

		public void FixedUpdate() {
			if (!this.startAIFlag) {
				return;
			}

			if (this.tickIntervalCounter > 0f) {
				this.tickIntervalCounter -= Time.deltaTime;
			}
			else {
				Tick();
				this.tickIntervalCounter = this.tickIntervals;
			}
		}

		public void OnValidate() {
			float sumRatio = this.mergeRatio + this.scoutRatio + this.splitRatio;
			if (sumRatio > 0f) {
				this.mergeRatio /= sumRatio;
				this.scoutRatio /= sumRatio;
				this.splitRatio /= sumRatio;
			}
		}

		public void Activate() {
			this.startAIFlag = true;
		}

		public void Deactivate() {
			this.startAIFlag = false;
		}

		//Actual update tick
		public void Tick() {
			this.unitCount = this.unitContainer.transform.childCount;
			switch (this.currentFiniteState) {
				case FSMState.Wait:
					if (this.selectedUnits.Count > 0) {
						this.selectedUnits.Clear();
					}

					if (this.unitCount <= 0) {
						//Defeat. No more AI units on the map.
						Debug.Log("AI Player is defeated.");
						this.startAIFlag = false;
						return;
					}

					if (this.unitCount == 1) {
						//Usually at the start of the game, or when the AI player is on the brink of defeat.
						Transform child = this.unitContainer.transform.GetChild(this.unitContainer.transform.childCount - 1);
						AIUnit unit = child.GetComponent<AIUnit>();
						AILineOfSight lineOfSight = child.GetComponentInChildren<AILineOfSight>();
						AIAttackRange attackRange = child.GetComponentInChildren<AIAttackRange>();

						//TODO: Refer to an attribute manager for AI units on what attributes are required.
						unit.teamFaction = this.teamFaction;
						lineOfSight.teamFaction = this.teamFaction;
						attackRange.teamFaction = this.teamFaction;

						//Always select the unit before doing other AI state machines.
						this.spawnList.Add(unit);
						this.currentFiniteState = FSMState.Split;
						break;
					}

					int scoutUnitCount = 0;
					int splitUnitCount = 0;
					int mergeUnitCount = 0;

					foreach (Transform child in this.unitContainer.transform) {
						if (splitUnitCount > 0) {
							splitPercentage = splitRatio / splitUnitCount;
						}
						else {
							splitPercentage = splitRatio / 1f;
						}

						if (mergeUnitCount > 0) {
							mergePercentage = mergeRatio / mergeUnitCount;
						}
						else {
							mergePercentage = mergeRatio / 1f;
						}

						if (scoutUnitCount > 0) {
							scoutPercentage = scoutRatio / scoutUnitCount;
						}
						else {
							scoutPercentage = scoutRatio / 1f;
						}

						if (this.splitPercentage > this.mergePercentage && this.splitPercentage > this.scoutPercentage) {
							this.spawnList.Add(child.GetComponent<AIUnit>());
							splitUnitCount++;
						}
						else if (this.splitPercentage > this.mergePercentage && this.splitPercentage < this.scoutPercentage) {
							this.selectedUnits.Add(child.GetComponent<AIUnit>());
							scoutUnitCount++;
						}
						else if (this.splitPercentage < this.mergePercentage && this.splitPercentage > this.scoutPercentage) {
							this.mergeList.Add(child.GetComponent<AIUnit>());
							mergeUnitCount++;
						}
						else if (this.splitPercentage < this.mergePercentage && this.splitPercentage < this.scoutPercentage) {
							if (this.mergePercentage > this.scoutPercentage) {
								this.mergeList.Add(child.GetComponent<AIUnit>());
								mergeUnitCount++;
							}
							else {
								this.selectedUnits.Add(child.GetComponent<AIUnit>());
								scoutUnitCount++;
							}
						}
					}
					this.currentFiniteState = FSMState.Merge;
					break;
				case FSMState.Split:
					if (this.spawnList.Count > 0) {
						foreach (AIUnit unit in this.spawnList) {
							if (unit != null && unit.currentState != State.Split) {
								if (this.unitCount < this.maxUnitCount) {
									GameObject splitObject = SplitUnit(unit);
									if (splitObject != null) {
										this.splitGroupList.Add(new SplitGroup(unit.gameObject, splitObject));
										this.unitCount = this.unitContainer.transform.childCount;
									}
								}
								else {
									break;
								}
							}
						}
						this.spawnList.Clear();
					}
					this.currentFiniteState = FSMState.Wait;
					break;
				case FSMState.Scout:
					Debug.Log("AI player is ready to scout.");
					if (this.selectedUnits.Count > 0) {
						for (int i = 0; i < this.selectedUnits.Count; i++) {
							if (this.selectedUnits[i] != null) {
								this.selectedUnits[i].SetScoutFlag();
							}
							else {
								this.selectedUnits.RemoveAt(i);
							}
						}
						this.selectedUnits.Clear();
					}
					if (this.spawnList.Count > 0) {
						this.currentFiniteState = FSMState.Split;
					}
					else {
						this.currentFiniteState = FSMState.Wait;
					}
					break;
				case FSMState.Merge:
					if (this.mergeList.Count > 0) {
						if (this.mergeList.Count > 1) {
							AIUnit owner = null;
							AIUnit merge = null;
							List<AIUnit> removeList = new List<AIUnit>();
							for (int i = 0; i < this.mergeList.Count - 1; i++) {
								if (this.mergeList[i] != null && !removeList.Contains(this.mergeList[i])) {
									owner = this.mergeList[i];
									if (owner.CheckMergeFlag()) {
										for (int j = i + 1; j < this.mergeList.Count; j++) {
											merge = this.mergeList[j];
											if (merge != null && !removeList.Contains(merge) && !owner.Equals(merge) && owner.level == merge.level) {
												//TODO: Once unit attribute manager is implemented for the AI player, change the 2f to the actual scaling factor.
												if (merge.CheckMergeFlag()) {
													owner.SetMergeFlag();
													merge.SetMergeFlag();
													this.mergeGroupList.Add(new MergeGroup(owner.gameObject, merge.gameObject, 1.5f));  //ScaleFactor is done here.
													removeList.Add(owner);
													removeList.Add(merge);
													break;
												}
												else {
													continue;
												}
											}
										}
									}
								}
							}
							removeList.Clear();
						}
						this.mergeList.Clear();
					}
					this.currentFiniteState = FSMState.Scout;
					break;
			}

			foreach (Transform child in this.unitContainer.transform) {
				if (child != null) {
					AIUnit unit = child.GetComponent<AIUnit>();
					if (unit != null) {
						unit.Tick();
					}
				}
			}
		}

		//------------------------------------

		private GameObject SplitUnit(AIUnit original) {
			original.SetSplitFlag();
			if (original.currentState == State.Split) {
				GameObject obj = MonoBehaviour.Instantiate(this.AIUnitPrefab) as GameObject;
				if (obj != null) {
					obj.transform.SetParent(original.transform.parent);
					obj.transform.position = original.transform.position;
					AIUnit unit = obj.GetComponent<AIUnit>();
					if (unit != null) {
						unit.Copy(original);
					}
				}
				return obj;
			}
			return null;
		}
	}
}