using UnityEngine;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
using System.Collections.Generic;
using LibTessDotNet;
using VectorShapes;

namespace VectorShapesInternal
{
	static class ShapeMeshGenerator
	{
		static Tess tesselator;
		static LineShaderCPU lineShaderCpu;

		public static void GenerateGPUStrokeMesh(ShapeData shape, MeshBuilder meshBuilder)
		{
			Profiler.BeginSample("GenerateStrokeMesh");

			if (shape.GetVertexInfoList().Count < 2)
				return;

			meshBuilder.currentSubmesh = 1;
			System.Action<VertexInputData> addVertDelegate = delegate (VertexInputData vertexData)
			{
				var encodedData = VertexShaderDataHelper.EncodeData(vertexData);

				meshBuilder.AddVert();
				meshBuilder.SetCurrentPosition(encodedData.vertex);
				meshBuilder.SetCurrentColor(encodedData.color);
				meshBuilder.SetCurrentUV0(encodedData.texcoord0);
				meshBuilder.SetCurrentUV1(encodedData.texcoord1);
				meshBuilder.SetCurrentNormal(encodedData.normal);
				meshBuilder.SetCurrentTangent(encodedData.tangent);
			};

			System.Action addQuadDelegate = delegate
			{
				meshBuilder.AddQuadTriangles();
			};

			var vertInfoList = shape.GetVertexInfoList();
			for (int i = 0; i < vertInfoList.Count - 3; i++)
			{
				DrawStrokeSegment(shape, 
								  vertInfoList,
				                  i,
				                  addVertDelegate ,
				                  addQuadDelegate);
			}
		}

		static List<VertexInputData> vertexDataList = new List<VertexInputData>();

		public static void GenerateCPUStrokeMesh(ShapeData shape, MeshBuilder meshBuilder, Transform transform, Camera camera)
		{
			Profiler.BeginSample("CPU Vertex shader");

			if (!camera || !transform)
			{
				Debug.LogWarning("Camera and transform is required for CPU mesh generation");
				return;
			}

			int startVertId = meshBuilder.currentVertCount;

			vertexDataList.Clear();
			System.Action<VertexInputData> addVertDelegate = delegate (VertexInputData data)
			{
				meshBuilder.AddVert();
				vertexDataList.Add(data);
			};

			System.Action addQuadDelegate = delegate
			{
				meshBuilder.AddQuadTriangles();
			};

			var vertInfoList = shape.GetVertexInfoList();
			for (int i = 0; i < vertInfoList.Count - 3; i++)
			{
				DrawStrokeSegment(shape,
								  vertInfoList,
				                  i,
								  addVertDelegate,
								  addQuadDelegate);
			}

			if (lineShaderCpu == null)
				lineShaderCpu = new LineShaderCPU();

			lineShaderCpu.camera = camera;
			lineShaderCpu.cornerType = shape.StrokeCornerType;
			lineShaderCpu.renderType = shape.StrokeRenderType;
			lineShaderCpu._StrokeMiterLimit = shape.StrokeMiterLimit;
			Matrix4x4 M = transform.localToWorldMatrix;
			Matrix4x4 V = camera.cameraToWorldMatrix;
			Matrix4x4 P = camera.projectionMatrix;
			lineShaderCpu.MVP = P * V * M;
			lineShaderCpu.MV = V * M;
			lineShaderCpu.P = P;

			for (int i = startVertId; i < meshBuilder.positions.Count; i++)
			{
				
				var output = lineShaderCpu.VertexProgram(vertexDataList[i]);


				meshBuilder.positions[i] = output.position;
				meshBuilder.m_Uv0S[i] = new Vector4(output.uv.x, output.uv.y, 0, 0);
				meshBuilder.m_Colors[i] = output.color;
			}

			Profiler.EndSample();
		}


		public static void GenerateFillMesh(ShapeData shape, MeshBuilder meshBuilder)
		{
			Profiler.BeginSample("GenerateFillMesh");
			var shapeVertexInfos = shape.GetVertexInfoList();
			if (shapeVertexInfos.Count < 3)
				return;

			Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation(shape.FillNormal), Vector3.one).inverse;

			// tesselation
			ContourVertex[] contour = new ContourVertex[shapeVertexInfos.Count - 2 - (shape.IsStrokeClosed ? 1 : 0)]; // ignore first and last point

