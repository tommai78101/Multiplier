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
		public GameObject attributeManager;
		public Difficulty difficulty;
		public FSMState currentFiniteState;
		public GameObject AIUnitPrefab;
		[Range(15, 50)]
		public int maxUnitCount;


		private float tickIntervalCounter;

		public void Start() {
			this.startAIFlag = false;
			this.unitCount = 0;
			if (this.maxUnitCount <= 0) {
				this.maxUnitCount = 50;
			}
			switch (this.difficulty) {
				case Difficulty.Easy:
					tickIntervals = 5f;
					break;
				case Difficulty.Normal:
					tickIntervals = 3f;
					break;
				case Difficulty.Hard:
					tickIntervals = 1f;
					break;
			}
			this.tickIntervalCounter = this.tickIntervals;
			this.currentFiniteState = FSMState.Wait;
			this.removeUnitList = new List<AIUnit>();
			this.selectedUnits = new List<AIUnit>();
			this.spawnList = new List<AIUnit>();
			this.splitGroupList = new List<SplitGroup>();
			this.mergeGroupList = new List<MergeGroup>();
		}

		public void Update() {
			if (this.splitGroupList.Count > 0) {
				for (int i = 0; i < this.splitGroupList.Count; i++) {
					SplitGroup group = this.splitGroupList[i];
					if (group.elapsedTime > 1f) {
						this.splitGroupList.RemoveAt(i);
						i--;
					}
					else {
						group.Update();
						group.elapsedTime += Time.deltaTime;
						this.splitGroupList[i] = group;
					}
				}
			}
			if (this.mergeGroupList.Count > 0) {
				for (int i = 0; i < this.mergeGroupList.Count; i++) {
					MergeGroup group = this.mergeGroupList[i];
					if (group.elapsedTime > 1f) {
						AIUnit unit = group.owner.GetComponent<AIUnit>();
						unit.previousLevel = unit.level;
						unit.level++;
						this.removeUnitList.Add(group.merge.GetComponent<AIUnit>());
						MonoBehaviour.Destroy(group.merge);
						this.mergeGroupList.RemoveAt(i);
						i--;
					}
					else {
						group.Update();
						group.elapsedTime += Time.deltaTime;
						this.mergeGroupList[i] = group;
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

		public void Activate() {
			this.startAIFlag = true;
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

					//INFLEXIBLE BUILD ORDER
					if (this.unitCount >= 8 && this.unitCount <= this.maxUnitCount) {
						//AI player is ready to merge.
						int chosen = Mathf.FloorToInt(Random.value * this.unitCount) + 1;
						int halfChosen = 0;
						if (chosen > 8) {
							halfChosen = chosen / 2;
						}
						int remaining = this.unitCount;
						while (chosen > 0 && halfChosen > 0) {
							int randomIndex = Mathf.FloorToInt(Random.value * this.unitCount);
							Transform child = this.unitContainer.transform.GetChild(randomIndex);
							this.mergeList.Add(child.GetComponent<AIUnit>());
							chosen--;
							halfChosen--;
							remaining--;
						}
						while (chosen > 0 && halfChosen <= 0) {
							int randomIndex = Mathf.FloorToInt(Random.value * this.unitCount);
							Transform child = this.unitContainer.transform.GetChild(randomIndex);
							this.selectedUnits.Add(child.GetComponent<AIUnit>());
							chosen--;
							remaining--;
						}
						if (remaining > 0) {
							foreach (Transform child in this.unitContainer.transform) {
								if (child != null) {
									AIUnit unit = child.GetComponent<AIUnit>();
									if (!this.selectedUnits.Contains(unit)) {
										this.spawnList.Add(unit);
									}
								}
							}
						}
						this.currentFiniteState = FSMState.Merge;
					}
					else if (this.unitCount >= 4 && this.unitCount < 8) {
						//AI is ready to scout.
						int chosen = Mathf.FloorToInt(Random.value * this.unitCount) + 1;
						int remaining = this.unitCount;
						while (chosen > 0) {
							int randomIndex = Mathf.FloorToInt(Random.value * this.unitCount);
							Transform child = this.unitContainer.transform.GetChild(randomIndex);
							this.selectedUnits.Add(child.GetComponent<AIUnit>());
							chosen--;
							remaining--;
						}
						if (remaining > 0) {
							foreach (Transform child in this.unitContainer.transform) {
								if (child != null) {
									AIUnit unit = child.GetComponent<AIUnit>();
									if (!this.selectedUnits.Contains(unit)) {
										this.spawnList.Add(unit);
									}
								}
							}
						}
						this.currentFiniteState = FSMState.Scout;
					}
					else if (this.unitCount > 1 && this.unitCount < 4) {
						//AI is getting ready. Build to 4 units
						foreach (Transform child in this.unitContainer.transform) {
							AIUnit unit = child.GetComponent<AIUnit>();
							if (unit != null) {
								this.spawnList.Add(unit);
							}
						}
						this.currentFiniteState = FSMState.Split;
					}
					else if (this.unitCount <= 1) {
						//Usually at the start of the game, or when the AI player is on the brink of defeat.
						Transform child = this.unitContainer.transform.GetChild(this.unitContainer.transform.childCount - 1);
						AIUnit unit = child.GetComponent<AIUnit>();

						//TODO: Refer to an attribute manager for AI units on what attributes are required.

						//Always select the unit before doing other AI state machines.
						this.spawnList.Add(unit);
						this.currentFiniteState = FSMState.Split;
					}
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
													this.mergeGroupList.Add(new MergeGroup(owner.gameObject, merge.gameObject, 2f));
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