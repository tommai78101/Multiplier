using UnityEngine;
using System.Collections.Generic;
using System;

namespace SinglePlayer {
	public class GameUnit : MonoBehaviour {
		public int currentHealth;
		public int maxHealth;
		public int attackPower;
		public float attackCooldown;
		public float attackCooldownCounter;
		public float recoveryCooldown;
		public float recoveryCooldownCounter;
		public float speed;
		public bool isSelected;
		public EnumTeam teamFaction;
		private Vector3 oldTargetPosition;
		public bool isDirected;
		public GameUnit targetEnemy;

		public void Start() {
		}

		public void Update() {

		}

		public void Copy(GameUnit original) {
			this.currentHealth = original.currentHealth;
			this.maxHealth = original.maxHealth;
			this.attackPower = original.attackPower;
			this.attackCooldown = original.attackCooldown;
			this.attackCooldownCounter = original.attackCooldownCounter;
			this.recoveryCooldown = original.recoveryCooldown;
			this.recoveryCooldownCounter = original.recoveryCooldownCounter;
			this.speed = original.speed;
			this.isSelected = original.isSelected;
		}

		public static GameUnit MakeClone(GameUnit original) {
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			gameObject.AddComponent<Rigidbody>();
			gameObject.AddComponent<NavMeshAgent>();
			GameUnit unit = gameObject.AddComponent<GameUnit>();
			unit.Copy(original);
			return unit;
		}

		public void CastRay(bool isMinimap, Vector3 mousePosition, Camera minimapCamera) {
			if (isMinimap) {
				Ray ray = minimapCamera.ViewportPointToRay(mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
				foreach (RaycastHit hit in hits) {
					if (hit.collider.gameObject.tag.Equals("Floor")) {
						SetTarget(this, hit.point);
						break;
					}
				}
			}
			else {
				Ray ray = Camera.main.ScreenPointToRay(mousePosition);
				RaycastHit[] hits = Physics.RaycastAll(ray);
				foreach (RaycastHit hit in hits) {
					if (hit.collider.gameObject.tag.Equals("Floor")) {
						//Call on the client->server method to start the action.
						SetTarget(this, hit.point);
						break;
					}
				}
			}
		}

		public void SetTarget(GameUnit unit, Vector3 target) {
			NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
			if (agent != null) {
				if (unit.oldTargetPosition != target) {
					agent.SetDestination(target);
					//Making sure that we actually track the new NavMeshAgent destination. If it's different, it may cause
					//desync among local and remote clients. That's a hunch though, so don't take my word for word on this.
					unit.oldTargetPosition = target;
					//Confirm that the player has issued an order for the game unit to follow/move to.
					//Syncing the isDirected flag.
					unit.isDirected = true;
					unit.targetEnemy = null;
				}
			}
		}
	}
}
