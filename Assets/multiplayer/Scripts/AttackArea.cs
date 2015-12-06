using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using MultiPlayer;
using SinglePlayer;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.
	public List<GameUnit> enemiesInAttackRange;
	public List<GameUnit> removeList;
	public List<GameUnit> exitedList;
	public List<AIUnit> otherEnemies;
	public List<AIUnit> exitedEnemies;
	public GameUnit parent;
	public Rigidbody sphereColliderRigidBody;

	public void Start() {
		this.enemiesInAttackRange = new List<GameUnit>();
		this.removeList = new List<GameUnit>();
		this.exitedList = new List<GameUnit>();
		this.parent = this.GetComponentInParent<GameUnit>();
		this.sphereColliderRigidBody = this.GetComponent<Rigidbody>();
		this.otherEnemies = new List<AIUnit>();
	}

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && unit.CheckIfVisible() && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
			this.enemiesInAttackRange.Add(unit);
		}
		else {
			AIUnit AIunit = other.GetComponent<AIUnit>();
			if (AIunit != null && !this.otherEnemies.Contains(AIunit)) {
				this.otherEnemies.Add(AIunit);
			}
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
			if ((!unit.hasAuthority && this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) || (!unit.CheckIfVisible() && this.enemiesInAttackRange.Contains(unit))) {
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
