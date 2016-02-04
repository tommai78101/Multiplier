using UnityEngine;
using System.Collections.Generic;

namespace Tutorial {
	[System.Serializable]
	public class Group {
		public GameObject owner;
		public GameObject clone;
		public float elapsedTime;
		public Vector3 rotationVector;
		public Vector3 splitOrigin;
		public Vector3 mergeOrigin;
		public Vector3 ownerOriginalPosition;
		public Vector3 cloneOriginalPosition;
		public Vector3 ownerOriginalScale;
		public Vector3 ownerNewScale;

		public Group(GameObject owner, GameObject clone) {
			this.owner = owner;
			this.clone = clone;

			this.rotationVector = Quaternion.Euler(new Vector3(0f, Random.Range(-180f, 180f), 0f)) * Vector3.one;
			this.rotationVector.y = 0f;
			this.elapsedTime = 0f;

			this.splitOrigin = owner.transform.position;
			this.ownerOriginalPosition = this.splitOrigin;
			this.cloneOriginalPosition = clone.transform.position;
			this.ownerOriginalScale = owner.transform.localScale;
			this.ownerNewScale = this.ownerOriginalScale * 2f;
			this.mergeOrigin = (this.ownerOriginalPosition + this.cloneOriginalPosition) / 2f;

		}

		public void UpdateTime(float factor) {
			this.elapsedTime += Time.deltaTime / factor;
		}

		public void SetOwnerPosition(Vector3 position) {
			this.owner.transform.position = position;
		}

		public void SetClonePosition(Vector3 position) {
			this.clone.transform.position = position;
		}

		public void SetOwnerScale(Vector3 scale) {
			this.owner.transform.localScale = scale;
		}

		public void SetCloneScale(Vector3 scale) {
			this.clone.transform.localScale = scale;
		}
	}

	public class SplitMergeManager : MonoBehaviour {
		public List<Group> splitGroupList;
		public List<Group> mergeGroupList;

		public void Start() {
			this.splitGroupList = new List<Group>();
			this.mergeGroupList = new List<Group>();
		}

		public void Update() {
			if (this.splitGroupList.Count > 0) {
				for (int i = 0; i < this.splitGroupList.Count; i++) {
					HandleSplitGroup(this.splitGroupList[i]);
				}
			}
			if (this.mergeGroupList.Count > 0) {
				for (int i = 0; i < this.mergeGroupList.Count; i++) {
					HandleMergeGroup(this.mergeGroupList[i]);
				}
			}
		}

		public void HandleSplitGroup(Group group) {
			if (group.elapsedTime > 1f) {
				this.splitGroupList.Remove(group);
			}
			else {
				group.SetOwnerPosition(Vector3.Lerp(group.splitOrigin, group.splitOrigin + group.rotationVector, group.elapsedTime));
				group.SetClonePosition(Vector3.Lerp(group.splitOrigin, group.splitOrigin - group.rotationVector, group.elapsedTime));
				group.UpdateTime(1f);
			}
		}

		public void HandleMergeGroup(Group group) {
			if (group.elapsedTime > 1f) {
				this.mergeGroupList.Remove(group);
			}
			else {
				group.SetOwnerPosition(Vector3.Lerp(group.ownerOriginalPosition, group.mergeOrigin, group.elapsedTime));
				group.SetClonePosition(Vector3.Lerp(group.cloneOriginalPosition, group.mergeOrigin, group.elapsedTime));
				group.SetOwnerScale(Vector3.Lerp(group.ownerOriginalScale, group.ownerNewScale, group.elapsedTime));
				group.SetCloneScale(Vector3.Lerp(group.ownerOriginalScale, group.ownerNewScale, group.elapsedTime));
				group.UpdateTime(1f);
			}
		}
	}
}
