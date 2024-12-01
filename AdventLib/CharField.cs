// ReSharper disable MemberCanBePrivate.Global

namespace adventofcode.AdventLib;

using System;
using System.Linq;
using System.Text;

internal sealed class CharField : IEquatable<CharField>
{
    public CharField(char[][] fieldData)
    {
        this.FieldData = fieldData;
        this.UpdateHash();
    }

    public int Height
    {
        get => this.FieldData.Length;
    }

    public int Width
    {
        get => this.FieldData[0].Length;
    }

    public char[][] FieldData { get; private set; }

    public ulong Hash { get; set; }

    public char this[int x, int y]
    {
        get
        {
            if (y >= 0 && y < this.FieldData.Length && x >= 0 && x < this.FieldData[y].Length)
            {
                return this.FieldData[y][x];
            }

            throw new AggregateException("Index out of range.");
        }
        set
        {
            if (y >= 0 && y < this.FieldData.Length && x >= 0 && x < this.FieldData[y].Length)
            {
                this.FieldData[y][x] = value;
            }
            else
            {
                throw new AggregateException("Index out of range.");
            }
        }
    }

    public static bool Equals(CharField x, CharField y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Hash == y.Hash;
    }

    public static int GetHashCode(CharField obj)
    {
        return obj.Hash.GetHashCode();
    }

    public static char[][] Rotate90Clockwise(char[][] array)
    {
        int n = array.Length;
        char[][] result = new char[n][];

        for (int i = 0; i < n; i++)
        {
            result[i] = new char[n];
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[j][n - i - 1] = array[i][j];
            }
        }

        return result;
    }

    public void ApplyToAll(Action<char, int, int> action)
    {
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                action(this.FieldData[y][x], x, y);
            }
        }
    }

    public void ApplyToAll(Action<char> action)
    {
        this.ApplyToAll((c, _, _) => action(c));
    }

    public void ApplyToAnyCharacter(Func<char, int, int, bool> match, Action<char, int, int> action)
    {
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                if (match(this.FieldData[y][x], x, y))
                {
                    action(this.FieldData[y][x], x, y);
                }
            }
        }
    }

    public bool Equals(CharField other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Hash == other.Hash;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((CharField)obj);
    }

    public override int GetHashCode()
    {
        return this.Hash.GetHashCode();
    }

    public CharField GetPart(int x, int y, int width, int height)
    {
        return new CharField(
            this.FieldData.Skip(y).Take(height).Select(row => row.Skip(x).Take(width).ToArray()).ToArray()
        );
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < this.Width && y >= 0 && y < this.Height;
    }

    public bool IsOutside(int x, int y)
    {
        return x < 0 || x >= this.Width || y < 0 || y >= this.Height;
    }

    public void RotateRight()
    {
        this.FieldData = Rotate90Clockwise(this.FieldData);
    }

    public void Swap(int x1, int y1, int x2, int y2)
    {
        (this.FieldData[y1][x1], this.FieldData[y2][x2]) = (this.FieldData[y2][x2], this.FieldData[y1][x1]);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        for (int y = 0; y < this.Height; y++)
        {
            for (int x = 0; x < this.Width; x++)
            {
                builder.Append(this.FieldData[y][x]);
            }

            builder.Append('\n');
        }

        builder.Append($"Width: {this.Width}, Height: {this.Height}, Hash: 0x{this.Hash:X8}");

        return builder.ToString();
    }

    private void UpdateHash()
    {
        unchecked
        {
            ulong hash = 5381;
            int y = 0;

            foreach (char[] array in this.FieldData)
            {
                int x = 0;
                foreach (char c in array)
                {
                    x++;
                    hash = (hash << 5) + hash + c;
                }

                y++;
            }

            this.Hash = hash;
        }
    }
}
