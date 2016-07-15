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

		const string STROKE_CORNER_BEVEL = "STROKE_CORNER_BEVEL";
		const string STROKE_CORNER_EXTEND_OR_CUT = "STROKE_CORNER_EXTEND_OR_CUT";
		const string STROKE_CORNER_EXTEND_OR_MITER = "STROKE_CORNER_EXTEND_OR_MITER";

		const string STROKE_RENDER_SCREEN_SPACE_PIXELS = "STROKE_RENDER_SCREEN_SPACE_PIXELS";
		const string STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT = "STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT";
		const string STROKE_RENDER_SHAPE_SPACE = "STROKE_RENDER_SHAPE_SPACE";

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
			cachedKeywords.Clear();
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
			
			Vector2 originalTilingScale = fillMaterial.GetTextureScale("_MainTex");
			Vector2 originalTilingOffset = fillMaterial.GetTextureOffset("_MainTex");
			float? originalStrokeMiterLimit = strokeMaterial.HasProperty("_StrokeMiterLimit") ? (float?)strokeMaterial.GetFloat("_StrokeMiterLimit") : null;
			string[] originalStrokeKeywords = strokeMaterial.shaderKeywords;

			SetCachedKeywordsBase(originalStrokeKeywords);

			for (int i = 0; i < shapeMeshCaches.Count; i++)
			{

				Profiler.BeginSample("Draw FillMaterial");
				if (fillMaterial)
				{
					Vector2 tilingScale = shapes[i].ShapeData.FillTextureTiling;
					Vector2 tilingOffset = shapes[i].ShapeData.FillTextureOffset;

					if (shapes[i].ShapeData.FillTextureMode == FillTextureMode.Normalized)
					{
						Vector2 fullSize = (Vector2)shapeMeshCaches[i].fillMesh.bounds.size;
						if (fullSize.x > 0 && fullSize.y > 0)
						{
							tilingScale = new Vector2(tilingScale.x / fullSize.x, tilingScale.y / fullSize.y);

							tilingOffset.Scale(fullSize);
							tilingOffset += (Vector2)shapeMeshCaches[i].fillMesh.bounds.center;
							tilingOffset.Scale(tilingScale);
							tilingOffset += Vector2.one * 0.5f;
						}
					}

					fillMaterial.SetTextureOffset("_MainTex", -tilingOffset);
					fillMaterial.SetTextureScale("_MainTex",tilingScale);
					fillMaterial.SetPass(0);
					Graphics.DrawMeshNow(shapeMeshCaches[i].fillMesh, shapes[i].transform.localToWorldMatrix, 0);
				}
				Profiler.EndSample();

				Profiler.BeginSample("Draw StrokeMaterial");
				if (strokeMaterial)
				{
					Profiler.BeginSample("Set stroke shader keywords");
					strokeMaterial.shaderKeywords = GetCachedKeywords(shapes[i].ShapeData.StrokeCornerType, shapes[i].ShapeData.StrokeRenderType);
					Profiler.EndSample();

					strokeMaterial.SetFloat("_StrokeMiterLimit", shapes[i].ShapeData.StrokeMiterLimit);
					strokeMaterial.SetPass(0);
					Graphics.DrawMeshNow(shapeMeshCaches[i].strokeMesh, shapes[i].transform.localToWorldMatrix, 1);
				}
				Profiler.EndSample();
			}

			// reset material values
			fillMaterial.SetTextureOffset("_MainTex", originalTilingOffset);
			fillMaterial.SetTextureScale("_MainTex", originalTilingScale);
			if( originalStrokeMiterLimit.HasValue)
				strokeMaterial.SetFloat("_StrokeMiterLimit", originalStrokeMiterLimit.Value);

			strokeMaterial.shaderKeywords = originalStrokeKeywords;

			Profiler.EndSample();
		}

		void RefreshMesh ()
		{
			if (Camera == null) {
				Camera = Camera.main;
			}

			if (checkedShader == null || checkedShader != strokeMaterial.shader) {
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

				wasMeshUpdated |= shapeMeshCaches [i].Refresh ();
			}

			return wasMeshUpdated;
		}

		#endregion

		#region keywords caching
		string[] cachedOriginalKeywords;
		Dictionary<int, string[]> cachedKeywords = new Dictionary<int, string[]>();
		void SetCachedKeywordsBase(string[] originalKeywords)
		{
			bool clearCache = false;
			if (cachedOriginalKeywords == null || originalKeywords.Length != cachedOriginalKeywords.Length)
			{
				cachedOriginalKeywords = new string[originalKeywords.Length];
				clearCache = true;
			}

			for (int i = 0; i < originalKeywords.Length; i++)
			{
				if (cachedOriginalKeywords[i] != originalKeywords[i])
				{
					cachedOriginalKeywords[i] = originalKeywords[i];
					clearCache = true;
				}
			}

			if (clearCache)
			{
				cachedKeywords.Clear();
			}
		}

		string[] GetCachedKeywords( StrokeCornerType cornerType, StrokeRenderType renderType)
		{
			int hash = (int)cornerType + 1000 * (int)renderType;
			if (!cachedKeywords.ContainsKey(hash))
			{
				string[] k = new string[cachedOriginalKeywords.Length+2];
				for (int i = 0; i < cachedOriginalKeywords.Length; i++)
				{
					k[i] = cachedOriginalKeywords[i];
				}
				if (cornerType == StrokeCornerType.Bevel)
				{
					k[cachedOriginalKeywords.Length+0] = STROKE_CORNER_BEVEL;
				}
				else if (cornerType == StrokeCornerType.ExtendOrCut)
				{
					k[cachedOriginalKeywords.Length + 0] = STROKE_CORNER_EXTEND_OR_CUT;
				}
				else if (cornerType == StrokeCornerType.ExtendOrMiter)
				{
					k[cachedOriginalKeywords.Length + 0] = STROKE_CORNER_EXTEND_OR_MITER;
				}
				if (renderType == StrokeRenderType.ShapeSpace)
				{
					k[cachedOriginalKeywords.Length + 1] = STROKE_RENDER_SHAPE_SPACE;
				}
				else if (renderType == StrokeRenderType.ScreenSpacePixels)
				{
					k[cachedOriginalKeywords.Length + 1] = STROKE_RENDER_SCREEN_SPACE_PIXELS;
				}
				else if (renderType == StrokeRenderType.ScreenSpaceRelativeToScreenHeight)
				{
					k[cachedOriginalKeywords.Length + 1] = STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT;
				}
				cachedKeywords.Add(hash, k);
			}
			return cachedKeywords[hash];
		}
		#endregion
	}
}