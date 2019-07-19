using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using VectorShapesInternal;

namespace VectorShapes
{
	[System.Serializable]
	public class ShapeData : ISerializationCallbackReceiver
	{

		#region public properties

		/// <summary>
		/// Shape Type
		/// </summary>
		/// <value>The shape type.</value>
		public ShapeType ShapeType { get { return shapeType; } set { shapeType = value; MarkDirty(); } }

		/// <summary>
		/// Polygon Dimension (2D or 3D)
		/// </summary>
		/// <value>The polygon dimension.</value>
		public ShapePolyDimension PolyDimension { get { return polyDimension; } set { polyDimension = value; MarkDirty(); } }

		/// <summary>
		/// Is polygon stroke closed?
		/// </summary>
		/// <value>Is the polygon stroke closed?</value>
		public bool IsPolygonStrokeClosed { get { return isPolygonStrokeClosed; } set { isPolygonStrokeClosed = value; MarkDirty(); } }

		/// <summary>
		/// Offset of circle or rectangle shape center
		/// </summary>
		/// <value>The offset.</value>
		public Vector3 ShapeOffset { get { return shapeOffset; } set { shapeOffset = value; MarkDirty(); } }

		/// <summary>
		/// Size of rectangle/ellipse shape
		/// </summary>
		/// <value>The size.</value>
		public Vector2 ShapeSize { get { return shapeSize; } set { shapeSize = value; MarkDirty(); } }

		/// <summary>
		/// Is the fill enabled?
		/// </summary>
		/// <value>Is the fill enabled?</value>
		public bool IsFillEnabled { get { return isFillEnabled; } set { isFillEnabled = value; MarkDirty(); } }

		/// <summary>
		/// Fill color
		/// </summary>
		/// <value>The color of the fill.</value>
		public Color FillColor { get { return fillColor; } set { fillColor = value; MarkDirty(); } }

		/// <summary>
		/// Fill normal
		/// </summary>
		/// <value>The fill normal.</value>
		public Vector3 FillNormal { get { return fillNormal; } set { fillNormal = value; MarkDirty(); } }

		/// <summary>
		/// Fill texture mode
		/// </summary>
		/// <value>The fill texture mode.</value>
		public FillTextureMode FillTextureMode { get { return fillTextureMode; } set { fillTextureMode = value; MarkDirty(); } }

		/// <summary>
		/// Fill texture offset
		/// </summary>
		/// <value>The fill texture offset.</value>
		public Vector2 FillTextureOffset { get { return fillTextureOffset; } set { fillTextureOffset = value; MarkDirty(); } }

		/// <summary>
		/// Fill texture tiling
		/// </summary>
		/// <value>The fill texture tiling.</value>
		public Vector2 FillTextureTiling { get { return fillTextureTiling; } set { fillTextureTiling = value; MarkDirty(); } }

		/// <summary>
		/// Draw the stroke?
		/// </summary>
		/// <value>Draw the stroke?.</value>
		public bool IsStrokeEnabled { get { return isStrokeEnabled; } set { isStrokeEnabled = value; MarkDirty(); } }

		/// <summary>
		/// Stroke corner type
		/// </summary>
		/// <value>The type of the stroke corner.</value>
		public StrokeCornerType StrokeCornerType { get { return strokeCornerType; } set { strokeCornerType = value; MarkDirty(); } }

		/// <summary>
		/// Stroke render type
		/// </summary>
		/// <value>The type of the stroke render.</value>
		public StrokeRenderType StrokeRenderType { get { return strokeRenderType; } set { strokeRenderType = value; MarkDirty(); } }

		/// <summary>
		/// Stroke texture type
		/// </summary>
		/// <value>The type of the stroke texture.</value>
		public StrokeTextureType StrokeTextureType { get { return strokeTextureType; } set { strokeTextureType = value; MarkDirty(); } }

		/// <summary>
		/// Stroke miter limit
		/// </summary>
		/// <value>The stroke miter limit.</value>
		public float StrokeMiterLimit { get { return strokeMiterLimit; } set { strokeMiterLimit = value; MarkDirty(); } }

