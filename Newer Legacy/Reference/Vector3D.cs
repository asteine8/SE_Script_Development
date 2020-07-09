// VRageMath.Vector3D
using ProtoBuf;
using System;
using System.Diagnostics;
using System.Globalization;
using Unsharper;
using VRageMath;

/// <summary>
/// Defines a vector with three components.
/// </summary>
[Serializable]
[ProtoContract]
public struct Vector3D : IEquatable<Vector3D>
{
	public static Vector3D Zero;

	public static Vector3D One;

	public static Vector3D Half;

	public static Vector3D PositiveInfinity;

	public static Vector3D NegativeInfinity;

	public static Vector3D UnitX;

	public static Vector3D UnitY;

	public static Vector3D UnitZ;

	public static Vector3D Up;

	public static Vector3D Down;

	public static Vector3D Right;

	public static Vector3D Left;

	public static Vector3D Forward;

	public static Vector3D Backward;

	public static Vector3D MaxValue;

	public static Vector3D MinValue;

	/// <summary>
	/// Gets or sets the x-component of the vector.
	/// </summary>
	[ProtoMember(34)]
	public double X;

	/// <summary>
	/// Gets or sets the y-component of the vector.
	/// </summary>
	[ProtoMember(39)]
	public double Y;

	/// <summary>
	/// Gets or sets the z-component of the vector.
	/// </summary>
	[ProtoMember(44)]
	public double Z;

	public double Sum => X + Y + Z;

	public double Volume => X * Y * Z;

	static Vector3D()
	{
		Zero = default(Vector3D);
		One = new Vector3D(1.0, 1.0, 1.0);
		Half = new Vector3D(0.5, 0.5, 0.5);
		PositiveInfinity = new Vector3D(double.PositiveInfinity);
		NegativeInfinity = new Vector3D(double.NegativeInfinity);
		UnitX = new Vector3D(1.0, 0.0, 0.0);
		UnitY = new Vector3D(0.0, 1.0, 0.0);
		UnitZ = new Vector3D(0.0, 0.0, 1.0);
		Up = new Vector3D(0.0, 1.0, 0.0);
		Down = new Vector3D(0.0, -1.0, 0.0);
		Right = new Vector3D(1.0, 0.0, 0.0);
		Left = new Vector3D(-1.0, 0.0, 0.0);
		Forward = new Vector3D(0.0, 0.0, -1.0);
		Backward = new Vector3D(0.0, 0.0, 1.0);
		MaxValue = new Vector3D(double.MaxValue, double.MaxValue, double.MaxValue);
		MinValue = new Vector3D(double.MinValue, double.MinValue, double.MinValue);
	}

	/// <summary>
	/// Initializes a new instance of Vector3.
	/// </summary>
	/// <param name="x">Initial value for the x-component of the vector.</param><param name="y">Initial value for the y-component of the vector.</param><param name="z">Initial value for the z-component of the vector.</param>
	public Vector3D(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	/// <summary>
	/// Creates a new instance of Vector3.
	/// </summary>
	/// <param name="value">Value to initialize each component to.</param>
	public Vector3D(double value)
	{
		X = (Y = (Z = value));
	}

	/// <summary>
	/// Initializes a new instance of Vector3.
	/// </summary>
	/// <param name="value">A vector containing the values to initialize x and y components with.</param><param name="z">Initial value for the z-component of the vector.</param>
	public Vector3D(Vector2 value, double z)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
	}

