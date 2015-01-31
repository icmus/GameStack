using System;
using System.Text;

namespace GameStack.Graphics {
	public class MeshShader : Shader {
		const string ShaderVersion = "150";

		int _maxNumLights;

		public MeshShader (MeshShaderSettings settings)
			: base(GetVertSource(settings), GetFragSource(settings), GetName(settings)) {
			_maxNumLights = settings.MaxLights;
		}

		public override int MaxNumLights { get { return _maxNumLights; } }

		static string GetName(MeshShaderSettings settings) {
			return typeof(MeshShader).FullName + "::" + settings.MaxLights;
		}

		static string GetVertSource (MeshShaderSettings settings) {
			var sb = new StringBuilder();
			sb.Append("#version ").AppendLine(ShaderVersion);
			AppendDefines(sb, settings);
			AppendUniform(sb, settings);
			AppendIn(sb, settings);
			AppendVarying(sb, settings, true);
			sb.AppendLine("void main() {");
			sb.AppendLine("    vec4 p = Position;");
			if (settings.BoneSlotCount > 0) {
				sb.AppendLine("    p = (");
				for (var i = 0; i < settings.BoneSlotCount; i++) {
					sb.Append("        BoneWeight").Append(i).Append(" * Bones[int(BoneIndex").Append(i).Append(")]");
					sb.AppendLine(i == settings.BoneSlotCount - 1 ? ") * p;" : " + ");
				}
			}
			sb.AppendLine("    normal = normalize((NormalMatrix * Normal).xyz);");
			sb.AppendLine("    v = (WorldView * p).xyz;");
			for (var i = 0; i < 4; i++)
				sb.Append("    texCoord").Append(i).Append(" = TexCoord").Append(i).AppendLine(";");
			sb.AppendLine("    gl_Position = WorldViewProjection * p;");
			sb.AppendLine("}");

			return sb.ToString();
		}

		static string GetFragSource (MeshShaderSettings settings) {
			var sb = new StringBuilder();
			sb.Append("#version ").AppendLine(ShaderVersion);
			AppendDefines(sb, settings);
			AppendUniform(sb, settings);
			AppendVarying(sb, settings, false);
			sb.AppendLine("out vec4 FragColor;");
			sb.AppendLine("void main() {");
			sb.AppendLine("    vec3 n = normal;");
			sb.AppendLine("    vec4 a = ColorAmbient;");
			sb.AppendLine("    vec4 d = ColorDiffuse;");
			sb.AppendLine("    vec4 s = ColorSpecular;");
			sb.AppendLine("    vec4 e = ColorEmissive;");
			sb.AppendLine("    vec4 tmp;");
			if (settings.Diffuse != null) {
				for (var i = 0; i < settings.Diffuse.Length; i++)
					sb.AppendFormat(FragDiffuse, i);
			}
			if (settings.Normal != null) {
				for (var i = 0; i < settings.Normal.Length; i++)
					sb.AppendFormat(FragNormal, i);
			}
			if (settings.Specular != null) {
				for (var i = 0; i < settings.Specular.Length; i++)
					sb.AppendFormat(FragSpecular, i);
			}
			if (settings.Emissive != null) {
				for (var i = 0; i < settings.Emissive.Length; i++)
					sb.AppendFormat(FragEmissive, i);
			}
			sb.AppendLine(FragPhong);
			sb.AppendLine("}");

			return sb.ToString();
		}

		static void AppendDefines (StringBuilder sb, MeshShaderSettings settings) {
			sb.Append("#define MAX_NUM_LIGHTS ").AppendLine(settings.MaxLights.ToString());
			if (settings.Diffuse != null) {
				for (var i = 0; i < settings.Diffuse.Length; i++) {
					var item = settings.Diffuse[0];
					sb.Append("#define DIFFUSE_INDEX").Append(i.ToString()).Append(" texCoord").AppendLine(item.Index.ToString());
					sb.Append("#define DIFFUSE_BLEND").Append(i.ToString()).Append(" ").AppendLine(item.BlendFactor.ToString());
					sb.Append("#define DIFFUSE_OP").Append(i.ToString()).Append(" ").AppendLine(((int)item.Operation).ToString());
				}
			}
			if (settings.Normal != null) {
				for (var i = 0; i < settings.Normal.Length; i++) {
					var item = settings.Normal[0];
					sb.Append("#define NORMAL_INDEX").Append(i.ToString()).Append(" texCoord").AppendLine(item.Index.ToString());
					sb.Append("#define NORMAL_BLEND").Append(i.ToString()).Append(" ").AppendLine(item.BlendFactor.ToString());
					sb.Append("#define NORMAL_OP").Append(i.ToString()).Append(" ").AppendLine(((int)item.Operation).ToString());
				}
			}
			if (settings.Specular != null) {
				for (var i = 0; i < settings.Specular.Length; i++) {
					var item = settings.Specular[0];
					sb.Append("#define SPECULAR_INDEX").Append(i.ToString()).Append(" texCoord").AppendLine(item.Index.ToString());
					sb.Append("#define SPECULAR_BLEND").Append(i.ToString()).Append(" ").AppendLine(item.BlendFactor.ToString());
					sb.Append("#define SPECULAR_OP").Append(i.ToString()).Append(" ").AppendLine(((int)item.Operation).ToString());
				}
			}
			if (settings.Emissive != null) {
				for (var i = 0; i < settings.Specular.Length; i++) {
					var item = settings.Specular[0];
					sb.Append("#define EMISSIVE_INDEX").Append(i.ToString()).Append(" texCoord").AppendLine(item.Index.ToString());
					sb.Append("#define EMISSIVE_BLEND").Append(i.ToString()).Append(" ").AppendLine(item.BlendFactor.ToString());
					sb.Append("#define EMISSIVE_OP").Append(i.ToString()).Append(" ").AppendLine(((int)item.Operation).ToString());
				}
			}
			sb.AppendLine();
		}

