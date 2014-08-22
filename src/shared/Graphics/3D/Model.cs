#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GameStack.Content;
using OpenTK;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
using BufferUsage = OpenTK.Graphics.OpenGL.BufferUsageHint;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	public class Model : IDisposable {
		Node _root;
		Dictionary<string, Animation> _anims;
		Dictionary<string, Mesh> _meshes;

        public Model (string path, Func<string, MeshShaderSettings, MeshMaterial> matBuilder = null) {
			using (var stream = Assets.ResolveStream(path)) {
				this.Initialize(stream, matBuilder);
			}
		}

        public Model (Stream stream, Func<string, MeshShaderSettings, MeshMaterial> matBuilder = null) {
			this.Initialize(stream, matBuilder);
		}

		public Node RootNode { get { return _root; } }

		public IDictionary<string, Animation> Animations { get { return _anims; } }

		public IDictionary<string, Mesh> Meshes { get { return _meshes; } }

		public void Draw (ref Matrix4 world) {
			_root.Draw(ref world);
		}

		public void Draw (Matrix4 world) {
			this.Draw(ref world);
		}

        void Initialize (Stream stream, Func<string, MeshShaderSettings, MeshMaterial> matBuilder) {
			var tr = new TarReader(stream);
			MemoryStream modelStream = null;
			var textures = new Dictionary<string, Texture>();

			try {
				while (tr.MoveNext(false)) {
					var name = tr.FileInfo.FileName;
					if (name == "model.bin") {
						modelStream = new MemoryStream();
						tr.Read(modelStream);
						modelStream.Position = 0;
					}
					else if (name.Length > 4 && name.EndsWith(".png")) {
						using(var ms = new MemoryStream()) {
							tr.Read(ms);
							ms.Position = 0;
							textures.Add(name.Substring(0, name.Length - 4), new Texture(ms, null));
						}
					}
					else
						throw new ContentException("Unrecognized model file " + tr.FileInfo.FileName);
				}
				if (modelStream == null)
					throw new ContentException("Model data not found.");
				using (var reader = new BinaryReader(modelStream)) {
                    this.InitModel(reader, textures, matBuilder);
				}
			} finally {
				if (modelStream != null)
					modelStream.Dispose();
			}
		}

        void InitModel (BinaryReader reader, Dictionary<string, Texture> textures, Func<string, MeshShaderSettings, MeshMaterial> matBuilder) {
            var materials = this.ReadMaterials(reader, textures, matBuilder);
			var meshes = this.ReadMeshes(reader, materials);
			_meshes = meshes.ToDictionary<Mesh,string>(m => m.Name);
			_anims = this.ReadAnimations(reader);
			_root = this.ReadNode(reader, null, meshes);
			this.Link(_root);
			foreach (var anim in _anims.Values) {
				this.Link(anim);
			}
		}

		void Link(Node node) {
			if (node.Meshes != null) {
				foreach (var mesh in node.Meshes) {
					mesh.Node = node;
					if (mesh.Bones != null) {
						foreach (var bone in mesh.Bones) {
							bone.Node = (node.Parent ?? node).FindByName(bone.Name);
							if (bone.Node == null)
								throw new ContentException("Could not find bone node " + bone.Name);
							bone.Node.Bone = bone;
						}
					}
				}
			}
			if (node.Children != null) {
				foreach (var child in node.Children)
					this.Link(child);
			}
		}

		void Link(Animation anim) {
			foreach (var chan in anim.Channels) {
				chan.Node = _root.FindByName(chan.NodeName);
			}
		}

		Node ReadNode(BinaryReader reader, Node parent, Mesh[] meshes) {
			var name = reader.ReadString();
			Matrix4 transform;
			reader.ReadMatrix4(out transform);
			var meshCount = reader.ReadInt32();
			Mesh[] meshList = null;
			if(meshCount > 0) {
				meshList = new Mesh[meshCount];
				for (var i = 0; i < meshCount; i++) {
					var idx = reader.ReadInt32();
					if (idx < 0 || idx >= meshes.Length)
						throw new ContentException("Invalid mesh index " + idx);
					meshList[i] = meshes[idx];
				}
			}
			var node = new Node(name, parent, ref transform, meshList);
			var childCount = reader.ReadInt32();
			Node[] children = null;
			if(childCount > 0) {
				children = new Node[childCount];
				for (var i = 0; i < childCount; i++) {
					children[i] = this.ReadNode(reader, node, meshes);
				}
			}
			node.Children = children;
			return node;
		}

        Material[] ReadMaterials (BinaryReader reader, Dictionary<string, Texture> textures, Func<string, MeshShaderSettings, MeshMaterial> matBuilder) {
			var matCount = reader.ReadInt32();
			var mats = new MeshMaterial[matCount];
			for (var i = 0; i < matCount; i++) {
				var name = reader.ReadString();

				var isTwoSided = reader.ReadBoolean();
				var isWireframeEnabled = reader.ReadBoolean();
				var shadingMode = (ShadingMode)reader.ReadInt32();
				var blendMode = (BlendMode)reader.ReadInt32();
				var opacity = reader.ReadSingle();
				var shininess = reader.ReadSingle();
				var shininessStrength = reader.ReadSingle();
				var colorAmbient = reader.ReadVector4();
				var colorDiffuse = reader.ReadVector4();
				var colorSpecular = reader.ReadVector4();
				var colorEmissive = reader.ReadVector4();
				var colorTransparent = reader.ReadVector4();
				var boneCount = reader.ReadInt32();
				var boneSlotCount = reader.ReadInt32();
				var diffuseMap = this.ReadTextureStack(reader, textures);
				var normalMap = this.ReadTextureStack(reader, textures);
				var specularMap = this.ReadTextureStack(reader, textures);
				var emissiveMap = this.ReadTextureStack(reader, textures);

				var settings = new MeshShaderSettings {
					MaxLights = 4,
					BoneCount = boneCount,
					BoneSlotCount = boneSlotCount,
					ShadingMode = shadingMode,
					Diffuse = GetMeshTextureStack(diffuseMap),
					Normal = GetMeshTextureStack(normalMap),
					Specular = GetMeshTextureStack(specularMap),
					Emissive = GetMeshTextureStack(emissiveMap)
				};

				MeshMaterial mat;
				if (matBuilder != null)
					mat = matBuilder(name, settings);
				else {
					mat = new MeshMaterial(null, name);
					mat.Shader = new MeshShader(settings);
				}

				mat.IsTwoSided = isTwoSided;
				mat.IsWireframeEnabled = isWireframeEnabled;
				mat.BlendMode = blendMode;
				mat.Opacity = opacity;
				mat.Shininess = shininess;
				mat.ShininessStrength = shininessStrength;
				mat.ColorAmbient = colorAmbient;
				mat.ColorDiffuse = colorDiffuse;
				mat.ColorSpecular = colorSpecular;
				mat.ColorEmissive = colorEmissive;
				mat.ColorTransparent = colorTransparent;
				mat.DiffuseMap = diffuseMap;
				mat.NormalMap = normalMap;
				mat.SpecularMap = specularMap;
				mat.EmissiveMap = emissiveMap;

				mats[i] = mat;
			}
			return mats;
		}

		static MeshTextureSlot[] GetMeshTextureStack(TextureSlot[] slots) {
			return slots == null ? null : slots.Select(s => new MeshTextureSlot(s.UVIndex, s.BlendFactor, s.Operation)).ToArray();
		}

		TextureSlot[] ReadTextureStack (BinaryReader reader, Dictionary<string, Texture> textures) {
			var count = reader.ReadInt32();
			if (count == 0)
				return null;
			var slots = new TextureSlot[count];
			for (var i = 0; i < count; i++) {
				var name = reader.ReadString();
				if (!textures.ContainsKey(name))
					throw new ContentException("Referenced texture not found: " + name);
				slots[i] = new TextureSlot(
					textures[name],
					reader.ReadInt32(),
					reader.ReadSingle(),
					(TextureOperation)reader.ReadInt32(),
					reader.ReadBoolean() ? TextureWrap.Repeat : TextureWrap.Clamp,
					reader.ReadBoolean() ? TextureWrap.Repeat : TextureWrap.Clamp);
			}
			return slots;
		}

		unsafe Mesh[] ReadMeshes (BinaryReader reader, Material[] materials) {
			var meshCount = reader.ReadInt32();
			var meshes = new Mesh[meshCount];
			for (var i = 0; i < meshCount; i++) {
				var name = reader.ReadString();
				var format = GetVertexFormat((VertexFlags)reader.ReadInt32(), reader.ReadInt32());
				var matIdx = reader.ReadInt32();
				if (matIdx < 0 || matIdx >= materials.Length)
					throw new ContentException("Invalid material index " + matIdx);
				var count = reader.ReadInt32();
				var vertices = new float[count];
				byte[] buf = new byte[count * sizeof(float)];
				reader.Read(buf, 0, buf.Length);
				fixed(byte *bp = &buf[0]) {
					fixed(float *fp = &vertices[0]) {
						float* src = (float*)bp;
						float* dst = fp;
						for (var j = 0; j < count; j++) {
							*(dst++) = *(src++);
						}
					}
				}
				count = reader.ReadInt32();
				var indices = new int[count];
				buf = new byte[count * sizeof(int)];
				reader.Read(buf, 0, buf.Length);
				fixed(byte *bp = &buf[0]) {
					fixed(int *ip = &indices[0]) {
						int* src = (int*)bp;
						int* dst = ip;
						for (var j = 0; j < count; j++) {
							*(dst++) = *(src++);
						}
					}
				}
				count = reader.ReadInt32();
				Bone[] bones = null;
				if (count > 0) {
					bones = new Bone[count];
					for (var j = 0; j < count; j++) {
						var boneName = reader.ReadString();
						Matrix4 offset;
						reader.ReadMatrix4(out offset);
						var weightCount = reader.ReadInt32();
						var weights = new VertexWeight[weightCount];
						var sz = weightCount * sizeof(VertexWeight);
						if (buf.Length < sz)
							buf = new byte[sz];
						reader.Read(buf, 0, sz);
						fixed(byte *src = &buf[0]) {
							var p = (VertexWeight*)src;
							fixed(VertexWeight *dst = &weights[0]) {
								for (var k = 0; k < weightCount; k++) {
									*(dst + k) = *(p + k);
								}
							}
						}
						bones[j] = new Bone(j, boneName, ref offset, weights);
					}
				}
				meshes[i] = new Mesh(name, materials[matIdx], format, vertices, indices, bones);
			}
			return meshes;
		}

		unsafe Dictionary<string, Animation> ReadAnimations(BinaryReader reader) {
			var anims = new Dictionary<string, Animation>();

			var count = reader.ReadInt32();
			for (var i = 0; i < count; i++) {
				var name = reader.ReadString();
				var duration = reader.ReadDouble();
				var rate = reader.ReadDouble();
				var chanCount = reader.ReadInt32();
				var channels = new AnimationChannel[chanCount];
				for (var j = 0; j < chanCount; j++) {
					var nodeName = reader.ReadString();
					var preState = (AnimationBehavior)reader.ReadInt32();
					var postState = (AnimationBehavior)reader.ReadInt32();
					var keyCount = reader.ReadInt32();
					var positions = new PositionKey[keyCount];
					var sz = keyCount * sizeof(PositionKey);
					var buf = new byte[sz];
					reader.Read(buf, 0, sz);
					fixed(byte *p = &buf[0]) {
						var src = (PositionKey*)p;
						fixed(PositionKey* dst = &positions[0]) {
							for (var k = 0; k < keyCount; k++) {
								*(dst + k) = *(src + k);
							}
						}
					}
					keyCount = reader.ReadInt32();
					var rotations = new RotationKey[keyCount];
					sz = keyCount * sizeof(RotationKey);
					if (buf.Length < sz)
						buf = new byte[sz];
					reader.Read(buf, 0, sz);
					fixed(byte *p = &buf[0]) {
						var src = (RotationKey*)p;
						fixed(RotationKey *dst = &rotations[0]) {
							for (var k = 0; k < keyCount; k++) {
								*(dst + k) = *(src + k);
							}
						}
					}
					keyCount = reader.ReadInt32();
					var scales = new ScalingKey[keyCount];
					sz = keyCount * sizeof(ScalingKey);
					if (buf.Length < sz)
						buf = new byte[sz];
					reader.Read(buf, 0, sz);
					fixed(byte *p = &buf[0]) {
						var src = (ScalingKey*)p;
						fixed(ScalingKey *dst = &scales[0]) {
							for (var k = 0; k < keyCount; k++) {
								*(dst + k) = *(src + k);
							}
						}
					}
					channels[j] = new AnimationChannel(nodeName, preState, postState, positions, rotations, scales);
				}
				anims[name] = new Animation(name, duration, rate, channels);
			}

			return anims;
		}

		static VertexFormat GetVertexFormat (VertexFlags flags, int boneSlotCount) {
			uint elemCount = 0, f = (uint)flags;
			for (elemCount = 0; f != 0; elemCount++)
				f &= f - 1;
			var elems = new VertexElement[(int)elemCount + 1 + (boneSlotCount * 2)];

			int offset = 3, idx = 1;
			elems[0] = new VertexElement("Position", 0, 3);

			if ((flags & VertexFlags.Normal) != 0) {
				elems[idx++] = new VertexElement("Normal", offset, 3);
				offset += 3;
			}
			for (var i = 0; i < 4; i++) {
				if (((int)flags & ((int)VertexFlags.TexCoord0 << i)) != 0) {
					elems[idx++] = new VertexElement("TexCoord" + i, offset, 2);
					offset += 2;
				}
			}
			for (var i = 0; i < 4; i++) {
				if (((int)flags & ((int)VertexFlags.Color0 << i)) != 0) {
					elems[idx++] = new VertexElement("Color" + i, offset, 4);
					offset += 4;
				}
			}
			for (var i = 0; i < boneSlotCount; i++)
				elems[idx++] = new VertexElement("BoneWeight" + i, offset++, 1);
			for (var i = 0; i < boneSlotCount; i++)
				elems[idx++] = new VertexElement("BoneIndex" + i, offset++, 1);
			return new VertexFormat(offset, elems);
		}

		[Flags]
		enum VertexFlags {
			Normal = 1,
			TexCoord0 = 2,
			TexCoord1 = 4,
			TexCoord2 = 8,
			TexCoord3 = 16,
			Color0 = 32,
			Color1 = 64,
			Color2 = 128,
			Color3 = 256
		}

		enum TextureWrapMode {
			Clamp,
		}

		public void Draw () {
		}

		public void Dispose () {
		}
	}
}
