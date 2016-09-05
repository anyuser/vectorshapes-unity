using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LibTessDotNet;
using System;

namespace VectorShapes
{
	[ExecuteInEditMode]
	public class ShapeRenderer : MonoBehaviour
	{
		#region public properties

		/// <summary>
		/// Gets or sets the camera for CPU stroke rendering. Not used when stroke is rendered in shader
		/// </summary>
		/// <value>The camera.</value>
		public Camera Camera { 
			get { return cam; } 
			set { cam = value;}
		}

		/// <summary>
		/// Gets or sets the stroke material.
		/// </summary>
		/// <value>The stroke material.</value>
		public Material StrokeMaterial { 
			get { return strokeMaterial; } 
			set { strokeMaterial = value; } 
		}

		/// <summary>
		/// Gets or sets the fill material.
		/// </summary>
		/// <value>The fill material.</value>
		public Material FillMaterial { 
			get { return fillMaterial; } 
			set { fillMaterial = value; } 
		}
		#endregion

		#region private fields

		[SerializeField]
		Camera cam;

		[SerializeField]
		Material strokeMaterial;

		[SerializeField]
		Material fillMaterial;

		List<Shape> shapes = new List<Shape>();
		List<ShapeMeshCache> shapeMeshCaches = new List<ShapeMeshCache>();

		bool useShader = false;
		Shader checkedShader;

		#endregion

		#region MonoBehaviour callbacks

		void Reset()
		{
			fillMaterial = Resources.Load<Material>("Default Shape Materials/Fill");
			strokeMaterial = Resources.Load<Material>("Default Shape Materials/Stroke");
		}

		void OnEnable()
		{
			var foundShapes = GetComponentsInChildren<Shape>();
			for (int i = 0; i < foundShapes.Length; i++)
			{
				foundShapes[i].UpdateShapeRenderer();
			}

			//UpdateMaterials ();
			RefreshMesh();
		}

		void OnDisable ()
		{
			for (int i = 0; i < shapes.Count; i++)
			{
				shapes[i].UpdateShapeRenderer();
			}
			for (int i = 0; i < shapeMeshCaches.Count; i++) {

				shapeMeshCaches [i].Release ();
			}
			//cachedKeywords.Clear();
		}

		void Start()
		{
		}

		void OnDestroy()
		{
		}

		/*	// Update is called once per frame
		void LateUpdate ()
		{

			Draw ();
		}
		*/

		void OnRenderObject()
		{
			RefreshMesh ();
			DrawNow ();
		}

		#endregion

		#region internal functions


		internal void AddShape(Shape shape)
		{
			if (shapes.Contains(shape))
				return;
			shapes.Add(shape);
		}

		internal void RemoveShape(Shape shape)
		{
			if (!shapes.Contains(shape))
				return;
			shapes.Remove(shape);
		}

		#endregion

		#region private functions

		/*void Draw()
		{
			Matrix4x4 fw = Matrix4x4.TRS(Vector3.back * 0.01f, Quaternion.identity, Vector3.one);
			Matrix4x4 m = transform.localToWorldMatrix;

			for (int i = 0; i < shapeMeshCaches.Count; i++)
			{
				m *= fw;

				Graphics.DrawMesh(shapeMeshCaches[i].fillMesh, m, fillMaterial, gameObject.layer, null, 0);
				Graphics.DrawMesh(shapeMeshCaches[i].strokeMesh, m, strokeMaterial, gameObject.layer, null, 0);
			}
		}*/

