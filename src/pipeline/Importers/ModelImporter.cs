using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GameStack.Pipeline.Tar;
using Assimp;

namespace GameStack.Pipeline
{
    [ContentType(".dae .obj .fbx .3ds .blend", ".model")]
    public class ModelImporter : ContentImporter
    {
        public override void Import (Stream input, Stream output, string filename)
        {
            using (var importer = new AssimpContext()) {
                var scene = importer.ImportFileFromStream(input,
                                PostProcessSteps.Triangulate | PostProcessSteps.SortByPrimitiveType | PostProcessSteps.GenerateNormals,
                                Path.GetExtension(filename));
                using (var tw = new TarWriter(output)) {
                    var textures = new Dictionary<string, string>();
                    using (var ms = new MemoryStream()) {
                        using (var bw = new BinaryWriter(ms)) {
                            this.WriteModel(scene, bw, textures);
                            bw.Flush();
                            //this.PrintNode(scene, scene.RootNode, 0);
                            ms.Position = 0;
                            tw.Write(ms, ms.Length, "model.bin");
                        }
                    }
                    this.WriteTextures(scene, tw, textures);
                }
            }
        }

        void WriteModel (Scene scene, BinaryWriter writer, Dictionary<string,string> textures)
        {
            var boneCounts = new Dictionary<int, int>();
            var boneSlotCounts = new Dictionary<int, int>();
            foreach (var mesh in scene.Meshes) {
                var matId = mesh.MaterialIndex;
                var boneSlotCount = this.GetBoneSlotCount(mesh);
                var boneCount = mesh.BoneCount;
                if (!boneCounts.ContainsKey(matId) || boneCounts[matId] < boneCount)
                    boneCounts[matId] = boneCount;
                if (!boneSlotCounts.ContainsKey(matId) || boneSlotCounts[matId] < boneSlotCount)
                    boneSlotCounts[matId] = boneSlotCount;
            }

            writer.Write(scene.MaterialCount);
            for (var i = 0; i < scene.MaterialCount; i++) {
                var mat = scene.Materials[i];
                this.WriteMaterial(mat, writer, textures,
                    boneCounts.ContainsKey(i) ? boneCounts[i] : 0,
                    boneSlotCounts.ContainsKey(i) ? boneSlotCounts[i] : 0);
            }

            writer.Write(scene.MeshCount);
            // when a mesh is split because parts of it use different materials, generate
            // new names for the additional meshes so they don't collide
            var meshNames = new HashSet<string>();
            foreach (var mesh in scene.Meshes) {
                int i = 0;
                while (meshNames.Contains(mesh.Name)) {
                    var idx = mesh.Name.LastIndexOf('.');
                    if (idx > 0)
                        mesh.Name = mesh.Name.Substring(0, idx);
                    mesh.Name = mesh.Name + "." + (i++).ToString("D3");
                }
                meshNames.Add(mesh.Name);
                this.WriteMesh(mesh, writer);
            }

            writer.Write(scene.Animations.Count(i => i.HasNodeAnimations));
            foreach (var anim in scene.Animations) {
                if (anim.HasMeshAnimations)
                    Console.WriteLine("WARNING: mesh animations are not currently supported");
                if (anim.HasNodeAnimations)
                    this.WriteAnimation(anim, writer);
            }

            this.WriteNode(scene.RootNode, writer);
        }

        void WriteMaterial (Material mat, BinaryWriter writer, Dictionary<string,string> textures, int boneCount, int boneSlotCount)
        {
            writer.Write(mat.Name);
            writer.Write(mat.IsTwoSided);
            writer.Write(mat.IsWireFrameEnabled);
            writer.Write((int)mat.ShadingMode);
            writer.Write((int)mat.BlendMode);
            writer.Write(mat.Opacity);
            writer.Write(mat.Shininess);
            writer.Write(mat.ShininessStrength);
            writer.Write(mat.ColorAmbient);
            writer.Write(mat.ColorDiffuse);
            writer.Write(mat.ColorSpecular);
            writer.Write(mat.ColorEmissive);
            writer.Write(mat.ColorTransparent);
            writer.Write(boneCount);
            writer.Write(boneSlotCount);
            this.WriteTextureStack(mat, TextureType.Diffuse, writer, textures);
            this.WriteTextureStack(mat, TextureType.Normals, writer, textures);
            this.WriteTextureStack(mat, TextureType.Specular, writer, textures);
            this.WriteTextureStack(mat, TextureType.Emissive, writer, textures);
        }

