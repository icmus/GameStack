using System;
using OpenTK;
using System.Runtime.InteropServices;

namespace GameStack.Graphics {
	public class Bone {
		int _index;
		string _name;
		VertexWeight[] _weights;
		Node _node;
		public Matrix4 _offset;

		public Bone (int index, string name, ref Matrix4 offset, VertexWeight[] weights) {
			_index = index;
			_name = name;
			_offset = offset;
			_weights = weights;
		}

		public int Index { get { return _index; } }

		public string Name { get { return _name; } }

		VertexWeight[] Weights { get { return _weights; } }

		public Node Node { get { return _node; } internal set { _node = value; } }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexWeight {
		public int VertexId;
		public float Weight;
	}
}