			for (int i = 0; i < contour.Length; i++)
			{
				Vector3 p = m.MultiplyPoint(shapeVertexInfos[i + 1].position);
				
				if(float.IsNaN(p.x) ||float.IsNaN(p.y))
					continue;

				var pos = new Vec3();
				pos.X = p.x;
				pos.Y = p.y;
				pos.Z = 0;

				var v = new ContourVertex();
				v.Position = pos;
				v.Data = shapeVertexInfos[i + 1].position;
				contour[i] = v;
			}

			if (tesselator == null)
			{

				tesselator = new Tess();
				tesselator.UsePooling = true;
			}
			tesselator.AddContour(contour, ContourOrientation.CounterClockwise);
			tesselator.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3, delegate (Vec3 position, object[] data, float[] weights)
			{

				return (Vector3)data[0] * weights[0] + (Vector3)data[1] * weights[1] + (Vector3)data[2] * weights[2] + (Vector3)data[3] * weights[3];
			});


			meshBuilder.currentSubmesh = 0;
			for (int i = 0; i < tesselator.ElementCount; i++)
			{
				meshBuilder.AddTriangle(
					meshBuilder.currentVertCount + tesselator.Elements[i * 3 + 0],
					meshBuilder.currentVertCount + tesselator.Elements[i * 3 + 1],
					meshBuilder.currentVertCount + tesselator.Elements[i * 3 + 2]);
			}

			for (int i = 0; i < tesselator.Vertices.Length; i++)
			{
				meshBuilder.AddVert();
				meshBuilder.SetCurrentPosition((Vector3)tesselator.Vertices[i].Data);
				meshBuilder.SetCurrentColor(shape.FillColor);
				meshBuilder.SetCurrentUV0((Vector2)(Vector3)tesselator.Vertices[i].Data);
			}

			Profiler.EndSample();
		}

		static VertexInputData tempVertexData;
		public static void DrawStrokeSegment(ShapeData shape,
		                                     List<ShapeVertexInfo> shapeVertInfoList, 
		                                     int startId,
		                                     System.Action<VertexInputData> addVertDelegate,
		                                     System.Action finishQuadDelegate)
		{
			Profiler.BeginSample("DrawStrokeSegment");

			// 0 & 1
			tempVertexData.position1 = shapeVertInfoList[startId + 0].position;
			tempVertexData.position2 = shapeVertInfoList[startId + 1].position;
			tempVertexData.position3 = shapeVertInfoList[startId + 2].position;
			tempVertexData.strokeWidth1 = shapeVertInfoList[startId + 0].strokeWidth;
			tempVertexData.strokeWidth2 = shapeVertInfoList[startId + 1].strokeWidth;
			tempVertexData.strokeWidth3 = shapeVertInfoList[startId + 2].strokeWidth;
			tempVertexData.color2 = shapeVertInfoList[startId + 1].strokeColor;

			// 0
			tempVertexData.vertexId = 0;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 1].posOnLine, 0);
			addVertDelegate(tempVertexData);

			// 1
			tempVertexData.vertexId = 1;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 1].posOnLine, 1);
			addVertDelegate(tempVertexData);

			// 2 - 7
			tempVertexData.position1 = shapeVertInfoList[startId + 1].position;
			tempVertexData.position2 = shapeVertInfoList[startId + 2].position;
			tempVertexData.position3 = shapeVertInfoList[startId + 3].position;
			tempVertexData.strokeWidth1 = shapeVertInfoList[startId + 1].strokeWidth;
			tempVertexData.strokeWidth2 = shapeVertInfoList[startId + 2].strokeWidth;
			tempVertexData.strokeWidth3 = shapeVertInfoList[startId + 3].strokeWidth;
			tempVertexData.color2 = shapeVertInfoList[startId + 2].strokeColor;

			// 2
			tempVertexData.vertexId = 2;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 1);
			addVertDelegate(tempVertexData);

			// 3
			tempVertexData.vertexId = 3;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 0);
			addVertDelegate(tempVertexData);

			// add first quad
			finishQuadDelegate();

			// 4
			tempVertexData.vertexId = 4;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 0);
			addVertDelegate(tempVertexData);

			// 5
			tempVertexData.vertexId = 5;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 1);
			addVertDelegate( tempVertexData);

			// 6
			tempVertexData.vertexId = 6;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 1);
			addVertDelegate( tempVertexData);

			// 3
			tempVertexData.vertexId = 7;
			tempVertexData.uv = new Vector2(shapeVertInfoList[startId + 2].posOnLine, 0);
			addVertDelegate( tempVertexData);

			finishQuadDelegate();

			Profiler.EndSample();
		}
	}
}

