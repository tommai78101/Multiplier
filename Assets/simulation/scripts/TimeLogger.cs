using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class TimeLogger : MonoBehaviour {
	public InputField inputField;
	private float totalSessionTime;
	private bool isPaused;

	public void Start() {
		if (this.inputField == null) {
			this.inputField = this.GetComponent<InputField>();
		}
		this.totalSessionTime = 0f;
		this.isPaused = false;
	}

	public void Update() {
		if (!this.isPaused) {
			this.totalSessionTime += Time.deltaTime;
		}

		if (Input.GetKeyUp(KeyCode.Escape)) {
			Print();
			this.isPaused = !this.isPaused;
		}
	}

	public void Print() {
		StringBuilder sB = new StringBuilder();
		sB.AppendLine("Total Simulation Session Time: " + this.totalSessionTime.ToString("0.000") + " seconds");
		sB.AppendLine();
		sB.AppendLine("COPY/PASTE THE DATA!");
		sB.AppendLine("EXCEL DATA DON'T MODIFY");
		sB.AppendLine("FOR RESEARCHERS ONLY!");
		sB.AppendLine();
		sB.AppendLine("ID;P");
		sB.Append(CreateRow(1, "Total Simulation Session Time", this.totalSessionTime.ToString("0.000")));
		sB.AppendLine("E");
		sB.AppendLine();
		this.inputField.text = sB.ToString();
	}

	private string CreateRow(int y, string tag, string value) {
		StringBuilder sB = new StringBuilder();
		sB.AppendLine("C;Y" + y + ";X1;K" + tag);
		sB.AppendLine("C;Y" + y + ";X2;K" + value);
		return sB.ToString();
	}
}
