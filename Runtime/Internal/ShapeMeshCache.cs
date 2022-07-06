using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using VectorShapes;
using Object = UnityEngine.Object;

namespace VectorShapesInternal
{
	[System.Serializable]
	public class ShapeMeshCache
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
				_canvasRenderer = null;
				isDirty = true;
			}
		}

		CanvasRenderer _canvasRenderer;
		public CanvasRenderer canvasRenderer
		{
			get
			{
				if (transform == null)
					return null;
				
				if (_canvasRenderer == null)
					_canvasRenderer = transform.GetComponent<CanvasRenderer>();
				return _canvasRenderer;
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

		public void Refresh()
		{
			RefreshMesh();
			RefreshMaterials();
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

					if (canvasRenderer)
					{
						canvasRenderer.SetMesh(mesh);
						canvasRenderer.DisableRectClipping();
					}
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

		public void ClearKeywords()
		{
			instanceKeywords = null;
		}

		void RefreshMaterials()
		{
			Profiler.BeginSample("RefreshMaterials");

			CreateMaterialsIfNeeded();
			//SetCachedKeywordsBase(originalStrokeKeywords);

			if (strokeMaterial)
			{
				Profiler.BeginSample("Set stroke shader keywords");

				SetCornerRenderKeywords(strokeMaterial,shape.StrokeCornerType);
				SetStrokeRenderKeywords(strokeMaterial,shape.StrokeRenderType);
				
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

			if (canvasRenderer)
			{
				if (canvasRenderer.materialCount != mesh.subMeshCount)
					canvasRenderer.materialCount = mesh.subMeshCount;

				if ( canvasRenderer.materialCount > 0 && canvasRenderer.GetMaterial(0) != fillMaterial)
					canvasRenderer.SetMaterial(fillMaterial, 0);

				if (canvasRenderer.materialCount > 1 && canvasRenderer.GetMaterial(1) != strokeMaterial)
					canvasRenderer.SetMaterial(strokeMaterial, 1);
				//canvasRenderer.SetColor(Color.white);
				//canvasRenderer.SetAlpha(1);
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
					instanceKeywords = null;
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
		const string STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA = "STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA";

		string[] instanceKeywords = null;

		void ResetAllKeywords(Material m)
		{
			m.DisableKeyword(STROKE_CORNER_BEVEL);
			m.DisableKeyword(STROKE_CORNER_EXTEND_OR_CUT);
			m.DisableKeyword(STROKE_CORNER_EXTEND_OR_MITER);
			m.DisableKeyword(STROKE_RENDER_SCREEN_SPACE_PIXELS);
			m.DisableKeyword(STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT);
			m.DisableKeyword(STROKE_RENDER_SHAPE_SPACE);
			m.DisableKeyword(STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA);
		}

		bool SetBaseInstanceKeywords()
		{
			bool changed = false;
			string[] sourceKeywords = sourceStrokeMaterial.shaderKeywords;

			if (instanceKeywords == null || instanceKeywords.Length != sourceKeywords.Length + 2)
			{
				instanceKeywords = new string[sourceKeywords.Length + 2];
				changed = true;
			}

			for (int i = 0; i < sourceKeywords.Length; i++)
			{
				if (instanceKeywords[i] != sourceKeywords[i])
				{
					instanceKeywords[i] = sourceKeywords[i];
					changed = true;
				}
			}

			return changed;
		}

		bool UpdateInstanceKeywords()
		{
			bool changed = false;


			return changed;
		}

		#endregion

		static string GetCornerTypeKeyword(StrokeCornerType strokeCornerType)
		{
			switch (strokeCornerType)
			{
				case StrokeCornerType.Bevel:
					return STROKE_CORNER_BEVEL;

				case StrokeCornerType.ExtendOrCut:
					return STROKE_CORNER_EXTEND_OR_CUT;

				case StrokeCornerType.ExtendOrMiter:
					return STROKE_CORNER_EXTEND_OR_MITER;
			}

			return null;
		}

		static string GetRenderTypeKeyword(StrokeRenderType strokeRenderType)
		{
			switch (strokeRenderType)
			{
				case StrokeRenderType.ShapeSpace:
					return STROKE_RENDER_SHAPE_SPACE;

				case StrokeRenderType.ScreenSpacePixels:
					return STROKE_RENDER_SCREEN_SPACE_PIXELS;

				case StrokeRenderType.ScreenSpaceRelativeToScreenHeight:
					return STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT;

				case StrokeRenderType.ShapeSpaceFacingCamera:
					return STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA;
			}
			return null;
		}
		
		
		static StrokeRenderType[] allRenderTypes = new []
		{
			StrokeRenderType.ShapeSpace,
			StrokeRenderType.ScreenSpacePixels,
			StrokeRenderType.ScreenSpaceRelativeToScreenHeight,
			StrokeRenderType.ShapeSpaceFacingCamera
		};

		void SetStrokeRenderKeywords(Material mat, StrokeRenderType renderType)
		{
			
			for (int i = 0; i < allRenderTypes.Length; i++)
			{
				var localKeyword = mat.shader.keywordSpace.FindKeyword(GetRenderTypeKeyword(allRenderTypes[i]));
				mat.SetKeyword( localKeyword,allRenderTypes[i] == renderType);
			}
		}

		static StrokeCornerType[] allCornerTypes = new []
		{
			StrokeCornerType.Bevel,
			StrokeCornerType.ExtendOrCut,
			StrokeCornerType.ExtendOrMiter,
		};
		void SetCornerRenderKeywords(Material mat, StrokeCornerType cornerType)
		{
			
			for (int i = 0; i < allCornerTypes.Length; i++)
			{
				var localKeyword = mat.shader.keywordSpace.FindKeyword(GetCornerTypeKeyword(allCornerTypes[i]));
				mat.SetKeyword( localKeyword,allCornerTypes[i] == cornerType);
			}
		}
	}
}

