using UnityEngine;
using System.Collections.Generic;

public class UnitAttributes : MonoBehaviour {
	public List<float> healthPrefabList;
	public List<float> attackPrefabList;
	public List<float> speedPrefabList;
	public List<float> mergePrefabList;
	public List<float> splitPrefabList;

	public void Awake() {
		this.healthPrefabList = new List<float>();
		this.attackPrefabList = new List<float>();
		this.speedPrefabList = new List<float>();
		this.mergePrefabList = new List<float>();
		this.splitPrefabList = new List<float>();
	}
}
