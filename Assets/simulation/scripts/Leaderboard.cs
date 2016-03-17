using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Leaderboard : MonoBehaviour {
	public Text leaderboardName;
	public Text splits;
	public Text merges;
	public Text kills;
	public Text deaths;
	public Text attacks;
	public Text attackTime;
	public Text battleTime;
	public Text wins;
	public Text losses;

	public void Start() {
		if (leaderboardName == null) {
			Debug.LogError("Leaderboard name is not set. Please check.");
		}
		bool flag = (splits == null) || (merges == null) || (kills == null) || (deaths == null) || (attacks == null) || (attackTime == null) || (battleTime == null) || (wins == null) || (losses == null);
		if (flag) {
			Debug.LogError("Initializing Leaderboard, " + leaderboardName + ", is incomplete. Please check.");
		}
	}
}
