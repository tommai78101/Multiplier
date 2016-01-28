using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class BoxSelector : MonoBehaviour {
		public Vector3 viewport_startVertex;
		public Vector3 viewport_endVertex;
		public float panningElapsedTime;
		public Vector3 viewport_vertexIterator;

		private Material borderMaterial;

		public void Start() {
			this.panningElapsedTime = 2f;
			this.borderMaterial = (Material) Resources.Load("Border");
			this.viewport_endVertex = this.viewport_startVertex = -Vector3.one;
		}

		public void Update() {
			if (this.panningElapsedTime < 1f) {
				this.panningElapsedTime += Time.deltaTime / 2f;
				this.viewport_vertexIterator = Vector3.Lerp(this.viewport_startVertex, this.viewport_endVertex, this.panningElapsedTime);
			}
		}

		public void OnPostRender() {
			if (this.borderMaterial == null) {
				Debug.LogError("Cannot obtain border material for selection box.");
				return;
			}

			if (this.viewport_startVertex == -Vector3.one || this.viewport_endVertex == -Vector3.one) {
				return;
			}

			if (this.panningElapsedTime >= 1f) {
				return;
			}

			GL.PushMatrix();
			{
				if (this.borderMaterial.SetPass(0)) {
					GL.LoadOrtho();
					GL.Begin(GL.LINES);
					{
						GL.Color(Color.green);
						//Top
						GL.Vertex(this.viewport_vertexIterator);
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_vertexIterator.y, 0f));
						//Right
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_vertexIterator.y, 0f));
						GL.Vertex(this.viewport_startVertex);
						//Bottom
						GL.Vertex(this.viewport_startVertex);
						GL.Vertex(new Vector3(this.viewport_vertexIterator.x, this.viewport_startVertex.y, 0f));
						//Left
						GL.Vertex(new Vector3(this.viewport_vertexIterator.x, this.viewport_startVertex.y, 0f));
						GL.Vertex(this.viewport_vertexIterator);
					}
					GL.End();
				}
			}
			GL.PopMatrix();
		}

		public void StartBoxSelection(CursorPanGroup group, float extraTime) {
			float startX = group.start.x / Screen.width;
			float startY = group.start.y / Screen.height;
			float endX = group.end.x / Screen.width;
			float endY = group.end.y / Screen.height;

			this.viewport_startVertex.x = startX;
			this.viewport_startVertex.y = startY;
			this.viewport_startVertex.z = 0f;

			this.viewport_endVertex.x = endX;
			this.viewport_endVertex.y = endY;
			this.viewport_endVertex.z = 0f;

			this.panningElapsedTime = 0f - extraTime;
		}
	}
}
