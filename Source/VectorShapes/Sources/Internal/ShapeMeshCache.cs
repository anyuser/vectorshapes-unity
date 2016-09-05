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

		MeshBuilder fillMeshBuilder = new MeshBuilder();
		MeshBuilder strokeMeshBuilder = new MeshBuilder();

		public Mesh fillMesh { get; private set; }
		public Mesh strokeMesh { get; private set; }

		public void Release()
		{
			if (strokeMesh)
				Object.DestroyImmediate(strokeMesh);
			if (fillMesh)
				Object.DestroyImmediate(fillMesh);
		}

		public ShapeMeshCache()
		{
			isDirty = true;
		}

		void CreateMeshesIfNeeded()
		{
			if (!strokeMesh)
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
			}
		}

		public bool Refresh()
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

			if (shape != null) {

				Profiler.BeginSample ("Update Shape Mesh Cache");

				if (regenerateFill) {
					Profiler.BeginSample ("Generate Fill Mesh");
					if (shape.IsFillEnabled) {
						ShapeMeshGenerator.GenerateFillMesh (shape, fillMeshBuilder);
						if (doubleSided)
							fillMeshBuilder.GenerateFlippedTriangles ();
					}
					fillMeshBuilder.ApplyToMesh (fillMesh);
					fillShapeHashId = shape.HashId;
					Profiler.EndSample ();
				}

				if (regenerateStroke) {
					Profiler.BeginSample ("Regenerate Stroke Mesh");
					if (shape.IsStrokeEnabled) {
						if (useShader)
							ShapeMeshGenerator.GenerateGPUStrokeMesh (shape, strokeMeshBuilder);
						else
							ShapeMeshGenerator.GenerateCPUStrokeMesh (shape, strokeMeshBuilder, transform, camera);

						if (doubleSided)
							strokeMeshBuilder.GenerateFlippedTriangles ();
					}
					strokeMeshBuilder.ApplyToMesh (strokeMesh);
					strokeShapeHashId = shape.HashId;
					Profiler.EndSample ();

				}
				isDirty = false;
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

			Profiler.EndSample ();

			return true;
		}

		bool IsStrokeRenderTypeCameraDependent(StrokeRenderType strokeRenderType)
		{
			if (strokeRenderType == StrokeRenderType.ShapeSpace)
				return false;
			return true;
		}
}
}

