using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class SelectionRing : NetworkBehaviour {
	public GameUnit gameUnit;
	public MeshRenderer meshRenderer;
	public Color color;

	void Start () {
		if (this.meshRenderer == null) {
			Debug.LogError("Something is wrong.");
		}
	}

	public void SetColor(Color color) {
		this.color = color;
	}
}
