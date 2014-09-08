using System;

namespace GameStack
{
	public static class RandomEx
	{
		public static double NextDouble(this Random rand, double min, double max) {
			return rand.NextDouble() * (max - min) + min;
		}
	}
}

