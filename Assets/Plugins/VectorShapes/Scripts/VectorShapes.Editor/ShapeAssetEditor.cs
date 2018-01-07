using UnityEngine;
using UnityEditor;
using VectorShapes;

namespace VectorShapesEditor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ShapeAsset))]
	internal class ShapeAssetEditor : Editor
	{
		
		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			EditorGUILayout.HelpBox("Vector shape. Use it with a shape component attached to a game object to edit it.", MessageType.Info);
			if (GUILayout.Button("Create shape object(s) in scene"))
			{
				for (int i = 0; i < targets.Length; i++)
				{
					ShapeMenuItems.CreateShapeObj((targets[i] as ShapeAsset));
				}
			}
		}

	}

}

