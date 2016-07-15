using UnityEngine;

namespace VectorShapes
{
	static class TransformUtils
	{

		public static T GetActiveComponentInObjOrParent<T>(Transform tf) where T : MonoBehaviour
		{
			T c = tf.GetComponent<T>();
			if (c && c.isActiveAndEnabled)
				return c;

			if (tf.parent != null)
				return GetActiveComponentInObjOrParent<T>(tf.parent);

			return null;
		}
	}
}

