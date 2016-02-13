using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


namespace MultiPlayer {
	public struct LevelingRate {
		public int level;
		public float health;
		public float attack;
		public float attackCooldown;
		public float speed;
		public float split;
		public float merge;
	}

	public class RateSyncList : SyncListStruct<LevelingRate> {
	}

	public class NewAttribute : NetworkBehaviour {
		public RateSyncList attributes = new RateSyncList();

		public void Start() {
			Debug.Log("Setting up new attributes manager for the new Multiplayer UI.");
			this.InitializeDefaultLevelingRate(attributes);
		}

		public void InitializeDefaultLevelingRate(RateSyncList list) {
			for (int i = 0; i < Attributes.MAX_NUM_OF_LEVELS; i++) {
				LevelingRate rate = new LevelingRate();
				rate.level = i + 1;
				rate.health = 3f + i * 2f;
				rate.attack = 1f + i;
				rate.attackCooldown = 1f - ((float)(i * 2) / (float)(i + 1f));
				rate.speed = 1f + i * 2f;
				rate.split = 3f;
				rate.merge = 3f + i * 2f;
				list.Add(rate);
			}
		}
	}
}
