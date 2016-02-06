#if UNITY_5_2_3 || UNITY_5_2_4
#else
using UnityEngine;
using UnityEngine.Analytics;
using System.Collections.Generic;

public class TestAnalytics : MonoBehaviour {
	IDictionary<string, object> customDictionary = new Dictionary<string, object>();

	void Start() {
		this.customDictionary.Add("Testing Event Message", "Hello world. And it is truly a 'Hello world' message.");
	}

	// Update is called once per frame
	void Update() {
		if (Input.GetKeyDown(KeyCode.P)) {
			//Analytics.CustomEvent("Hello World Event", this.customDictionary);

			//Analytics.Transaction("12345abcde", 0.99m, "USD", null, null);

			Gender gender = Gender.Female;
			Analytics.SetUserGender(gender);

			int birthYear = 2014;
			Analytics.SetUserBirthYear(birthYear);

			Analytics.SetUserId("Test11234");
		}
	}
}
#endif
