using VectorShapes;
using UnityEngine;
using UnityEditor;

namespace VectorShapesEditor
{
	internal class ShapeMenuItems
	{

		[MenuItem("GameObject/2D Object/Rectangle Shape")]
		static void CreateRectShapeObj(MenuCommand cmd)
		{
			Shape s = CreateShapeObj("Rectangle Shape", cmd);
			s.ShapeData.ShapeType = ShapeType.Rectangle;
		}

		[MenuItem("GameObject/2D Object/Circle Shape")]
		static void CreateCircleShapeObj(MenuCommand cmd)
		{
			Shape s = CreateShapeObj("Circle Shape", cmd);
			s.ShapeData.ShapeType = ShapeType.Circle;
		}

		[MenuItem("GameObject/2D Object/Polygon Shape")]
		static void CreatePolygonShapeObj(MenuCommand cmd)
		{
			Shape s = CreateShapeObj("Polygon Shape", cmd);
			s.ShapeData = DefaultShapes.CreatePolygon(1, 5);
		}

		[MenuItem("GameObject/2D Object/Polyline Shape")]
		static void CreatePolyLineShapeObj(MenuCommand cmd)
		{
			Shape s = CreateShapeObj("Polyline Shape", cmd);
			s.ShapeData.ShapeType = ShapeType.Polygon;
			s.ShapeData.AddPolyPoint(Vector3.zero);
			s.ShapeData.AddPolyPoint(Vector3.up);
			s.ShapeData.AddPolyPoint(Vector3.right);
			s.ShapeData.AddPolyPoint(Vector3.right + Vector3.up);
			s.ShapeData.IsPolygonStrokeClosed = false;
			s.ShapeData.IsFillEnabled = false;
		}

		static Shape CreateShapeObj(string name, MenuCommand cmd)
		{
			return CreateShapeObj(name, (GameObject)cmd.context);
		}

		public static Shape CreateShapeObj(string name, GameObject parent = null)
		{
			GameObject obj = new GameObject(name);
			Shape s = obj.AddComponent<Shape>();
			GameObjectUtility.SetParentAndAlign(obj, parent);
			if (s.ShapeRenderer == null)
				obj.AddComponent<ShapeRenderer>();

			Undo.RegisterCreatedObjectUndo(obj, "Create shape");
			Selection.activeObject = obj;

			return s;
		}

		public static Shape CreateShapeObj(ShapeAsset shapeAsset, GameObject parent = null)
		{
			var s = CreateShapeObj(shapeAsset.name);
			s.ShapeAsset = shapeAsset;

			return s;
		}
	}
}