		/// <summary>
		/// Stroke texture tiling
		/// </summary>
		/// <value>The stroke texture tiling.</value>
		public float StrokeTextureTiling { get { return strokeTextureTiling; } set { strokeTextureTiling = value; MarkDirty(); } }

		/// <summary>
		/// Stroke texture offset
		/// </summary>
		/// <value>The stroke texture offset.</value>
		public float StrokeTextureOffset { get { return strokeTextureOffset; } set { strokeTextureOffset = value; MarkDirty(); } }

		/// <summary>
		/// Is stroke width is variable per point?
		/// </summary>
		/// <value>Is stroke width variable</value>
		public bool HasVariableStrokeWidth { get { return hasVariableStrokeWidth; } set { hasVariableStrokeWidth = value; MarkDirty(); } }

		/// <summary>
		/// Is stroke color is variable per point?
		/// </summary>
		/// <value>Is stroke color variable</value>
		public bool HasVariableStrokeColor { get { return hasVariableStrokeColor; } set { hasVariableStrokeColor = value; MarkDirty(); } }

		#endregion

		#region private serialized properties

		[SerializeField]
		ShapeType shapeType = ShapeType.Rectangle;

		[SerializeField]
		ShapePolyDimension polyDimension = ShapePolyDimension.TwoDimensional;

		[SerializeField]
		bool isPolygonStrokeClosed = false;

		[SerializeField]
		List<ShapePointType> polyPointTypes = new List<ShapePointType>();

		[SerializeField]
		List<Vector3> polyPointPositions = new List<Vector3>();

		[SerializeField]
		List<Vector3> polyPointInTangents = new List<Vector3>();

		[SerializeField]
		List<Vector3> polyPointOutTangents = new List<Vector3>();

		[SerializeField]
		List<Color> polyPointStrokeColors = new List<Color>();

		[SerializeField]
		List<float> polyPointStrokeWidths = new List<float>();

		[SerializeField]
		Vector3 shapeOffset = Vector3.zero;

		[SerializeField]
		Vector2 shapeSize = Vector2.one;

		[SerializeField]
		Color strokeColor = Color.black;

		[SerializeField]
		float strokeWidth = 0.01f;

		[SerializeField]
		bool isFillEnabled = true;

		[SerializeField]
		Color fillColor = Color.white;

		[SerializeField]
		Vector3 fillNormal = Vector3.forward;

		[SerializeField]
		FillTextureMode fillTextureMode = FillTextureMode.Normalized;

		[SerializeField]
		Vector2 fillTextureOffset = Vector3.zero;

		[SerializeField]
		Vector2 fillTextureTiling = Vector3.one;

		[SerializeField]
		bool isStrokeEnabled = true;

		[SerializeField]
		StrokeCornerType strokeCornerType = StrokeCornerType.ExtendOrCut;

		[SerializeField]
		StrokeRenderType strokeRenderType = StrokeRenderType.ScreenSpaceRelativeToScreenHeight;

		[SerializeField]
		StrokeTextureType strokeTextureType = StrokeTextureType.Normalized;

		[SerializeField]
		float strokeMiterLimit = 3;

		[SerializeField]
		float strokeTextureTiling = 1;

		[SerializeField]
		float strokeTextureOffset = 0;

		[SerializeField]
		bool hasVariableStrokeWidth = false;

		[SerializeField]
		bool hasVariableStrokeColor = false;


		#endregion

		#region runtime properties & fields

		internal int HashId { get; private set; }
		internal bool IsDirty { get; private set; }

		List<ShapeVertexInfo> vertexInfoListSubdivided = new List<ShapeVertexInfo>();
		List<ShapeVertexInfo> vertexInfoList = new List<ShapeVertexInfo>();