		void DrawNow()
		{
			Profiler.BeginSample("DrawNow");
			if ((1 << gameObject.layer & Camera.current.cullingMask) == 0)
				return;
			
			//Vector2 originalTilingScale = fillMaterial.GetTextureScale("_MainTex");
			//Vector2 originalTilingOffset = fillMaterial.GetTextureOffset("_MainTex");
			//float? originalStrokeMiterLimit = strokeMaterial.HasProperty("_StrokeMiterLimit") ? (float?)strokeMaterial.GetFloat("_StrokeMiterLimit") : null;
			//string[] originalStrokeKeywords = strokeMaterial.shaderKeywords;


			for (int i = 0; i < shapeMeshCaches.Count; i++)
			{
				CanvasRenderer canvasRenderer = shapeMeshCaches[i].transform.GetComponent<CanvasRenderer>();
				bool useCanvas = canvasRenderer && Camera.current.cameraType != CameraType.SceneView;
				if (useCanvas)
				{
					canvasRenderer.SetMesh(shapeMeshCaches[i].mesh);
					canvasRenderer.materialCount = 2;
					canvasRenderer.SetMaterial(shapeMeshCaches[i].fillMaterial, 0);
					canvasRenderer.SetMaterial(shapeMeshCaches[i].strokeMaterial, 1);
					canvasRenderer.SetColor(Color.white);
					canvasRenderer.SetAlpha(1);
				}
				else
				{
					Profiler.BeginSample("Draw FillMaterial");
					if (shapeMeshCaches[i].fillMaterial != null)
					{
						shapeMeshCaches[i].fillMaterial.SetPass(0);
						Graphics.DrawMeshNow(shapeMeshCaches[i].mesh, shapes[i].transform.localToWorldMatrix, 0);
					}
					Profiler.EndSample();

					Profiler.BeginSample("Draw StrokeMaterial");
					if (shapeMeshCaches[i].strokeMaterial != null)
					{
						shapeMeshCaches[i].strokeMaterial.SetPass(0);
						Graphics.DrawMeshNow(shapeMeshCaches[i].mesh, shapes[i].transform.localToWorldMatrix, 1);
					}
				}
				Profiler.EndSample();
			}

			// reset material values
			//fillMaterial.SetTextureOffset("_MainTex", originalTilingOffset);
			//fillMaterial.SetTextureScale("_MainTex", originalTilingScale);
			//if( originalStrokeMiterLimit.HasValue)
			//	strokeMaterial.SetFloat("_StrokeMiterLimit", originalStrokeMiterLimit.Value);

			//strokeMaterial.shaderKeywords = originalStrokeKeywords;

			Profiler.EndSample();
		}

		void RefreshMesh ()
		{
			if (Camera == null) {
				Camera = Camera.main;
			}

			if (strokeMaterial == null)
			{
				useShader = false;
				checkedShader = null;
			}
			else if (checkedShader == null || checkedShader != strokeMaterial.shader) {
				useShader = strokeMaterial && VectorShapesUtils.IsStrokeShader(strokeMaterial.shader);
				checkedShader = strokeMaterial ? strokeMaterial.shader : null;
			}

			UpdateMeshCaches ();
		}

		bool UpdateMeshCaches ()
		{
			if (shapes.Count == 0) {

				if (shapeMeshCaches.Count > 0) {
				
					for (int i = 0; i < shapeMeshCaches.Count; i++) {
						shapeMeshCaches [i].Release ();
					}
					shapeMeshCaches.Clear ();
					return true;
				}
				return false;
			}

			bool wasMeshUpdated = false;
			if (shapes.Count != shapeMeshCaches.Count) 
			{
				// init mesh cache if necessary
				while (shapeMeshCaches.Count < shapes.Count) {
					shapeMeshCaches.Add (new ShapeMeshCache ());
				}

				while (shapeMeshCaches.Count > shapes.Count) {

					shapeMeshCaches [shapeMeshCaches.Count - 1].Release();
					shapeMeshCaches.RemoveAt (shapeMeshCaches.Count - 1);
				}

				wasMeshUpdated = true;
			}

			// update data if necessary
			for (int i = 0; i < shapes.Count; i++) {

				shapeMeshCaches [i].shape = shapes[i].ShapeData;
				shapeMeshCaches[i].transform = shapes[i].transform;
				shapeMeshCaches [i].camera = Camera;
				shapeMeshCaches [i].useShader = useShader;

				wasMeshUpdated |= shapeMeshCaches[i].RefreshMesh();

				shapeMeshCaches[i].sourceFillMaterial = fillMaterial;
				shapeMeshCaches[i].sourceStrokeMaterial = strokeMaterial;

				shapeMeshCaches[i].RefreshMaterials();
			}

			return wasMeshUpdated;
		}

		#endregion

	}
}