        void WriteTextureStack (Material mat, TextureType type, BinaryWriter writer, Dictionary<string,string> textures)
        {
            var count = mat.GetMaterialTextureCount(type);
            writer.Write(count);
            for (var i = 0; i < count; i++) {
                TextureSlot slot;
                mat.GetMaterialTexture(type, i, out slot);
                if (slot.Mapping != TextureMapping.FromUV)
                    throw new ContentException("Unsupported texture mapping type, textures must be UV-mapped.");
                if (!textures.ContainsKey(slot.FilePath)) {
                    textures.Add(slot.FilePath, Path.GetFileNameWithoutExtension(slot.FilePath));
                }
                writer.Write(textures[slot.FilePath]);
                writer.Write(slot.UVIndex);
                writer.Write(slot.BlendFactor);
                writer.Write((int)slot.Operation);
                writer.Write(slot.WrapModeU == TextureWrapMode.Wrap);
                writer.Write(slot.WrapModeV == TextureWrapMode.Wrap);
            }
        }

        unsafe void WriteMesh (Mesh mesh, BinaryWriter writer)
        {
            var boneSlotCount = this.GetBoneSlotCount(mesh);

            VertexFlags flags;
            int stride;
            this.GetVertexFormat(mesh, boneSlotCount, out flags, out stride);
            var count = mesh.VertexCount;

            writer.Write(mesh.Name);
            writer.Write((uint)flags);
            writer.Write(boneSlotCount);
            writer.Write(mesh.MaterialIndex);
            var vertices = new float[stride * count];
            int idx = 0, offset = 0;
            foreach (var pos in mesh.Vertices) {
                vertices[idx] = pos.X;
                vertices[idx + 1] = pos.Y;
                vertices[idx + 2] = pos.Z;
                idx += stride;
            }
            offset = idx = 3;
            if (mesh.HasNormals) {
                foreach (var norm in mesh.Normals) {
                    vertices[idx] = norm.X;
                    vertices[idx + 1] = norm.Y;
                    vertices[idx + 2] = norm.Z;
                    idx += stride;
                }
                offset += 3;
                idx = offset;
            }
            for (var i = 0; i < 4; i++) {
                if (mesh.HasTextureCoords(i)) {
                    if (mesh.UVComponentCount[i] != 2)
                        Console.WriteLine("WARNING: texture coordinates should have 2 components, but this channel has " + mesh.UVComponentCount[i]);
                    foreach (var uv in mesh.TextureCoordinateChannels[i]) {
                        vertices[idx] = uv.X;
                        vertices[idx + 1] = uv.Y;
                        idx += stride;
                    }
                    offset += 2;
                    idx = offset;
                }
            }
            for (var i = 0; i < 4; i++) {
                if (mesh.HasVertexColors(i)) {
                    foreach (var c in mesh.VertexColorChannels[i]) {
                        vertices[idx] = c.R;
                        vertices[idx + 1] = c.G;
                        vertices[idx + 2] = c.B;
                        vertices[idx + 3] = c.A;
                        idx += stride;
                    }
                    offset += 4;
                    idx = offset;
                }
            }
            // add bone information to every vertex
            for (var i = 0; i < mesh.BoneCount; i++) {
                foreach (var weight in mesh.Bones[i].VertexWeights) {
                    if (weight.Weight > 0f) {
                        idx = (stride * weight.VertexID) + offset;
                        while (vertices[idx] != 0f)
                            idx++;
                        vertices[idx] = weight.Weight;
                        vertices[idx + boneSlotCount] = i;
                    }
                }
            }
            writer.Write(vertices.Length);
            writer.Flush();
            fixed(float *fp = &vertices[0]) {
                byte* bp = (byte*)fp;
                using (var ms = new UnmanagedMemoryStream(bp, vertices.Length * sizeof(float))) {
                    ms.CopyTo(writer.BaseStream);
                }
            }

            count = mesh.FaceCount;
            var indices = new int[count * 3];
            idx = 0;
            for (var i = 0; i < count; i++) {
                var face = mesh.Faces[i];
                var faceIndices = face.Indices;
                if (face.IndexCount != 3)
                    throw new InvalidOperationException("Polygonal faces are not supported, faces must be triangles.");
                indices[idx++] = faceIndices[0];
                indices[idx++] = faceIndices[1];
                indices[idx++] = faceIndices[2];
            }
            writer.Write(indices.Length);
            writer.Flush();
            fixed(int *ip = &indices[0]) {
                byte* bp = (byte*)ip;
                using (var ms = new UnmanagedMemoryStream(bp, indices.Length * sizeof(float))) {
                    ms.CopyTo(writer.BaseStream);
                }
            }

            count = mesh.BoneCount;
            writer.Write(count);
            for (var i = 0; i < count; i++) {
                var bone = mesh.Bones[i];
                writer.Write(bone.Name);
                writer.Write(bone.OffsetMatrix);
                var weightCount = bone.VertexWeightCount;
                writer.Write(weightCount);
                for (var j = 0; j < weightCount; j++) {
                    var weight = bone.VertexWeights[i];
                    writer.Write(weight.VertexID);
                    writer.Write(weight.Weight);
                }
            }
        }

