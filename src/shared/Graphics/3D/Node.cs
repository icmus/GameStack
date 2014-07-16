using System;
using OpenTK;

namespace GameStack.Graphics {
	public class Node {
		string _name;
		Node _parent;
		Node[] _children;
		Mesh[] _meshes;
		Bone _bone;
		public Matrix4 transform;

		internal Node (string name, Node parent, ref Matrix4 transform, Mesh[] meshes) {
			_name = name;
			_parent = parent;
			this.transform = transform;
			_meshes = meshes;
		}

		public string Name { get { return _name; } }

		public Node Parent { get { return _parent; } }

		public Node[] Children { get { return _children; } internal set { _children = value; } }

		public Mesh[] Meshes { get { return _meshes; } }

		public Bone Bone { get { return _bone; } internal set { _bone = value; } }

		public Node FindByName (string name) {
			if (name == _name)
				return this;
			if (_children != null) {
				foreach (var child in _children) {
					var node = child.FindByName(name);
					if (node != null)
						return node;
				}
			}
			return null;
		}

		public void Draw (ref Matrix4 parent) {
			Matrix4 world;
			Matrix4.Mult(ref transform, ref parent, out world);
			if (_meshes != null && _meshes.Length > 0) {
				foreach (var mesh in _meshes)
					mesh.Draw(ref world);
			}
			if (_children != null) {
				foreach (var child in _children)
					child.Draw(ref world);
			}
		}
	}
}
