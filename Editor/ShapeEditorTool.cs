using Unity.Collections;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using VectorShapes;

namespace VectorShapesEditor
{
	[EditorTool("Edit Shape Tool", typeof(Shape))]
	public class ShapeEditorTool : EditorTool
	{
		public Texture2D m_ToolIcon;

		GUIContent m_IconContent;

		void OnEnable()
		{
			m_IconContent = new GUIContent()
			{
				image = m_ToolIcon,
				text = "Edit Shape Tool",
				tooltip = "Edit Shape Tool"
			};
		}

		public override GUIContent toolbarIcon => m_IconContent;

		int currentPointId;

		public override void OnToolGUI(EditorWindow window)
		{
			base.OnToolGUI(window);
		
			var shape = target as Shape;
			if (shape == null || shape.ShapeData == null)
				return;

			if (shape.ShapeData == null || shape.ShapeData.GetPolyPointCount() == 0)
			{
				currentPointId = -1;
				ShapeEditorUtils.SetDataDirty(shape);
			}
			else if (currentPointId > shape.ShapeData.GetPolyPointCount() - 1)
			{
				currentPointId = shape.ShapeData.GetPolyPointCount() - 1;
				ShapeEditorUtils.SetDataDirty(shape);
			}

			if (currentPointId >= 0 && currentPointId < shape.ShapeData.GetPolyPointCount())
			{
				ShapeEditorUtils.DrawPointEditWindow(window, shape, currentPointId);
			}

			Handles.color = Color.blue;
			Handles.matrix = shape.transform.localToWorldMatrix;

			var inTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			var outTangentControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			var pointControlIds = new NativeArray<int>(shape.ShapeData.GetPolyPointCount(), Allocator.Temp);
			ShapeEditorUtils.Fill(inTangentControlIds, -1);
			ShapeEditorUtils.Fill(outTangentControlIds, -1);
			ShapeEditorUtils.Fill(pointControlIds, -1);

			ShapeEditorUtils.DrawTangentLines(shape);
			ShapeEditorUtils.DrawPositionHandles(shape, pointControlIds);
			ShapeEditorUtils.DrawTangentHandles(shape, inTangentControlIds, outTangentControlIds);

			var newPoint = ShapeEditorUtils.GetCurrentSelectedPoint(pointControlIds, inTangentControlIds, outTangentControlIds);
			if (newPoint != -1)
			{
				currentPointId = newPoint;
				ShapeEditorUtils.SetDataDirty(shape);
			}

			inTangentControlIds.Dispose();
			outTangentControlIds.Dispose();
			pointControlIds.Dispose();
		}
	}
}