using System;
using UnityEngine;
using VectorShapesInternal;
using Object = UnityEngine.Object;

namespace VectorShapes
{
	/// <summary>
	/// Shape. Attaches a shape to
	/// </summary>
	[ExecuteInEditMode]
	public class Shape : MonoBehaviour
	{
		#region public properties

		/// <summary>
		/// Gets or sets the shape asset.
		/// </summary>
		/// <value>The shape asset.</value>
		public ShapeAsset ShapeAsset
		{
			get { return shapeAsset; }
			set { shapeAsset = value; }
		}

		public Object dataContainerObject
		{
			get
			{
				if (ShapeAsset != null)
				{
					return ShapeAsset;
				}

				return this;
			}

		}

		/// <summary>
		/// Gets or sets the shape data, which is saved either in this object or the referenced shape asset.
		/// </summary>
		/// <value>The shape data.</value>
		public ShapeData ShapeData
		{
			get
			{
				if (ShapeAsset != null)
				{
					return ShapeAsset.ShapeData;
				}

				if (localShapeData == null)
					localShapeData = new ShapeData();

				return localShapeData;
			}
			set
			{
				if (ShapeAsset != null)
					ShapeAsset.ShapeData = value;
				else
					localShapeData = value;
			}
		}


		#endregion

		#region internal properties

		public ShapeMeshCache GizmoCache
		{
			get
			{
				if (gizmoCache == null)
				{
					gizmoCache = new ShapeMeshCache();
				}
				gizmoCache.shape = ShapeData;
				gizmoCache.transform = transform;
				gizmoCache.useShader = false;
				gizmoCache.doubleSided = true;
				return gizmoCache;
			}

			set
			{


				gizmoCache = value;
			}
		}

		public ShapeRenderer ShapeRenderer
		{
			get
			{
				return shapeRenderer;
			}

			private set
			{

				if (shapeRenderer)
					shapeRenderer.RemoveShape(this);

				shapeRenderer = value;

				if (shapeRenderer)
					shapeRenderer.AddShape(this);
			}
		}

		internal void UpdateShapeRenderer()
		{
			ShapeRenderer newRenderer = null;
			if (isActiveAndEnabled)
			{
				newRenderer = TransformUtils.GetActiveComponentInObjOrParent<ShapeRenderer>(transform);
			}

			ShapeRenderer = newRenderer;
		}

		#endregion

		#region private fields

		[SerializeField]
		ShapeAsset shapeAsset;

		[SerializeField]
		ShapeData localShapeData = new ShapeData();

		ShapeMeshCache gizmoCache;
		ShapeRenderer shapeRenderer;

		#endregion

		#region MonoBehaviour callbacks

		void OnEnable()
		{
			UpdateShapeRenderer();
		}

		void OnDisable()
		{
			ShapeRenderer = null;
		}

		void OnTransformParentChanged()
		{
			UpdateShapeRenderer();
		}

		void OnDestroy()
		{
			if (GizmoCache != null)
			{
				GizmoCache.Release();
				GizmoCache = null;
			}

		}

		#endregion

	}
}

