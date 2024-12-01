using System;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace adventofcode.AdventLib;

public struct BinaryMap128
{
    public static readonly BinaryMap128 ZERO = new BinaryMap128();
    public static readonly BinaryMap128 FULL = BinaryMap128.Filled(ulong.MaxValue); 
    private const int Width = 128;
    private const int Height = 128;
    private const int MapSize = Width*Height; // 128 * 128 bits
    private const int LongsInMap = MapSize / 64; // number of 64-bit longs to represent the map
    private unsafe fixed ulong map[LongsInMap];

    public unsafe static BinaryMap128 Filled(ulong val)
    {
        BinaryMap128 result = new BinaryMap128();
        for(var i=0;i<LongsInMap;i++)
            result.map[i] = val;
        return result;
    }
    
    public unsafe bool this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;

            int bitIndex = y * Width + x;
            int longIndex = bitIndex / 64;
            int bitPosition = bitIndex % 64;

            return (map[longIndex] & ((ulong)1 << bitPosition)) != 0;
        }
        set
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            int bitIndex = y * Width + x;
            int longIndex = bitIndex / 64;
            int bitPosition = bitIndex % 64;

            if (value)
                map[longIndex] |= ((ulong)1 << bitPosition); // Set bit
            else
                map[longIndex] &= ~((ulong)1 << bitPosition); // Clear bit
        }
    }
    
    public unsafe BinaryMap128 And(BinaryMap128 other)
    {
        if (!Avx2.IsSupported) throw new PlatformNotSupportedException("AVX2 is not supported on this processor.");

        BinaryMap128 result = new BinaryMap128();
        for (var i = 0; i < LongsInMap; i += 4)
        {
            fixed (ulong* currentAdr = &map[i])
            {
                var current = Avx.LoadVector256(currentAdr);
                var otherCurrent = Avx.LoadVector256(&other.map[i]);
                var resultOp = Avx2.And(current, otherCurrent);
                Avx.Store(&result.map[i], resultOp);
            }
        }

        return result;
    }

    public unsafe void ApplyOr(BinaryMap128 other)
    {
        if (!Avx2.IsSupported) throw new PlatformNotSupportedException("AVX2 is not supported on this processor.");

        for (var i = 0; i < LongsInMap; i += 4)
        {
            fixed (ulong* currentAdr = &map[i])
            {
                var current = Avx.LoadVector256(currentAdr);
                var otherCurrent = Avx.LoadVector256(&other.map[i]);
                var resultOp = Avx2.Or(current, otherCurrent);
                Avx.Store(currentAdr, resultOp);
            } 
        }
    }
    
    public unsafe BinaryMap128 Or(BinaryMap128 other)
    {
        if (!Avx2.IsSupported) throw new PlatformNotSupportedException("AVX2 is not supported on this processor.");

        BinaryMap128 result = new BinaryMap128();
        for (var i = 0; i < LongsInMap; i += 4)
        {
            fixed (ulong* currentAdr = &map[i])
            {
                var current = Avx.LoadVector256(currentAdr);
                var otherCurrent = Avx.LoadVector256(&other.map[i]);
                var resultOp = Avx2.Or(current, otherCurrent);
                Avx.Store(&result.map[i], resultOp);
            } 
        }
        return result;
    }

    public unsafe BinaryMap128 Xor(BinaryMap128 other)
    {
        if (!Avx2.IsSupported) throw new PlatformNotSupportedException("AVX2 is not supported on this processor.");

        BinaryMap128 result = new BinaryMap128();
        for (var i = 0; i < LongsInMap; i += 4)
        {
            fixed (ulong* currentAdr = &map[i])
            {
                var current = Avx.LoadVector256(currentAdr);
                var otherCurrent = Avx.LoadVector256(&other.map[i]);
                var resultOp = Avx2.Xor(current, otherCurrent);
                Avx.Store(&result.map[i], resultOp);
            }
        }

        return result;
    }

    public unsafe void DrawLine(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2)
        {
            for(var yy=Math.Min(y1,y2);yy<=Math.Max(y1,y2);yy++)
                this[x2, yy] = true;
        }
        else
        {
            for(var xx=Math.Min(x1,x2);xx<=Math.Max(x1,x2);xx++)
                this[xx, y1] = true;
        }
    }
    
    public unsafe BinaryMap128 AndNot(BinaryMap128 other)
    {
        if (!Avx2.IsSupported) throw new PlatformNotSupportedException("AVX2 is not supported on this processor.");

        BinaryMap128 result = new BinaryMap128();
        for (var i = 0; i < LongsInMap; i += 4)
        {
            fixed (ulong* currentAdr = &map[i])
            {
                var current = Avx.LoadVector256(currentAdr);
                var otherCurrent = Avx.LoadVector256(&other.map[i]);
                var resultOp = Avx2.AndNot(current, otherCurrent);
                Avx.Store(&result.map[i], resultOp);
            }
        }

        return result;
    }
    
    public unsafe BinaryMap128 Copy()
    {
        BinaryMap128 result = new BinaryMap128();
        for (var i = 0; i < LongsInMap; i++)
        {
            result.map[i] = map[i];
        }

        return result;
    }
    
    public static BinaryMap128 operator &(BinaryMap128 a, BinaryMap128 b) => a.And(b);
    public static BinaryMap128 operator |(BinaryMap128 a, BinaryMap128 b) => a.Or(b);
    public static BinaryMap128 operator ^(BinaryMap128 a, BinaryMap128 b) => a.Xor(b);
    
    public unsafe void PrintMap()
    {
        PrintMap(Width, Height);
    }
    
    public unsafe void PrintMap(int width, int height, int xSpecial = -1, int ySpecial = -1)
    {
        StringBuilder sb = new StringBuilder();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if(this[x,y]) {
                    if(x == xSpecial && y == ySpecial)
                        sb.Append('*');
                    else
                        sb.Append('1');
                }
                else
                {
                    if(x == xSpecial && y == ySpecial)
                        sb.Append('O');
                    else
                        sb.Append('0');
                }

                if ((x + 1) % 8 == 0)
                    sb.Append(' '); // Add a space after every 8 bits
            }
            sb.AppendLine(); // New line after each row
        }

        Console.WriteLine(sb.ToString());
    }

    public unsafe int CountSetBits()
    {
        int count = 0;
        for (int i = 0; i < LongsInMap; i++)
        {
            count += BitOperations.PopCount(map[i]);
        }
        return count;
    }
}