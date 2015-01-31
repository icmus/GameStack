#pragma warning disable 0618
#pragma warning disable 0420

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif
#if __ANDROID__
using ActiveUniformType = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public abstract class Shader : IDisposable {
		static Dictionary<IntPtr,Dictionary<string, Shader>> _shaders = new Dictionary<IntPtr, Dictionary<string, Shader>>();

		string _name;
		Shader _master;
		int _refCount;
		uint _handle;
		uint _vertHandle, _fragHandle, _geomHandle;
		Dictionary<string, int> _uniforms;
		Dictionary<string, int> _attributes;

		public uint Handle { get { return _handle; } }

		public Shader (string vertSource, string fragSource, string name = null, string geomSource = null) {
			_name = name ?? this.GetType().FullName;
			var glContext = ThreadContext.Current.EnsureGLContext();
			lock (_shaders) {
				Dictionary<string, Shader> shaders;
				if (_shaders.TryGetValue(glContext, out shaders)) {
					Shader master;
					if (shaders.TryGetValue(_name, out master)) {
						_master = master;
						_master._refCount++;
						_handle = master._handle;
						_vertHandle = master._vertHandle;
						_fragHandle = master._fragHandle;
						_uniforms = master._uniforms;
						_attributes = master._attributes;
						return;
					}
				} else {
					_shaders.Add(glContext, new Dictionary<string, Shader>());
				}
				_shaders[glContext].Add(_name, this);
				_master = this;

				bool hasGeom = !string.IsNullOrWhiteSpace(geomSource);

				// create shader program and link
#if __ANDROID__
            	_vertHandle = (uint)GL.CreateShader(All.VertexShader);
				if (hasGeom)
					_geomHandle = (uint)GL.CreateShader(All.GeometryShader);
            	_fragHandle = (uint)GL.CreateShader(All.FragmentShader);
#else
				_vertHandle = (uint)GL.CreateShader(ShaderType.VertexShader);
				if (hasGeom)
					_geomHandle = (uint)GL.CreateShader(ShaderType.GeometryShader);
				_fragHandle = (uint)GL.CreateShader(ShaderType.FragmentShader);
#endif

				if (_vertHandle <= 0 || _fragHandle <= 0 || (hasGeom && _geomHandle <= 0))
					throw new ShaderException("Failed to create shader.");
				_handle = (uint)GL.CreateProgram();
				if (_handle <= 0)
					throw new ShaderException("Failed to create program.");
				CompileShader(_vertHandle, vertSource);
				if (hasGeom)
					CompileShader(_geomHandle, geomSource);
				CompileShader(_fragHandle, fragSource);
				GL.AttachShader(_handle, _vertHandle);
				if (hasGeom)
					GL.AttachShader(_handle, _geomHandle);
				GL.AttachShader(_handle, _fragHandle);
				GL.LinkProgram(_handle);
			}

			// catalog linked shader uniforms
			int total = 0;
#if __ANDROID__
            GL.GetProgram(_handle, All.ActiveUniforms, out total);
#else
			GL.GetProgram(_handle, ProgramParameter.ActiveUniforms, out total);
#endif
			var sb = new StringBuilder(100);
			_uniforms = new Dictionary<string, int>();
			for (var i = 0; i < total; ++i) {
				int length = 0, size = 0;
				ActiveUniformType type;
				#if __ANDROID__
				GL.GetActiveUniform((uint)_handle, (uint)i, 100, out length, out size, out type, sb);
				#else
				GL.GetActiveUniform(_handle, (uint)i, 100, out length, out size, out type, sb);
				#endif
				var n = sb.ToString();
				#if __ANDROID__
				_uniforms.Add(n, GL.GetUniformLocation(_handle, sb));
				#else
				_uniforms.Add(n, GL.GetUniformLocation(_handle, n));
				#endif
				sb.Length = 0;
			}

			// catalog linked vertex attributes
#if __ANDROID__
            GL.GetProgram(_handle, All.ActiveAttributes, out total);
#else
			GL.GetProgram(_handle, ProgramParameter.ActiveAttributes, out total);
#endif
			_attributes = new Dictionary<string, int>();
			for (var i = 0; i < total; ++i) {
				int length = 0, size = 0;
				#if __ANDROID__
                All type;
				#else
				ActiveAttribType type;
				#endif
				GL.GetActiveAttrib(_handle, (uint)i, 100, out length, out size, out type, sb);
				var n = sb.ToString();
				#if __ANDROID__
				_attributes.Add(n, GL.GetAttribLocation(_handle, sb));
				#else
				_attributes.Add(n, GL.GetAttribLocation(_handle, n));
				#endif
				sb.Length = 0;
			}

			_refCount = 1;
		}

		public virtual int MaxNumLights { get { return 0; } }

		public int Uniform (string name) {
			int loc;
			if (!_uniforms.TryGetValue(name, out loc)) {
				return -1;
			}
			return loc;
		}

		public int Attribute (string name) {
			int loc;
			if (!_attributes.TryGetValue(name, out loc)) {
				return -1;
			}
			return loc;
		}

		public void Uniform (string name, int value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform1(loc, value);
		}

		public void Uniform (string name, int[] values) {
			var loc = this.Uniform(name);
			if (loc >= 0) {
				for (var i = 0; i < values.Length; i++)
					GL.Uniform1(loc + i, values[i]);
			}
		}

		public void Uniform (string name, float value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform1(loc, value);
		}

		public void Uniform (string name, Vector2 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform2(loc, ref value);
		}

		public void Uniform (string name, Vector2[] values) {
			var loc = this.Uniform(name);
			if (loc >= 0) {
				for (var i = 0; i < values.Length; i++) {
					GL.Uniform2(loc + i, ref values[i]);
				}
			}
		}

		public void Uniform (string name, Vector3 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform3(loc, ref value);
		}

		public void Uniform (string name, ref Vector3 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform3(loc, ref value);
		}

		public void Uniform (string name, Vector3[] values) {
			var loc = this.Uniform(name);
			if (loc >= 0) {
				for (var i = 0; i < values.Length; i++) {
					GL.Uniform3(loc + i, ref values[i]);
				}
			}
		}

		public void Uniform (string name, Vector4 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform4(loc, ref value);
		}

		public void Uniform (string name, ref Vector4 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.Uniform4(loc, ref value);
		}

		public void Uniform (string name, Vector4[] values) {
			var loc = this.Uniform(name);
			if (loc >= 0) {
				for (var i = 0; i < values.Length; i++) {
					GL.Uniform4(loc + i, ref values[i]);
				}
			}
		}

		public void Uniform (string name, Matrix4 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.UniformMatrix4(loc, false, ref value);
		}

		public void Uniform (string name, ref Matrix4 value) {
			var loc = this.Uniform(name);
			if (loc >= 0)
				GL.UniformMatrix4(loc, false, ref value);
		}

		public void Uniform (string name, Matrix4[] values) {
			var loc = this.Uniform(name);
			if (loc >= 0) {
				for (var i = 0; i < values.Length; i++) {
					GL.UniformMatrix4(loc + i, false, ref values[i]);
				}
			}
		}

		public void SetTransforms (ref Matrix4 modelView, ref Matrix4 projection) {
			Matrix4 mvp;
			Matrix4.Mult(ref modelView, ref projection, out mvp);

			GL.UniformMatrix4(this.Uniform("ModelViewMatrix"), false, ref modelView);
			GL.UniformMatrix4(this.Uniform("ProjectionMatrix"), false, ref projection);
			GL.UniformMatrix4(this.Uniform("ModelViewProjectionMatrix"), false, ref mvp);
		}

		public void Dispose () {
			if (--_master._refCount == 0) {
				GL.DetachShader(_handle, _vertHandle);
				GL.DeleteShader(_vertHandle);
				if (_geomHandle > 0) {
					GL.DetachShader(_handle, _geomHandle);
					GL.DeleteShader(_geomHandle);
				}
				GL.DetachShader(_handle, _fragHandle);
				GL.DeleteShader(_fragHandle);
				GL.DeleteProgram(_handle);
				lock (_shaders) {
					if(_shaders.ContainsKey(ThreadContext.Current.GLContext))
						_shaders[ThreadContext.Current.GLContext].Remove(_name);
				}
			}
		}

		static void CompileShader (uint shader, string src) {
#if __DESKTOP__
			var len = src.Length;
			GL.ShaderSource(shader, 1, new[] { src }, ref len);
#else
			GL.ShaderSource (shader, 1, new[] { src }, new[] { src.Length });
#endif
			GL.CompileShader(shader);

			var info = new StringBuilder(256);
			int i;
			GL.GetShaderInfoLog(shader, 256, out i, info);

			int compileResult = -1;
#if __ANDROID__
            GL.GetShader((uint)shader, All.CompileStatus, out compileResult);
#else
			GL.GetShader((uint)shader, ShaderParameter.CompileStatus, out compileResult);
#endif
			if (compileResult != 1) {
				throw new ShaderException(string.Format("Shader compile error: {0}: {1}",
					compileResult, info));
			}
		}
	}

	public class ShaderException : Exception {
		public ShaderException (string message) : base(message) {
		}

		public ShaderException (string message, Exception innerException) : base(message, innerException) {
		}
	}
}

