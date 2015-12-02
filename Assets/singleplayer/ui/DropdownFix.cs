using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace SinglePlayer {
	public class DropdownFix : Dropdown, IPointerClickHandler {
		public override void OnPointerClick(PointerEventData eventData) {
			base.OnPointerClick(eventData);

			Canvas dropdown = transform.GetComponentInChildren<Canvas>();
			if (dropdown != null) {
				dropdown.transform.SetParent(this.transform.root);
				RectTransform ownerRectTransform = this.GetComponent<RectTransform>();
				RectTransform dropdownRectTransform = dropdown.GetComponent<RectTransform>();
				if (ownerRectTransform != null && dropdownRectTransform != null) {
					//dropdownRectTransform.anchorMin = new Vector2(0f, ownerRectTransform.anchorMin.x);
					dropdownRectTransform.offsetMax = new Vector2(dropdownRectTransform.offsetMax.x, ownerRectTransform.anchorMin.x);
				}
			}
		}
	}
}