		public Bounds bounds
		{
			get
			{
				// TODO cache
				var list = GetVertexInfoList();
				if (list.Count == 0)
					return new Bounds();

				Bounds b = new Bounds(list[0].position, Vector3.zero);
				for (int i = 1; i < list.Count; i++)
				{
					b.Encapsulate(list[i].position);
				}
				return b;
			}
		}

		#endregion

		#region constructor & serialization

		public ShapeData()
		{
			MarkDirty();
		}

		public void OnBeforeSerialize()
		{
			//MarkDirty();
		}

		public void OnAfterDeserialize()
		{
			MarkDirty();
		}

		#endregion

		#region polygon: add, remove, clear, edit

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		public int AddPolyPoint(Vector3 position)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		public int AddPolyPoint(Vector3 position, Color color)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointStrokeColors[num] = color;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="color">Color.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, Color color, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointStrokeColors[num] = color;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="color">Color.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Color color)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointStrokeColors[num] = color;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="color">Color.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Color color, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointStrokeColors[num] = color;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="inTangent">In tangent.</param>
		/// <param name="outTangent">Out tangent.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Vector3 inTangent, Vector3 outTangent)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointInTangents[num] = inTangent;
			this.polyPointOutTangents[num] = outTangent;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="inTangent">In tangent.</param>
		/// <param name="outTangent">Out tangent.</param>
		/// <param name="color">Color.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Vector3 inTangent, Vector3 outTangent, Color color)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointInTangents[num] = inTangent;
			this.polyPointOutTangents[num] = outTangent;
			this.polyPointStrokeColors[num] = color;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="inTangent">In tangent.</param>
		/// <param name="outTangent">Out tangent.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Vector3 inTangent, Vector3 outTangent, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointInTangents[num] = inTangent;
			this.polyPointOutTangents[num] = outTangent;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		/// <param name="position">Position.</param>
		/// <param name="pointType">Point type.</param>
		/// <param name="inTangent">In tangent.</param>
		/// <param name="outTangent">Out tangent.</param>
		/// <param name="color">Color.</param>
		/// <param name="strokeWidth">Stroke width.</param>
		public int AddPolyPoint(Vector3 position, ShapePointType pointType, Vector3 inTangent, Vector3 outTangent, Color color, float strokeWidth)
		{
			int num = this.AddPolyPoint();
			this.polyPointPositions[num] = position;
			this.polyPointTypes[num] = pointType;
			this.polyPointInTangents[num] = inTangent;
			this.polyPointOutTangents[num] = outTangent;
			this.polyPointStrokeColors[num] = color;
			this.polyPointStrokeWidths[num] = strokeWidth;
			return num;
		}

		/// <summary>
		/// Inserts a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		public void InsertPolyPoint(int id)
		{
			if (ShapeType != ShapeType.Polygon)
				throw new UnityException("Can't add point to non-polygon shape");
			
			if(id < 0 || id > GetPolyPointCount())
				throw new IndexOutOfRangeException();

			if (id == GetPolyPointCount())
				AddPolyPoint();

			polyPointPositions.Insert(id,polyPointPositions[id]);
			polyPointInTangents.Insert(id,polyPointInTangents[id]);
			polyPointOutTangents.Insert(id,polyPointOutTangents[id]);
			polyPointTypes.Insert(id,polyPointTypes[id]);
			polyPointStrokeColors.Insert(id,polyPointStrokeColors[id]);
			polyPointStrokeWidths.Insert(id,polyPointStrokeWidths[id]);

			MarkDirty();
		}

