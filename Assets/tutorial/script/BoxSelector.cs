using UnityEngine;
using System.Collections;

namespace Tutorial {
	public class BoxSelector : MonoBehaviour {
		public Material borderMaterial;
		public Vector3 viewport_startVertex;
		public Vector3 viewport_endVertex;
		public Vector3 viewport_startVertexBegin;
		public Vector3 viewport_startVertexEnd;
		public float panningElapsedTime;

		public Vector3 beforeVertex;
		public Vector3 afterVertex;



		public void Start() {
			this.panningElapsedTime = 2f;
			this.borderMaterial = (Material) Resources.Load("Border");
			this.viewport_endVertex = this.viewport_startVertex = this.viewport_startVertexBegin = this.viewport_startVertexEnd = -Vector3.one;
		}

		public void Update() {
			if (this.panningElapsedTime < 1f) {
				this.panningElapsedTime += Time.deltaTime;
				this.viewport_startVertex = Vector3.Lerp(this.viewport_startVertexBegin, this.viewport_startVertexEnd, this.panningElapsedTime);
			}

			if (Input.GetKeyUp(KeyCode.G)) {
				//Vector3 start = new Vector3(10f, -10f);
				//Vector3 end = new Vector3(-10f, 10f);
				Vector3 start, end;

				start = Camera.main.WorldToScreenPoint(this.beforeVertex);
				start.z = 0f;
				end = Camera.main.WorldToScreenPoint(this.afterVertex);
				end.z = 0f;
				this.DrawBoxSelection(start, end);
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

			GL.PushMatrix();
			{
				if (this.borderMaterial.SetPass(0)) {
					GL.LoadOrtho();
					GL.Begin(GL.LINES);
					{
						GL.Color(Color.green);
						//Top
						GL.Vertex(this.viewport_startVertex);
						GL.Vertex(new Vector3(this.viewport_endVertex.x, this.viewport_startVertex.y, 0f));
						//Right
						GL.Vertex(new Vector3(this.viewport_endVertex.x, this.viewport_startVertex.y, 0f));
						GL.Vertex(this.viewport_endVertex);
						//Bottom
						GL.Vertex(this.viewport_endVertex);
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_endVertex.y, 0f));
						//Left
						GL.Vertex(new Vector3(this.viewport_startVertex.x, this.viewport_endVertex.y, 0f));
						GL.Vertex(this.viewport_startVertex);
					}
					GL.End();
				}
			}
			GL.PopMatrix();
		}

		public void DrawBoxSelection(Vector3 viewportStart, Vector3 viewportEnd) {
			//float x = viewportEnd.x < viewportStart.x ? viewportEnd.x : viewportStart.x;
			//float y = 


			this.viewport_startVertexBegin = viewportStart;
			this.viewport_startVertexEnd = viewportEnd;
			this.viewport_endVertex = viewportStart;
			this.panningElapsedTime = 0f;
		}
	}
}
