using System;
using OpenTK;

namespace GameStack
{
    public static class Hex
    {
        public static Vector4 ToVector4(string value) {
            if (value == null)
                throw new ArgumentNullException();
            if (value.Length < 8 || value.Length > 9)
                throw new ArgumentException("bad value", "value");
            if (value[0] == '#')
                value = value.Substring(1);
            uint v;
            uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out v);
            Console.WriteLine(v);

            return new Vector4(
                (float)(v >> 24) / 255f,
                (float)((v >> 16) & 0xff) / 255f,
                (float)((v >> 8) & 0xff) / 255f,
                (float)(v & 0xff) / 255f
            );
        }
    }
}

