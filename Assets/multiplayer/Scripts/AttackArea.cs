using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using MultiPlayer;
using SinglePlayer;
using Common;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.
	public List<BaseUnit> enemiesInAttackRange;
	public List<BaseUnit> removeList;
	public List<BaseUnit> exitedList;
	public List<BaseUnit> otherEnemies;
	public List<BaseUnit> exitedEnemies;
	public GameUnit parent;
	public Rigidbody sphereColliderRigidBody;

	public void Start() {
		this.enemiesInAttackRange = new List<BaseUnit>();
		this.removeList = new List<BaseUnit>();
		this.exitedList = new List<BaseUnit>();
		this.parent = this.GetComponentInParent<GameUnit>();
		this.sphereColliderRigidBody = this.GetComponent<Rigidbody>();
		this.otherEnemies = new List<BaseUnit>();
	}

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null) {
			Renderer unitRenderer = unit.GetComponent<Renderer>();
			if (!unit.hasAuthority && unitRenderer.enabled && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
				this.enemiesInAttackRange.Add(unit);
			}
		}

		AIUnit AIunit = other.GetComponent<AIUnit>();
		if (AIunit != null && !this.otherEnemies.Contains(AIunit)) {
			this.otherEnemies.Add(AIunit);
		}

		if (this.exitedList.Count > 0) {
			this.exitedList.Clear();
		}
		if (this.exitedEnemies.Count > 0) {
			this.exitedEnemies.Clear();
		}
	}

	public void OnTriggerExit(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null) {
			Renderer unitRenderer = unit.GetComponent<Renderer>();
			if ((!unit.hasAuthority && this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) || (!unitRenderer.enabled && this.enemiesInAttackRange.Contains(unit))) {
				this.enemiesInAttackRange.Remove(unit);
				this.exitedList.Add(unit);
			}
		}
		else {
			AIUnit AIunit = other.GetComponent<AIUnit>();
			if (AIunit != null && this.otherEnemies.Contains(AIunit)) {
				this.otherEnemies.Remove(AIunit);
			}
		}
	}

	//public void OnTriggerStay(Collider other) {
	//	GameUnit unit = other.GetComponent<GameUnit>();
	//	if (unit != null && !unit.hasAuthority && unit.CheckIfVisible() && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent) && !this.exitedList.Contains(unit)) {
	//		this.enemiesInAttackRange.Add(unit);
	//	}
	//}

	public void FixedUpdate() {
		this.sphereColliderRigidBody.WakeUp();

		if (this.enemiesInAttackRange.Count > 0) {
			foreach (GameUnit unit in this.enemiesInAttackRange) {
				if (unit == null) {
					this.removeList.Add(unit);
				}
			}
		}

		if (this.otherEnemies.Count > 0) {
			for (int i = 0; i < this.otherEnemies.Count; i++) {
				if (this.otherEnemies[i] == null) {
					this.otherEnemies.RemoveAt(i);
				}
			}
		}

		if (this.removeList.Count > 0) {
			foreach (GameUnit unit in this.removeList) {
				if (this.enemiesInAttackRange.Contains(unit)) {
					this.enemiesInAttackRange.Remove(unit);
				}
			}
			this.removeList.Clear();
		}
	}
}
