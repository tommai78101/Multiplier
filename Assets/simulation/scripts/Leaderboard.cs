using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Analytics;

public class Leaderboard : MonoBehaviour {
	public int index;
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
	public SimulationMetricsLogger metricsLogger;

	public void Start() {
		if (leaderboardName == null) {
			Debug.LogError("Leaderboard name is not set. Please check.");
		}
		bool flag = (splits == null) || (merges == null) || (kills == null) || (deaths == null) || (attacks == null) || (attackTime == null) || (battleTime == null) || (wins == null) || (losses == null);
		if (flag) {
			Debug.LogError("Initializing Leaderboard, " + leaderboardName + ", is incomplete. Please check.");
		}
	}

	public void Update() {
		TeamMetric metrics = this.metricsLogger.teamMetrics[this.index];
		this.splits.text = metrics.numberOfSplits.ToString();
		this.merges.text = metrics.numberOfMerges.ToString();
		this.kills.text = metrics.numberOfKills.ToString();
		this.deaths.text = metrics.numberOfDeaths.ToString();
		this.attacks.text = metrics.numberOfAttacks.ToString();
		this.attackTime.text = metrics.totalAttackTime.ToString();
		this.battleTime.text = metrics.totalBattleEngagementTime.ToString();
		this.wins.text = metrics.winCount.ToString();
		this.losses.text = metrics.lossCount.ToString();
	}

	public void Reset() {
		this.splits.text = "0";
		this.merges.text = "0";
		this.kills.text = "0";
		this.deaths.text = "0";
		this.attacks.text = "0";
		this.attackTime.text = "0";
		this.battleTime.text = "0";
		this.wins.text = "0";
		this.losses.text = "0";
	}
}
