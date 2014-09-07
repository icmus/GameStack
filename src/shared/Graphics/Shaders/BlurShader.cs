using System;
using System.Linq;
using System.Text;
using OpenTK;
using GameStack;
using GameStack.Graphics;

namespace GameStack.Graphics {
	public class BlurShader : Shader {
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
				sb.AppendFormat("c += texture2D(DiffuseMap, blurTexCoords[{0}]) * {1};\n",
					i, weights[length - i].ToString("0.0###################"));
			sb.AppendFormat("c += texture2D(DiffuseMap, texCoord0) * {0};\n",
				weights[0].ToString("0.0###################"));
			for (var i = length; i < length * 2; i++)
				sb.AppendFormat("c += texture2D(DiffuseMap, blurTexCoords[{0}]) * {1};\n",
					i, weights[i - length + 1].ToString("0.0###################"));

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
uniform sampler2D DiffuseMap;
uniform vec4 Tint;

varying vec2 texCoord0;
varying vec2 blurTexCoords[{0}];

void main() {{
	vec4 c = vec4(0.0);
	vec4 tmp = vec4(0.0);
	{1}
	gl_FragColor = c * Tint;
}}";
	}

	public enum BlurDirection {
		Horizontal,
		Vertical
	}
}

