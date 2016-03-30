using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class TutorialSectionActions : MonoBehaviour {
		public void ShowTutorialSection(int index) {
			TutorialAIManager.Instance.dialogueBox.gameObject.SetActive(true);
			switch (index) {
				case 0: //General
					TutorialAIManager.Instance.PrepareGeneralSection();
					break;
				case 1: //Attributes Editor
					TutorialAIManager.Instance.PrepareEditorSection();
					break;
				case 2: //Camera controls
					TutorialAIManager.Instance.PrepareCameraSection();
					break;
				case 3: //Gameplay (Unit Controls)
					TutorialAIManager.Instance.PrepareGameplaySection();
					break;
				default:
					break;
			}
			TutorialAIManager.Instance.tutorialSections.gameObject.SetActive(false);
		}
	}
}