		/// <summary>
		/// Adds a polygon point.
		/// </summary>
		/// <returns>The polygon point.</returns>
		public int AddPolyPoint()
		{
			if (ShapeType != ShapeType.Polygon)
				throw new UnityException("Can't add point to non-polygon shape");

			if (GetPolyPointCount() == 0)
			{
				polyPointPositions.Add(Vector3.zero);
				polyPointInTangents.Add(Vector3.zero);
				polyPointOutTangents.Add(Vector3.zero);
				polyPointTypes.Add(ShapePointType.Corner);
				polyPointStrokeColors.Add(strokeColor);
				polyPointStrokeWidths.Add(strokeWidth);
			}
			else {
				polyPointPositions.Add(polyPointPositions[polyPointPositions.Count - 1]);
				polyPointInTangents.Add(polyPointInTangents[polyPointInTangents.Count - 1]);
				polyPointOutTangents.Add(polyPointOutTangents[polyPointOutTangents.Count - 1]);
				polyPointTypes.Add(polyPointTypes[polyPointTypes.Count - 1]);
				polyPointStrokeColors.Add(polyPointStrokeColors[polyPointStrokeColors.Count - 1]);
				polyPointStrokeWidths.Add(polyPointStrokeWidths[polyPointStrokeWidths.Count - 1]);
			}

			MarkDirty();
			return GetPolyPointCount() - 1;
		}

		/// <summary>
		/// Gets the polygon in tangent.
		/// </summary>
		/// <returns>The polygon in tangent.</returns>
		/// <param name="pointId">Point identifier.</param>
		public Vector3 GetPolyInTangent(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointInTangents[pointId];
		}

		/// <summary>
		/// Gets the polygon out tangent.
		/// </summary>
		/// <returns>The polygon out tangent.</returns>
		/// <param name="pointId">Point identifier.</param>
		public Vector3 GetPolyOutTangent(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointOutTangents[pointId];
		}

		/// <summary>
		/// Gets the width of the polygon stroke.
		/// </summary>
		/// <returns>The polygon stroke width.</returns>
		/// <param name="pointId">Point identifier.</param>
		public float GetPolyStrokeWidth(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointStrokeWidths[pointId];
		}

		/// <summary>
		/// Gets the color of the polygon stroke.
		/// </summary>
		/// <returns>The polygon stroke color.</returns>
		/// <param name="pointId">Point identifier.</param>
		public Color GetPolyStrokeColor(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointStrokeColors[pointId];
		}

		/// <summary>
		/// Gets the polygon position.
		/// </summary>
		/// <returns>The polygon position.</returns>
		/// <param name="pointId">Point identifier.</param>
		public Vector3 GetPolyPosition(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointPositions[pointId];
		}

		/// <summary>
		/// Gets the type of the polygon point.
		/// </summary>
		/// <returns>The polygon point type.</returns>
		/// <param name="pointId">Point identifier.</param>
		public ShapePointType GetPolyPointType(int pointId)
		{
			CheckIfPointInRange(pointId);

			return polyPointTypes[pointId];
		}

		/// <summary>
		/// Gets the polygon point count.
		/// </summary>
		/// <returns>The polygon point count.</returns>
		public int GetPolyPointCount()
		{
			return polyPointPositions.Count;
		}

		/// <summary>
		/// Gets the polygon point positions.
		/// </summary>
		/// <returns>The polygon point count.</returns>
		public List<Vector3> GetPolyPointPositions()
		{
			return polyPointPositions;
		}

		/// <summary>
		/// Sets the polygon position.
		/// </summary>
		/// <returns>The polygon position.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyPosition(int pointId, Vector3 value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointPositions[pointId] == value)
				return;

			polyPointPositions[pointId] = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the type of the polygon point.
		/// </summary>
		/// <returns>The polygon point type.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyPointType(int pointId, ShapePointType value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointTypes[pointId] == value)
				return;

			polyPointTypes[pointId] = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the polygon in tangent.
		/// </summary>
		/// <returns>The polygon in tangent.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyInTangent(int pointId, Vector3 value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointInTangents[pointId] == value)
				return;

			polyPointInTangents[pointId] = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the polygon out tangent.
		/// </summary>
		/// <returns>The polygon out tangent.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyOutTangent(int pointId, Vector3 value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointOutTangents[pointId] == value)
				return;

			polyPointOutTangents[pointId] = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the width of the stroke for a polygon point.
		/// </summary>
		/// <returns>The stroke width.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyStrokeWidth(int pointId, float value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointStrokeWidths[pointId] == value)
				return;