        int GetBoneSlotCount (Mesh mesh)
        {
            if (mesh.BoneCount == 0)
                return 0;
            var map = new Dictionary<int, int>(mesh.VertexCount);
            foreach (var bone in mesh.Bones) {
                foreach (var weight in bone.VertexWeights) {
                    if (weight.Weight != 0f) {
                        if (!map.ContainsKey(weight.VertexID))
                            map.Add(weight.VertexID, 1);
                        else
                            map[weight.VertexID]++;
                    }
                }
            }
            if (map.Count == 0)
                return 0;
            // return next power of 2
            var n = map.Values.Max();
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            n++;
            return n;
        }

        void WriteAnimation (Animation anim, BinaryWriter writer)
        {
            writer.Write(anim.Name);
            writer.Write(anim.DurationInTicks);
            writer.Write(anim.TicksPerSecond);
            var count = anim.NodeAnimationChannelCount;
            writer.Write(count);
            for (var i = 0; i < count; i++) {
                var chan = anim.NodeAnimationChannels[i];
                writer.Write(chan.NodeName);
                writer.Write((int)chan.PreState);
                writer.Write((int)chan.PostState);
                var keyCount = chan.PositionKeyCount;
                writer.Write(keyCount);
                for (var j = 0; j < keyCount; j++) {
                    var key = chan.PositionKeys[j];
                    writer.Write(key.Time);
                    writer.Write(key.Value);
					writer.Write(0);
                }
                keyCount = chan.RotationKeyCount;
                writer.Write(keyCount);
                for (var j = 0; j < keyCount; j++) {
                    var key = chan.RotationKeys[j];
                    writer.Write(key.Time);
                    writer.Write(key.Value);
                }
                keyCount = chan.ScalingKeyCount;
                writer.Write(keyCount);
                for (var j = 0; j < keyCount; j++) {
                    var key = chan.ScalingKeys[j];
                    writer.Write(key.Time);
                    writer.Write(key.Value);
					writer.Write(0);
                }
            }
        }

        void WriteNode (Node node, BinaryWriter writer)
        {
            writer.Write(node.Name);
            writer.Write(node.Transform);
            var meshCount = node.MeshCount;
            writer.Write(meshCount);
            for (var i = 0; i < meshCount; i++) {
                writer.Write(node.MeshIndices[i]);
            }
            var childCount = node.ChildCount;
            writer.Write(childCount);
            for (var i = 0; i < childCount; i++) {
                this.WriteNode(node.Children[i], writer);
            }
        }

