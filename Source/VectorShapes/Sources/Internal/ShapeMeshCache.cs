using System.Collections.Generic;
using UnityEngine;

namespace VectorShapes
{
	[System.Serializable]
	class ShapeMeshCache
	{
		bool isDirty;
		int strokeShapeHashId;
		int fillShapeHashId;

		ShapeData _shape;
		public ShapeData shape
		{
			get
			{
				return _shape;
			}
			set
			{
				if (_shape == value)
					return;
				_shape = value;
				isDirty = true;
			}
		}

		Camera _camera;
		public Camera camera
		{
			get
			{
				return _camera;
			}
			set
			{
				if (_camera == value)
					return;
				_camera = value;
				isDirty = true;
			}
		}

		bool _useShader;
		public bool useShader
		{
			get
			{
				return _useShader;
			}
			set
			{
				if (_useShader == value)
					return;
				_useShader = value;
				isDirty = true;
			}
		}

		internal Material fillMaterial { get; private set; }
		internal Material strokeMaterial { get; private set; }

		Material _sourceStrokeMaterial;
		public Material sourceStrokeMaterial
		{
			get
			{
				return _sourceStrokeMaterial;
			}
			set
			{
				if (_sourceStrokeMaterial == value)
					return;
				
				_sourceStrokeMaterial = value;
				strokeMaterial = null;
			}
		}

		Material _sourceFillMaterial;
		public Material sourceFillMaterial
		{
			get
			{
				return _sourceFillMaterial;
			}
			set
			{
				if (_sourceFillMaterial == value)
					return;
				_sourceFillMaterial = value;
				fillMaterial = null;
			}
		}

		Transform _transform;
		public Transform transform
		{
			get
			{
				return _transform;
			}
			set
			{
				if (_transform == value)
					return;
				_transform = value;
				isDirty = true;
			}
		}

		bool _doubleSided;
		public bool doubleSided
		{
			get
			{
				return _doubleSided;
			}
			set
			{
				if (_doubleSided == value)
					return;
				_doubleSided = value;
				isDirty = true;
			}
		}

		Matrix4x4 lastCameraMatrix;
		Matrix4x4 lastCameraProjectionMatrix;
		Matrix4x4 lastTransformMatrix;

		MeshBuilder sharedMeshBuilder = new MeshBuilder();
		MeshBuilder fillMeshBuilder = new MeshBuilder();
		MeshBuilder strokeMeshBuilder = new MeshBuilder();

		//public Mesh fillMesh { get; private set; }
		//public Mesh strokeMesh { get; private set; }
		public Mesh mesh { get; private set; }

		public void Release()
		{
			/*if (strokeMesh)
				Object.DestroyImmediate(strokeMesh);
			if (fillMesh)
				Object.DestroyImmediate(fillMesh);*/
			if (mesh)
				Object.DestroyImmediate(mesh);

			if (strokeMaterial != null)
				Object.DestroyImmediate(strokeMaterial);
			
			if (fillMaterial != null )
				Object.DestroyImmediate(fillMaterial);
		}

		public ShapeMeshCache()
		{
			isDirty = true;
		}

		void CreateMeshesIfNeeded()
		{
			/*if (!strokeMesh)
			{
				strokeMesh = new Mesh();
				strokeMesh.hideFlags = HideFlags.DontSave;
				strokeMesh.MarkDynamic();
				strokeMesh.name = "Generated Mesh";
				//meshFilter.sharedMesh = mesh;
				isDirty = true;
			}
			if (!fillMesh)
			{
				fillMesh = new Mesh();
				fillMesh.hideFlags = HideFlags.DontSave;
				fillMesh.MarkDynamic();
				fillMesh.name = "Generated Mesh";
				//meshFilter.sharedMesh = mesh;
				isDirty = true;
			}*/
			if (!mesh)
			{
				mesh = new Mesh();
				mesh.hideFlags = HideFlags.DontSave;
				mesh.MarkDynamic();
				mesh.name = "Generated Mesh";
				//meshFilter.sharedMesh = mesh;
				isDirty = true;
			}
		}

