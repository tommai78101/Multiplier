using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class UnitAttributes : NetworkBehaviour {
	public List<float> healthPrefabList;
	public List<float> attackPrefabList;
	public List<float> speedPrefabList;
	public List<float> mergePrefabList;
	public List<float> splitPrefabList;

	public void Awake() {
		this.healthPrefabList = new List<float>(10);
		this.attackPrefabList = new List<float>(10);
		this.speedPrefabList = new List<float>(10);
		this.mergePrefabList = new List<float>(10);
		this.splitPrefabList = new List<float>(10);

		GameObject content = GameObject.FindGameObjectWithTag("Content");
		if (content != null) {
			Attributes attr = content.GetComponent<Attributes>();
			if (attr != null) {
				attr.unitAttributes = this;
				Debug.Log("Unit attributes set successfully.");

				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < Attributes.MAX_NUM_OF_LEVELS; j++) {
						switch (i) {
							case 0:
								this.healthPrefabList.Add(1f);
								break;
							case 1:
								this.attackPrefabList.Add(1f);
								break;
							case 2:
								this.speedPrefabList.Add(1f);
								break;
							case 3:
								this.mergePrefabList.Add(3f);
								break;
							case 4:
								this.splitPrefabList.Add(3f);
								break;
						}
					}
				}
			}
		}
	}
}
