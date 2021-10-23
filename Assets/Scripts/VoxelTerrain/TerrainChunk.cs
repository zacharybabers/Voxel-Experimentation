using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ChunkData chunkData;
    public Vector2 chunkCoord;

    public void CreateChunkData(Vector2 chunkCoordinate, int[,,] voxelAtlas)
    {
        chunkCoord = chunkCoordinate;
        chunkData = new ChunkData(voxelAtlas);
    }
}



public class ChunkData
{
    public int[,,] voxelAtlas;

    public ChunkData(int[,,] voxelAtlas)
    {
        this.voxelAtlas = voxelAtlas;
    }
  
}