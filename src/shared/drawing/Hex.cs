using System;
using Cairo;

namespace GameStack
{
	public static partial class Hex
	{
		public static Color ToColor(string value) {
			if (value == null)
				throw new ArgumentNullException();
			if (value.Length < 8 || value.Length > 9)
				throw new ArgumentException("bad value", "value");
			if (value[0] == '#')
				value = value.Substring(1);
			uint v;
			uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out v);

			return new Color(
				(double)(v >> 24) / 255.0,
				(double)((v >> 16) & 0xff) / 255.0,
				(double)((v >> 8) & 0xff) / 255.0,
				(double)(v & 0xff) / 255.0
			);
		}
	}
}

