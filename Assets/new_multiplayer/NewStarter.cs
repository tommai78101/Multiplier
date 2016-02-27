using UnityEngine;
using System.Collections;

public class NewStarter : MonoBehaviour {
	private bool isTaken = false;

	public void SetIsTakenFlag(bool value) {
		this.isTaken = value;
	}

	public bool GetIsTakenFlag() {
		return this.isTaken;
	}
}