	public Vector3D(Vector2D value, double z)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
	}

	public Vector3D(Vector4 xyz)
	{
		X = xyz.X;
		Y = xyz.Y;
		Z = xyz.Z;
	}

	public Vector3D(Vector4D xyz)
	{
		X = xyz.X;
		Y = xyz.Y;
		Z = xyz.Z;
	}

	public Vector3D(Vector3 value)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
	}

	public Vector3D(ref Vector3I value)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
	}

	public Vector3D(Vector3I value)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
	}

	public Vector3D(Vector3D value)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
	}

	/// <summary>
	/// Returns a vector pointing in the opposite direction.
	/// </summary>
	/// <param name="value">Source vector.</param>
	public static Vector3D operator -(Vector3D value)
	{
		Vector3D result = default(Vector3D);
		result.X = 0.0 - value.X;
		result.Y = 0.0 - value.Y;
		result.Z = 0.0 - value.Z;
		return result;
	}

	/// <summary>
	/// Tests vectors for equality.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static bool operator ==(Vector3D value1, Vector3D value2)
	{
		if (value1.X == value2.X && value1.Y == value2.Y)
		{
			return value1.Z == value2.Z;
		}
		return false;
	}

	public static bool operator ==(Vector3 value1, Vector3D value2)
	{
		if ((double)value1.X == value2.X && (double)value1.Y == value2.Y)
		{
			return (double)value1.Z == value2.Z;
		}
		return false;
	}

	public static bool operator ==(Vector3D value1, Vector3 value2)
	{
		if (value1.X == (double)value2.X && value1.Y == (double)value2.Y)
		{
			return value1.Z == (double)value2.Z;
		}
		return false;
	}

	/// <summary>
	/// Tests vectors for inequality.
	/// </summary>
	/// <param name="value1">Vector to compare.</param><param name="value2">Vector to compare.</param>
	public static bool operator !=(Vector3D value1, Vector3D value2)
	{
		if (value1.X == value2.X && value1.Y == value2.Y)
		{
			return value1.Z != value2.Z;
		}
		return true;
	}

	public static bool operator !=(Vector3 value1, Vector3D value2)
	{
		if ((double)value1.X == value2.X && (double)value1.Y == value2.Y)
		{
			return (double)value1.Z != value2.Z;
		}
		return true;
	}

	public static bool operator !=(Vector3D value1, Vector3 value2)
	{
		if (value1.X == (double)value2.X && value1.Y == (double)value2.Y)
		{
			return value1.Z != (double)value2.Z;
		}
		return true;
	}

	public static Vector3D operator %(Vector3D value1, double value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X % value2;
		result.Y = value1.Y % value2;
		result.Z = value1.Z % value2;
		return result;
	}

	/// <summary>
	/// Modulo division of two vectors.
	/// </summary>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <returns></returns>
	public static Vector3D operator %(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X % value2.X;
		result.Y = value1.Y % value2.Y;
		result.Z = value1.Z % value2.Z;
		return result;
	}

	/// <summary>
	/// Adds two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D operator +(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + value2.X;
		result.Y = value1.Y + value2.Y;
		result.Z = value1.Z + value2.Z;
		return result;
	}

	public static Vector3D operator +(Vector3D value1, Vector3 value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + (double)value2.X;
		result.Y = value1.Y + (double)value2.Y;
		result.Z = value1.Z + (double)value2.Z;
		return result;
	}

	public static Vector3D operator +(Vector3 value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = (double)value1.X + value2.X;
		result.Y = (double)value1.Y + value2.Y;
		result.Z = (double)value1.Z + value2.Z;
		return result;
	}

	public static Vector3D operator +(Vector3D value1, Vector3I value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + (double)value2.X;
		result.Y = value1.Y + (double)value2.Y;
		result.Z = value1.Z + (double)value2.Z;
		return result;
	}

	public static Vector3D operator +(Vector3D value1, double value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + value2;
		result.Y = value1.Y + value2;
		result.Z = value1.Z + value2;
		return result;
	}

	public static Vector3D operator +(Vector3D value1, float value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + (double)value2;
		result.Y = value1.Y + (double)value2;
		result.Z = value1.Z + (double)value2;
		return result;
	}

	/// <summary>
	/// Subtracts a vector from a vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D operator -(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X - value2.X;
		result.Y = value1.Y - value2.Y;
		result.Z = value1.Z - value2.Z;
		return result;
	}

	public static Vector3D operator -(Vector3D value1, Vector3 value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X - (double)value2.X;
		result.Y = value1.Y - (double)value2.Y;
		result.Z = value1.Z - (double)value2.Z;
		return result;
	}

	public static Vector3D operator -(Vector3 value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = (double)value1.X - value2.X;
		result.Y = (double)value1.Y - value2.Y;
		result.Z = (double)value1.Z - value2.Z;
		return result;
	}

	public static Vector3D operator -(Vector3D value1, double value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X - value2;
		result.Y = value1.Y - value2;
		result.Z = value1.Z - value2;
		return result;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D operator *(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X * value2.X;
		result.Y = value1.Y * value2.Y;
		result.Z = value1.Z * value2.Z;
		return result;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D operator *(Vector3D value1, Vector3 value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X * (double)value2.X;
		result.Y = value1.Y * (double)value2.Y;
		result.Z = value1.Z * (double)value2.Z;
		return result;
	}

	/// <summary>
	/// Multiplies a vector by a scalar value.
	/// </summary>
	/// <param name="value">Source vector.</param><param name="scaleFactor">Scalar value.</param>
	public static Vector3D operator *(Vector3D value, double scaleFactor)
	{
		Vector3D result = default(Vector3D);
		result.X = value.X * scaleFactor;
		result.Y = value.Y * scaleFactor;
		result.Z = value.Z * scaleFactor;
		return result;
	}

	/// <summary>
	/// Multiplies a vector by a scalar value.
	/// </summary>
	/// <param name="scaleFactor">Scalar value.</param><param name="value">Source vector.</param>
	public static Vector3D operator *(double scaleFactor, Vector3D value)
	{
		Vector3D result = default(Vector3D);
		result.X = value.X * scaleFactor;
		result.Y = value.Y * scaleFactor;
		result.Z = value.Z * scaleFactor;
		return result;
	}

	/// <summary>
	/// Divides the components of a vector by the components of another vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Divisor vector.</param>
	public static Vector3D operator /(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X / value2.X;
		result.Y = value1.Y / value2.Y;
		result.Z = value1.Z / value2.Z;
		return result;
	}

	/// <summary>
	/// Divides a vector by a scalar value.
	/// </summary>
	/// <param name="value">Source vector.</param><param name="divider">The divisor.</param>
	public static Vector3D operator /(Vector3D value, double divider)
	{
		double num = 1.0 / divider;
		Vector3D result = default(Vector3D);
		result.X = value.X * num;
		result.Y = value.Y * num;
		result.Z = value.Z * num;
		return result;
	}

	public static Vector3D operator /(double value, Vector3D divider)
	{
		Vector3D result = default(Vector3D);
		result.X = value / divider.X;
		result.Y = value / divider.Y;
		result.Z = value / divider.Z;
		return result;
	}

	public static Vector3D Abs(Vector3D value)
	{
		return new Vector3D((value.X < 0.0) ? (0.0 - value.X) : value.X, (value.Y < 0.0) ? (0.0 - value.Y) : value.Y, (value.Z < 0.0) ? (0.0 - value.Z) : value.Z);
	}

	public static Vector3D Sign(Vector3D value)
	{
		return new Vector3D(Math.Sign(value.X), Math.Sign(value.Y), Math.Sign(value.Z));
	}

	/// <summary>
	/// Returns per component sign, never returns zero.
	/// For zero component returns sign 1.
	/// Faster than Sign.
	/// </summary>
	public static Vector3D SignNonZero(Vector3D value)
	{
		return new Vector3D((!(value.X < 0.0)) ? 1 : (-1), (!(value.Y < 0.0)) ? 1 : (-1), (!(value.Z < 0.0)) ? 1 : (-1));
	}

	public void Interpolate3(Vector3D v0, Vector3D v1, double rt)
	{
		double num = 1.0 - rt;
		X = num * v0.X + rt * v1.X;
		Y = num * v0.Y + rt * v1.Y;
		Z = num * v0.Z + rt * v1.Z;
	}

	public bool IsValid()
	{
		if (X.IsValid() && Y.IsValid())
		{
			return Z.IsValid();
		}
		return false;
	}

	[Conditional("DEBUG")]
	public void AssertIsValid()
	{
	}

	public static bool IsUnit(ref Vector3D value)
	{
		double num = value.LengthSquared();
		if (num >= 0.99989998340606689)
		{
			return num < 1.0001;
		}
		return false;
	}

	public static bool ArePerpendicular(ref Vector3D a, ref Vector3D b)
	{
		double num = a.Dot(b);
		return num * num < 1E-08 * a.LengthSquared() * b.LengthSquared();
	}

	public static bool IsZero(Vector3D value)
	{
		return IsZero(value, 0.0001);
	}

	public static bool IsZero(Vector3D value, double epsilon)
	{
		if (Math.Abs(value.X) < epsilon && Math.Abs(value.Y) < epsilon)
		{
			return Math.Abs(value.Z) < epsilon;
		}
		return false;
	}

	public static Vector3D IsZeroVector(Vector3D value)
	{
		return new Vector3D((value.X == 0.0) ? 1 : 0, (value.Y == 0.0) ? 1 : 0, (value.Z == 0.0) ? 1 : 0);
	}

	public static Vector3D IsZeroVector(Vector3D value, double epsilon)
	{
		return new Vector3D((Math.Abs(value.X) < epsilon) ? 1 : 0, (Math.Abs(value.Y) < epsilon) ? 1 : 0, (Math.Abs(value.Z) < epsilon) ? 1 : 0);
	}

	public static Vector3D Step(Vector3D value)
	{
		return new Vector3D((value.X > 0.0) ? 1 : ((value.X < 0.0) ? (-1) : 0), (value.Y > 0.0) ? 1 : ((value.Y < 0.0) ? (-1) : 0), (value.Z > 0.0) ? 1 : ((value.Z < 0.0) ? (-1) : 0));
	}

	/// <summary>
	/// Retrieves a string representation of the current object.
	/// </summary>
	public override string ToString()
	{
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2}}}", X.ToString(currentCulture), Y.ToString(currentCulture), Z.ToString(currentCulture));
	}

	public static bool TryParse(string str, out Vector3D retval)
	{
		retval = Zero;
		if (string.IsNullOrWhiteSpace(str))
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool result = true;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '{')
			{
				num++;
			}
			else if (str[i] == ':')
			{
				if (num == 1)
				{
					num2 = i + 1;
				}
				else
				{
					result = false;
				}
			}
			else if (str[i] == ' ')
			{
				if (num == 1)
				{
					int length = i - num2;
					string s = str.Substring(num2, length);
					double result2 = 0.0;
					if (!double.TryParse(s, out result2))
					{
						result = false;
					}
					switch (num3)
					{
					case 0:
						retval.X = result2;
						break;
					case 1:
						retval.Y = result2;
						break;
					case 2:
						retval.Z = result2;
						break;
					default:
						result = false;
						break;
					}
					num3++;
				}
			}
			else
			{
				if (str[i] != '}')
				{
					continue;
				}
				num--;
				if (num != 0)
				{
					result = false;
					continue;
				}
				int length2 = i - num2;
				string s2 = str.Substring(num2, length2);
				double result3 = 0.0;
				if (!double.TryParse(s2, out result3))
				{
					result = false;
				}
				switch (num3)
				{
				case 0:
					retval.X = result3;
					break;
				case 1:
					retval.Y = result3;
					break;
				case 2:
					retval.Z = result3;
					break;
				default:
					result = false;
					break;
				}
				num3++;
			}
		}
		if (num != 0 || num3 != 3)
		{
			result = false;
		}
		return result;
	}

	public string ToString(string format)
	{
		CultureInfo currentCulture = CultureInfo.CurrentCulture;
		return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2}}}", X.ToString(format, currentCulture), Y.ToString(format, currentCulture), Z.ToString(format, currentCulture));
	}

	/// <summary>
	/// Determines whether the specified Object is equal to the Vector3.
	/// </summary>
	/// <param name="other">The Vector3 to compare with the current Vector3.</param>
	public bool Equals(Vector3D other)
	{
		if (X == other.X && Y == other.Y)
		{
			return Z == other.Z;
		}
		return false;
	}

	public bool Equals(Vector3D other, double epsilon)
	{
		if (Math.Abs(X - other.X) < epsilon && Math.Abs(Y - other.Y) < epsilon)
		{
			return Math.Abs(Z - other.Z) < epsilon;
		}
		return false;
	}

	/// <summary>
	/// Returns a value that indicates whether the current instance is equal to a specified object.
	/// </summary>
	/// <param name="obj">Object to make the comparison with.</param>
	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj is Vector3D)
		{
			result = Equals((Vector3D)obj);
		}
		return result;
	}

	public override int GetHashCode()
	{
		return ((((int)(X * 997.0) * 397) ^ (int)(Y * 997.0)) * 397) ^ (int)(Z * 997.0);
	}

	/// <summary>
	/// Gets the hash code of the vector object.
	/// </summary>
	public long GetHash()
	{
		int num = 0;
		long num2 = (long)Math.Round(Math.Abs(X * 1000.0));
		num = 2;
		long num3 = (num2 * 397) ^ (long)Math.Round(Math.Abs(Y * 1000.0));
		num += 4;
		long num4 = (num3 * 397) ^ (long)Math.Round(Math.Abs(Z * 1000.0));
		num += 16;
		long num5 = (num4 * 397) ^ (Math.Sign(X) + 5);
		num += 256;
		long num6 = (num5 * 397) ^ (Math.Sign(Y) + 7);
		num += 65536;
		long num7 = (num6 * 397) ^ (Math.Sign(Z) + 11);
		num++;
		return (num7 * 397) ^ num;
	}

	/// <summary>
	/// Calculates the length of the vector.
	/// </summary>
	public double Length()
	{
		return Math.Sqrt(X * X + Y * Y + Z * Z);
	}

	/// <summary>
	/// Calculates the length of the vector squared.
	/// </summary>
	public double LengthSquared()
	{
		return X * X + Y * Y + Z * Z;
	}

	/// <summary>
	/// Calculates the distance between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static double Distance(Vector3D value1, Vector3D value2)
	{
		double num = value1.X - value2.X;
		double num2 = value1.Y - value2.Y;
		double num3 = value1.Z - value2.Z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public static double Distance(Vector3D value1, Vector3 value2)
	{
		double num = value1.X - (double)value2.X;
		double num2 = value1.Y - (double)value2.Y;
		double num3 = value1.Z - (double)value2.Z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	public static double Distance(Vector3 value1, Vector3D value2)
	{
		double num = (double)value1.X - value2.X;
		double num2 = (double)value1.Y - value2.Y;
		double num3 = (double)value1.Z - value2.Z;
		return Math.Sqrt(num * num + num2 * num2 + num3 * num3);
	}

	/// <summary>
	/// Calculates the distance between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The distance between the vectors.</param>
	public static void Distance(ref Vector3D value1, ref Vector3D value2, out double result)
	{
		double num = value1.X - value2.X;
		double num2 = value1.Y - value2.Y;
		double num3 = value1.Z - value2.Z;
		double num4 = num * num + num2 * num2 + num3 * num3;
		result = Math.Sqrt(num4);
	}

	/// <summary>
	/// Calculates the distance between two vectors squared.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static double DistanceSquared(Vector3D value1, Vector3D value2)
	{
		double num = value1.X - value2.X;
		double num2 = value1.Y - value2.Y;
		double num3 = value1.Z - value2.Z;
		return num * num + num2 * num2 + num3 * num3;
	}

	/// <summary>
	/// Calculates the distance between two vectors squared.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The distance between the two vectors squared.</param>
	public static void DistanceSquared(ref Vector3D value1, ref Vector3D value2, out double result)
	{
		double num = value1.X - value2.X;
		double num2 = value1.Y - value2.Y;
		double num3 = value1.Z - value2.Z;
		result = num * num + num2 * num2 + num3 * num3;
	}

	/// <summary>
	/// Calculates rectangular distance (a.k.a. Manhattan distance or Chessboard distace) between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static double RectangularDistance(Vector3D value1, Vector3D value2)
	{
		Vector3D vector3D = Abs(value1 - value2);
		return vector3D.X + vector3D.Y + vector3D.Z;
	}

	/// <summary>
	/// Calculates rectangular distance (a.k.a. Manhattan distance or Chessboard distace) between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static double RectangularDistance(ref Vector3D value1, ref Vector3D value2)
	{
		Vector3D vector3D = Abs(value1 - value2);
		return vector3D.X + vector3D.Y + vector3D.Z;
	}

	/// <summary>
	/// Calculates the dot product of two vectors. If the two vectors are unit vectors, the dot product returns a doubleing point value between -1 and 1 that can be used to determine some properties of the angle between two vectors. For example, it can show whether the vectors are orthogonal, parallel, or have an acute or obtuse angle between them.
	/// </summary>
	/// <param name="vector1">Source vector.</param><param name="vector2">Source vector.</param>
	public static double Dot(Vector3D vector1, Vector3D vector2)
	{
		return vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
	}

	public static double Dot(Vector3D vector1, Vector3 vector2)
	{
		return vector1.X * (double)vector2.X + vector1.Y * (double)vector2.Y + vector1.Z * (double)vector2.Z;
	}

	/// <summary>
	/// Calculates the dot product of two vectors and writes the result to a user-specified variable. If the two vectors are unit vectors, the dot product returns a doubleing point value between -1 and 1 that can be used to determine some properties of the angle between two vectors. For example, it can show whether the vectors are orthogonal, parallel, or have an acute or obtuse angle between them.
	/// </summary>
	/// <param name="vector1">Source vector.</param><param name="vector2">Source vector.</param><param name="result">[OutAttribute] The dot product of the two vectors.</param>
	public static void Dot(ref Vector3D vector1, ref Vector3D vector2, out double result)
	{
		result = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
	}

	public static void Dot(ref Vector3D vector1, ref Vector3 vector2, out double result)
	{
		result = vector1.X * (double)vector2.X + vector1.Y * (double)vector2.Y + vector1.Z * (double)vector2.Z;
	}

	public static void Dot(ref Vector3 vector1, ref Vector3D vector2, out double result)
	{
		result = (double)vector1.X * vector2.X + (double)vector1.Y * vector2.Y + (double)vector1.Z * vector2.Z;
	}

	public double Dot(Vector3D v)
	{
		return Dot(this, v);
	}

	public double Dot(Vector3 v)
	{
		return X * (double)v.X + Y * (double)v.Y + Z * (double)v.Z;
	}

	public double Dot(ref Vector3D v)
	{
		return X * v.X + Y * v.Y + Z * v.Z;
	}

	public Vector3D Cross(Vector3D v)
	{
		return Cross(this, v);
	}

	/// <summary>
	/// Turns the current vector into a unit vector. The result is a vector one unit in length pointing in the same direction as the original vector.
	/// </summary>
	/// returns length
	public double Normalize()
	{
		double num = Math.Sqrt(X * X + Y * Y + Z * Z);
		double num2 = 1.0 / num;
		X *= num2;
		Y *= num2;
		Z *= num2;
		return num;
	}

	/// <summary>
	/// Creates a unit vector from the specified vector. The result is a vector one unit in length pointing in the same direction as the original vector.
	/// </summary>
	/// <param name="value">The source Vector3.</param>
	public static Vector3D Normalize(Vector3D value)
	{
		double num = 1.0 / Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
		Vector3D result = default(Vector3D);
		result.X = value.X * num;
		result.Y = value.Y * num;
		result.Z = value.Z * num;
		return result;
	}

	/// <summary>
	/// Creates a unit vector from the specified vector, writing the result to a user-specified variable. The result is a vector one unit in length pointing in the same direction as the original vector.
	/// </summary>
	/// <param name="value">Source vector.</param><param name="result">[OutAttribute] The normalized vector.</param>
	public static void Normalize(ref Vector3D value, out Vector3D result)
	{
		double num = 1.0 / Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
		result.X = value.X * num;
		result.Y = value.Y * num;
		result.Z = value.Z * num;
	}

	/// <summary>
	/// Calculates the cross product of two vectors.
	/// </summary>
	/// <param name="vector1">Source vector.</param><param name="vector2">Source vector.</param>
	public static Vector3D Cross(Vector3D vector1, Vector3D vector2)
	{
		Vector3D result = default(Vector3D);
		result.X = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
		result.Y = vector1.Z * vector2.X - vector1.X * vector2.Z;
		result.Z = vector1.X * vector2.Y - vector1.Y * vector2.X;
		return result;
	}

	/// <summary>
	/// Calculates the cross product of two vectors.
	/// </summary>
	/// <param name="vector1">Source vector.</param><param name="vector2">Source vector.</param><param name="result">[OutAttribute] The cross product of the vectors.</param>
	public static void Cross(ref Vector3D vector1, ref Vector3D vector2, out Vector3D result)
	{
		double x = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
		double y = vector1.Z * vector2.X - vector1.X * vector2.Z;
		double z = vector1.X * vector2.Y - vector1.Y * vector2.X;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	/// <summary>
	/// Returns the reflection of a vector off a surface that has the specified normal.  Reference page contains code sample.
	/// </summary>
	/// <param name="vector">Source vector.</param><param name="normal">Normal of the surface.</param>
	public static Vector3D Reflect(Vector3D vector, Vector3D normal)
	{
		double num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		Vector3D result = default(Vector3D);
		result.X = vector.X - 2.0 * num * normal.X;
		result.Y = vector.Y - 2.0 * num * normal.Y;
		result.Z = vector.Z - 2.0 * num * normal.Z;
		return result;
	}

	/// <summary>
	/// Returns the reflection of a vector off a surface that has the specified normal.  Reference page contains code sample.
	/// </summary>
	/// <param name="vector">Source vector.</param><param name="normal">Normal of the surface.</param><param name="result">[OutAttribute] The reflected vector.</param>
	public static void Reflect(ref Vector3D vector, ref Vector3D normal, out Vector3D result)
	{
		double num = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		result.X = vector.X - 2.0 * num * normal.X;
		result.Y = vector.Y - 2.0 * num * normal.Y;
		result.Z = vector.Z - 2.0 * num * normal.Z;
	}

	/// <summary>
	/// Returns the rejection of vector from direction, i.e. projection of vector onto the plane defined by origin and direction
	/// </summary>
	/// <param name="vector">Vector which is to be rejected</param>
	/// <param name="direction">Direction from which the input vector will be rejected</param>
	/// <returns>Rejection of the vector from the given direction</returns>
	public static Vector3D Reject(Vector3D vector, Vector3D direction)
	{
		Reject(ref vector, ref direction, out Vector3D result);
		return result;
	}

	/// <summary>
	/// Returns the rejection of vector from direction, i.e. projection of vector onto the plane defined by origin and direction
	/// </summary>
	/// <param name="vector">Vector which is to be rejected</param>
	/// <param name="direction">Direction from which the input vector will be rejected</param>
	/// <param name="result">Rejection of the vector from the given direction</param>
	public static void Reject(ref Vector3D vector, ref Vector3D direction, out Vector3D result)
	{
		Dot(ref direction, ref direction, out double result2);
		result2 = 1.0 / result2;
		Dot(ref direction, ref vector, out double result3);
		result3 *= result2;
		Vector3D vector3D = default(Vector3D);
		vector3D.X = direction.X * result2;
		vector3D.Y = direction.Y * result2;
		vector3D.Z = direction.Z * result2;
		result.X = vector.X - result3 * vector3D.X;
		result.Y = vector.Y - result3 * vector3D.Y;
		result.Z = vector.Z - result3 * vector3D.Z;
	}

	/// <summary>
	/// Returns the component of the vector that is smallest of all the three components.
	/// </summary>
	public double Min()
	{
		if (X < Y)
		{
			if (X < Z)
			{
				return X;
			}
			return Z;
		}
		if (Y < Z)
		{
			return Y;
		}
		return Z;
	}

	/// <summary>
	/// Returns the component of the vector, whose absolute value is smallest of all the three components.
	/// </summary>
	public double AbsMin()
	{
		if (Math.Abs(X) < Math.Abs(Y))
		{
			if (Math.Abs(X) < Math.Abs(Z))
			{
				return Math.Abs(X);
			}
			return Math.Abs(Z);
		}
		if (Math.Abs(Y) < Math.Abs(Z))
		{
			return Math.Abs(Y);
		}
		return Math.Abs(Z);
	}

	/// <summary>
	/// Returns the component of the vector that is largest of all the three components.
	/// </summary>
	public double Max()
	{
		if (X > Y)
		{
			if (X > Z)
			{
				return X;
			}
			return Z;
		}
		if (Y > Z)
		{
			return Y;
		}
		return Z;
	}

	/// <summary>
	/// Returns the component of the vector, whose absolute value is largest of all the three components.
	/// </summary>
	public double AbsMax()
	{
		if (Math.Abs(X) > Math.Abs(Y))
		{
			if (Math.Abs(X) > Math.Abs(Z))
			{
				return Math.Abs(X);
			}
			return Math.Abs(Z);
		}
		if (Math.Abs(Y) > Math.Abs(Z))
		{
			return Math.Abs(Y);
		}
		return Math.Abs(Z);
	}

	public int AbsMaxComponent()
	{
		if (Math.Abs(X) > Math.Abs(Y))
		{
			if (Math.Abs(X) > Math.Abs(Z))
			{
				return 0;
			}
			return 2;
		}
		if (Math.Abs(Y) > Math.Abs(Z))
		{
			return 1;
		}
		return 2;
	}

	/// <summary>
	/// Returns a vector that contains the lowest value from each matching pair of components.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D Min(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = ((value1.X < value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
		result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
		return result;
	}

	/// <summary>
	/// Returns a vector that contains the lowest value from each matching pair of components.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The minimized vector.</param>
	public static void Min(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = ((value1.X < value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
		result.Z = ((value1.Z < value2.Z) ? value1.Z : value2.Z);
	}

	/// <summary>
	/// Returns a vector that contains the highest value from each matching pair of components.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D Max(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = ((value1.X > value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
		return result;
	}

	/// <summary>
	/// Returns a vector that contains the highest value from each matching pair of components.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The maximized vector.</param>
	public static void Max(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = ((value1.X > value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
		result.Z = ((value1.Z > value2.Z) ? value1.Z : value2.Z);
	}

	/// <summary>
	/// Separates minimal and maximal values of any two input vectors
	/// </summary>
	/// <param name="min">minimal values of the two vectors</param>
	/// <param name="max">maximal values of the two vectors</param>
	public static void MinMax(ref Vector3D min, ref Vector3D max)
	{
		if (min.X > max.X)
		{
			double x = min.X;
			min.X = max.X;
			max.X = x;
		}
		if (min.Y > max.Y)
		{
			double x = min.Y;
			min.Y = max.Y;
			max.Y = x;
		}
		if (min.Z > max.Z)
		{
			double x = min.Z;
			min.Z = max.Z;
			max.Z = x;
		}
	}

	/// <summary>
	/// Returns a vector that is equal to the projection of the input vector to the coordinate axis that corresponds
	/// to the original vector's largest value.
	/// </summary>
	/// <param name="value1">Source vector.</param>
	public static Vector3D DominantAxisProjection(Vector3D value1)
	{
		if (Math.Abs(value1.X) > Math.Abs(value1.Y))
		{
			value1.Y = 0.0;
			if (Math.Abs(value1.X) > Math.Abs(value1.Z))
			{
				value1.Z = 0.0;
			}
			else
			{
				value1.X = 0.0;
			}
		}
		else
		{
			value1.X = 0.0;
			if (Math.Abs(value1.Y) > Math.Abs(value1.Z))
			{
				value1.Z = 0.0;
			}
			else
			{
				value1.Y = 0.0;
			}
		}
		return value1;
	}

	/// <summary>
	/// Calculates a vector that is equal to the projection of the input vector to the coordinate axis that corresponds
	/// to the original vector's largest value. The result is saved into a user-specified variable.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="result">[OutAttribute] The projected vector.</param>
	public static void DominantAxisProjection(ref Vector3D value1, out Vector3D result)
	{
		if (Math.Abs(value1.X) > Math.Abs(value1.Y))
		{
			if (Math.Abs(value1.X) > Math.Abs(value1.Z))
			{
				result = new Vector3D(value1.X, 0.0, 0.0);
			}
			else
			{
				result = new Vector3D(0.0, 0.0, value1.Z);
			}
		}
		else if (Math.Abs(value1.Y) > Math.Abs(value1.Z))
		{
			result = new Vector3D(0.0, value1.Y, 0.0);
		}
		else
		{
			result = new Vector3D(0.0, 0.0, value1.Z);
		}
	}

	/// <summary>
	/// Restricts a value to be within a specified range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param><param name="min">The minimum value.</param><param name="max">The maximum value.</param>
	public static Vector3D Clamp(Vector3D value1, Vector3D min, Vector3D max)
	{
		Clamp(ref value1, ref min, ref max, out Vector3D result);
		return result;
	}

	/// <summary>
	/// Restricts a value to be within a specified range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param><param name="min">The minimum value.</param><param name="max">The maximum value.</param><param name="result">[OutAttribute] The clamped value.</param>
	public static void Clamp(ref Vector3D value1, ref Vector3D min, ref Vector3D max, out Vector3D result)
	{
		double x = value1.X;
		double x2 = (x > max.X) ? max.X : ((x < min.X) ? min.X : x);
		double y = value1.Y;
		double y2 = (y > max.Y) ? max.Y : ((y < min.Y) ? min.Y : y);
		double z = value1.Z;
		double z2 = (z > max.Z) ? max.Z : ((z < min.Z) ? min.Z : z);
		result.X = x2;
		result.Y = y2;
		result.Z = z2;
	}

	[UnsharperDisableReflection]
	public static Vector3D ClampToSphere(Vector3D vector, double radius)
	{
		double num = vector.LengthSquared();
		double num2 = radius * radius;
		if (num > num2)
		{
			return vector * Math.Sqrt(num2 / num);
		}
		return vector;
	}

	[UnsharperDisableReflection]
	public static void ClampToSphere(ref Vector3D vector, double radius)
	{
		double num = vector.LengthSquared();
		double num2 = radius * radius;
		if (num > num2)
		{
			vector *= Math.Sqrt(num2 / num);
		}
	}

	/// <summary>
	/// Performs a linear interpolation between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
	public static Vector3D Lerp(Vector3D value1, Vector3D value2, double amount)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + (value2.X - value1.X) * amount;
		result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		result.Z = value1.Z + (value2.Z - value1.Z) * amount;
		return result;
	}

	/// <summary>
	/// Performs a linear interpolation between two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="amount">Value between 0 and 1 indicating the weight of value2.</param><param name="result">[OutAttribute] The result of the interpolation.</param>
	public static void Lerp(ref Vector3D value1, ref Vector3D value2, double amount, out Vector3D result)
	{
		result.X = value1.X + (value2.X - value1.X) * amount;
		result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		result.Z = value1.Z + (value2.Z - value1.Z) * amount;
	}

	/// <summary>
	/// Returns a Vector3 containing the 3D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 3D triangle.
	/// </summary>
	/// <param name="value1">A Vector3 containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param><param name="value2">A Vector3 containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param><param name="value3">A Vector3 containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param><param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in value2).</param><param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in value3).</param>
	public static Vector3D Barycentric(Vector3D value1, Vector3D value2, Vector3D value3, double amount1, double amount2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
		result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
		result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
		return result;
	}

	/// <summary>
	/// Returns a Vector3 containing the 3D Cartesian coordinates of a point specified in barycentric (areal) coordinates relative to a 3D triangle.
	/// </summary>
	/// <param name="value1">A Vector3 containing the 3D Cartesian coordinates of vertex 1 of the triangle.</param><param name="value2">A Vector3 containing the 3D Cartesian coordinates of vertex 2 of the triangle.</param><param name="value3">A Vector3 containing the 3D Cartesian coordinates of vertex 3 of the triangle.</param><param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in value2).</param><param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in value3).</param><param name="result">[OutAttribute] The 3D Cartesian coordinates of the specified point are placed in this Vector3 on exit.</param>
	public static void Barycentric(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, double amount1, double amount2, out Vector3D result)
	{
		result.X = value1.X + amount1 * (value2.X - value1.X) + amount2 * (value3.X - value1.X);
		result.Y = value1.Y + amount1 * (value2.Y - value1.Y) + amount2 * (value3.Y - value1.Y);
		result.Z = value1.Z + amount1 * (value2.Z - value1.Z) + amount2 * (value3.Z - value1.Z);
	}

	/// <summary>
	/// Compute barycentric coordinates (u, v, w) for point p with respect to triangle (a, b, c)
	/// From : Real-Time Collision Detection, Christer Ericson, CRC Press
	/// 3.4 Barycentric Coordinates
	/// </summary>
	public static void Barycentric(Vector3D p, Vector3D a, Vector3D b, Vector3D c, out double u, out double v, out double w)
	{
		Vector3D vector3D = b - a;
		Vector3D vector3D2 = c - a;
		Vector3D vector = p - a;
		double num = Dot(vector3D, vector3D);
		double num2 = Dot(vector3D, vector3D2);
		double num3 = Dot(vector3D2, vector3D2);
		double num4 = Dot(vector, vector3D);
		double num5 = Dot(vector, vector3D2);
		double num6 = num * num3 - num2 * num2;
		v = (num3 * num4 - num2 * num5) / num6;
		w = (num * num5 - num2 * num4) / num6;
		u = 1.0 - v - w;
	}

	/// <summary>
	/// Interpolates between two values using a cubic equation.
	/// </summary>
	/// <param name="value1">Source value.</param><param name="value2">Source value.</param><param name="amount">Weighting value.</param>
	public static Vector3D SmoothStep(Vector3D value1, Vector3D value2, double amount)
	{
		amount = ((amount > 1.0) ? 1.0 : ((amount < 0.0) ? 0.0 : amount));
		amount = amount * amount * (3.0 - 2.0 * amount);
		Vector3D result = default(Vector3D);
		result.X = value1.X + (value2.X - value1.X) * amount;
		result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		result.Z = value1.Z + (value2.Z - value1.Z) * amount;
		return result;
	}

	/// <summary>
	/// Interpolates between two values using a cubic equation.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="amount">Weighting value.</param><param name="result">[OutAttribute] The interpolated value.</param>
	public static void SmoothStep(ref Vector3D value1, ref Vector3D value2, double amount, out Vector3D result)
	{
		amount = ((amount > 1.0) ? 1.0 : ((amount < 0.0) ? 0.0 : amount));
		amount = amount * amount * (3.0 - 2.0 * amount);
		result.X = value1.X + (value2.X - value1.X) * amount;
		result.Y = value1.Y + (value2.Y - value1.Y) * amount;
		result.Z = value1.Z + (value2.Z - value1.Z) * amount;
	}

	/// <summary>
	/// Performs a Catmull-Rom interpolation using the specified positions.
	/// </summary>
	/// <param name="value1">The first position in the interpolation.</param><param name="value2">The second position in the interpolation.</param><param name="value3">The third position in the interpolation.</param><param name="value4">The fourth position in the interpolation.</param><param name="amount">Weighting factor.</param>
	public static Vector3D CatmullRom(Vector3D value1, Vector3D value2, Vector3D value3, Vector3D value4, double amount)
	{
		double num = amount * amount;
		double num2 = amount * num;
		Vector3D result = default(Vector3D);
		result.X = 0.5 * (2.0 * value2.X + (0.0 - value1.X + value3.X) * amount + (2.0 * value1.X - 5.0 * value2.X + 4.0 * value3.X - value4.X) * num + (0.0 - value1.X + 3.0 * value2.X - 3.0 * value3.X + value4.X) * num2);
		result.Y = 0.5 * (2.0 * value2.Y + (0.0 - value1.Y + value3.Y) * amount + (2.0 * value1.Y - 5.0 * value2.Y + 4.0 * value3.Y - value4.Y) * num + (0.0 - value1.Y + 3.0 * value2.Y - 3.0 * value3.Y + value4.Y) * num2);
		result.Z = 0.5 * (2.0 * value2.Z + (0.0 - value1.Z + value3.Z) * amount + (2.0 * value1.Z - 5.0 * value2.Z + 4.0 * value3.Z - value4.Z) * num + (0.0 - value1.Z + 3.0 * value2.Z - 3.0 * value3.Z + value4.Z) * num2);
		return result;
	}

	/// <summary>
	/// Performs a Catmull-Rom interpolation using the specified positions.
	/// </summary>
	/// <param name="value1">The first position in the interpolation.</param><param name="value2">The second position in the interpolation.</param><param name="value3">The third position in the interpolation.</param><param name="value4">The fourth position in the interpolation.</param><param name="amount">Weighting factor.</param><param name="result">[OutAttribute] A vector that is the result of the Catmull-Rom interpolation.</param>
	public static void CatmullRom(ref Vector3D value1, ref Vector3D value2, ref Vector3D value3, ref Vector3D value4, double amount, out Vector3D result)
	{
		double num = amount * amount;
		double num2 = amount * num;
		result.X = 0.5 * (2.0 * value2.X + (0.0 - value1.X + value3.X) * amount + (2.0 * value1.X - 5.0 * value2.X + 4.0 * value3.X - value4.X) * num + (0.0 - value1.X + 3.0 * value2.X - 3.0 * value3.X + value4.X) * num2);
		result.Y = 0.5 * (2.0 * value2.Y + (0.0 - value1.Y + value3.Y) * amount + (2.0 * value1.Y - 5.0 * value2.Y + 4.0 * value3.Y - value4.Y) * num + (0.0 - value1.Y + 3.0 * value2.Y - 3.0 * value3.Y + value4.Y) * num2);
		result.Z = 0.5 * (2.0 * value2.Z + (0.0 - value1.Z + value3.Z) * amount + (2.0 * value1.Z - 5.0 * value2.Z + 4.0 * value3.Z - value4.Z) * num + (0.0 - value1.Z + 3.0 * value2.Z - 3.0 * value3.Z + value4.Z) * num2);
	}

	/// <summary>
	/// Performs a Hermite spline interpolation.
	/// </summary>
	/// <param name="value1">Source position vector.</param><param name="tangent1">Source tangent vector.</param><param name="value2">Source position vector.</param><param name="tangent2">Source tangent vector.</param><param name="amount">Weighting factor.</param>
	public static Vector3D Hermite(Vector3D value1, Vector3D tangent1, Vector3D value2, Vector3D tangent2, double amount)
	{
		double num = amount * amount;
		double num2 = amount * num;
		double num3 = 2.0 * num2 - 3.0 * num + 1.0;
		double num4 = -2.0 * num2 + 3.0 * num;
		double num5 = num2 - 2.0 * num + amount;
		double num6 = num2 - num;
		Vector3D result = default(Vector3D);
		result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
		result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
		result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
		return result;
	}

	/// <summary>
	/// Performs a Hermite spline interpolation.
	/// </summary>
	/// <param name="value1">Source position vector.</param><param name="tangent1">Source tangent vector.</param><param name="value2">Source position vector.</param><param name="tangent2">Source tangent vector.</param><param name="amount">Weighting factor.</param><param name="result">[OutAttribute] The result of the Hermite spline interpolation.</param>
	public static void Hermite(ref Vector3D value1, ref Vector3D tangent1, ref Vector3D value2, ref Vector3D tangent2, double amount, out Vector3D result)
	{
		double num = amount * amount;
		double num2 = amount * num;
		double num3 = 2.0 * num2 - 3.0 * num + 1.0;
		double num4 = -2.0 * num2 + 3.0 * num;
		double num5 = num2 - 2.0 * num + amount;
		double num6 = num2 - num;
		result.X = value1.X * num3 + value2.X * num4 + tangent1.X * num5 + tangent2.X * num6;
		result.Y = value1.Y * num3 + value2.Y * num4 + tangent1.Y * num5 + tangent2.Y * num6;
		result.Z = value1.Z * num3 + value2.Z * num4 + tangent1.Z * num5 + tangent2.Z * num6;
	}

	/// <summary>
	/// Transforms a 3D vector by the given matrix.
	/// </summary>
	/// <param name="position">The source vector.</param><param name="matrix">The transformation matrix.</param>
	public static Vector3D Transform(Vector3D position, MatrixD matrix)
	{
		double num = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
		double num2 = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
		double num3 = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
		double num4 = 1.0 / (position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44);
		Vector3D result = default(Vector3D);
		result.X = num * num4;
		result.Y = num2 * num4;
		result.Z = num3 * num4;
		return result;
	}

	public static Vector3D Transform(Vector3 position, MatrixD matrix)
	{
		double num = (double)position.X * matrix.M11 + (double)position.Y * matrix.M21 + (double)position.Z * matrix.M31 + matrix.M41;
		double num2 = (double)position.X * matrix.M12 + (double)position.Y * matrix.M22 + (double)position.Z * matrix.M32 + matrix.M42;
		double num3 = (double)position.X * matrix.M13 + (double)position.Y * matrix.M23 + (double)position.Z * matrix.M33 + matrix.M43;
		double num4 = 1.0 / ((double)position.X * matrix.M14 + (double)position.Y * matrix.M24 + (double)position.Z * matrix.M34 + matrix.M44);
		Vector3D result = default(Vector3D);
		result.X = num * num4;
		result.Y = num2 * num4;
		result.Z = num3 * num4;
		return result;
	}

	/// <summary>
	/// Transforms a 3D vector by the given matrix.
	/// </summary>
	/// <param name="position">The source vector.</param><param name="matrix">The transformation matrix.</param>
	public static Vector3D Transform(Vector3D position, Matrix matrix)
	{
		double num = position.X * (double)matrix.M11 + position.Y * (double)matrix.M21 + position.Z * (double)matrix.M31 + (double)matrix.M41;
		double num2 = position.X * (double)matrix.M12 + position.Y * (double)matrix.M22 + position.Z * (double)matrix.M32 + (double)matrix.M42;
		double num3 = position.X * (double)matrix.M13 + position.Y * (double)matrix.M23 + position.Z * (double)matrix.M33 + (double)matrix.M43;
		double num4 = 1.0 / (position.X * (double)matrix.M14 + position.Y * (double)matrix.M24 + position.Z * (double)matrix.M34 + (double)matrix.M44);
		Vector3D result = default(Vector3D);
		result.X = num * num4;
		result.Y = num2 * num4;
		result.Z = num3 * num4;
		return result;
	}

	public static Vector3D Transform(Vector3D position, ref MatrixD matrix)
	{
		Transform(ref position, ref matrix, out position);
		return position;
	}

	/// <summary>
	/// Transforms a Vector3 by the given Matrix.
	/// </summary>
	/// <param name="position">The source Vector3.</param><param name="matrix">The transformation Matrix.</param><param name="result">[OutAttribute] The transformed vector.</param>
	public static void Transform(ref Vector3D position, ref MatrixD matrix, out Vector3D result)
	{
		double num = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
		double num2 = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
		double num3 = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
		double num4 = 1.0 / (position.X * matrix.M14 + position.Y * matrix.M24 + position.Z * matrix.M34 + matrix.M44);
		result.X = num * num4;
		result.Y = num2 * num4;
		result.Z = num3 * num4;
	}

	public static void Transform(ref Vector3 position, ref MatrixD matrix, out Vector3D result)
	{
		double num = (double)position.X * matrix.M11 + (double)position.Y * matrix.M21 + (double)position.Z * matrix.M31 + matrix.M41;
		double num2 = (double)position.X * matrix.M12 + (double)position.Y * matrix.M22 + (double)position.Z * matrix.M32 + matrix.M42;
		double num3 = (double)position.X * matrix.M13 + (double)position.Y * matrix.M23 + (double)position.Z * matrix.M33 + matrix.M43;
		double num4 = 1.0 / ((double)position.X * matrix.M14 + (double)position.Y * matrix.M24 + (double)position.Z * matrix.M34 + matrix.M44);
		result.X = num * num4;
		result.Y = num2 * num4;
		result.Z = num3 * num4;
	}

	/// Transform the provided vector only about the rotation, scale and translation terms of a matrix.
	///
	/// This effectively treats the matrix as a 3x4 matrix and the input vector as a 4 dimensional vector with unit W coordinate.
	public static void TransformNoProjection(ref Vector3D vector, ref MatrixD matrix, out Vector3D result)
	{
		double x = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + matrix.M41;
		double y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + matrix.M42;
		double z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + matrix.M43;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	/// Transform the provided vector only about the rotation and scale terms of a matrix.
	public static void RotateAndScale(ref Vector3D vector, ref MatrixD matrix, out Vector3D result)
	{
		double x = vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31;
		double y = vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32;
		double z = vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public static void Transform(ref Vector3D position, ref MatrixI matrix, out Vector3D result)
	{
		result = position.X * new Vector3D(Base6Directions.GetVector(matrix.Right)) + position.Y * new Vector3D(Base6Directions.GetVector(matrix.Up)) + position.Z * new Vector3D(Base6Directions.GetVector(matrix.Backward)) + new Vector3D(matrix.Translation);
	}

	/// <summary>
	/// Transforms a 3D vector normal by a matrix.
	/// </summary>
	/// <param name="normal">The source vector.</param><param name="matrix">The transformation matrix.</param>
	public static Vector3D TransformNormal(Vector3D normal, Matrix matrix)
	{
		double x = normal.X * (double)matrix.M11 + normal.Y * (double)matrix.M21 + normal.Z * (double)matrix.M31;
		double y = normal.X * (double)matrix.M12 + normal.Y * (double)matrix.M22 + normal.Z * (double)matrix.M32;
		double z = normal.X * (double)matrix.M13 + normal.Y * (double)matrix.M23 + normal.Z * (double)matrix.M33;
		Vector3D result = default(Vector3D);
		result.X = x;
		result.Y = y;
		result.Z = z;
		return result;
	}

	/// <summary>
	/// Transforms a 3D vector normal by a matrix.
	/// </summary>
	/// <param name="normal">The source vector.</param><param name="matrix">The transformation matrix.</param>
	public static Vector3D TransformNormal(Vector3 normal, MatrixD matrix)
	{
		double x = (double)normal.X * matrix.M11 + (double)normal.Y * matrix.M21 + (double)normal.Z * matrix.M31;
		double y = (double)normal.X * matrix.M12 + (double)normal.Y * matrix.M22 + (double)normal.Z * matrix.M32;
		double z = (double)normal.X * matrix.M13 + (double)normal.Y * matrix.M23 + (double)normal.Z * matrix.M33;
		Vector3D result = default(Vector3D);
		result.X = x;
		result.Y = y;
		result.Z = z;
		return result;
	}

	/// <summary>
	/// Transforms a 3D vector normal by a matrix.
	/// </summary>
	/// <param name="normal">The source vector.</param><param name="matrix">The transformation matrix.</param>
	public static Vector3D TransformNormal(Vector3D normal, MatrixD matrix)
	{
		double x = normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31;
		double y = normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32;
		double z = normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33;
		Vector3D result = default(Vector3D);
		result.X = x;
		result.Y = y;
		result.Z = z;
		return result;
	}

	/// <summary>
	/// Transforms a vector normal by a matrix.
	/// </summary>
	/// <param name="normal">The source vector.</param><param name="matrix">The transformation Matrix.</param><param name="result">[OutAttribute] The Vector3 resulting from the transformation.</param>
	public static void TransformNormal(ref Vector3D normal, ref MatrixD matrix, out Vector3D result)
	{
		double x = normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31;
		double y = normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32;
		double z = normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public static void TransformNormal(ref Vector3 normal, ref MatrixD matrix, out Vector3D result)
	{
		double x = (double)normal.X * matrix.M11 + (double)normal.Y * matrix.M21 + (double)normal.Z * matrix.M31;
		double y = (double)normal.X * matrix.M12 + (double)normal.Y * matrix.M22 + (double)normal.Z * matrix.M32;
		double z = (double)normal.X * matrix.M13 + (double)normal.Y * matrix.M23 + (double)normal.Z * matrix.M33;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public static void TransformNormal(ref Vector3D normal, ref MatrixI matrix, out Vector3D result)
	{
		result = normal.X * new Vector3D(Base6Directions.GetVector(matrix.Right)) + normal.Y * new Vector3D(Base6Directions.GetVector(matrix.Up)) + normal.Z * new Vector3D(Base6Directions.GetVector(matrix.Backward));
	}

	public static Vector3D TransformNormal(Vector3D normal, MyBlockOrientation orientation)
	{
		TransformNormal(ref normal, orientation, out Vector3D result);
		return result;
	}

	public static void TransformNormal(ref Vector3D normal, MyBlockOrientation orientation, out Vector3D result)
	{
		result = (0.0 - normal.X) * new Vector3D(Base6Directions.GetVector(orientation.Left)) + normal.Y * new Vector3D(Base6Directions.GetVector(orientation.Up)) - normal.Z * new Vector3D(Base6Directions.GetVector(orientation.Forward));
	}

	public static Vector3D TransformNormal(Vector3D normal, ref MatrixD matrix)
	{
		TransformNormal(ref normal, ref matrix, out normal);
		return normal;
	}

	/// <summary>
	/// Transforms a Vector3 by a specified Quaternion rotation.
	/// </summary>
	/// <param name="value">The Vector3 to rotate.</param><param name="rotation">The Quaternion rotation to apply.</param>
	public static Vector3D Transform(Vector3D value, Quaternion rotation)
	{
		double num = rotation.X + rotation.X;
		double num2 = rotation.Y + rotation.Y;
		double num3 = rotation.Z + rotation.Z;
		double num4 = (double)rotation.W * num;
		double num5 = (double)rotation.W * num2;
		double num6 = (double)rotation.W * num3;
		double num7 = (double)rotation.X * num;
		double num8 = (double)rotation.X * num2;
		double num9 = (double)rotation.X * num3;
		double num10 = (double)rotation.Y * num2;
		double num11 = (double)rotation.Y * num3;
		double num12 = (double)rotation.Z * num3;
		double x = value.X * (1.0 - num10 - num12) + value.Y * (num8 - num6) + value.Z * (num9 + num5);
		double y = value.X * (num8 + num6) + value.Y * (1.0 - num7 - num12) + value.Z * (num11 - num4);
		double z = value.X * (num9 - num5) + value.Y * (num11 + num4) + value.Z * (1.0 - num7 - num10);
		Vector3D result = default(Vector3D);
		result.X = x;
		result.Y = y;
		result.Z = z;
		return result;
	}

	/// <summary>
	/// Transforms a Vector3 by a specified Quaternion rotation.
	/// </summary>
	/// <param name="value">The Vector3 to rotate.</param><param name="rotation">The Quaternion rotation to apply.</param><param name="result">[OutAttribute] An existing Vector3 filled in with the results of the rotation.</param>
	public static void Transform(ref Vector3D value, ref Quaternion rotation, out Vector3D result)
	{
		double num = rotation.X + rotation.X;
		double num2 = rotation.Y + rotation.Y;
		double num3 = rotation.Z + rotation.Z;
		double num4 = (double)rotation.W * num;
		double num5 = (double)rotation.W * num2;
		double num6 = (double)rotation.W * num3;
		double num7 = (double)rotation.X * num;
		double num8 = (double)rotation.X * num2;
		double num9 = (double)rotation.X * num3;
		double num10 = (double)rotation.Y * num2;
		double num11 = (double)rotation.Y * num3;
		double num12 = (double)rotation.Z * num3;
		double x = value.X * (1.0 - num10 - num12) + value.Y * (num8 - num6) + value.Z * (num9 + num5);
		double y = value.X * (num8 + num6) + value.Y * (1.0 - num7 - num12) + value.Z * (num11 - num4);
		double z = value.X * (num9 - num5) + value.Y * (num11 + num4) + value.Z * (1.0 - num7 - num10);
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public static void Rotate(ref Vector3D vector, ref MatrixD rotationMatrix, out Vector3D result)
	{
		double x = vector.X * rotationMatrix.M11 + vector.Y * rotationMatrix.M21 + vector.Z * rotationMatrix.M31;
		double y = vector.X * rotationMatrix.M12 + vector.Y * rotationMatrix.M22 + vector.Z * rotationMatrix.M32;
		double z = vector.X * rotationMatrix.M13 + vector.Y * rotationMatrix.M23 + vector.Z * rotationMatrix.M33;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	public static Vector3D Rotate(Vector3D vector, MatrixD rotationMatrix)
	{
		Rotate(ref vector, ref rotationMatrix, out Vector3D result);
		return result;
	}

	/// <summary>
	/// Transforms a source array of Vector3s by a specified Matrix and writes the results to an existing destination array.
	/// </summary>
	/// <param name="sourceArray">The source array.</param><param name="matrix">The transform Matrix to apply.</param><param name="destinationArray">An existing destination array into which the transformed Vector3s are written.</param>
	public static void Transform(Vector3D[] sourceArray, ref MatrixD matrix, Vector3D[] destinationArray)
	{
		for (int i = 0; i < sourceArray.Length; i++)
		{
			double x = sourceArray[i].X;
			double y = sourceArray[i].Y;
			double z = sourceArray[i].Z;
			destinationArray[i].X = x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + matrix.M41;
			destinationArray[i].Y = x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + matrix.M42;
			destinationArray[i].Z = x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + matrix.M43;
		}
	}

	/// <summary>
	/// Transforms a source array of Vector3s by a specified Matrix and writes the results to an existing destination array.
	/// </summary>
	/// <param name="sourceArray">The source array.</param>
	/// <param name="matrix">The transform Matrix to apply.</param>
	/// <param name="destinationArray">An existing destination array into which the transformed Vector3s are written.</param>
	public unsafe static void Transform(Vector3D[] sourceArray, ref MatrixD matrix, Vector3D* destinationArray)
	{
		for (int i = 0; i < sourceArray.Length; i++)
		{
			double x = sourceArray[i].X;
			double y = sourceArray[i].Y;
			double z = sourceArray[i].Z;
			destinationArray[i].X = x * matrix.M11 + y * matrix.M21 + z * matrix.M31 + matrix.M41;
			destinationArray[i].Y = x * matrix.M12 + y * matrix.M22 + z * matrix.M32 + matrix.M42;
			destinationArray[i].Z = x * matrix.M13 + y * matrix.M23 + z * matrix.M33 + matrix.M43;
		}
	}

	/// <summary>
	/// Applies a specified transform Matrix to a specified range of an array of Vector3s and writes the results into a specified range of a destination array.
	/// </summary>
	/// <param name="sourceArray">The source array.</param><param name="sourceIndex">The index in the source array at which to start.</param><param name="matrix">The transform Matrix to apply.</param><param name="destinationArray">The existing destination array.</param><param name="destinationIndex">The index in the destination array at which to start.</param><param name="length">The number of Vector3s to transform.</param>
	public static void Transform(Vector3D[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3D[] destinationArray, int destinationIndex, int length)
	{
		while (length > 0)
		{
			double x = sourceArray[sourceIndex].X;
			double y = sourceArray[sourceIndex].Y;
			double z = sourceArray[sourceIndex].Z;
			destinationArray[destinationIndex].X = x * (double)matrix.M11 + y * (double)matrix.M21 + z * (double)matrix.M31 + (double)matrix.M41;
			destinationArray[destinationIndex].Y = x * (double)matrix.M12 + y * (double)matrix.M22 + z * (double)matrix.M32 + (double)matrix.M42;
			destinationArray[destinationIndex].Z = x * (double)matrix.M13 + y * (double)matrix.M23 + z * (double)matrix.M33 + (double)matrix.M43;
			sourceIndex++;
			destinationIndex++;
			length--;
		}
	}

	/// <summary>
	/// Transforms an array of 3D vector normals by a specified Matrix.
	/// </summary>
	/// <param name="sourceArray">The array of Vector3 normals to transform.</param><param name="matrix">The transform matrix to apply.</param><param name="destinationArray">An existing Vector3 array into which the results of the transforms are written.</param>
	public static void TransformNormal(Vector3D[] sourceArray, ref Matrix matrix, Vector3D[] destinationArray)
	{
		for (int i = 0; i < sourceArray.Length; i++)
		{
			double x = sourceArray[i].X;
			double y = sourceArray[i].Y;
			double z = sourceArray[i].Z;
			destinationArray[i].X = x * (double)matrix.M11 + y * (double)matrix.M21 + z * (double)matrix.M31;
			destinationArray[i].Y = x * (double)matrix.M12 + y * (double)matrix.M22 + z * (double)matrix.M32;
			destinationArray[i].Z = x * (double)matrix.M13 + y * (double)matrix.M23 + z * (double)matrix.M33;
		}
	}

	/// <summary>
	/// Transforms an array of 3D vector normals by a specified Matrix.
	/// </summary>
	/// <param name="sourceArray">The array of Vector3 normals to transform.</param>
	/// <param name="matrix">The transform matrix to apply.</param>
	/// <param name="destinationArray">An existing Vector3 array into which the results of the transforms are written.</param>
	public unsafe static void TransformNormal(Vector3D[] sourceArray, ref Matrix matrix, Vector3D* destinationArray)
	{
		for (int i = 0; i < sourceArray.Length; i++)
		{
			double x = sourceArray[i].X;
			double y = sourceArray[i].Y;
			double z = sourceArray[i].Z;
			destinationArray[i].X = x * (double)matrix.M11 + y * (double)matrix.M21 + z * (double)matrix.M31;
			destinationArray[i].Y = x * (double)matrix.M12 + y * (double)matrix.M22 + z * (double)matrix.M32;
			destinationArray[i].Z = x * (double)matrix.M13 + y * (double)matrix.M23 + z * (double)matrix.M33;
		}
	}

	/// <summary>
	/// Transforms a specified range in an array of 3D vector normals by a specified Matrix and writes the results to a specified range in a destination array.
	/// </summary>
	/// <param name="sourceArray">The source array of Vector3 normals.</param><param name="sourceIndex">The starting index in the source array.</param><param name="matrix">The transform Matrix to apply.</param><param name="destinationArray">The destination Vector3 array.</param><param name="destinationIndex">The starting index in the destination array.</param><param name="length">The number of vectors to transform.</param>
	public static void TransformNormal(Vector3D[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3D[] destinationArray, int destinationIndex, int length)
	{
		while (length > 0)
		{
			double x = sourceArray[sourceIndex].X;
			double y = sourceArray[sourceIndex].Y;
			double z = sourceArray[sourceIndex].Z;
			destinationArray[destinationIndex].X = x * (double)matrix.M11 + y * (double)matrix.M21 + z * (double)matrix.M31;
			destinationArray[destinationIndex].Y = x * (double)matrix.M12 + y * (double)matrix.M22 + z * (double)matrix.M32;
			destinationArray[destinationIndex].Z = x * (double)matrix.M13 + y * (double)matrix.M23 + z * (double)matrix.M33;
			sourceIndex++;
			destinationIndex++;
			length--;
		}
	}

	/// <summary>
	/// Transforms a source array of Vector3s by a specified Quaternion rotation and writes the results to an existing destination array.
	/// </summary>
	/// <param name="sourceArray">The source array.</param><param name="rotation">The Quaternion rotation to apply.</param><param name="destinationArray">An existing destination array into which the transformed Vector3s are written.</param>
	public static void Transform(Vector3D[] sourceArray, ref Quaternion rotation, Vector3D[] destinationArray)
	{
		double num = rotation.X + rotation.X;
		double num2 = rotation.Y + rotation.Y;
		double num3 = rotation.Z + rotation.Z;
		double num4 = (double)rotation.W * num;
		double num5 = (double)rotation.W * num2;
		double num6 = (double)rotation.W * num3;
		double num7 = (double)rotation.X * num;
		double num8 = (double)rotation.X * num2;
		double num9 = (double)rotation.X * num3;
		double num10 = (double)rotation.Y * num2;
		double num11 = (double)rotation.Y * num3;
		double num12 = (double)rotation.Z * num3;
		double num13 = 1.0 - num10 - num12;
		double num14 = num8 - num6;
		double num15 = num9 + num5;
		double num16 = num8 + num6;
		double num17 = 1.0 - num7 - num12;
		double num18 = num11 - num4;
		double num19 = num9 - num5;
		double num20 = num11 + num4;
		double num21 = 1.0 - num7 - num10;
		for (int i = 0; i < sourceArray.Length; i++)
		{
			double x = sourceArray[i].X;
			double y = sourceArray[i].Y;
			double z = sourceArray[i].Z;
			destinationArray[i].X = x * num13 + y * num14 + z * num15;
			destinationArray[i].Y = x * num16 + y * num17 + z * num18;
			destinationArray[i].Z = x * num19 + y * num20 + z * num21;
		}
	}

	/// <summary>
	/// Applies a specified Quaternion rotation to a specified range of an array of Vector3s and writes the results into a specified range of a destination array.
	/// </summary>
	/// <param name="sourceArray">The source array.</param><param name="sourceIndex">The index in the source array at which to start.</param><param name="rotation">The Quaternion rotation to apply.</param><param name="destinationArray">The existing destination array.</param><param name="destinationIndex">The index in the destination array at which to start.</param><param name="length">The number of Vector3s to transform.</param>
	public static void Transform(Vector3D[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector3D[] destinationArray, int destinationIndex, int length)
	{
		double num = rotation.X + rotation.X;
		double num2 = rotation.Y + rotation.Y;
		double num3 = rotation.Z + rotation.Z;
		double num4 = (double)rotation.W * num;
		double num5 = (double)rotation.W * num2;
		double num6 = (double)rotation.W * num3;
		double num7 = (double)rotation.X * num;
		double num8 = (double)rotation.X * num2;
		double num9 = (double)rotation.X * num3;
		double num10 = (double)rotation.Y * num2;
		double num11 = (double)rotation.Y * num3;
		double num12 = (double)rotation.Z * num3;
		double num13 = 1.0 - num10 - num12;
		double num14 = num8 - num6;
		double num15 = num9 + num5;
		double num16 = num8 + num6;
		double num17 = 1.0 - num7 - num12;
		double num18 = num11 - num4;
		double num19 = num9 - num5;
		double num20 = num11 + num4;
		double num21 = 1.0 - num7 - num10;
		while (length > 0)
		{
			double x = sourceArray[sourceIndex].X;
			double y = sourceArray[sourceIndex].Y;
			double z = sourceArray[sourceIndex].Z;
			destinationArray[destinationIndex].X = x * num13 + y * num14 + z * num15;
			destinationArray[destinationIndex].Y = x * num16 + y * num17 + z * num18;
			destinationArray[destinationIndex].Z = x * num19 + y * num20 + z * num21;
			sourceIndex++;
			destinationIndex++;
			length--;
		}
	}

	/// <summary>
	/// Returns a vector pointing in the opposite direction.
	/// </summary>
	/// <param name="value">Source vector.</param>
	public static Vector3D Negate(Vector3D value)
	{
		Vector3D result = default(Vector3D);
		result.X = 0.0 - value.X;
		result.Y = 0.0 - value.Y;
		result.Z = 0.0 - value.Z;
		return result;
	}

	/// <summary>
	/// Returns a vector pointing in the opposite direction.
	/// </summary>
	/// <param name="value">Source vector.</param><param name="result">[OutAttribute] Vector pointing in the opposite direction.</param>
	public static void Negate(ref Vector3D value, out Vector3D result)
	{
		result.X = 0.0 - value.X;
		result.Y = 0.0 - value.Y;
		result.Z = 0.0 - value.Z;
	}

	/// <summary>
	/// Adds two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D Add(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X + value2.X;
		result.Y = value1.Y + value2.Y;
		result.Z = value1.Z + value2.Z;
		return result;
	}

	/// <summary>
	/// Adds two vectors.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] Sum of the source vectors.</param>
	public static void Add(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = value1.X + value2.X;
		result.Y = value1.Y + value2.Y;
		result.Z = value1.Z + value2.Z;
	}

	/// <summary>
	/// Subtracts a vector from a vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D Subtract(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X - value2.X;
		result.Y = value1.Y - value2.Y;
		result.Z = value1.Z - value2.Z;
		return result;
	}

	/// <summary>
	/// Subtracts a vector from a vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The result of the subtraction.</param>
	public static void Subtract(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = value1.X - value2.X;
		result.Y = value1.Y - value2.Y;
		result.Z = value1.Z - value2.Z;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param>
	public static Vector3D Multiply(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X * value2.X;
		result.Y = value1.Y * value2.Y;
		result.Z = value1.Z * value2.Z;
		return result;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Source vector.</param><param name="result">[OutAttribute] The result of the multiplication.</param>
	public static void Multiply(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = value1.X * value2.X;
		result.Y = value1.Y * value2.Y;
		result.Z = value1.Z * value2.Z;
	}

	/// <summary>
	/// Multiplies a vector by a scalar value.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="scaleFactor">Scalar value.</param>
	public static Vector3D Multiply(Vector3D value1, double scaleFactor)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X * scaleFactor;
		result.Y = value1.Y * scaleFactor;
		result.Z = value1.Z * scaleFactor;
		return result;
	}

	/// <summary>
	/// Multiplies a vector by a scalar value.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="scaleFactor">Scalar value.</param><param name="result">[OutAttribute] The result of the multiplication.</param>
	public static void Multiply(ref Vector3D value1, double scaleFactor, out Vector3D result)
	{
		result.X = value1.X * scaleFactor;
		result.Y = value1.Y * scaleFactor;
		result.Z = value1.Z * scaleFactor;
	}

	/// <summary>
	/// Divides the components of a vector by the components of another vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">Divisor vector.</param>
	public static Vector3D Divide(Vector3D value1, Vector3D value2)
	{
		Vector3D result = default(Vector3D);
		result.X = value1.X / value2.X;
		result.Y = value1.Y / value2.Y;
		result.Z = value1.Z / value2.Z;
		return result;
	}

	/// <summary>
	/// Divides the components of a vector by the components of another vector.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">The divisor.</param><param name="result">[OutAttribute] The result of the division.</param>
	public static void Divide(ref Vector3D value1, ref Vector3D value2, out Vector3D result)
	{
		result.X = value1.X / value2.X;
		result.Y = value1.Y / value2.Y;
		result.Z = value1.Z / value2.Z;
	}

	/// <summary>
	/// Divides a vector by a scalar value.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">The divisor.</param>
	public static Vector3D Divide(Vector3D value1, double value2)
	{
		double num = 1.0 / value2;
		Vector3D result = default(Vector3D);
		result.X = value1.X * num;
		result.Y = value1.Y * num;
		result.Z = value1.Z * num;
		return result;
	}

	/// <summary>
	/// Divides a vector by a scalar value.
	/// </summary>
	/// <param name="value1">Source vector.</param><param name="value2">The divisor.</param><param name="result">[OutAttribute] The result of the division.</param>
	public static void Divide(ref Vector3D value1, double value2, out Vector3D result)
	{
		double num = 1.0 / value2;
		result.X = value1.X * num;
		result.Y = value1.Y * num;
		result.Z = value1.Z * num;
	}

	[UnsharperDisableReflection]
	public static Vector3D CalculatePerpendicularVector(Vector3D v)
	{
		v.CalculatePerpendicularVector(out Vector3D result);
		return result;
	}

	[UnsharperDisableReflection]
	public void CalculatePerpendicularVector(out Vector3D result)
	{
		if (Math.Abs(Y + Z) > 9.9999997473787516E-05 || Math.Abs(X) > 9.9999997473787516E-05)
		{
			result = new Vector3D(0.0 - (Y + Z), X, X);
		}
		else
		{
			result = new Vector3D(Z, Z, 0.0 - (X + Y));
		}
		Normalize(ref result, out result);
	}

	public static void GetAzimuthAndElevation(Vector3D v, out double azimuth, out double elevation)
	{
		Dot(ref v, ref Up, out double result);
		v.Y = 0.0;
		v.Normalize();
		Dot(ref v, ref Forward, out double result2);
		elevation = Math.Asin(result);
		if (v.X >= 0.0)
		{
			azimuth = 0.0 - Math.Acos(result2);
		}
		else
		{
			azimuth = Math.Acos(result2);
		}
	}

	public static void CreateFromAzimuthAndElevation(double azimuth, double elevation, out Vector3D direction)
	{
		MatrixD matrix = MatrixD.CreateRotationY(azimuth);
		MatrixD matrix2 = MatrixD.CreateRotationX(elevation);
		direction = Forward;
		TransformNormal(ref direction, ref matrix2, out direction);
		TransformNormal(ref direction, ref matrix, out direction);
	}

	public long VolumeInt(double multiplier)
	{
		return (long)(X * multiplier) * (long)(Y * multiplier) * (long)(Z * multiplier);
	}

	public bool IsInsideInclusive(ref Vector3D min, ref Vector3D max)
	{
		if (min.X <= X && X <= max.X && min.Y <= Y && Y <= max.Y && min.Z <= Z)
		{
			return Z <= max.Z;
		}
		return false;
	}

	public static Vector3D SwapYZCoordinates(Vector3D v)
	{
		return new Vector3D(v.X, v.Z, 0.0 - v.Y);
	}

	public double GetDim(int i)
	{
		switch (i)
		{
		case 0:
			return X;
		case 1:
			return Y;
		case 2:
			return Z;
		default:
			return GetDim((i % 3 + 3) % 3);
		}
	}

	public void SetDim(int i, double value)
	{
		switch (i)
		{
		case 0:
			X = value;
			break;
		case 1:
			Y = value;
			break;
		case 2:
			Z = value;
			break;
		default:
			SetDim((i % 3 + 3) % 3, value);
			break;
		}
	}

	public static explicit operator Vector3I(Vector3D v)
	{
		return new Vector3I((int)v.X, (int)v.Y, (int)v.Z);
	}

	public static implicit operator Vector3(Vector3D v)
	{
		return new Vector3((float)v.X, (float)v.Y, (float)v.Z);
	}

	public static implicit operator Vector3D(Vector3 v)
	{
		return new Vector3D(v.X, v.Y, v.Z);
	}

	public static Vector3I Round(Vector3D vect3d)
	{
		return new Vector3I(vect3d + 0.5);
	}

	public static Vector3I Floor(Vector3D vect3d)
	{
		return new Vector3I((int)Math.Floor(vect3d.X), (int)Math.Floor(vect3d.Y), (int)Math.Floor(vect3d.Z));
	}

	public static void Fract(ref Vector3D o, out Vector3D r)
	{
		r.X = o.X - Math.Floor(o.X);
		r.Y = o.Y - Math.Floor(o.Y);
		r.Z = o.Z - Math.Floor(o.Z);
	}

	public static Vector3D Round(Vector3D v, int numDecimals)
	{
		return new Vector3D(Math.Round(v.X, numDecimals), Math.Round(v.Y, numDecimals), Math.Round(v.Z, numDecimals));
	}

	public static void Abs(ref Vector3D vector3D, out Vector3D abs)
	{
		abs.X = Math.Abs(vector3D.X);
		abs.Y = Math.Abs(vector3D.Y);
		abs.Z = Math.Abs(vector3D.Z);
	}

	/// <summary>
	/// Projects given vector on plane specified by it's normal.
	/// </summary>
	/// <param name="vec">Vector which is to be projected</param>
	/// <param name="planeNormal">Plane normal (may or may not be normalized)</param>
	/// <returns>Vector projected on plane</returns>
	public static Vector3D ProjectOnPlane(ref Vector3D vec, ref Vector3D planeNormal)
	{
		double num = vec.Dot(planeNormal);
		double num2 = planeNormal.LengthSquared();
		return vec - num / num2 * planeNormal;
	}

	/// <summary>
	/// Projects vector on another vector resulting in new vector in guided vector's direction with different length.
	/// </summary>
	/// <param name="vec">Vector which is to be projected</param>
	/// <param name="guideVector">Guide vector (may or may not be normalized)</param>
	/// <returns>Vector projected on guide vector</returns>
	public static Vector3D ProjectOnVector(ref Vector3D vec, ref Vector3D guideVector)
	{
		if (IsZero(ref vec) || IsZero(ref guideVector))
		{
			return Zero;
		}
		return guideVector * Dot(vec, guideVector) / guideVector.LengthSquared();
	}

	private static bool IsZero(ref Vector3D vec)
	{
		if (IsZero(vec.X) && IsZero(vec.Y))
		{
			return IsZero(vec.Z);
		}
		return false;
	}

	private static bool IsZero(double d)
	{
		if (d > -9.9999997473787516E-06)
		{
			return d < 9.9999997473787516E-06;
		}
		return false;
	}
}
