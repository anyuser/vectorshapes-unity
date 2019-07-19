using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VectorShapes;
using VectorShapesInternal;

namespace VectorShapesEditor
{
	[CustomEditor(typeof(ShapeRenderer))]
	internal class ShapeRendererEditor : Editor
	{
		ShapeRenderer shapeRenderer;

		Editor fillMaterialEditor;
		Editor strokeMaterialEditor;

		void OnEnable()
		{
			shapeRenderer = (ShapeRenderer)target;


			fillMaterialEditor = CreateEditor(shapeRenderer.FillMaterial);
			strokeMaterialEditor = CreateEditor(shapeRenderer.StrokeMaterial);

		}

		void OnDisable()
		{
			if (fillMaterialEditor != null)
			{
				DestroyImmediate(fillMaterialEditor);
			}
			if (strokeMaterialEditor != null)
			{
				DestroyImmediate(strokeMaterialEditor);
			}
		}

		void MaterialField(string propertyName, ref Editor editor)
		{
			EditorGUI.BeginChangeCheck();

			var prop = serializedObject.FindProperty(propertyName);

			EditorGUILayout.PropertyField(prop);

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();

				if (editor)
					DestroyImmediate(editor);

				if( prop.objectReferenceValue )
					editor = CreateEditor(prop.objectReferenceValue);
			}
		}

		public override void OnInspectorGUI()
		{
			//base.OnInspectorGUI();

			bool useStrokeShader = shapeRenderer.StrokeMaterial ? VectorShapesUtils.IsStrokeShader(shapeRenderer.StrokeMaterial.shader) : false;
			if(!useStrokeShader)
				EditorGUILayout.PropertyField(serializedObject.FindProperty("cam"));

			///if( shapeRenderer.FillMaterial )
				MaterialField("fillMaterial", ref fillMaterialEditor);
			
			//if (shapeRenderer.StrokeMaterial)
				MaterialField("strokeMaterial", ref strokeMaterialEditor);


			if (fillMaterialEditor)
			{
				fillMaterialEditor.DrawHeader();

				fillMaterialEditor.OnInspectorGUI();
			}
			if (strokeMaterialEditor)
			{
				strokeMaterialEditor.DrawHeader();

				strokeMaterialEditor.OnInspectorGUI();
			}

			if (!useStrokeShader)
			{
				EditorGUILayout.HelpBox("Shader doesn't support stroke rendering, falling back to CPU mode (slow, only supports one camera).", MessageType.Warning);
			}
		}

		/*public override bool HasPreviewGUI()
		{
			return (strokeMaterialEditor && strokeMaterialEditor.HasPreviewGUI() )|| 
				(fillMaterialEditor && fillMaterialEditor.HasPreviewGUI());// base.HasPreviewGUI();
		}

		public override void OnPreviewSettings()
		{
			if (strokeMaterialEditor)
				strokeMaterialEditor.OnPreviewSettings();
			if (fillMaterialEditor)
				fillMaterialEditor.OnPreviewSettings();
			//base.OnPreviewSettings();
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			//base.OnPreviewGUI(r, background);

			if(strokeMaterialEditor)
				strokeMaterialEditor.OnPreviewGUI(r,background);
			if (fillMaterialEditor)
				fillMaterialEditor.OnPreviewGUI(r,background);
		}*/
	}
}