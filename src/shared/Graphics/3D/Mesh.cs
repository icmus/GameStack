using System;
using System.Collections.Generic;
using System.Linq;
using GameStack.Graphics;
using OpenTK;
#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Graphics {
	public class Mesh {
		string _name;
		Material _material;
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;
		Bone[] _bones;
		Matrix4[] _boneTransforms;
		Node _node;
		HashSet<Node> _boneSet;

		public Mesh (string name, Material mat, VertexFormat format, float[] vertices, int[] indices, Bone[] bones) {
			_name = name;
			_material = mat;
			_vbuffer = new VertexBuffer(format, vertices);
			_ibuffer = new IndexBuffer(indices, BeginMode.Triangles);
			_bones = bones;
			if(_bones != null)
				_boneTransforms = new Matrix4[_bones.Length];
		}

		public string Name { get { return _name; } }

		public Material Material { get { return _material; } }

		public Bone[] Bones { get { return _bones; } }

		public Node Node { get { return _node; } internal set { _node = value; } }

		public void Draw (ref Matrix4 world) {
			var cam = ScopedObject.Find<Camera>();
			if (cam == null)
				throw new InvalidOperationException("There is no active camera.");
			var lights = ScopedObject.Find<Lighting>();

			if (_bones != null) {
				if (_boneSet == null)
					_boneSet = new HashSet<Node>(_bones.Select(b => b.Node));
				var identity = Matrix4.Identity;
				foreach (var node in _node.Parent.Children) {
					if (node != _node)
						this.PrepareBoneTransforms(node, ref identity);
				}
				if (_node.Children != null) {
					foreach (var child in _node.Children)
						this.PrepareBoneTransforms(child, ref identity);
				}
			}

			var mat = ScopedObject.Find<Material>();
			if (mat == null) {
				using (_material.Begin()) {
					cam.Apply(ref world);
					if (lights != null)
						lights.Apply(ref world);
					if(_bones != null && _bones.Length > 0)
						_material.Shader.Uniform("Bones[0]", _boneTransforms);
					using (_vbuffer.Begin()) {
						_ibuffer.Draw();
					}
				}
			} else {
				cam.Apply(ref world);
				if (lights != null)
					lights.Apply(ref world);
				if(_bones != null && _bones.Length > 0)
					mat.Shader.Uniform("Bones[0]", _boneTransforms);
				using (_vbuffer.Begin()) {
					_ibuffer.Draw();
				}
			}
		}

		void PrepareBoneTransforms(Node node, ref Matrix4 parentTransform) {
			Matrix4 g;
			Matrix4.Mult(ref node.transform, ref parentTransform, out g);
			if (_boneSet.Contains(node))
				Matrix4.Mult(ref node.Bone._offset, ref g, out _boneTransforms[node.Bone.Index]);
			if (node.Children != null) {
				foreach (var child in node.Children)
					this.PrepareBoneTransforms(child, ref g);
			}
		}
	}
}
