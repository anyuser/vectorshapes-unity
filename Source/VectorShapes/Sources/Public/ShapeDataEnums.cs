using System;
namespace VectorShapes
{
	/// <summary>
	/// Fill texture mode.
	/// </summary>
	public enum FillTextureMode
	{
		Normalized,
		Absolute
	}

	/// <summary>
	/// Shape polygon dimension.
	/// </summary>
	public enum ShapePolyDimension
	{
		TwoDimensional,
		ThreeDimensional
	}

	/// <summary>
	/// Shape type.
	/// </summary>
	public enum ShapeType
	{
		Rectangle,
		Circle,
		Polygon
	}

	/// <summary>
	/// Shape point type.
	/// </summary>
	public enum ShapePointType
	{
		Corner,
		Bezier,
		BezierContinous,
		Smooth
	}

	/// <summary>
	/// Stroke corner type.
	/// </summary>
	public enum StrokeCornerType
	{
		Bevel,
		ExtendOrCut,
		ExtendOrMiter,
	}

	/// <summary>
	/// Stroke render type.
	/// </summary>
	public enum StrokeRenderType
	{
		ScreenSpacePixels,
		ScreenSpaceRelativeToScreenHeight,
		ShapeSpace,
		ShapeSpaceFacingCamera
	}

	/// <summary>
	/// Stroke texture type.
	/// </summary>
	public enum StrokeTextureType
	{
		Absolute,
		Normalized
	}
}

