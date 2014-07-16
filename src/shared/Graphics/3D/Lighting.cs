using System;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	public class Lighting : ScopedObject {
		Light[] _lights;

		public Lighting (params Light[] lights) {
			_lights = lights;
		}

		public void Apply(ref Matrix4 world) {
			var mat = ScopedObject.Find<Material>();
			if (mat == null)
				throw new InvalidOperationException("A material must be in scope to use lighting.");
			var cam = ScopedObject.Find<Camera>();
			if (cam == null)
				throw new InvalidOperationException("A camera must be in scope to use lighting.");
			var shader = mat.Shader;
			if (shader.MaxNumLights == 0)
				throw new InvalidOperationException("The selected shader does not support lights.");
			if (shader.MaxNumLights < _lights.Length)
				throw new InvalidOperationException("Too many lights for the selected shader.");

			var view = cam.View;
			var viewRot = view;
			viewRot.Invert();
			viewRot.Transpose();

			var locType = shader.Uniform("LightType[0]");
			var locVector = shader.Uniform("LightVector[0]");
			var locAmbient = shader.Uniform("LightAmbient[0]");
			var locDiffuse = shader.Uniform("LightDiffuse[0]");
			var locSpecular = shader.Uniform("LightSpecular[0]");
			var locAtten = shader.Uniform("LightAttenuation[0]");
			for (var i = 0; i < _lights.Length; i++) {
				var light = _lights[i];
				if (locType >= 0)
					GL.Uniform1(locType + i, (int)light.Type);
				if (locVector >= 0) {
					var v = light.Vector;
					if (light.Type == LightType.Directional) {
						Vector3.Transform(ref v, ref viewRot, out v);
						v.Normalize();
					}
					else if (light.Type == LightType.Point)
						Vector3.Transform(ref v, ref view, out v);
					GL.Uniform3(locVector + i, v);
				}
				if (locAmbient >= 0)
					GL.Uniform3(locAmbient + i, light.Ambient);
				if (locDiffuse >= 0)
					GL.Uniform3(locDiffuse + i, light.Diffuse);
				if (locSpecular >= 0)
					GL.Uniform3(locSpecular + i, light.Specular);
				if (locAtten >= 0)
					GL.Uniform3(locAtten + i, light.Attenuation);
			}
		}
	}

	public abstract class Light {
		Vector3 _vector, _ambient, _diffuse, _specular, _atten;

		public Light (Vector3 vector, Vector3 ambient, Vector3 diffuse, Vector3 specular, Vector3 attenuation) {
			_vector = vector;
			_ambient = ambient;
			_specular = specular;
			_diffuse = diffuse;
			_atten = attenuation;
		}

		public Vector3 Vector { get { return _vector; } set { _vector = value; } }

		public Vector3 Ambient { get { return _ambient; } set { _ambient = value; } }

		public Vector3 Diffuse { get { return _diffuse; } set { _diffuse = value; } }

		public Vector3 Specular { get { return _specular; } set { _specular = value; } }

		public Vector3 Attenuation { get { return _atten; } set { _atten = value; } }

		public abstract LightType Type { get; }
	}

	public class DirectionalLight : Light {
		public DirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular) : base(direction, ambient, diffuse, specular, Vector3.Zero) {
		}

		public override LightType Type { get { return LightType.Directional; } }
	}

	public class PointLight : Light {
		public PointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular, Vector3 attenuation) : base(position, ambient, diffuse, specular, attenuation) {
		}

		public PointLight(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular, float attenQuadratic, float attenLinear, float attenConstant) 
			: base(position, ambient, diffuse, specular, new Vector3(attenQuadratic, attenLinear, attenConstant)) {
		}

		public override LightType Type { get { return LightType.Point; } }
	}

	public enum LightType {
		Directional = 0x0,
		Point = 0x1
	}
}
