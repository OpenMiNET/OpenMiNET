namespace OpenAPI.Utils
{
	public static class FastMath
	{
		public const float PI = 3.1415926535f;
		public const float HALF_PI = 0.5f * PI;
		public const float DOUBLE_PI = 2.0f * PI;
		public const float TWO_PI_INV = 1.0f / DOUBLE_PI;

		private const float B = 4 / PI;
		private const float C = -4 / (PI * PI);
		private const float P = 0.225f;

		public static float Sin(float x)
		{
			var x1 = x % PI;
			var x2 = x % DOUBLE_PI;

			if (x > 0)
			{
				var y = x1 * (B + C * x1);
				y = y > 0
					? P * (y * y - y) + y
					: P * (-y * y - y) + y;
				var xp = x2 - DOUBLE_PI;
				if (!(xp < 0 && xp < -PI))
				{
					y = -y;
				}
				return y;
			}
			else
			{
				var y = x1 * (B - C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 + DOUBLE_PI;
				if (xp > 0 && xp < PI)
				{
					y = -y;
				}
				return y;
			}
		}

		public static float Cos(float x)
		{
			var x0 = x + HALF_PI;
			var x1 = x0 % PI;
			var x2 = x0 % DOUBLE_PI;

			if (x0 > 0)
			{
				var y = x1 * (B + C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 - DOUBLE_PI;
				if (!(xp < 0 && xp < -PI))
				{
					y = -y;
				}
				return y;
			}
			else
			{
				var y = x1 * (B - C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 + DOUBLE_PI;
				if (xp > 0 && xp < PI)
				{
					y = -y;
				}
				return y;
			}
		}

		public static double Sin(double x)
		{
			var x1 = x % PI;
			var x2 = x % DOUBLE_PI;

			if (x > 0)
			{
				var y = x1 * (B + C * x1);
				y = y > 0
					? P * (y * y - y) + y
					: P * (-y * y - y) + y;
				var xp = x2 - DOUBLE_PI;
				if (!(xp < 0 && xp < -PI))
				{
					y = -y;
				}
				return y;
			}
			else
			{
				var y = x1 * (B - C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 + DOUBLE_PI;
				if (xp > 0 && xp < PI)
				{
					y = -y;
				}
				return y;
			}
		}

		public static double Cos(double x)
		{
			var x0 = x + HALF_PI;
			var x1 = x0 % PI;
			var x2 = x0 % DOUBLE_PI;

			if (x0 > 0)
			{
				var y = x1 * (B + C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 - DOUBLE_PI;
				if (!(xp < 0 && xp < -PI))
				{
					y = -y;
				}
				return y;
			}
			else
			{
				var y = x1 * (B - C * x1);
				y = (y > 0)
					? (P * (y * y - y) + y)
					: (P * (-y * y - y) + y);
				var xp = x2 + DOUBLE_PI;
				if (xp > 0 && xp < PI)
				{
					y = -y;
				}
				return y;
			}
		}

		public static double Tan(double d)
		{
			return (Sin(d) / Cos(d));
		}

		public static decimal Abs(decimal d)
		{
			return d < 0 ? -d : d;
		}

		public static double Abs(double d)
		{
			return d < 0 ? -d : d;
		}

		public static short Abs(short d)
		{
			return (short)(d < 0 ? -d : d);
		}

		public static int Abs(int d)
		{
			return d < 0 ? -d : d;
		}

		public static long Abs(long d)
		{
			return d < 0 ? -d : d;
		}

		public static sbyte Abs(sbyte d)
		{
			return (sbyte)(d < 0 ? -d : d);
		}

		public static float Abs(float d)
		{
			return d < 0 ? -d : d;
		}

		public static double Sqrt(double x)
		{
			double n = x / 2.0;
			double lstX = 0.0;
			while (n != lstX)
			{
				lstX = n;
				n = (n + x / n) / 2.0;
			}
			return n;
		}

		public static float Sqrt(float x)
		{
			float n = x / 2.0f;
			float lstX = 0.0f;
			while (n != lstX)
			{
				lstX = n;
				n = (n + x / n) / 2.0f;
			}
			return n;
		}

		public static double Cbrt(double x)
		{
			double i = x / 4;
			while (Abs(i - x / i / i) / i > 0.00000000000001)
				i = (i + (x / i / i) + i) / 3;
			return i;
		}

		public static float Cbrt(float x)
		{
			float i = x / 4f;
			while (Abs(i - x / i / i) / i > 0.00000000000001f)
				i = (i + (x / i / i) + i) / 3f;
			return i;
		}

		public static double Hypot(double x, double y)
		{
			return Sqrt(x * x + y * y);
		}

		public static double Hypot(float x, float y)
		{
			return Sqrt(x * x + y * y);
		}

		public static int Ceiling(decimal x)
		{
			if (x < 0) return (int)x;
			return (int)x + 1;
		}

		public static int Ceiling(double x)
		{
			if (x < 0) return (int)x;
			return (int)x + 1;
		}

		public static int Ceiling(float x)
		{
			if (x < 0) return (int)x;
			return (int)x + 1;
		}

		public static double Copysign(double x, double signval)
		{
			if (signval < 0 && x < 0) return x;
			if (signval < 0 && x > 0) return -x;
			if (signval > 0 && x > 0) return x;
			if (signval > 0 && x < 0) return -x;
			return x;
		}

		public static float Copysign(float x, float signval)
		{
			if (signval < 0 && x < 0) return x;
			if (signval < 0 && x > 0) return -x;
			if (signval > 0 && x > 0) return x;
			if (signval > 0 && x < 0) return -x;
			return x;
		}

		public static double Dim(double a, double b)
		{
			if (a - b < 1) return 0;
			return a - b;
		}

		public static double Dim(float a, float b)
		{
			if (a - b < 1) return 0;
			return a - b;
		}

		public static double Max(double a, double b)
		{
			return a > b ? a : b;
		}

		public static double Max(float a, float b)
		{
			return a > b ? a : b;
		}

		public static int Floor(decimal x)
		{
			return Floor((double)x);
		}

		public static int Floor(double x)
		{
			if (x > 0) return (int)x;
			return (int)(x - 0.9999999999999999);
		}

		public static int Floor(float x)
		{
			if (x > 0) return (int)x;
			return (int)(x - 0.9999999999999999f);
		}

		public static double Min(double a, double b)
		{
			return a < b ? a : b;
		}

		public static float Min(float a, float b)
		{
			return a < b ? a : b;
		}

		public static double Mod(double a, double b)
		{
			return (int)((((a / b) - ((int)(a / b))) * b) + 0.5);
		}

		public static float Mod(float a, float b)
		{
			return (int)((((a / b) - ((int)(a / b))) * b) + 0.5f);
		}

		public static int Round(double arg)
		{
			return (int)(arg + 0.5);
		}

		public static int Round(float arg)
		{
			return (int)(arg + 0.5f);
		}

		public static double Remainder(double a, double b)
		{
			return Mod(a, b);
		}

		public static float Remainder(float a, float b)
		{
			return Mod(a, b);
		}

		public static int Trunc(double arg)
		{
			return (int)arg;
		}

		public static int Trunc(float arg)
		{
			return (int)arg;
		}

		public static double Exp2(double exp)
		{
			int i;
			double value = 1;
			for (i = 1; i <= exp; i++)
			{
				value = value * 2;
			}
			return value;
		}

		public static float Exp2(float exp)
		{
			int i;
			float value = 1f;
			for (i = 1; i <= exp; i++)
			{
				value = value * 2f;
			}
			return value;
		}

		public static double Ma(double a, double b, double c)
		{
			return (int)((a * b) + c + 0.5);
		}

		public static float Ma(float a, float b, float c)
		{
			return (int)((a * b) + c + 0.5F);
		}

		public static double Ldexp(double num, int exp)
		{
			return num * Exp2(exp);
		}

		public static float Ldexp(float num, int exp)
		{
			return num * Exp2(exp);
		}

		public static int Pow(int a, int b)
		{
			int x = a >> 32;
			int y = b * (x - 1072632447) + 1072632447;
			return y << 32;
		}

		public static double Pow(double a, double b)
		{
			long tmp = (DoubleToLong(a) >> 32);
			double tmp2 = (b * (tmp - 1072632447) + 1072632447);
			return LongToDouble((long)tmp2 << 32);
		}

		private static long DoubleToLong(double value)
		{
			return (long)(value * 32D);
		}

		private static double LongToDouble(long value)
		{
			return value / 32D;
		}

		public static float ToRadians(float angle)
		{
			return PI * angle / 180.0f;
		}
	}
}
