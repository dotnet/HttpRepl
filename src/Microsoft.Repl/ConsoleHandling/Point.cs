// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Repl.ConsoleHandling
{
    public struct Point : IEquatable<Point>
    {
        public int X { get; }

        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (!(obj is Point other))
            {
                return false;
            }
            else
            {
                return Equals(other);
            }
        }

        public override int GetHashCode()
        {
            int hash = 27;
            hash = (13 * hash) + X.GetHashCode();
            hash = (13 * hash) + Y.GetHashCode();
            return hash;
        }

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        public bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}
