using UnityEngine;
using System.Collections.Generic;

namespace SinglePlayer {
	public class SelectionManager : MonoBehaviour {
		public List<GameUnit> allObjects;
		public List<GameUnit> selectedObjects;
		public List<GameUnit> removeList;
		public EnumTeam teamFaction;

		public void Start() {
			this.allObjects = new List<GameUnit>();
			this.selectedObjects = new List<GameUnit>();
			this.removeList = new List<GameUnit>();
		}

		public void FixedUpdate() {
			if (this.removeList.Count > 0) {
				foreach (GameUnit unit in this.removeList) {
					if (this.allObjects.Contains(unit)) {
						this.allObjects.Remove(unit);
					}
				}
				this.removeList.Clear();
			}
		}
	}
};
