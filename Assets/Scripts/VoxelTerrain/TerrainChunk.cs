using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ChunkData chunkData;
    public Vector2 chunkCoord;
    public bool loaded = true;

    public void CreateChunkData(Vector2 chunkCoordinate, int[,,] voxelAtlas)
    {
        chunkCoord = chunkCoordinate;
        chunkData = new ChunkData(voxelAtlas, chunkCoordinate, 32);
        chunkData.chunkMesh = WorldInfo.meshBuilder.Build(chunkData);
    }

    public void BuildMesh()
    {
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = chunkData.chunkMesh;
    }

    public void UpdatePositionAndMesh()
    {
        transform.position = new Vector3(chunkCoord.x * chunkData.size, 0, chunkCoord.y * chunkData.size);
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = chunkData.chunkMesh;
    }

  
}



public class ChunkData
{
    public Vector2 chunkCoord;

    public int size;

    public Mesh chunkMesh;
    
    public int[,,] voxelAtlas;

    public ChunkData(int[,,] voxelAtlas, Vector2 chunkCoord, int size)
    {
        this.voxelAtlas = voxelAtlas;
        this.chunkCoord = chunkCoord;
        this.size = size;
    }
  
}