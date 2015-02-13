using System;
using System.Linq;
using System.Text;
using OpenTK;
using GameStack;
using GameStack.Graphics;

namespace GameStack.Graphics {
	public class BlurShader : Shader {
#if __MOBILE__
		const string TextureFunc = "texture2D";
#else 
		const string TextureFunc = "texture";
#endif

		public BlurShader (BlurDirection dir, int kernel, double sigma) :
			base(BuildVertSource(dir, kernel), BuildFragSource(kernel, sigma), string.Format("blur::{0}:{1}:{2}", dir, kernel, sigma)) {
		}

		static string BuildVertSource (BlurDirection dir, int length) {
			var sb = new StringBuilder();
			var fmt = dir == BlurDirection.Horizontal ?
			          "blurTexCoords[{0}] = texCoord0 + vec2(TexelSize * {1}.0, 0.0);\n"
			          : "blurTexCoords[{0}] = texCoord0 + vec2(0.0, TexelSize * {1}.0);\n";
			for (var i = 0; i < length; i++)
				sb.AppendFormat(fmt, i, i - length);
			for (var i = length; i < length * 2; i++)
				sb.AppendFormat(fmt, i, i - length + 1);

			return string.Format(VertSourceFormat, length * 2, sb.ToString());
		}

		static string BuildFragSource (int length, double sigma) {
			var sb = new StringBuilder();
			var weights = BuildWeights(length, sigma);
			for (var i = 0; i < length; i++)
				sb.AppendFormat("c += {2}(Texture, blurTexCoords[{0}]) * {1};\n",
					i, weights[length - i].ToString("0.0###################"), TextureFunc);
			sb.AppendFormat("c += {1}(Texture, texCoord0) * {0};\n",
				weights[0].ToString("0.0###################"), TextureFunc);
			for (var i = length; i < length * 2; i++)
				sb.AppendFormat("c += {2}(Texture, blurTexCoords[{0}]) * {1};\n",
					i, weights[i - length + 1].ToString("0.0###################"), TextureFunc);

			return string.Format(FragSourceFormat, length * 2, sb.ToString());
		}

		static double[] BuildWeights (int size, double sigma) {
			var gbase = 1.0 / (Math.Sqrt(2.0 * Math.PI) * sigma);
			var weights = new double[size + 1];
			for (var i = 0; i <= size; i++) {
				weights[i] = gbase * Math.Exp(-((i * i) / (2.0 * sigma * sigma)));
			}
			return weights;
		}

#if __MOBILE__
		const string VertSourceFormat = @"
uniform mat4 WorldViewProjection;
uniform mat4 World;
uniform float TexelSize;

attribute vec4 Position;
attribute vec2 TexCoord0;

varying vec2 texCoord0;
varying vec2 blurTexCoords[{0}];

void main() {{
	gl_Position = WorldViewProjection * Position;
	texCoord0 = TexCoord0;
	{1}
}}
";
		const string FragSourceFormat = @"
uniform sampler2D Texture;
uniform mediump vec4 Tint;

varying mediump vec2 texCoord0;
varying mediump vec2 blurTexCoords[{0}];

void main() {{
	mediump vec4 c = vec4(0.0);
	mediump vec4 tmp = vec4(0.0);
	{1}
	gl_FragColor = c * Tint;
}}
";
#else
		const string VertSourceFormat = @"#version 150
uniform mat4 WorldViewProjection;
uniform mat4 World;
uniform float TexelSize;

in vec4 Position;
in vec2 TexCoord0;

out vec2 texCoord0;
out vec2 blurTexCoords[{0}];

void main() {{
	gl_Position = WorldViewProjection * Position;
	texCoord0 = TexCoord0;
	{1}
}}
";
		const string FragSourceFormat = @"#version 150
uniform sampler2D Texture;
uniform vec4 Tint;

in vec2 texCoord0;
in vec2 blurTexCoords[{0}];

out vec4 FragColor;

void main() {{
	vec4 c = vec4(0.0);
	vec4 tmp = vec4(0.0);
	{1}
	FragColor = c * Tint;
}}";
#endif
	}

	public enum BlurDirection {
		Horizontal,
		Vertical
	}
}

