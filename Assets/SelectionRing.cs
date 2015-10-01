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
	
	void Update () {
		if (!this.hasAuthority) {
			return;
		}

		//All you do is enable/disable the selection ring when the game unit is selected/unselected.
		if (this.meshRenderer != null) {
			if (this.gameUnit.isSelected) {
				this.meshRenderer.gameObject.SetActive(true);
			}
			else {
				this.meshRenderer.gameObject.SetActive(false);
			}
		}
	}

	public void SetColor(Color color) {
		this.color = color;
	}
}
