using System;
using UnityEngine;

namespace VectorShapes
{
	/// <summary>
	/// Shape asset. Allows saving shape data in an asset.
	/// </summary>
	[CreateAssetMenu]
	public class ShapeAsset : ScriptableObject
	{
		#region public properties
		/// <summary>
		/// Gets or sets the shape data.
		/// </summary>
		/// <value>The shape data.</value>
		public ShapeData ShapeData
		{ 
			get
			{
				return shapeData;
			}
			set {
				shapeData = value;
			}
		}
		#endregion

		#region private serialized fields

		[SerializeField]
		ShapeData shapeData;

		#endregion
	}
}

