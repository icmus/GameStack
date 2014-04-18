using System;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	public class MeshMaterial : Material {
		static readonly string[] DiffuseMapNames = new[] { "DiffuseMap0", "DiffuseMap1", "DiffuseMap2", "DiffuseMap3" };
		static readonly string[] DiffuseBlendNames = new[] { "DiffuseBlendFactor0", "DiffuseBlendFactor1", "DiffuseBlendFactor2", "DiffuseBlendFactor3" };
		static readonly string[] NormalMapNames = new[] { "NormalMap0", "NormalMap1", "NormalMap2", "NormalMap3" };
		static readonly string[] NormalBlendNames = new[] { "NormalBlendFactor0", "NormalBlendFactor1", "NormalBlendFactor2", "NormalBlendFactor3" };
		static readonly string[] SpecularMapNames = new[] { "SpecularMap0", "SpecularMap1", "SpecularMap2", "SpecularMap3" };
		static readonly string[] SpecularBlendNames = new[] { "SpecularBlendFactor0", "SpecularBlendFactor1", "SpecularBlendFactor2", "SpecularBlendFactor3" };
		static readonly string[] EmissiveMapNames = new[] { "EmissiveMap0", "EmissiveMap1", "EmissiveMap2", "EmissiveMap3" };
		static readonly string[] EmissiveBlendNames = new[] { "EmissiveBlendFactor0", "EmissiveBlendFactor1", "EmissiveBlendFactor2", "EmissiveBlendFactor3" };

		string _name;
		bool _isTwoSided, _isWireframeEnabled;
		BlendMode _blendMode;
		float _opacity, _shininess, _shininessStrength;
		Vector4 _colorAmbient, _colorDiffuse, _colorSpecular, _colorEmissive, _colorTransparent;
		TextureSlot[] _diffuseMap, _normalMap, _specularMap, _emissiveMap;

		int _units;
		bool _cullingState;
		int[] _polygonState;
		int _blendSrcState, _blendDstState;

		public MeshMaterial (Shader shader = null, string name = "") : base(shader) {
			_name = name;
			_polygonState = new int[2];
		}

		public string Name { get { return _name; } }

		public bool IsTwoSided { get { return _isTwoSided; } set { _isTwoSided = value; } }

		public bool IsWireframeEnabled { get { return _isWireframeEnabled; } set { _isWireframeEnabled = value; } }

		public BlendMode BlendMode { get { return _blendMode; } set { _blendMode = value; } }

		public float Opacity { get { return _opacity; } set { _opacity = value; } }

		public float Shininess { get { return _shininess; } set { _shininess = value; } }

		public float ShininessStrength { get { return _shininessStrength; } set { _shininessStrength = value; } }

		public Vector4 ColorAmbient { get { return _colorAmbient; } set { _colorAmbient = value; } }

		public Vector4 ColorDiffuse { get { return _colorDiffuse; } set { _colorDiffuse = value; } }

		public Vector4 ColorSpecular { get { return _colorSpecular; } set { _colorSpecular = value; } }

		public Vector4 ColorEmissive { get { return _colorEmissive; } set { _colorEmissive = value; } }

		public Vector4 ColorTransparent { get { return _colorTransparent; } set { _colorTransparent = value; } }

		public TextureSlot[] DiffuseMap { get { return _diffuseMap; } set { _diffuseMap = value; } }

		public TextureSlot[] NormalMap { get { return _normalMap; } set { _normalMap = value; } }

		public TextureSlot[] SpecularMap { get { return _specularMap; } set { _specularMap = value; } }

		public TextureSlot[] EmissiveMap { get { return _emissiveMap; } set { _emissiveMap = value; } }

		protected override void OnBegin () {
			base.OnBegin();

			if (_isTwoSided) {
				GL.GetBoolean(GetPName.CullFace, out _cullingState);
				if (_cullingState)
					GL.Disable(EnableCap.CullFace);
			}

			#if __DESKTOP__
			if (_isWireframeEnabled) {
				GL.GetInteger(GetPName.PolygonMode, _polygonState);
				GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
				if (_isTwoSided)
					GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
			}
			#endif

			#if __DESKTOP__
			GL.GetInteger(GetPName.BlendSrc, out _blendSrcState);
			GL.GetInteger(GetPName.BlendDst, out _blendDstState);
			if (_blendMode == BlendMode.Additive) {
				GL.GetInteger(GetPName.BlendSrc, out _blendSrcState);
				GL.GetInteger(GetPName.BlendDst, out _blendDstState);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			}
			#else
			GL.GetInteger(GetPName.BlendSrcRgb, out _blendSrcState);
			GL.GetInteger(GetPName.BlendDstRgb, out _blendDstState);
			if (_blendMode == BlendMode.Additive) {
				GL.GetInteger(GetPName.BlendSrcRgb, out _blendSrcState);
				GL.GetInteger(GetPName.BlendDstRgb, out _blendDstState);
				GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
			}
			#endif

			var shader = this.Shader;
			shader.Uniform("ColorAmbient", _colorAmbient);
			shader.Uniform("ColorDiffuse", _colorDiffuse);
			shader.Uniform("ColorSpecular", _colorSpecular);
			shader.Uniform("ColorEmissive", _colorEmissive);
			shader.Uniform("ColorTransparent", _colorTransparent);
			shader.Uniform("Opacity", _opacity);
			shader.Uniform("Shininess", _shininess);
			shader.Uniform("ShininessStrength", _shininessStrength);
			_units = 0;
			if (_diffuseMap != null)
				SetTextures(shader, ref _units, _diffuseMap, DiffuseMapNames, DiffuseBlendNames);
			if (_normalMap != null)
				SetTextures(shader, ref _units, _normalMap, NormalMapNames, NormalBlendNames);
			if (_specularMap != null)
				SetTextures(shader, ref _units, _specularMap, SpecularMapNames, SpecularBlendNames);
			if (_emissiveMap != null)
				SetTextures(shader, ref _units, _emissiveMap, EmissiveMapNames, EmissiveBlendNames);
		}

		protected override void OnEnd () {
			for (var i = 0; i < _units; i++) {
				GL.ActiveTexture(TextureUnit.Texture0 + i);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
			if (_isTwoSided && _cullingState)
				GL.Enable(EnableCap.CullFace);
			#if __DESKTOP__
			if (_isWireframeEnabled) {
				GL.PolygonMode(MaterialFace.Front, (PolygonMode)_polygonState[0]);
				if (_isTwoSided)
					GL.PolygonMode(MaterialFace.Back, (PolygonMode)_polygonState[1]);
			}
			#endif
			if (_blendMode == BlendMode.Additive)
				GL.BlendFunc((BlendingFactorSrc)_blendSrcState, (BlendingFactorDest)_blendDstState);

			base.OnEnd();
		}

		static void SetTextures(Shader shader, ref int unit, TextureSlot[] stack, string[] names, string[] blendNames) {
			for (var i = 0; i < stack.Length; i++) {
				var slot = stack[i];
				GL.ActiveTexture(TextureUnit.Texture0 + unit);
				GL.BindTexture(TextureTarget.Texture2D, slot.Texture.Handle);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)slot.WrapS);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)slot.WrapT);
				shader.Uniform(names[i], unit++);
				shader.Uniform(blendNames[i], slot.BlendFactor);
			}
		}
	}

	public class TextureSlot {
		public Texture Texture { get; set; }
		public int UVIndex { get; set; }
		public float BlendFactor { get; set; }
		public TextureOperation Operation { get; set; }
		public TextureWrap WrapS { get; set; }
		public TextureWrap WrapT { get; set; }

		public TextureSlot(Texture texture, int uvIndex, float blendFactor, TextureOperation operation, TextureWrap wrapS, TextureWrap wrapT) {
			this.Texture = texture;
			this.UVIndex = uvIndex;
			this.BlendFactor = blendFactor;
			this.Operation = operation;
			this.WrapS = wrapS;
			this.WrapT = wrapT;
		}
	}

	public enum BlendMode {
		Default = 0x0,
		Additive = 0x1,
	}
}