		static void AppendUniform (StringBuilder sb, MeshShaderSettings settings) {
			sb.AppendLine("uniform mat4 WorldViewProjection;");
			sb.AppendLine("uniform mat4 WorldView;");
			sb.AppendLine("uniform mat4 NormalMatrix;");
			sb.AppendLine("uniform vec4 ColorAmbient;");
			sb.AppendLine("uniform vec4 ColorDiffuse;");
			sb.AppendLine("uniform vec4 ColorSpecular;");
			sb.AppendLine("uniform vec4 ColorEmissive;");
			sb.AppendLine("uniform float Shininess;");
			sb.AppendLine("uniform float ShininessStrength;");
			if (settings.Diffuse != null) {
				for (var i = 0; i < settings.Diffuse.Length; i++)
					sb.Append("uniform sampler2D DiffuseMap").Append(i.ToString()).AppendLine(";");
			}
			if (settings.Normal != null) {
				for (var i = 0; i < settings.Normal.Length; i++)
					sb.Append("uniform sampler2D NormalMap").Append(i.ToString()).AppendLine(";");
			}
			if (settings.Specular != null) {
				for (var i = 0; i < settings.Specular.Length; i++)
					sb.Append("uniform sampler2D SpecularMap").Append(i.ToString()).AppendLine(";");
			}
			if (settings.Emissive != null) {
				for (var i = 0; i < settings.Emissive.Length; i++)
					sb.Append("uniform sampler2D EmissiveMap").Append(i.ToString()).AppendLine(";");
			}
			if (settings.BoneCount > 0)
				sb.Append("uniform mat4 Bones[").Append(settings.BoneCount).AppendLine("];");
			sb.AppendLine("uniform int NumLights;");
			if (settings.MaxLights > 0) {
				sb.AppendLine("uniform int LightType[MAX_NUM_LIGHTS];");
				sb.AppendLine("uniform vec3 LightVector[MAX_NUM_LIGHTS];");
				sb.AppendLine("uniform vec3 LightAmbient[MAX_NUM_LIGHTS];");
				sb.AppendLine("uniform vec3 LightDiffuse[MAX_NUM_LIGHTS];");
				sb.AppendLine("uniform vec3 LightSpecular[MAX_NUM_LIGHTS];");
				sb.AppendLine("uniform vec3 LightAttenuation[MAX_NUM_LIGHTS];");
			}
		}

		static void AppendIn (StringBuilder sb, MeshShaderSettings settings) {
			sb.AppendLine("in vec4 Position;");
			sb.AppendLine("in vec4 Normal;");
			for (var i = 0; i < 4; i++)
				sb.Append("in vec2 TexCoord").Append(i).AppendLine(";");
			for (var i = 0; i < settings.BoneSlotCount; i++) {
				sb.Append("in float BoneWeight").Append(i).AppendLine(";");
				sb.Append("in float BoneIndex").Append(i).AppendLine(";");
			}
		}

		static void AppendVarying (StringBuilder sb, MeshShaderSettings settings, bool output) {
			var dir = output ? "out" : "in";
			sb.Append(dir).AppendLine(" vec3 normal;");
			sb.Append(dir).AppendLine(" vec3 v;");
			for (var i = 0; i < 4; i++)
				sb.Append(dir).Append(" vec2 texCoord").Append(i).AppendLine(";");
		}

		const string FragPhong = @"
	vec3 la = vec3(0.0);
	vec3 ld = vec3(0.0);
	vec3 ls = vec3(0.0);

