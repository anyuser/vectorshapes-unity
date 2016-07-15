using UnityEngine;
using System.Collections;
using VectorShapes;

public class DrawTest : MonoBehaviour {

	Shape shape;
	float minDistance = .1f;
	Vector3 lastMousePos;
	Vector3 smoothVelocity;
	public bool defaultDrawFill = true;
	public bool defaultDrawStroke = true;
	public Color defaultFillColor = Color.blue;
	public Color defaultStrokeColor = Color.black;
	public float defaultStrokeWidth = 6;
	public StrokeCornerType strokeCornerType = StrokeCornerType.Bevel;
	public StrokeRenderType strokeRenderType = StrokeRenderType.ScreenSpaceRelativeToScreenHeight;
	public StrokeTextureType strokeTextureType = StrokeTextureType.Absolute;
	public float strokeTextureTiling = 1;

	int shapesCreated = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButtonDown (0) || shape == null) {

			GameObject obj = new GameObject("Draw Shape");
			obj.transform.parent = transform;
			shape = obj.AddComponent<Shape>();
			shape.ShapeData.ShapeType = ShapeType.Polygon;
			shape.ShapeData.IsPolygonStrokeClosed = false;
			shape.ShapeData.IsStrokeEnabled = defaultDrawStroke;
			shape.ShapeData.IsFillEnabled = defaultDrawFill;
			shape.ShapeData.FillColor = defaultFillColor;
			shape.ShapeData.SetStrokeColor(defaultStrokeColor);
			shape.ShapeData.SetStrokeWidth(defaultStrokeWidth);
			shape.ShapeData.StrokeCornerType = strokeCornerType;
			shape.ShapeData.StrokeRenderType = strokeRenderType;
			shape.ShapeData.StrokeTextureType = strokeTextureType;
			shape.ShapeData.StrokeTextureTiling = strokeTextureTiling;
			shape.ShapeData = shape.ShapeData;
			shapesCreated++;
		}

		if (!shape) return;

		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane p = new Plane(Vector3.forward, shape.transform.position);
		float distance;
		p.Raycast(mouseRay, out distance);

		Vector3 mousePositionWorld = mouseRay.GetPoint(distance);
		Vector3 mousePositionLocal = shape.transform.InverseTransformPoint(mousePositionWorld);

		Vector3 velocity = (Input.mousePosition - lastMousePos) / Time.deltaTime;
		lastMousePos = Input.mousePosition;

		smoothVelocity = Vector3.SlerpUnclamped (smoothVelocity, velocity, Time.deltaTime*3);


		if (Input.GetMouseButton (0)) 
		{
			if (shape.ShapeData.GetPolyPointCount () == 0 ||
			   Vector3.Distance (mousePositionLocal, shape.ShapeData.GetPolyPosition (shape.ShapeData.GetPolyPointCount () - 1)) > minDistance) {
				int pointId = shape.ShapeData.AddPolyPoint (mousePositionLocal);
				/*,Mathf.Lerp(.5f,3f, Mathf.InverseLerp(0,700, smoothVelocity.magnitude))*/
				shape.ShapeData.SetPolyPointType (pointId, ShapePointType.Smooth);
			}
		}

		if (Input.GetKey(KeyCode.Backspace)) {

			Destroy(shape);
		}
	}
}
