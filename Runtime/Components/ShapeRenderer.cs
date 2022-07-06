using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using VectorShapesInternal;

namespace VectorShapes
{
	[ExecuteInEditMode]
	[DefaultExecutionOrder(9999)]
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

		#if UNITY_EDITOR
		void Reset()
		{
			fillMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Packages/ch.mariov.vectorshapes/Materials/VectorShapes-Fill-Default.mat");
			strokeMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Packages/ch.mariov.vectorshapes/Materials/VectorShapes-Stroke-Default.mat");
		}
		#endif
		
		void OnEnable()
		{
			var foundShapes = GetComponentsInChildren<Shape>();
			for (int i = 0; i < foundShapes.Length; i++)
			{
				foundShapes[i].UpdateShapeRenderer();
			}

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
		}

		void Start()
		{
		}

		void OnDestroy()
		{
		}

		void LateUpdate ()
		{
			RefreshMesh ();
			Draw();
		}

		void OnRenderObject()
		{
			//DrawNow ();
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

		string[] lastStrokeKeywords;
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
			else if (checkedShader == null || checkedShader != strokeMaterial.shader) 
			{
				useShader = strokeMaterial && VectorShapesUtils.IsStrokeShader(strokeMaterial.shader);
				checkedShader = strokeMaterial ? strokeMaterial.shader : null;
			}

			// init mesh cache if necessary
			while (shapeMeshCaches.Count < shapes.Count) {
				shapeMeshCaches.Add (new ShapeMeshCache ());
			}

			while (shapeMeshCaches.Count > shapes.Count) {

				shapeMeshCaches [shapeMeshCaches.Count - 1].Release();
				shapeMeshCaches.RemoveAt (shapeMeshCaches.Count - 1);
			}

			bool strokeMaterialHasChanged = false;
			var strokeKeywords = strokeMaterial ? strokeMaterial.shaderKeywords : null;
			if (lastStrokeKeywords == null || (strokeKeywords != null ? strokeKeywords.Length : 0) != lastStrokeKeywords.Length)
			{
				strokeMaterialHasChanged = true;
			}
			else
			{
				for (int i = 0; i < strokeKeywords.Length; i++)
				{
					if (strokeKeywords[i] != lastStrokeKeywords[i])
					{
						strokeMaterialHasChanged = true;
						break;
					}
				}
			}
			lastStrokeKeywords = strokeKeywords;

			// update data if necessary
			for (int i = 0; i < shapeMeshCaches.Count; i++) {

				shapeMeshCaches[i].shape = shapes[i].ShapeData;
				shapeMeshCaches[i].transform = shapes[i].transform;
				shapeMeshCaches[i].camera = Camera;
				shapeMeshCaches[i].useShader = useShader;
				shapeMeshCaches[i].sourceFillMaterial = fillMaterial;

				if(strokeMaterialHasChanged)
					shapeMeshCaches[i].sourceStrokeMaterial = null;
				
				shapeMeshCaches[i].sourceStrokeMaterial = strokeMaterial;

				shapeMeshCaches[i].Refresh();
			}
		}
		void Draw()
		{
			Profiler.BeginSample("Draw");
			for (int i = 0; i < shapeMeshCaches.Count; i++)
			{
				DrawShape(shapeMeshCaches[i]);
			}
			Profiler.EndSample();
		}

		void DrawNow()
		{
			Profiler.BeginSample("DrawNow");
			if ((1 << gameObject.layer & Camera.current.cullingMask) == 0)
				return;

			for (int i = 0; i < shapeMeshCaches.Count; i++)
			{
				DrawShapeNow(shapeMeshCaches[i]);
			}

			Profiler.EndSample();
		}

		void DrawShape(ShapeMeshCache shapeCache) 
		{
			if (shapeCache.canvasRenderer != null )
				return;
			
			if (shapeCache.fillMaterial != null && shapeCache.mesh.subMeshCount > 0)
			{
				Graphics.DrawMesh(shapeCache.mesh, shapeCache.transform.localToWorldMatrix, shapeCache.fillMaterial, gameObject.layer, null, 0);
			}
			if (shapeCache.strokeMaterial != null && shapeCache.mesh.subMeshCount > 1)
			{
				Graphics.DrawMesh(shapeCache.mesh, shapeCache.transform.localToWorldMatrix, shapeCache.strokeMaterial, gameObject.layer, null, 1);
			}
		}

		void DrawShapeNow(ShapeMeshCache shapeCache)
		{
			if (shapeCache.canvasRenderer != null && Camera.current.cameraType != CameraType.SceneView)
				return;
			
			Profiler.BeginSample("Draw FillMaterial");
			if (shapeCache.fillMaterial != null)
			{
				shapeCache.fillMaterial.SetPass(0);
				Graphics.DrawMeshNow(shapeCache.mesh, shapeCache.transform.localToWorldMatrix, 0);
			}
			Profiler.EndSample();

			Profiler.BeginSample("Draw StrokeMaterial");
			if (shapeCache.strokeMaterial != null)
			{
				shapeCache.strokeMaterial.SetPass(0);
				Graphics.DrawMeshNow(shapeCache.mesh, shapeCache.transform.localToWorldMatrix, 1);
			}

			Profiler.EndSample();
		}

		#endregion

		internal ShapeMeshCache GetMeshCache(Shape shape)
		{
			return shapeMeshCaches[shapes.IndexOf(shape)];
		}
	}
}