	float atten;
	vec3 l;
	for(int i = 0; i < MAX_NUM_LIGHTS; i++) {
		if(LightType[i] == 0) {
			atten = 1.0;
			l = -LightVector[i];
		} else if (LightType[i] == 1) {
			l = LightVector[i] - v;
			float dist = length(l);
			l /= dist;
			vec3 att = LightAttenuation[i];
			atten = 1.0 / (att.x * dist * dist + att.y * dist + att.z);
		}
		la += a.xyz * LightAmbient[i];
		ld += d.xyz * LightDiffuse[i] * max(dot(n, l), 0.0) * atten;
		vec3 e = normalize(-v);
		vec3 r = normalize(-reflect(l, n));
		ls += s.xyz * LightSpecular[i] * pow(max(dot(r, e), 0.0), Shininess) * ShininessStrength * atten;
	}
	FragColor = vec4(la, 1.0) + vec4(ld, 1.0) + vec4(ls, 1.0);
";

		const string FragDiffuse = @"
    tmp = texture(DiffuseMap{0}, DIFFUSE_INDEX{0}) * DIFFUSE_BLEND{0};
#if DIFFUSE_OP{0}==0
	d *= tmp;
#elif DIFFUSE_OP{0}==1
	d += tmp;
#elif DIFFUSE_OP{0}==2
	d -= tmp;
#elif DIFFUSE_OP{0}==3
	d /= tmp;
#elif DIFFUSE_OP{0}==4
	d = (d + tmp) - (d * tmp);
#elif DIFFUSE_OP{0}==5
	d += (tmp - 0.5);
#endif
";
		const string FragNormal = @"
	tmp = texture(NormalMap{0}, NORMAL_INDEX{0}) * NORMAL_BLEND{0};
#if NORMAL_OP{0}==0
	n *= tmp;
#elif NORMAL_OP{0}==1
	n += tmp;
#elif NORMAL_OP{0}==2
	n -= tmp;
#elif NORMAL_OP{0}==3
	n /= tmp;
#elif NORMAL_OP{0}==4
	n = (n + tmp) - (c * tmp);
#elif NORMAL_OP{0}==5
	n += (tmp - 0.5);
#elif NORMAL_OP{0}==0xff
	n = tmp;
#endif
";
		const string FragSpecular = @"
	tmp = texture(SpecularMap{0}, SPECULAR_INDEX{0}) * SPECULAR_BLEND{0};
#if SPECULAR_OP{0}==0
	s *= tmp;
#elif SPECULAR_OP{0}==1
	s += tmp;
#elif SPECULAR_OP{0}==2
	s -= tmp;
#elif SPECULAR_OP{0}==3
	s /= tmp;
#elif SPECULAR_OP{0}==4
	s = (s + tmp) - (s * tmp);
#elif SPECULAR_OP{0}==5
	s += (tmp - 0.5);
#endif
";
		const string FragEmissive = @"
	tmp = texture(EmissiveMap{0}, EMISSIVE_INDEX{0}) * EMISSIVE_BLEND{0};
#if EMISSIVE_OP{0}==0
	e *= tmp;
#elif EMISSIVE_OP{0}==1
	e += tmp;
#elif EMISSIVE_OP{0}==2
	e -= tmp;
#elif EMISSIVE_OP{0}==3
	e /= tmp;
#elif EMISSIVE_OP{0}==4
	e = (e + tmp) - (e * tmp);
#elif EMISSIVE_OP{0}==5
	e += (tmp - 0.5);
#endif
";
	}

	public class MeshShaderSettings {
		public int BoneSlotCount { get; set; }

		public int BoneCount { get; set; }

		public int MaxLights { get; set; }

		public ShadingMode ShadingMode { get; set; }

		public MeshTextureSlot[] Diffuse { get; set; }

		public MeshTextureSlot[] Normal { get; set; }

		public MeshTextureSlot[] Specular { get; set; }

		public MeshTextureSlot[] Emissive { get; set; }
	}

	public struct MeshTextureSlot {
		public int Index;
		public float BlendFactor;
		public TextureOperation Operation;

		public MeshTextureSlot(int index, float blendFactor, TextureOperation operation) {
			this.Index = index;
			this.BlendFactor = blendFactor;
			this.Operation = operation;
		}
	}

	public enum TextureOperation {
		Multiply = 0x0,
		Add = 0x1,
		Subtract = 0x2,
		Divide = 0x3,
		SmoothAdd = 0x4,
		SignedAdd = 0x5,
		Replace = 0xff
	}

	public enum ShadingMode {
		None = 0x0,
		Flat = 0x1,
		Gouraud = 0x2,
		Phong = 0x3,
		Blinn = 0x4,
		Toon = 0x5,
		OrenNayar = 0x6,
		Minnaert = 0x7,
		CookTorrance = 0x8,
		NoShading = 0x9,
		Fresnel = 0xa
	}
}
