using UnityEngine;
using System.Collections;

namespace Tutorial {
	public enum Section {
		//public const byte GENERAL = 0x00;
		//public const byte ATTRIBUTES_EDITOR = 0x01;
		//public const byte CAMERA_CONTROLS = 0x02;
		//public const byte GAMEPLAY = 0x03;
		INVALID, GENERAL, ATTRIBUTES_EDITOR, CAMERA_CONTROLS, GAMEPLAY
	}

	public class TutorialSectionActions : MonoBehaviour {
		public void ShowTutorialSection(Section section) {
			switch (section) {
				case Section.GENERAL:
					TutorialAIManager.Instance.PrepareGeneralSection();
					break;
				case Section.ATTRIBUTES_EDITOR:
					TutorialAIManager.Instance.PrepareEditorSection();
					break;
				case Section.CAMERA_CONTROLS:
					TutorialAIManager.Instance.PrepareCameraSection();
					break;
				case Section.GAMEPLAY:
					TutorialAIManager.Instance.PrepareGameplaySection();
					break;
				default:
				case Section.INVALID:
					break;
			}
		}
	}
}
