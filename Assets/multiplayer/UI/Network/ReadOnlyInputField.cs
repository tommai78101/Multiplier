using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[AddComponentMenu("UI/Read Only InputField", 32)]
public class ReadOnlyInputField : InputField {
	private Event processEvent = new Event();

	public override void OnUpdateSelected(BaseEventData eventData) {
		if (!this.isFocused) {
			return;
		}

		bool consumedEvent = false;
		while (Event.PopEvent(processEvent)) {
			if (processEvent.rawType == EventType.KeyDown) {
				if (!CombinationPressed(processEvent)) {
					continue;
				}
				consumedEvent = true;
				EditState state = this.KeyPressed(processEvent);
				if (state == EditState.Finish) {
					DeactivateInputField();
				}
				break;
			}
		}

		if (consumedEvent) {
			this.UpdateLabel();
		}

		eventData.Use();
	}

	private bool CombinationPressed(Event processEvent) {
		EventModifiers modifiers = processEvent.modifiers;
		RuntimePlatform platform = Application.platform;
		bool isMac = (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXWebPlayer);
		bool ctrlIsPressed = isMac ? (modifiers & EventModifiers.Command) != 0 : (modifiers & EventModifiers.Control) != 0;

		switch (processEvent.keyCode) {
			case KeyCode.Home:
			case KeyCode.End:
			case KeyCode.LeftControl:
			case KeyCode.RightControl: 
				return true;
			case KeyCode.A:
				if (ctrlIsPressed) {
					return true;
				}
				break;
			case KeyCode.C:
				if (ctrlIsPressed) {
					return true;
				}
				break;
			case KeyCode.LeftArrow:
			case KeyCode.RightArrow:
			case KeyCode.UpArrow:
			case KeyCode.DownArrow:
				return true;
		}
		return false;
	}
}

