using UnityEngine;
using System.Collections;
using VectorShapes;

public class StressTest : MonoBehaviour {

	public int pointCount = 2000;

	// Use this for initialization
	void Start () {

		Shape shape = GetComponent<Shape>();
		shape.ShapeData.ClearPolyPoints();

		for (int i = 0; i < pointCount; i++)
		{
			shape.ShapeData.AddPolyPoint( Random.insideUnitSphere*8);
			
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
