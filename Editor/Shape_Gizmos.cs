using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VectorShapes;
using VectorShapesInternal;

namespace VectorShapesEditor
{
	internal class Shape_Gizmos
	{
		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
		static void DrawGizmo(Shape src, GizmoType gizmoType)
		{
			
			if (SceneView.currentDrawingSceneView == null)
				return;
			
			bool drawWireframe = SceneView.currentDrawingSceneView.cameraMode == SceneView.GetBuiltinCameraMode(DrawCameraMode.Wireframe) ||
			                     SceneView.currentDrawingSceneView.cameraMode == SceneView.GetBuiltinCameraMode(DrawCameraMode.TexturedWire);
			Color wireframeColor = Color.white;
			Color surfaceColor = Color.clear;
			if (SceneView.currentDrawingSceneView.cameraMode == SceneView.GetBuiltinCameraMode(DrawCameraMode.TexturedWire))
			{
				surfaceColor = new Color(1, 1, 1, 0.5f);
			}
			Gizmos.matrix = src.transform.localToWorldMatrix;

			ShapeMeshCache cache = src.GizmoCache;
			cache.camera = SceneView.currentDrawingSceneView.camera;
			cache.RefreshMesh();

			/*if (cache.strokeMesh.subMeshCount > 0 && cache.strokeMesh.vertexCount > 0)
			{
				Gizmos.color = surfaceColor;
				Gizmos.DrawMesh(cache.strokeMesh);

				if (drawWireframe)
				{
					Gizmos.color = wireframeColor;
					Gizmos.DrawWireMesh(cache.strokeMesh);
				}
			}
			if (cache.fillMesh.subMeshCount > 0 && cache.fillMesh.vertexCount > 0)
			{
				Gizmos.color = surfaceColor;
				Gizmos.DrawMesh(cache.fillMesh);

				if (drawWireframe)
				{
					Gizmos.color = wireframeColor;
					Gizmos.DrawWireMesh(cache.fillMesh);
				}
			}*/
			if (cache.mesh.subMeshCount > 0 && cache.mesh.vertexCount > 0)
			{
				Gizmos.color = surfaceColor;
				Gizmos.DrawMesh(cache.mesh);

				if (drawWireframe)
				{
					Gizmos.color = wireframeColor;
					Gizmos.DrawWireMesh(cache.mesh);
				}
			}
		}
	}
}

