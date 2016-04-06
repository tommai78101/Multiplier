using UnityEngine;
using System.Collections.Generic;
using SinglePlayer;
using Analytics;

public class SingleAIPlayer : MonoBehaviour {
	public AIManager aiManager;

	public void Update() {
		if (!this.aiManager.isSingleAIPlayer) {
			this.aiManager.isSingleAIPlayer = true;
		}
	}
}
