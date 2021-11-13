using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ChunkData chunkData;
    public Vector3 chunkCoord;
    public bool loaded = true;

    public void CreateChunkData(Vector3 chunkCoordinate, int[,,] voxelAtlas)
    {
        chunkCoord = chunkCoordinate;
        chunkData = new ChunkData(voxelAtlas, chunkCoordinate, 32);
        chunkData.chunkMesh = WorldInfo.meshBuilder.Build(chunkData);
    }

    public void SetChunkData(ChunkData chunkData)
    {
        this.chunkData = chunkData;
        chunkCoord = chunkData.chunkCoord;
    }

    public void BuildMesh()
    {
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = chunkData.chunkMesh;
    }

    public void UpdatePositionAndMesh()
    {
        transform.position = new Vector3(chunkCoord.x * chunkData.size, chunkCoord.z * chunkData.size, chunkCoord.y * chunkData.size);
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = chunkData.chunkMesh;
        this.chunkCoord = chunkData.chunkCoord;
        gameObject.name = "chunk (" + chunkCoord.x + ", " + chunkCoord.y + ")";

        //Debug.Log("Updating Chunk at new coord (" + chunkCoord.x + ", " + chunkCoord.y + ") to position (" + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
    }

    public void SetUnloaded()
    {
        this.loaded = false; 
        gameObject.SetActive(false);
    }

    public void SetLoaded()
    {
        this.loaded = true;
        gameObject.SetActive(true);
    }

  
}



public class ChunkData
{
    public Vector3 chunkCoord;

    public int size;

    public Mesh chunkMesh;
    
    public int[,,] voxelAtlas;

    public ChunkData(int[,,] voxelAtlas, Vector3 chunkCoord, int size)
    {
        this.voxelAtlas = voxelAtlas;
        this.chunkCoord = chunkCoord;
        this.size = size;
    }
  
}