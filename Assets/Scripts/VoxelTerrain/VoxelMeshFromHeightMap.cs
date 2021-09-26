using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshFromHeightMap : MonoBehaviour
{
    
}

public class ChunkData
{
    private int[,,] voxelAtlas;
    private int width;
    private int length;
    private int height;

    public ChunkData(int length, int width, int height, int[,] heightMap)
    {
        this.width = width;
        this.length = length;
        this.height = height;
        assignHeightMapToVoxelAtlas(heightMap);
    }

    public ChunkData(int[,,] voxelArray)
    {
        this.length = voxelArray.GetLength(0);
        this.width = voxelArray.GetLength(1);
        this.height = voxelArray.GetLength(2);

        this.voxelAtlas = voxelArray;
    }

    private void assignHeightMapToVoxelAtlas(int[,] heightMap)
    {
        voxelAtlas = new int[length, width, height];

        int[,] properlySizedHeightMap = new int[length, width];
        
        //fill the new heightmap with given heightmap values and 0s where none exist
        int lengthComparison = heightMap.GetLength(0) - this.length;  //if 0, lengths are the same. If positive, heightmap is longer. If negative, heightmap is shorter.
        int widthComparison = heightMap.GetLength(1) - this.width;

        if (lengthComparison >= 0)
        {
            for (int x = 0; x < this.length; x++)
            {
                if (widthComparison >= 0)
                {
                    for (int y = 0; y < this.width; y++)
                    {
                        properlySizedHeightMap[x, y] = heightMap[x, y];
                    }
                }
                else
                {
                    int y;
                    
                    for (y = 0; y < heightMap.GetLength(1); y++)
                    {
                        properlySizedHeightMap[x, y] = heightMap[x, y];
                    }

                    for (y=y; y < this.width; y++)
                    {
                        properlySizedHeightMap[x, y] = 0;
                    }
                }
            }
        }
        else
        {
            int x;

            for (x = 0; x < heightMap.GetLength(0); x++)
            {
                if (widthComparison >= 0)
                {
                    for (int y = 0; y < this.width; y++)
                    {
                        properlySizedHeightMap[x, y] = heightMap[x, y];
                    }
                }
                else
                {
                    int y;
                    
                    for (y = 0; y < heightMap.GetLength(1); y++)
                    {
                        properlySizedHeightMap[x, y] = heightMap[x, y];
                    }

                    for (y=y; y < this.width; y++)
                    {
                        properlySizedHeightMap[x, y] = 0;
                    }
                }
            }

            for (x = x; x < this.length; x++)
            {
                for (int y = 0; y < this.width; y++)
                {
                    properlySizedHeightMap[x, y] = 0;
                }
                
            }
        }
        //properly sized heightmap is of the proper dimensions, filled with given heightmap values and 0s where none exist.
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (properlySizedHeightMap[x, y] != 0)
                {
                    for (int z = 0; z < properlySizedHeightMap[x, y]; z++)    //loop fills the voxel atlas with 1 for each index where a voxel should be (0s are empty)
                    {
                        voxelAtlas[x, y, z] = 1;
                    }
                }
                
            }
        }
        
        
        
        
    }
}