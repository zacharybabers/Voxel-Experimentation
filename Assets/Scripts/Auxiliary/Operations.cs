using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class Operations
{
    public static int GetIndex1(int ySize, int zSize, int x, int y, int z)
    {
        return z + y * zSize + x * zSize * ySize;
    }

    public static int3 GetIndex3(int xSize, int ySize, int zSize, int index)
    {
        int3 result = int3.zero;
        
        if (index < xSize * ySize * zSize)
        {
            result.z = index % zSize;
            result.y = (index / zSize) % ySize;
            result.x = index / (ySize * zSize);
        }

        return result;
    }
}
