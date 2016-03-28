using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tutorial {
	[RequireComponent(typeof(ImageIndex))]
	public class NewRawImage : RawImage {
		public ImageIndex index;

		public void Initialize() {
			this.index = this.GetComponent<ImageIndex>();
		}

		public void ToggleImage(bool flag) {
			Color col = this.color;
			col.a = flag ? 1f : 0f;
			this.color = col;
		}
	}
}