        unsafe void WriteTextures (Scene scene, TarWriter tw, Dictionary<string, string> textures)
        {
            for (var i = 0; i < scene.TextureCount; i++) {
                var tex = scene.Textures[i];
                var name = "*" + i;
                if (!textures.ContainsKey(name))
                    continue;
                textures.Remove(name);
                Bitmap bmp = null;
                if (tex.HasCompressedData) {
                    using (var ms = new MemoryStream(tex.CompressedData)) {
                        bmp = new Bitmap(ms);
                    }
                } else if (tex.HasNonCompressedData) {
                    fixed(Texel *p = tex.NonCompressedData) {
                        bmp = new Bitmap(tex.Width, tex.Height, tex.Width * 4, PixelFormat.Format32bppArgb, (IntPtr)p);
                    }
                }
                if (bmp != null) {
                    using (var ms = new MemoryStream()) {
                        bmp.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        tw.Write(ms, ms.Length, name + ".png");
                    }
                }
            }
            foreach (var kvp in textures) {
                try {
                    var img = ImageLoader.Load(kvp.Key);
                    using (var ms = new MemoryStream()) {
                        img.Save(ms, ImageFormat.Png);
                        ms.Position = 0;
                        tw.Write(ms, ms.Length, kvp.Value + ".png");
                    }
                } catch (Exception ex) {
                    throw new ContentException("Could not load referenced texture: " + kvp.Key, ex);
                }
            }
        }

        void GetVertexFormat (Mesh mesh, int boneCount, out VertexFlags flags, out int stride)
        {
            stride = 3;
            flags = 0;
            if (mesh.HasNormals) {
                stride += 3;
                flags |= VertexFlags.Normal;
            }
            if (mesh.HasTextureCoords(0)) {
                stride += 2;
                flags |= VertexFlags.TexCoord0;
            }
            if (mesh.HasTextureCoords(1)) {
                stride += 2;
                flags |= VertexFlags.TexCoord0;
            }
            if (mesh.HasTextureCoords(2)) {
                stride += 2;
                flags |= VertexFlags.TexCoord0;
            }
            if (mesh.HasTextureCoords(3)) {
                stride += 2;
                flags |= VertexFlags.TexCoord0;
            }
            if (mesh.HasVertexColors(0)) {
                stride += 4;
                flags |= VertexFlags.Color0;
            }
            if (mesh.HasVertexColors(1)) {
                stride += 4;
                flags |= VertexFlags.Color1;
            }
            if (mesh.HasVertexColors(2)) {
                stride += 4;
                flags |= VertexFlags.Color2;
            }
            if (mesh.HasVertexColors(3)) {
                stride += 4;
                flags |= VertexFlags.Color3;
            }
            stride += boneCount * 2;
        }

        void PrintNode (Scene scene, Node node, int indent)
        {
            Console.WriteLine(new string(' ', indent) + string.Format("Name = {0}", node.Name));
            foreach (var mesh in node.MeshIndices.Select(i => scene.Meshes[i])) {
                Console.WriteLine(new string(' ', indent + 2) + string.Format("Mesh: Name = {0}, Vertices = {1}, Faces = {2}, PrimitiveType = {3}",
                    mesh.Name, mesh.VertexCount, mesh.FaceCount, mesh.PrimitiveType));
            }
            foreach (var child in node.Children)
                this.PrintNode(scene, child, indent + 2);
        }

        [Flags]
        enum VertexFlags
        {
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
    }

    static class BinaryWriterAssimpExtensions
    {
        public static void Write (this BinaryWriter writer, Vector3D val)
        {
            writer.Write(val.X);
            writer.Write(val.Y);
            writer.Write(val.Z);
        }

        public static void Write (this BinaryWriter writer, Color4D val)
        {
            writer.Write(val.R);
            writer.Write(val.G);
            writer.Write(val.B);
            writer.Write(val.A);
        }

        public static void Write (this BinaryWriter writer, Quaternion val)
        {
            writer.Write(val.X);
            writer.Write(val.Y);
            writer.Write(val.Z);
            writer.Write(val.W);
        }

        public static void Write (this BinaryWriter writer, Matrix4x4 val)
        {
            writer.Write(val.A1);
            writer.Write(val.B1);
            writer.Write(val.C1);
            writer.Write(val.D1);
            writer.Write(val.A2);
            writer.Write(val.B2);
            writer.Write(val.C2);
            writer.Write(val.D2);
            writer.Write(val.A3);
            writer.Write(val.B3);
            writer.Write(val.C3);
            writer.Write(val.D3);
            writer.Write(val.A4);
            writer.Write(val.B4);
            writer.Write(val.C4);
            writer.Write(val.D4);
        }
    }
}
