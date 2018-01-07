using UnityEngine;
using System.Collections;
using VectorShapes;

public class StressTestMultishape : MonoBehaviour {

	public int shapeCount = 1000;
	public bool alternateShaderStyles = false;
	// Use this for initialization
	void Start () {


		for (int i = 0; i < shapeCount; i++)
		{
			GameObject obj = new GameObject("Test Shape");
			obj.transform.parent = transform;
			Shape shape = obj.AddComponent<Shape>();
			shape.ShapeData.ShapeType = ShapeType.Polygon;

			if (alternateShaderStyles)
			{
				shape.ShapeData.StrokeCornerType = i % 2 == 0 ? StrokeCornerType.Bevel : StrokeCornerType.ExtendOrMiter;
				shape.ShapeData.StrokeRenderType = i % 2 == 0 ? StrokeRenderType.ScreenSpacePixels : StrokeRenderType.ScreenSpaceRelativeToScreenHeight;
				shape.ShapeData.SetStrokeWidth(i % 2 == 0 ? 1 : 0.001f);
			}
			else
			{
				shape.ShapeData.StrokeCornerType = StrokeCornerType.Bevel;
				shape.ShapeData.StrokeRenderType = StrokeRenderType.ScreenSpacePixels;
				shape.ShapeData.SetStrokeWidth(1);
			}

			int id1 = shape.ShapeData.AddPolyPoint();
			shape.ShapeData.SetPolyPosition(id1, Random.insideUnitSphere * 8);
			int id2 = shape.ShapeData.AddPolyPoint();
			shape.ShapeData.SetPolyPosition(id2, Random.insideUnitSphere * 8);
			
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