			polyPointStrokeWidths[pointId] = value;
			strokeWidth = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the color of the stroke for a polygon point.
		/// </summary>
		/// <returns>The stroke color.</returns>
		/// <param name="pointId">Point identifier.</param>
		/// <param name="value">Value.</param>
		public void SetPolyStrokeColor(int pointId, Color value)
		{
			CheckIfPointInRange(pointId);
			if (polyPointStrokeColors[pointId] == value)
				return;

			polyPointStrokeColors[pointId] = value;
			strokeColor = value;

			MarkDirty();
		}

		/// <summary>
		/// Sets the type of the polygon point.
		/// </summary>
		/// <returns>The polygon point type.</returns>
		/// <param name="newPointType">New point type.</param>
		public void SetPolyPointType(ShapePointType newPointType)
		{
			for (int i = 0; i < polyPointTypes.Count; i++)
			{
				polyPointTypes[i] = newPointType;
			}

			MarkDirty();
		}

		/// <summary>
		/// Removes the polygon point.
		/// </summary>
		/// <returns>The poly point.</returns>
		/// <param name="pointId">Point identifier.</param>
		public void RemovePolyPoint(int pointId)
		{
			CheckIfPointInRange(pointId);

			polyPointTypes.RemoveAt(pointId);
			polyPointPositions.RemoveAt(pointId);
			polyPointInTangents.RemoveAt(pointId);
			polyPointOutTangents.RemoveAt(pointId);
			polyPointStrokeColors.RemoveAt(pointId);
			polyPointStrokeWidths.RemoveAt(pointId);

			MarkDirty();
		}

		/// <summary>
		/// Clears all polygon points.
		/// </summary>
		/// <returns>The poly points.</returns>
		public void ClearPolyPoints()
		{

			polyPointTypes.Clear();
			polyPointPositions.Clear();
			polyPointInTangents.Clear();
			polyPointOutTangents.Clear();
			polyPointStrokeColors.Clear();
			polyPointStrokeWidths.Clear();

			MarkDirty();
		}

		#endregion

		#region get & set shape data

		/// <summary>
		/// Is the stroke closed?
		/// </summary>
		/// <value>The is stroke closed.</value>
		public bool IsStrokeClosed
		{
			get
			{
				return ShapeType == ShapeType.Rectangle || ShapeType == ShapeType.Circle || (ShapeType == ShapeType.Polygon && IsPolygonStrokeClosed);
			}
		}

		/// <summary>
		/// Gets the center point.
		/// </summary>
		/// <returns>The center point.</returns>
		public Vector3 GetCenterPoint()
		{
			Vector3 p = Vector3.zero;
			for (int i = 0; i < polyPointPositions.Count; i++)
			{
				p += polyPointPositions[i];
			}

			p /= polyPointPositions.Count;

			return p;
		}

		/// <summary>
		/// Gets the length of the stroke.
		/// </summary>
		/// <returns>The stroke length.</returns>
		float GetStrokeLength()
		{
			float length = 0;
			for (int i = 1; i < GetPolyPointCount(); i++)
			{

				length += (GetPolyPosition(i) - GetPolyPosition(i - 1)).magnitude;
			}
			return length;
		}

		/// <summary>
		/// Gets the color of the stroke.
		/// </summary>
		/// <returns>The stroke color.</returns>
		public Color GetStrokeColor()
		{
			return strokeColor;
		}

		/// <summary>
		/// Gets the width of the stroke.
		/// </summary>
		/// <returns>The stroke width.</returns>
		public float GetStrokeWidth()
		{
			return strokeWidth;
		}

		/// <summary>
		/// Sets the color of the stroke.
		/// </summary>
		/// <returns>The stroke color.</returns>
		/// <param name="newColor">New color.</param>
		public void SetStrokeColor(Color newColor)
		{
			strokeColor = newColor;
			for (int i = 0; i < polyPointStrokeColors.Count; i++)
			{
				polyPointStrokeColors[i] = newColor;
			}

			MarkDirty();
		}

