using UnityEngine;
using System.Collections;

namespace VectorShapes
{
	static class MathUtils
	{

		// always returns a positive value
		public static int CircularModulo (int x, int m)
		{
			if (m == 0)
				return x;
			int r = x % m;
			return r < 0 ? r + m : r;
		}
	}
}