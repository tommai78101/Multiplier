using UnityEngine;
using System.Collections.Generic;
using System;
using Common;

namespace SinglePlayer {
	public enum State {
		Idle, Move, Attack, Split, Merge, Patrol, Random
	};

	public class AIUnit : MonoBehaviour {
		public State currentState;
		public float splitFactor;
		public AIManager unitManager;
		public int level;
		public int previousLevel;

		public void Start() {
			this.currentState = State.Idle;
			this.level = 1;
			this.previousLevel = 1;
			if (this.splitFactor == 0f) {
				this.splitFactor = 1f;
			}
		}

		public void Update() {
			switch (this.currentState) {
				default:
				case State.Idle:
					break;
				case State.Split:
					this.currentState = State.Idle;
					break;
			}
		}

		public void Tick() {
			switch (this.currentState) {
				default:
				case State.Idle:
					break;
				case State.Split:
					break;
			}
		}

		public GameObject Split(GameObject AIUnitPrefab) {
			if (this.level != 1) {
				return null;
			}
			if (this.currentState != State.Split) {
				//Setting up Split.
				this.currentState = State.Split;

				GameObject obj = MonoBehaviour.Instantiate(AIUnitPrefab) as GameObject;
				if (obj != null) {
					obj.transform.SetParent(this.transform.parent);
					obj.transform.position = this.transform.position;
					AIUnit unit = obj.GetComponent<AIUnit>();
					if (unit != null) {
						unit.Copy(this);
					}
				}
				Debug.Log("Splitting units.");
				return obj;
			}
			return null;
		}

		public void Copy(AIUnit original) {
			this.currentState = original.currentState;
			this.splitFactor = original.splitFactor;
			this.unitManager = original.unitManager;
		}
	}
}