		/// <summary>
		/// Sets the width of the stroke.
		/// </summary>
		/// <returns>The stroke width.</returns>
		/// <param name="newWidth">New width.</param>
		public void SetStrokeWidth(float newWidth)
		{
			strokeWidth = newWidth;
			for (int i = 0; i < polyPointStrokeWidths.Count; i++)
			{
				polyPointStrokeWidths[i] = newWidth;
			}
			MarkDirty();
		}



		#endregion

		#region misc

		void CheckIfPointInRange(int pointId)
		{
			if (pointId < 0 || pointId > GetPolyPointCount() - 1)
				throw new UnityException(string.Format("Point id out of range, pointId={0} pointCount={1}", pointId, GetPolyPointCount()));
		}

		void FixTangents()
		{
			if (shapeType == ShapeType.Polygon)
			{
				for (int i = 0; i < GetPolyPointCount(); i++)
				{

					var pointType = GetPolyPointType(i);
					switch (pointType)
					{
						case ShapePointType.Smooth:

							Vector3 prevPos = ShapeVertexInfoUtils.GetPolyVertexInfo(this, i - 1).position;
							Vector3 thisPoint = ShapeVertexInfoUtils.GetPolyVertexInfo(this, i).position;
							Vector3 nextPos = ShapeVertexInfoUtils.GetPolyVertexInfo(this, i + 1).position;
							Vector3 normTangent = (prevPos - nextPos).normalized;

							polyPointInTangents[i] = normTangent * Vector3.Distance(prevPos, thisPoint) * .33f;
							polyPointOutTangents[i] = -normTangent * Vector3.Distance(nextPos, thisPoint) * .33f;
							break;

						case ShapePointType.BezierContinous:
							polyPointOutTangents[i] = -polyPointInTangents[i];
							break;

						case ShapePointType.Corner:
							polyPointOutTangents[i] = Vector3.zero;
							polyPointInTangents[i] = Vector3.zero;
							break;

					}

					if (!IsStrokeClosed)
					{
						if (i == 0)
						{
							polyPointInTangents[i] = -polyPointOutTangents[i];
							continue;
						}
						if (i == polyPointPositions.Count - 1)
						{
							polyPointOutTangents[i] = -polyPointInTangents[i];
							continue;
						}
					}

				}
			}
		}

		public InterpInfo GetInterpInfo(float t)
		{
			var vertexInfos = GetVertexInfoList();

			// length calculation is not accurate
			float idFloat = Mathf.Lerp(0, vertexInfos.Count - 1, t);
			var info = new InterpInfo();
			info.startId = Mathf.FloorToInt(idFloat);
			info.endId = info.startId + 1;
			info.lerp = t - info.startId;
			return info;
		}


		public List<ShapeVertexInfo> GetVertexInfoList()
		{
			if (IsDirty)
			{
				FixTangents();

				// read base list
				ShapeVertexInfoUtils.ReadVertexInfoList(this, vertexInfoList);

				// subdivide list
				ShapeVertexInfoUtils.SubdivideVertexInfoList(vertexInfoList, vertexInfoListSubdivided);

				IsDirty = false;
				HashId = unchecked(HashId + 1);
			}
			return vertexInfoListSubdivided;

		}

		public void AddFromVertexInfoList(List<ShapeVertexInfo> vertexList)
		{
			for (int i = 0; i < vertexList.Count; i++)
			{

				int id = AddPolyPoint();
				SetPolyPosition(id, vertexList[i].position);
				SetPolyPointType(id, vertexList[i].type);
				SetPolyInTangent(id, vertexList[i].inTangent);
				SetPolyOutTangent(id, vertexList[i].outTangent);
				SetPolyStrokeWidth(id, vertexList[i].strokeWidth);
				SetPolyStrokeColor(id, vertexList[i].strokeColor);
			}
		}

		void MarkDirty()
		{
			IsDirty = true;
		}

		#endregion
	}

	public struct InterpInfo
	{
		public int startId;
		public int endId;
		public float lerp;
	}
}