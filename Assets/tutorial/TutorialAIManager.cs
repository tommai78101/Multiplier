using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tutorial {
	public enum Parts {
		Introduction, Camera_Controls, Unit_Controls, Splitting, Merging, Invalid, Error
	}

	public class StringConstants {
		public static readonly string Intro_1 = "Hello world.";

		public static string Values(Parts parts, int section) {
			switch (parts) {
				case Parts.Introduction:
					switch (section) {
						case 0:
							return "Hello World.";
						case 1:
							return "How are you doing?";
						case 2:
							return "おはいよ";
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
					this.dialogueSectionCounter = ++this.dialogueSectionCounter % 3;
					break;
			}
			this.dialogueText.text = "";
			this.stringLetterCounter = 0;
			this.startTextRollingFlag = true;
		}
	}
}
