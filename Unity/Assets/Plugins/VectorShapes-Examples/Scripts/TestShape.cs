using UnityEngine;
using System.Collections;
using VectorShapes;

public class TestShape : MonoBehaviour {

	// Use this for initialization
	void Start () {


		/*	var shapeList = GetComponent<ShapeList> ();

			// rect
			ShapeData shape1 = DefaultShapes.Rect (new Vector3 (-3, 0), Quaternion.Euler (0, 0, 0), 3, 3);
			shape1.fillColor = Color.red;
			shape1.SetStrokeWidth( 5);
			shapeList.shapes.Add (shape1);

			// circle
			ShapeData shape4 = DefaultShapes.Circle (new Vector3 (5, 5), Quaternion.Euler (0, 0, 0), 2);
			shape4.fillColor = Color.blue;
			shape4.SetStrokeWidth( 5f);
			shapeList.shapes.Add (shape4);

			// circle2
			ShapeData shape6 = DefaultShapes.Circle (new Vector3 (-5, -5), Quaternion.Euler (30, 0, 0), 3,32);
			shape6.fillColor = Color.blue;
			shape6.SetStrokeWidth( 5f);
			shapeList.shapes.Add (shape6);

			// polygon
			ShapeData shape3 = new ShapeData ();
			shape3.type = ShapeType.Polygon;
			shape3.fillColor = Color.blue;
			shape3.SetStrokeWidth(5f);
			shape3.AddPoint(new Vector3(0,3+0));
			shape3.AddPoint(new Vector3(1,3+1));
			shape3.AddPoint(new Vector3(2,3+1));
			shape3.AddPoint(new Vector3(1,3+0));
			shape3.AddPoint(new Vector3(-.5f,3-1));
			shapeList.shapes.Add (shape3);

			// sinewave
			ShapeData shape2 = new ShapeData ();
			shape2.drawFill = false;
			shape2.drawStroke = true;
			shape2.SetStrokeWidth( 5);
			Vector3 start = new Vector3 (-3, -3);
			shape2.AddPoint (start);
			int c = 20;
			for (int i = 1; i < c; i++) {

				shape2.AddPoint(start + new Vector3(i*.3f,Mathf.Sin(Mathf.Lerp(0,Mathf.PI*2,Mathf.InverseLerp(0,c,i)))));
			}
			shapeList.shapes.Add (shape2);*/

	}
}
