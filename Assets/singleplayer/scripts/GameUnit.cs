using UnityEngine;
using System.Collections.Generic;

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
	}
}
