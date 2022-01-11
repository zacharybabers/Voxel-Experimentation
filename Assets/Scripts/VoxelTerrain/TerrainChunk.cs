using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public ChunkData chunkData;
    public Vector3 chunkCoord;
    public MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
    }
    
}



public class ChunkData
{
    public Vector3 chunkCoord;

    public bool isEmpty;
    
    public Mesh chunkMesh;
    
    public int[,,] voxelAtlas;

    public TerrainChunk terrainChunk;

    public ChunkData(Vector3 chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        GetVoxelAtlas();
        isEmpty = IsEmpty();
    }

    public void BuildMesh()
    {
        if (!isEmpty)
        {
            chunkMesh = WorldInfo.meshBuilder.Build(this);
        }
        UpdatePositionAndMesh();
    }

    private void GetVoxelAtlas()
    {
        voxelAtlas = WorldInfo.terrainGenerator.GenerateChunkAtlas(chunkCoord);
    }

    public void UpdatePositionAndMesh()
    {
        if (terrainChunk == null)
        {
            return;
        }
        terrainChunk.transform.position = new Vector3(chunkCoord.x * WorldInfo.chunkSize, chunkCoord.z * WorldInfo.chunkSize, chunkCoord.y * WorldInfo.chunkSize);
        terrainChunk.meshFilter.mesh = chunkMesh;
        terrainChunk.chunkCoord = chunkCoord;
        terrainChunk.gameObject.name = "chunk (" + chunkCoord.x + ", " + chunkCoord.y + ", " + chunkCoord.z + ")";
    }

    public void AssignTerrainChunk(TerrainChunk terrainChunk)
    {
        terrainChunk.gameObject.SetActive(true);
        this.terrainChunk = terrainChunk;
        terrainChunk.chunkData = this;
        UpdatePositionAndMesh();
    }

    public void UnloadTerrainChunk()
    {
        if (terrainChunk != null)
        {
            this.terrainChunk.gameObject.SetActive(false);
            this.terrainChunk = null;
        }
    }

    public bool HasMesh()
    {
        return this.chunkMesh != null;
    }

    private bool IsEmpty()
    {
        
        for (int i = 0; i < WorldInfo.chunkSize; i++)
        {
            for (int j = 0; j < WorldInfo.chunkSize; j++)
            {
                for (int k = 0; k < WorldInfo.chunkSize; k++)
                {
                    if (voxelAtlas[i, j, k] != 0)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    
    
}