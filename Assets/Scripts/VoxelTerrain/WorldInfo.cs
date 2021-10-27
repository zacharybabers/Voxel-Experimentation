using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInfo : MonoBehaviour
{
    public Dictionary<Vector2, ChunkData> loadedChunkDictionary;
    public Queue<Vector2> chunksToLoad;
    public ChunkData[] chunkPool;
    public List<TerrainChunk> chunkObjects;

    [SerializeField] private int chunkPoolSize = 9;

    [SerializeField] private FastNoiseUnity fastNoiseUnity;

    public const int chunkSize = 32;
    
    [SerializeField] private CulledMeshBuilder meshBuilder;
    [SerializeField] private Transform chunkParent;
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private int mapScale;
    [SerializeField] private int mapHeightOnNoise;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private float drawDistance;

    private FastNoise fastNoise;


    private void Awake()
    {
        meshBuilder = gameObject.AddComponent<CulledMeshBuilder>();
        loadedChunkDictionary = new Dictionary<Vector2, ChunkData>();
        chunksToLoad = new Queue<Vector2>();
        chunkPool = new ChunkData[9];
        chunkObjects = new List<TerrainChunk>();

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GetChunkFromCoordinates(new Vector2(i, j));
            }
        }

       
    }


    private void GetChunkFromCoordinates(Vector2 chunkCoord)
    {
        GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
        var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
        
        
        terrainChunk.CreateChunkData(chunkCoord, GenerateChunkAtlas(chunkCoord));
        terrainChunk.chunkData.chunkMesh = meshBuilder.Build(terrainChunk.chunkData);
        terrainChunk.chunkCoord = chunkCoord;
        
        chunkObject.transform.position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
        
        terrainChunk.BuildMesh();
    }

    private int[,,] GenerateChunkAtlas(Vector2 chunkCoord)
    {
        int[,,] ints = new int[chunkSize,chunkSize,chunkSize];

        int[,] heightMap = GenerateHeightMap(chunkCoord);

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                for (int k = mapHeightOnNoise; k < heightMap[i, j]; k++)
                {
                    int counter = k - mapHeightOnNoise;

                    ints[i, j, counter] = 1;
                }
            }
        }

        return ints;


    }

    private int[,] GenerateHeightMap(Vector2 chunkCoord)
    {
        int[,] initMap = new int[chunkSize,chunkSize];

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
               float temp = fastNoiseUnity.fastNoise.GetNoise(chunkSize * chunkCoord.x + i, chunkSize * chunkCoord.y + j) * (mapScale / 2);
               temp += (mapScale / 2f);
               int itemp = (int) temp;
               initMap[i, j] = itemp;
            }
        }

        return initMap;
    }

    public void CreateChunkData(Vector2 chunkCoordinate)
    {
        var chunkData = new ChunkData(GenerateChunkAtlas(chunkCoordinate), chunkCoordinate, chunkSize);
        chunkData.chunkMesh = meshBuilder.Build(chunkData);
        loadedChunkDictionary.Add(chunkData.chunkCoord, chunkData);
    }

    public void UpdateChunksToLoad()
    {
        var viewDistSquared = drawDistance * drawDistance;
        
        //loop through x,y,z... if dist(player,(x,y,z)) < radius, do our check for coords being in dictionary, if not, add to chunksToLoad... this method would allow implementation of steps as well
    }








}
