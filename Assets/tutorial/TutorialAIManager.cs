using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tutorial {
	public enum Parts {
		Introduction, Camera_Controls, Unit_Controls, Splitting, Merging, Invalid, Error
	}

	//This code layout is the most readable layout that I can think of for others to understand and follow. Nothing wrong with redundancy.
	public class StringConstants {
		public static string Values(Parts parts, int section) {
			switch (parts) {
				case Parts.Introduction:
					switch (section) {
						case 0:
							return "Hello there. Welcome to the tutorial for Multiplier (working title). To begin, please press the \"Next Step\" button on the left. You may exit the tutorial at anytime by pressing the \"Return to Main Menu\" button on the left.";
						case 1:
							return "In this tutorial, you will learn how the game is to be played, as well as how this game can be used as a tool for you to play around with.";
						case 2:
							return "Let's begin, shall we?";
					}
					break;
				case Parts.Camera_Controls:
					switch (section) {
						case 0:
							return "The first lesson is Camera Controls. Here, we introduce to you the basics of moving around the camera.";
						case 1:
							return "To start, move your mouse to the edge of the game boundaries. Press \"Next Step\" to continue once you are done.";
						case 2:
							return "When you put your mouse at the edge of where you wished to go, the camera will move in that general direction. This is useful if you want to \"nudge\" the camera just a tiny bit to see clearly.";
						case 3:
							return "The other method of moving the camera is by clicking and dragging on the minimap in the lower right of your screen.";
						case 4:
							return "This method of control is best suited for quickly panning the camera to where you want to see.";
					}
					break;
			}
			return "";
		}
	}

	public class TutorialAIManager : MonoBehaviour {
		public Parts currentTutorialStage;
		public Text dialogueText;
		public int stringLetterCounter;
		public string dialogue;
		public bool startTextRollingFlag;
		public float delay;
		[Range(0.01f, 2f)]
		public float delayInterval;
		public int dialogueSectionCounter;

		public void Start() {
			this.delay = 0f;
			this.delayInterval = 0.01f;
			this.dialogueSectionCounter = 0;
			this.currentTutorialStage = Parts.Introduction;
			this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
            this.dialogueSectionCounter++;
			this.stringLetterCounter = 0;
			this.startTextRollingFlag = true;
			if (this.dialogueText != null) {
				this.dialogueText.text = "";
			}
		}

		public void Update() {
			if (this.startTextRollingFlag) {
				if (this.delay < this.delayInterval) {
					this.delay += Time.deltaTime;
					return;
				}
				else {
					if (this.stringLetterCounter < this.dialogue.Length) {
						this.dialogueText.text = this.dialogueText.text.Insert(this.dialogueText.text.Length, this.dialogue[this.stringLetterCounter].ToString());
						this.stringLetterCounter++;
					}
					else {
						this.startTextRollingFlag = false;
					}
					this.delay = 0f;
				}
			}
		}

		//This code layout aims to have the highest readability level possible. Dialogues are broken into tutorial stages and sections. Magic numbers correspond to the total sections listed above
		//in the StringConstants class.
		public void OnClickAction() {
			switch (this.currentTutorialStage) {
				default:
				case Parts.Error:
					this.currentTutorialStage = Parts.Invalid;
					Debug.LogError("Current tutorial state has hit error. It will be reset to Invalid state.");
					break;
				case Parts.Invalid:
					Debug.LogWarning("Current tutorial state has hit an invalid state. Please check.");
					break;
				case Parts.Introduction:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					this.dialogueSectionCounter++;
					if (this.dialogueSectionCounter >= 3) {
						this.currentTutorialStage = Parts.Camera_Controls;
						this.dialogueSectionCounter = 0;
					}
					break;
				case Parts.Camera_Controls:
					this.dialogue = StringConstants.Values(this.currentTutorialStage, this.dialogueSectionCounter);
					this.dialogueSectionCounter++;
					if (this.dialogueSectionCounter >= 5) {
						this.currentTutorialStage = Parts.Camera_Controls;
						this.dialogueSectionCounter = 0;
					}
					break;
			}
			this.dialogueText.text = "";
			this.stringLetterCounter = 0;
			this.startTextRollingFlag = true;
		}
	}
}
