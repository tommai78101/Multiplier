using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class AttackArea : MonoBehaviour {
	//Same design pattern as LineOfSight class.
	public List<GameUnit> enemiesInAttackRange;
	public List<GameUnit> removeList;
	public List<GameUnit> exitedList;
	public GameUnit parent;

	public void Start() {
		this.enemiesInAttackRange = new List<GameUnit>();
		this.removeList = new List<GameUnit>();
		this.exitedList = new List<GameUnit>();
		this.parent = this.GetComponentInParent<GameUnit>();
	}

	public void OnTriggerEnter(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && unit.CheckIfVisible() && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent)) {
			this.enemiesInAttackRange.Add(unit);
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
	}

	public void OnTriggerStay(Collider other) {
		GameUnit unit = other.GetComponent<GameUnit>();
		if (unit != null && !unit.hasAuthority && unit.CheckIfVisible() && !this.enemiesInAttackRange.Contains(unit) && !unit.Equals(this.parent) && !this.exitedList.Contains(unit)) {
			this.enemiesInAttackRange.Add(unit);
		}
	}

	public void FixedUpdate() {
		if (this.enemiesInAttackRange.Count > 0) {
			foreach (GameUnit unit in this.enemiesInAttackRange) {
				if (unit == null) {
					this.removeList.Add(unit);
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

		if (this.exitedList.Count > 0) {
			this.exitedList.Clear();
		}
	}
}
