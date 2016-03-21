using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Simulation;

public class RunStopButtonAction : MonoBehaviour {
	public Text runStopButtonText;
	public bool isRunning;
	public SimulationStarter simulationStarter;

	public void Start() {
		this.isRunning = false;
		this.runStopButtonText.text = "Start Simulation";
	}

	public void Toggle() {
		if (!this.isRunning) {
			this.runStopButtonText.text = "Stop Simulation";
			this.isRunning = true;
			this.simulationStarter.StartSimulation();
		}
		else {
			this.runStopButtonText.text = "Start Simulation";
			this.isRunning = false;
			this.simulationStarter.StopSimulation();
		}
	}
}
