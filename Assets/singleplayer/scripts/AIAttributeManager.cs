using UnityEngine;
using System;
using System.Collections.Generic;
using SinglePlayer.UI;

namespace SinglePlayer {
	[Serializable]
	public struct TierUpgrade {
		public int level;
		public float health;
		public float attack;
		public float speed;
		public float split;
		public float merge;
	}

	public class AIAttributeManager : MonoBehaviour {
		public List<TierUpgrade> tiers;
		public int maxTiersLimit;
		public AttributePanelUI attributePanelUI;

		public void Awake() {
			Debug.Log("Initializing AI attribute manager.");
			this.maxTiersLimit = 10;
			this.tiers = new List<TierUpgrade>();
			for (int i = 0; i < this.maxTiersLimit; i++) {
				TierUpgrade tier = new TierUpgrade();
				if (i == 0) {
					tier.level = i + 1;
					tier.health = 2f;
					tier.attack = 2f;
					tier.speed = 1.2f;
					tier.split = 2f;
					tier.merge = 2f;
				}
				else {
					TierUpgrade previous = this.tiers[i - 1];
					tier.level = i + 1;
					tier.health = previous.health * 2f;
					tier.attack = previous.attack * 2f;
					tier.speed = previous.speed * 1.2f;
					tier.split = previous.split * 2f;
					tier.merge = previous.merge * 2f;
				}
				this.tiers.Add(tier);
			}
			GameObject tempAttribute = GameObject.FindGameObjectWithTag("AIAttributePanel");
			if (tempAttribute != null) {
				this.attributePanelUI = tempAttribute.GetComponent<AttributePanelUI>();
				if (this.attributePanelUI == null) {
					Debug.LogError("Something is not right.");
				}
			}
		}

		public void Start() {

		}
	}
}
