using UnityEngine;
using System.Collections.Generic;
using System;
using Common;
using Extension;

namespace SinglePlayer {
	public enum State {
		Idle, Move, Attack, Split, Merge, Scout, Random
	};

	public class AIUnit : MonoBehaviour {
		public State currentState;
		public float splitFactor;
		public float mergeFactor;
		public AIManager unitManager;
		public int level;
		public int previousLevel;
		public int currentHealth;
		[Range(3, 100)]
		public int maxHealth;
		public AILineOfSight lineOfSight;
		public AIAttackRange attackRange;
		public bool isSplitting;

		private float splitCounter;
		private float mergeCounter;
		private NavMeshAgent agent;

		public void Start() {
			this.currentState = State.Idle;
			this.level = 1;
			this.previousLevel = 1;
			this.isSplitting = false;
			this.splitCounter = 0f;
			if (this.splitFactor == 0f) {
				this.splitFactor = 1f;
			}
			if (this.mergeFactor == 0f) {
				this.mergeFactor = 1f;
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
		}

		public void Update() {
			switch (this.currentState) {
				default:
				case State.Idle:
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
			this.unitManager = original.unitManager;
			this.currentHealth = original.currentHealth;
			this.maxHealth = original.maxHealth;
			this.splitCounter = original.splitCounter;
		}
	}
}
