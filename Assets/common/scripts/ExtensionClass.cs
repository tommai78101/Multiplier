using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace Extension {
	public static class ExtensionClass {
		public static bool ReachedDestination(this NavMeshAgent agent) {
			if (!agent.pathPending) {
				if (agent.stoppingDistance > 0f) {
					if (agent.remainingDistance <= agent.stoppingDistance) {
						if (!agent.hasPath || agent.velocity.sqrMagnitude <= float.Epsilon) {
							return true;
						}
					}
				}
				else {
					if (agent.remainingDistance <= float.Epsilon) {
						if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public static void AnchorToCorners(this RectTransform transform) {
			if (transform == null)
				throw new ArgumentNullException("There is no transform being referenced.");

			if (transform.parent == null)
				return;

			var parent = transform.parent.GetComponent<RectTransform>();

			Vector2 newAnchorsMin = new Vector2(transform.anchorMin.x + transform.offsetMin.x / parent.rect.width,
							  transform.anchorMin.y + transform.offsetMin.y / parent.rect.height);

			Vector2 newAnchorsMax = new Vector2(transform.anchorMax.x + transform.offsetMax.x / parent.rect.width,
							  transform.anchorMax.y + transform.offsetMax.y / parent.rect.height);

			transform.anchorMin = newAnchorsMin;
			transform.anchorMax = newAnchorsMax;
			transform.offsetMin = transform.offsetMax = new Vector2(0, 0);
		}

		public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec) {
			trans.pivot = aVec;
			trans.anchorMin = aVec;
			trans.anchorMax = aVec;
		}

		public static Vector2 GetSize(this RectTransform trans) {
			return trans.rect.size;
		}

		public static float GetWidth(this RectTransform trans) {
			return trans.rect.width;
		}

		public static float GetHeight(this RectTransform trans) {
			return trans.rect.height;
		}

		public static void SetSize(this RectTransform trans, Vector2 newSize) {
			Vector2 oldSize = trans.rect.size;
			Vector2 deltaSize = newSize - oldSize;
			trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
			trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
		}

		public static void SetWidth(this RectTransform trans, float newSize) {
			SetSize(trans, new Vector2(newSize, trans.rect.size.y));
		}

		public static void SetHeight(this RectTransform trans, float newSize) {
			SetSize(trans, new Vector2(trans.rect.size.x, newSize));
		}

		public static void SetBottomLeftPosition(this RectTransform trans, Vector2 newPos) {
			trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
		}

		public static void SetTopLeftPosition(this RectTransform trans, Vector2 newPos) {
			trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
		}

		public static void SetBottomRightPosition(this RectTransform trans, Vector2 newPos) {
			trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
		}

		public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos) {
			trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
		}

		public static Vector3 GetRandomPosition(this GameObject gameObject) {
			Renderer renderer = gameObject.GetComponent<Renderer>();
			if (renderer != null) {
				Vector3 size = renderer.bounds.size;
				Vector3 halfSize = size / 2f;
				return new Vector3(UnityEngine.Random.Range(0f, size.x) - halfSize.x, UnityEngine.Random.Range(0f, size.y) - halfSize.y, UnityEngine.Random.Range(0f, size.z) - halfSize.z);
            }
			return Vector3.zero;
		}

		public static Toggle GetSingleActiveToggle(this ToggleGroup group) {
			foreach (Toggle toggle in group.ActiveToggles()) {
				if (toggle.isOn) {
					return toggle;
				}
			}
			return null;
		}
	}
}
