using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Steganography
{
    public static class EllipticCurve
    {
        private const uint Q = 26681;
        private const long A = -2;
        private const long B = 0;
        private static readonly Point ZeroPoint = new Point(0, 0);
        private static readonly Point P = new Point(26597, 13742);

        public static string CryptInfo(string plainText)
        {
            StringBuilder result = new StringBuilder();
            List<int> numberArrPlainText = new List<int>();
            List<int> numberArrCryptText = new List<int>();
            foreach (var chunk in plainText)
            {
                numberArrPlainText.Add(Convert.ToInt32(chunk));
            }

            foreach (var number in numberArrPlainText)
            {
                var A_Point = (uint)number * P;
                numberArrCryptText.Add((int)A_Point.X.Value);
                numberArrCryptText.Add((int)A_Point.Y.Value);
            }

            foreach (var value in numberArrCryptText)
            {
                result.Append(value.ToString()+" ");
            }

            return result.ToString();
        }

        private struct GroupElement
        {
            public long Value { get; set; }

            public static GroupElement operator +(GroupElement a, GroupElement b)
            {
                return a.Value + b.Value;
            }

            public static GroupElement operator -(GroupElement a)
            {
                return (Q - 1) * a;
            }

            public static GroupElement operator -(GroupElement a, GroupElement b)
            {
                return a + (-b);
            }

            public static GroupElement operator *(GroupElement a, GroupElement b)
            {
                return a.Value * b.Value;
            }

            public static bool operator ==(GroupElement a, GroupElement b)
            {
                return a.Value == b.Value;
            }

            public static bool operator !=(GroupElement a, GroupElement b)
            {
                return !(a == b);
            }

            public static implicit operator long(GroupElement g)
            {
                return g.Value;
            }

            public static implicit operator GroupElement(long n)
            {
                return new GroupElement { Value = (n % Q + Q) % Q };
            }
        }

        private struct Point
        {
            public Point(GroupElement x, GroupElement y)
            {
                X = x;
                Y = y;
            }

            public GroupElement X { get; set; }
            public GroupElement Y { get; set; }

            public bool IsZero { get { return this == ZeroPoint; } }

            public static bool operator ==(Point p1, Point p2)
            {
                return p1.X == p2.X && p1.Y == p2.Y;
            }

            public static bool operator !=(Point p1, Point p2)
            {
                return !(p1 == p2);
            }

            public static Point operator +(Point p1, Point p2)
            {
                if (p1.IsZero)
                {
                    return p2;
                }

                if (p2.IsZero)
                {
                    return p1;
                }

                if (p1 == p2 && p1.Y != 0)
                {
                    long a, b;
                    long g = gcd(2 * p1.Y, Q, out a, out b);

                    GroupElement lambda = (3 * p1.X * p1.X + A) * a;
                    GroupElement x = lambda * lambda - 2 * p1.X;
                    GroupElement y = lambda * (p1.X - x) - p1.Y;

                    return new Point(x, y);
                }
                if (p1.X != p2.X)
                {
                    GroupElement dy = p2.Y - p1.Y;
                    GroupElement dx = p2.X - p1.X;

                    long a, b;
                    long g = gcd(dx, Q, out a, out b);

                    if (g != 1)
                    {
                        Console.WriteLine("Не существует!");
                    }

                    GroupElement lambda = dy * a;
                    GroupElement x = lambda * lambda - p1.X - p2.X;
                    GroupElement y = lambda * (p1.X - x) - p1.Y;

                    return new Point(x, y);
                }
                if (p1.X == p2.X && p2.Y == -(p1.Y))
                {
                    return new Point(0, 0);
                }
                return new Point(0, 0);
            }

            public static Point operator *(uint n, Point p)
            {
                var ans = p;
                for (uint i = 1; i < n; ++i)
                {
                    ans += p;
                }
                return ans;
            }

            static long gcd(long a, long b, out long x, out long y)
            {
                if (a == 0)
                {
                    x = 0; y = 1;
                    return b;
                }
                long x1, y1;
                long d = gcd(b % a, a, out x1, out y1);
                x = y1 - (b / a) * x1;
                y = x1;
                return d;
            }
        }
    }
}
