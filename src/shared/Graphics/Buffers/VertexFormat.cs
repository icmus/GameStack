using System;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack.Graphics {
	public class VertexFormat {
		VertexElement[] _elements;

		public int Stride { get; private set; }

		public VertexElement[] Elements { get { return _elements; } }

		public VertexFormat (int stride, params VertexElement[] elements) {
			this.Stride = stride;
			_elements = elements;
		}

		public VertexElement this [string name] {
			get {
				for (var i = 0; i < _elements.Length; i++) {
					if (_elements[i].Name == name)
						return _elements[i];
				}
				return null;
			}
		}

		public static readonly VertexFormat Position = new VertexFormat(
			                                               3,
			                                               new VertexElement("Position", 0, 3)
		                                               );
		public static readonly VertexFormat PositionUV = new VertexFormat(
			                                                 5,
			                                                 new VertexElement("Position", 0, 3),
			                                                 new VertexElement("MultiTexCoord0", 3, 2)
		                                                 );
		public static readonly VertexFormat PositionColor = new VertexFormat(
			                                                    7,
			                                                    new VertexElement("Position", 0, 3),
			                                                    new VertexElement("Color", 3, 4)
		                                                    );
		public static readonly VertexFormat PositionColorUV = new VertexFormat(
			                                                      9,
			                                                      new VertexElement("Position", 0, 3),
			                                                      new VertexElement("Color", 3, 4),
			                                                      new VertexElement("MultiTexCoord0", 7, 2)
		                                                      );
	}

	public class VertexElement {
		public string Name { get; set; }

		public int Offset { get; set; }

		public int Size { get; set; }

		public VertexElement (string name, int offset, int size) {
			this.Name = name;
			this.Offset = offset;
			this.Size = size;
		}
	}
}
