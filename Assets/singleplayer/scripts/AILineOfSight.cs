using UnityEngine;
using System.Collections.Generic;
using MultiPlayer;

namespace SinglePlayer {
	public class AILineOfSight : MonoBehaviour {
		public EnumTeam teamFaction;
		public List<GameObject> enemies;
		public Rigidbody sphereColliderRigidBody;

		public void Start() {
			this.enemies = new List<GameObject>();
		}

		public void OnTriggerEnter(Collider other) {
			AILineOfSight sight = other.GetComponent<AILineOfSight>();
			if (sight != null && sight.teamFaction != this.teamFaction) {
				this.enemies.Add(sight.gameObject);
			}
			else {
				GameUnit unit = other.GetComponent<GameUnit>();
				if (unit != null && unit.teamFaction != this.teamFaction && !this.enemies.Contains(unit.gameObject)) {
					this.enemies.Add(unit.gameObject);
				}
			}
		}

		public void OnTriggerExit(Collider other) {
			AILineOfSight sight = other.GetComponent<AILineOfSight>();
			if (sight != null && sight.teamFaction != this.teamFaction) {
				this.enemies.Remove(sight.gameObject);
			}
			else {
				GameUnit unit = other.GetComponent<GameUnit>();
				if (unit != null && unit.teamFaction != this.teamFaction && this.enemies.Contains(unit.gameObject)) {
					this.enemies.Remove(unit.gameObject);
				}
			}
		}

		public void FixedUpdate() {
			this.sphereColliderRigidBody.WakeUp();
			if (this.enemies.Count > 0) {
				for (int i = 0; i < this.enemies.Count; i++) {
					if (this.enemies[i] == null) {
						this.enemies.RemoveAt(i);
					}
				}
			}
		}
	}
}
