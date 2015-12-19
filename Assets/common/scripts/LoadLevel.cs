using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Common {
	public class LoadLevel : MonoBehaviour {
		public void Load(string value) {
			//Application.LoadLevel(value);
			SceneManager.LoadScene(value);
		}
	}
}