		public bool RefreshMesh()
		{
			CreateMeshesIfNeeded();

			bool hasCameraChanged = false;
			if ((camera && camera.worldToCameraMatrix != lastCameraMatrix) ||
				(transform && transform.localToWorldMatrix != lastTransformMatrix) ||
				(camera && camera.projectionMatrix != lastCameraProjectionMatrix))
			{
				hasCameraChanged = true;
			}

			bool hasShapeChanged = shape.IsDirty;

			int requiredShapeHashId = shape != null ? shape.HashId : -1;
			bool strokeHashIdChanged = strokeShapeHashId != requiredShapeHashId;
			bool fillHashIdChanged = fillShapeHashId != requiredShapeHashId;

			if (!isDirty && !hasCameraChanged && !fillHashIdChanged && !strokeHashIdChanged && !hasShapeChanged)
				return false;

			// regenerate meshes

			bool regenerateFill = isDirty || hasShapeChanged || fillHashIdChanged;
			bool regenerateStroke = isDirty || hasShapeChanged || strokeHashIdChanged || (hasCameraChanged && !useShader && IsStrokeRenderTypeCameraDependent(shape.StrokeRenderType));

			if (regenerateStroke)
			{
				strokeMeshBuilder.Clear(false);
			}
			if (regenerateFill)
			{
				fillMeshBuilder.Clear(false);
			}


			if (shape != null)
			{

				Profiler.BeginSample("Update Shape Mesh Cache");

				if (regenerateFill)
				{
					Profiler.BeginSample("Generate Fill Mesh");
					if (shape.IsFillEnabled)
					{
						ShapeMeshGenerator.GenerateFillMesh(shape, fillMeshBuilder);
						if (doubleSided)
						{
							fillMeshBuilder.GenerateFlippedTriangles();
						}
					}
					//fillMeshBuilder.ApplyToMesh (fillMesh);
					fillShapeHashId = shape.HashId;
					Profiler.EndSample();
				}

				if (regenerateStroke)
				{
					Profiler.BeginSample("Regenerate Stroke Mesh");
					if (shape.IsStrokeEnabled)
					{

						if (useShader)
						{
							ShapeMeshGenerator.GenerateGPUStrokeMesh(shape, strokeMeshBuilder);
						}
						else
						{
							ShapeMeshGenerator.GenerateCPUStrokeMesh(shape, strokeMeshBuilder, transform, camera);
						}

						if (doubleSided)
						{
							strokeMeshBuilder.GenerateFlippedTriangles();
						}
					}
					//strokeMeshBuilder.ApplyToMesh(strokeMesh);
					strokeShapeHashId = shape.HashId;
					Profiler.EndSample();

				}

				if (regenerateFill || regenerateStroke)
				{
					sharedMeshBuilder.Clear(false);
					sharedMeshBuilder.AddStream(fillMeshBuilder);
					sharedMeshBuilder.AddStream(strokeMeshBuilder);
					sharedMeshBuilder.ApplyToMesh(mesh);
				}
				isDirty = false;
				Profiler.EndSample();
			}
			else {
				fillShapeHashId = -1;
				strokeShapeHashId = -1;
			}

			if (!useShader)
			{
				lastCameraMatrix = camera.worldToCameraMatrix;
				lastCameraProjectionMatrix = camera.projectionMatrix;
				lastTransformMatrix = transform.localToWorldMatrix;
			}

			return true;
		}

		public void RefreshMaterials()
		{
			Profiler.BeginSample("RefreshMaterials");

			CreateMaterialsIfNeeded();
			//SetCachedKeywordsBase(originalStrokeKeywords);

			if (strokeMaterial)
			{
				Profiler.BeginSample("Set stroke shader keywords");
				strokeMaterial.shaderKeywords = GetCachedKeywords(shape.StrokeCornerType, shape.StrokeRenderType);
				Profiler.EndSample();

				strokeMaterial.SetFloat("_StrokeMiterLimit", shape.StrokeMiterLimit);
			}

			if (fillMaterial)
			{
				Vector2 tilingScale = shape.FillTextureTiling;
				Vector2 tilingOffset = shape.FillTextureOffset;

				if (shape.FillTextureMode == FillTextureMode.Normalized)
				{
					Vector2 fullSize = (Vector2)mesh.bounds.size;
					if (fullSize.x > 0 && fullSize.y > 0)
					{
						tilingScale = new Vector2(tilingScale.x / fullSize.x, tilingScale.y / fullSize.y);

						tilingOffset.Scale(fullSize);
						tilingOffset += (Vector2)mesh.bounds.center;
						tilingOffset.Scale(tilingScale);
						tilingOffset += Vector2.one * 0.5f;
					}
				}

				fillMaterial.SetTextureOffset("_MainTex", -tilingOffset);
				fillMaterial.SetTextureScale("_MainTex", tilingScale);

			}
			Profiler.EndSample();
		}

		void CreateMaterialsIfNeeded()
		{
			if (fillMaterial == null || fillMaterial.shader != sourceFillMaterial.shader)
			{
				if (fillMaterial != null)
					Object.DestroyImmediate(fillMaterial);
				
				if (sourceFillMaterial != null)
					fillMaterial = new Material(sourceFillMaterial);
				else
					fillMaterial = null;
			}

			if (strokeMaterial == null || strokeMaterial.shader != sourceStrokeMaterial.shader)
			{
				if (strokeMaterial != null)
					Object.DestroyImmediate(strokeMaterial);
				
				if (sourceStrokeMaterial != null)
				{
					strokeMaterial = new Material(sourceStrokeMaterial);
					SetCachedKeywordsBase(strokeMaterial.shaderKeywords);
				}
				else
				{
					strokeMaterial = null;
				}
			}
		}

		bool IsStrokeRenderTypeCameraDependent(StrokeRenderType strokeRenderType)
		{
			if (strokeRenderType == StrokeRenderType.ShapeSpace)
				return false;
			return true;
		}
		#region keywords caching
		const string STROKE_CORNER_BEVEL = "STROKE_CORNER_BEVEL";
		const string STROKE_CORNER_EXTEND_OR_CUT = "STROKE_CORNER_EXTEND_OR_CUT";
		const string STROKE_CORNER_EXTEND_OR_MITER = "STROKE_CORNER_EXTEND_OR_MITER";

		const string STROKE_RENDER_SCREEN_SPACE_PIXELS = "STROKE_RENDER_SCREEN_SPACE_PIXELS";
		const string STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT = "STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT";
		const string STROKE_RENDER_SHAPE_SPACE = "STROKE_RENDER_SHAPE_SPACE";


		string[] cachedOriginalKeywords = new string[0];
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

		string[] GetCachedKeywords(StrokeCornerType cornerType, StrokeRenderType renderType)
		{
			int hash = (int)cornerType + 1000 * (int)renderType;
			if (!cachedKeywords.ContainsKey(hash))
			{
				string[] k = new string[cachedOriginalKeywords.Length + 2];
				for (int i = 0; i < cachedOriginalKeywords.Length; i++)
				{
					k[i] = cachedOriginalKeywords[i];
				}
				if (cornerType == StrokeCornerType.Bevel)
				{
					k[cachedOriginalKeywords.Length + 0] = STROKE_CORNER_BEVEL;
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

