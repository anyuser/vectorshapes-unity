using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VectorShapes;

namespace VectorShapesEditor
{
	internal partial class ShapeEditor
	{
		[DrawGizmo(GizmoType.Active | GizmoType.Selected | GizmoType.NotInSelectionHierarchy | GizmoType.Pickable | GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
		static void DrawGizmo(Shape src, GizmoType gizmoType)
		{
			bool drawWireframe = false;
			if (SceneView.currentDrawingSceneView.renderMode == DrawCameraMode.Wireframe ||
				SceneView.currentDrawingSceneView.renderMode == DrawCameraMode.TexturedWire)
			{
				drawWireframe = true;
			}
			Color wireframeColor = Color.white;
			Color surfaceColor = Color.clear;
			if (SceneView.currentDrawingSceneView.renderMode == DrawCameraMode.TexturedWire)
			{
				surfaceColor = new Color(1, 1, 1, 0.5f);
			}
			Gizmos.matrix = src.transform.localToWorldMatrix;

			ShapeMeshCache cache = src.GizmoCache;
			cache.camera = SceneView.currentDrawingSceneView.camera;
			cache.Refresh();

			if (cache.strokeMesh.subMeshCount > 0 && cache.strokeMesh.vertexCount > 0)
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
			}
		}
	}
}

