using UnityEngine;
using System.Collections.Generic;

namespace SinglePlayer {
	public class AIManager : MonoBehaviour {
		public List<AIUnit> allUnits;
		public List<AIUnit> removeList;
		public bool startAIFlag;

		//TODO: Continue working on the AI.



		public void Start() {
			this.startAIFlag = false;
		}

		public void Update() {
			if (!this.startAIFlag) {
				return;
			}

			
		}

		public void Activate() {
			this.startAIFlag = true;
		}
	}
}