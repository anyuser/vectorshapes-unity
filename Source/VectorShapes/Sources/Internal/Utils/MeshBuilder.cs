using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace VectorShapes
{
	class MeshBuilder
	{
		//
		// Static Fields
		//
		private static readonly Vector3 s_DefaultNormal = Vector3.back;

		private static readonly Vector4 s_DefaultTangent = new Vector4 (1, 0, 0, -1);
		private static readonly Vector2 s_DefaultUV0 = new Vector2(0,0);
		private static readonly Color s_DefaultColor = Color.white;

		//
		// Fields
		//


		public List<Vector3> positions = new List<Vector3>();
		public List<Vector4> m_Tangents = new List<Vector4> ();
		public List<Color32> m_Colors = new List<Color32> ();
		public List<Vector4> m_Uv0S = new List<Vector4> ();
		public List<Vector4> m_Uv1S = new List<Vector4> ();
		public List<Vector3> m_Normals = new List<Vector3> ();

		private List<List<int>> m_Indicies = new List<List<int>> ();

		public int currentSubmesh = 0;
		//
		// Properties
		//

		public int GetCurrentIndexCount(int submeshId)
		{
			InitSubmeshIfRequired ();
			return this.m_Indicies[submeshId].Count;
		}

		public int currentVertCount {
			get {
				return this.positions.Count;
			}
		}

		public int lastAddedVertId {
			get {
				return this.positions.Count-1;
			}
		}


		public MeshBuilder ()
		{
		}
	//
		// Methods
		//
		public void AddQuadTriangles(int offset = 0)
		{
			AddQuad (currentVertCount - 4 + offset, currentVertCount - 3 + offset, currentVertCount - 2 + offset, currentVertCount - 1 + offset);
		}
		public void AddQuad(int t1, int t2, int t3, int t4)
		{
			this.AddTriangle (t1, t4, t2);
			this.AddTriangle (t3, t2, t4);
		}

		public void AddTriangle()
		{
			AddTriangle(currentVertCount-3,currentVertCount-2,currentVertCount-1);
		}

		public void AddTriangles (List<int> triangles)
		{
			InitSubmeshIfRequired ();

			this.m_Indicies[currentSubmesh].AddRange (triangles);
		}

		public void AddTriangle (int idx0, int idx1, int idx2)
		{
			InitSubmeshIfRequired ();
			
			this.m_Indicies[currentSubmesh].Add (idx0);
			this.m_Indicies[currentSubmesh].Add (idx1);
			this.m_Indicies[currentSubmesh].Add (idx2);
		}

		void InitSubmeshIfRequired()
		{
			while (this.m_Indicies.Count < currentSubmesh+1)
				this.m_Indicies.Add (new List<int> ());
		}

		public void AddVert ()
		{
			this.positions.Add (Vector3.zero);
			this.m_Colors.Add ((Color32)MeshBuilder.s_DefaultColor);
			this.m_Uv0S.Add (MeshBuilder.s_DefaultUV0);
			this.m_Uv1S.Add (MeshBuilder.s_DefaultUV0);
			this.m_Normals.Add ( MeshBuilder.s_DefaultNormal);
			this.m_Tangents.Add (MeshBuilder.s_DefaultTangent);
		}

		public void SetCurrentPosition(Vector3 pos)
		{
			positions [lastAddedVertId] = pos;
		}

		public void SetCurrentColor(Color color)
		{
			// HACK use color32 for canvas renderer...?
			m_Colors[lastAddedVertId] = (Color32)color;
		}

		public void SetCurrentUV0(Vector4 uv0)
		{
			m_Uv0S [lastAddedVertId] = uv0;
		}

		public void SetCurrentUV1(Vector4 uv1)
		{
			m_Uv1S [lastAddedVertId] = uv1;
		}

		public void SetCurrentNormal(Vector3 normal)
		{
			m_Normals [lastAddedVertId] = normal;
		}

		public void SetCurrentTangent(Vector4 tangent)
		{
			m_Tangents [lastAddedVertId] = tangent;
		}

		public void Clear (bool resetSubmeshes)
		{
			this.positions.Clear ();
			this.m_Colors.Clear ();
			this.m_Uv0S.Clear ();
			this.m_Uv1S.Clear ();
			this.m_Normals.Clear ();
			this.m_Tangents.Clear ();

			if (resetSubmeshes) {
				m_Indicies.Clear ();
			} else {
				for (int i = 0; i < this.m_Indicies.Count; i++) {

					this.m_Indicies [i].Clear ();
				}
			}
		}

		public void ApplyToMesh (Mesh mesh)
		{
			mesh.Clear ();
			if (this.positions.Count >= 65000) {
				throw new ArgumentException ("Mesh can not have more than 65000 vertices");
			}
			mesh.SetVertices (this.positions);
			mesh.SetColors (this.m_Colors);
			mesh.SetUVs (0, this.m_Uv0S);
			mesh.SetUVs (1, this.m_Uv1S);
			mesh.SetNormals (this.m_Normals);
			mesh.SetTangents (this.m_Tangents);
			mesh.subMeshCount = m_Indicies.Count;
			for (int i = 0; i < m_Indicies.Count; i++) {

				mesh.SetTriangles (this.m_Indicies[i], i);
			}
			mesh.RecalculateBounds ();
		}

		public void AddStream(MeshBuilder stream)
		{
			int vertCountBeforeAdd = currentVertCount;
			for (int i = 0; i < stream.currentVertCount; i++)
			{
				positions.Add(stream.positions [i]);
				m_Colors.Add(stream.m_Colors [i]);
				m_Normals.Add(stream.m_Normals [i]);
				m_Tangents.Add(stream.m_Tangents [i]);
				m_Uv0S.Add(stream.m_Uv0S [i]);
				m_Uv1S.Add(stream.m_Uv1S [i]);
			}

			for (int submeshId = 0; submeshId < stream.m_Indicies.Count; submeshId++) {

				currentSubmesh = submeshId;
				InitSubmeshIfRequired ();
				for (int i = 0; i < stream.GetCurrentIndexCount(submeshId); i++)
				{
					m_Indicies[currentSubmesh].Add (stream.m_Indicies [submeshId][i] + vertCountBeforeAdd);
				}
			}
		}

		internal void GenerateFlippedTriangles()
		{
			for (int i = 0; i < this.m_Indicies.Count; i++)
			{
				int indexCount = this.m_Indicies[i].Count;
				for (int j = 0; j < indexCount; j+=3)
				{
					this.m_Indicies[i].Add( this.m_Indicies[i][j + 0]);
					this.m_Indicies[i].Add( this.m_Indicies[i][j + 2]);
					this.m_Indicies[i].Add( this.m_Indicies[i][j + 1]);
				}

			}
		}
	}
}
