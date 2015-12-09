using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Extension;

namespace SinglePlayer.UI {
	public class DropdownFix : Dropdown, IPointerClickHandler {
		public override void OnPointerClick(PointerEventData eventData) {
			base.OnPointerClick(eventData);

			Dropdown dropdown = transform.GetComponentInChildren<Dropdown>();
			if (dropdown != null) {
				Canvas dropdownMenu = dropdown.GetComponentInChildren<Canvas>();
				if (dropdownMenu != null) {
					//dropdownMenu.transform.SetParent(this.transform.root);
					RectTransform ownerRectTransform = dropdown.GetComponent<RectTransform>();
					RectTransform dropdownRectTransform = dropdownMenu.GetComponent<RectTransform>();
					if (ownerRectTransform != null && dropdownRectTransform != null) {
						float height = dropdownRectTransform.GetHeight();
						float ownerHeight = ownerRectTransform.GetHeight();
						dropdownRectTransform.offsetMax = new Vector2(dropdownRectTransform.offsetMax.x, -ownerHeight);
						dropdownRectTransform.offsetMin = new Vector2(dropdownRectTransform.offsetMin.x, -(ownerHeight + height));
					}
				}
			}
		}
	}
}
