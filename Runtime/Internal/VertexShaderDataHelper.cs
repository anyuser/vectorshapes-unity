using UnityEngine;
using VectorShapes;

namespace VectorShapesInternal
{
	class VertexShaderDataHelper
	{
		public struct VertexInputDataEncoded
		{
			public Vector4 vertex;
			public Color color;
			public Vector4 texcoord0;
			public Vector4 texcoord1;
			public Vector3 normal;
			public Vector4 tangent;
		}


		public static VertexInputData DecodeData(VertexInputDataEncoded encodedData)
		{
			var obj = new VertexInputData();

			obj.position1 = encodedData.normal;
			obj.position2 = encodedData.vertex;
			obj.position3 = (Vector3) encodedData.tangent;
			obj.strokeWidth1 = encodedData.texcoord1.y;
			obj.strokeWidth2 = encodedData.texcoord1.z;
			obj.strokeWidth3 = encodedData.texcoord1.w;
			obj.uv = new Vector2(encodedData.texcoord0.x, encodedData.texcoord0.y);
			obj.color2 = encodedData.color;
			obj.vertexId = Mathf.RoundToInt(encodedData.texcoord1.x);

			return obj;
		}

		public static VertexInputDataEncoded EncodeData(VertexInputData data)
		{
			var obj = new VertexInputDataEncoded();

			obj.normal = data.position1;
			obj.vertex = data.position2;
			obj.tangent = new Vector4(data.position3.x, data.position3.y, data.position3.z, 0);
			obj.texcoord0 = new Vector4(data.uv.x, data.uv.y, 0,0);
			obj.texcoord1 = new Vector4(data.vertexId, data.strokeWidth1, data.strokeWidth2, data.strokeWidth3);
			obj.color = data.color2;

			return obj;
		}
	}

	struct VertexInputData
	{
		public Vector3 position1;
		public Vector3 position2;
		public Vector3 position3;
		public float strokeWidth1;
		public float strokeWidth2;
		public float strokeWidth3;
		public Color color2;
		public int vertexId;
		public Vector2 uv;
	}

	struct VertexOutputData
	{
		public Vector3 position;
		public Vector2 uv;
		public Color color;
	}
}

