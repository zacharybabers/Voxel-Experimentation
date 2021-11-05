using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInfo : MonoBehaviour
{
    public Dictionary<Vector2, TerrainChunk> loadedChunkDictionary;
    public Queue<Vector2> chunksToLoad;
    public List<ChunkData> chunkPool;

    [SerializeField] private int chunkPoolSize = 9;

    [SerializeField] private FastNoiseUnity fastNoiseUnity;

    public const int chunkSize = 32;
    
    public static CulledMeshBuilder meshBuilder;
    
    [SerializeField] private Transform chunkParent;
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private int mapScale;
    [SerializeField] private int mapHeightOnNoise;

    [SerializeField] private Transform targetTransform;
    [SerializeField] private float drawDistance;

    private float scanStep = 0f;
    [SerializeField] private float scanLength = 10f;
    
    

    


    private void Awake()
    {
        meshBuilder = gameObject.AddComponent<CulledMeshBuilder>();
        loadedChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        chunksToLoad = new Queue<Vector2>();
        chunkPool = new List<ChunkData>();
        
       InitializeWorld();
    }

   /* private void Update()
    {
        UpdateChunks();
    } */

    private TerrainChunk GetChunkFromCoordinates(Vector2 chunkCoord)
    {
        GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
        var terrainChunk = chunkObject.AddComponent<TerrainChunk>();

        terrainChunk.CreateChunkData(chunkCoord, GenerateChunkAtlas(chunkCoord));
        terrainChunk.chunkData.chunkMesh = meshBuilder.Build(terrainChunk.chunkData);
        terrainChunk.chunkCoord = chunkCoord;
        
        chunkObject.transform.position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
        
        terrainChunk.BuildMesh();

        return terrainChunk;
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
               float temp = fastNoiseUnity.fastNoise.GetNoise(chunkSize * chunkCoord.x + i, chunkSize * chunkCoord.y + j) * (mapScale / 2f);
               temp += (mapScale / 2f);
               int intTemp = (int) temp;
               initMap[i, j] = intTemp;
            }
        }

        return initMap;
    }
    
    public void FindInitialChunksToLoad()
    {
        var viewDistSquared = drawDistance * drawDistance;

        int currentChunkX = (int) targetTransform.position.x / chunkSize;
        int currentChunkY = (int) targetTransform.position.z / chunkSize;

        int chunksInLinearDist = (int) drawDistance / chunkSize;

        //loop through x,y,z... if dist(player,(x,y,z)) < radius, do our check for coords being in dictionary, if not, add to chunksToLoad... this method would allow implementation of steps as well
        for (int i = currentChunkX - chunksInLinearDist; i <= currentChunkX + chunksInLinearDist; i++) //edit this loop in order to create steps, for example, per frame we only go through one x and repeat every set number of frames ( this would probably be done for y generally but no real difference )
        {
            for (int j = currentChunkY - chunksInLinearDist; j <= currentChunkY + chunksInLinearDist; j++)
            {
                if (!loadedChunkDictionary.ContainsKey(new Vector2(i, j)))
                {
                    var distSquared = Mathf.Pow((float) (i - currentChunkX) * chunkSize, 2) + Mathf.Pow((float) (j - currentChunkY) * chunkSize, 2);
                    if (distSquared < viewDistSquared)
                    {
                        chunksToLoad.Enqueue(new Vector2(i, j));
                    }
                }
            }
        }
    }
    
    public void UpdateChunksToLoad()
    {
        var viewDistSquared = drawDistance * drawDistance;

        int currentChunkX = (int) targetTransform.position.x / chunkSize;
        int currentChunkY = (int) targetTransform.position.y / chunkSize;

        int chunksInLinearDist = (int) drawDistance / chunkSize;

        //loop through x,y,z... if dist(player,(x,y,z)) < radius, do our check for coords being in dictionary, if not, add to chunksToLoad... this method would allow implementation of steps as well
        for (int i = currentChunkX - chunksInLinearDist + ((int) (scanStep / scanLength) *  chunksInLinearDist); i <= currentChunkX - chunksInLinearDist + ((int) ((scanStep + 1) / scanLength) * chunksInLinearDist); i++) //edit this loop in order to create steps, for example, per frame we only go through one x and repeat every set number of frames ( this would probably be done for y generally but no real difference )
        {
            for (int j = currentChunkY - chunksInLinearDist; j <= chunksInLinearDist + currentChunkY; j++)
            {
                if (!loadedChunkDictionary.ContainsKey(new Vector2(i, j)))
                {
                    var distSquared = Mathf.Pow((float) (i - currentChunkX) * chunkSize, 2) + Mathf.Pow((float) (j - currentChunkY) * chunkSize, 2);
                    if (distSquared < viewDistSquared)
                    {
                        chunksToLoad.Enqueue(new Vector2(i, j));
                    }
                }
            }
        }

        if (scanStep >= scanLength - 1)
        {
            scanStep = 0f;
        }
    }

    public void InitializeWorld()
    {
        FindInitialChunksToLoad();
        
        //initialize chunk objects
        int initialNumChunks = chunksToLoad.Count;
        
        for (int i = 0; i < initialNumChunks; i++)
        {
            var chunkLoading = chunksToLoad.Dequeue();
            var terrainChunk = GetChunkFromCoordinates(chunkLoading);
            loadedChunkDictionary.Add(terrainChunk.chunkCoord, terrainChunk);
            terrainChunk.loaded = true;
        }
    }

    public void UpdateChunks()
    {
        UpdateChunksToLoad();
        PoolAsNeeded();
    }

    public void PoolAsNeeded() //determine number of chunks that will be loaded, iterate through loaded chunk dictionary and add chunks that will be replaced to chunk pool
    {
       
        int numChunksToLoad = chunksToLoad.Count;
        int counter = 0;
        
        //calculate view distance, check which chunks are outside of it, then add them to pool
        var viewDistSquared = drawDistance * drawDistance;
        int currentChunkX = (int) targetTransform.position.x / chunkSize;
        int currentChunkY = (int) targetTransform.position.y / chunkSize;


        foreach (KeyValuePair<Vector2, TerrainChunk> chunk in loadedChunkDictionary)
        {
            var distSquared = Mathf.Pow((float) (chunk.Key.x - currentChunkX) * chunkSize, 2) + Mathf.Pow((float) (chunk.Key.y - currentChunkY) * chunkSize, 2);
            if (distSquared >= viewDistSquared)
            {
                var terrainChunk = chunk.Value;
                chunkPool.Add(terrainChunk.chunkData);

                if (chunkPool.Count > chunkPoolSize)
                {
                    chunkPool.RemoveAt(0);
                }

                var chunkToLoad = chunksToLoad.Dequeue();

                foreach (var chunkData in chunkPool) //todo figure out how to remove while still in loop
                {
                    if (chunkData.chunkCoord == chunkToLoad)
                    {
                        terrainChunk.chunkData = chunkData;
                        chunkPool.Remove(chunkData);
                    }
                }
                
                terrainChunk.CreateChunkData(chunkToLoad, GenerateChunkAtlas(chunkToLoad));
                terrainChunk.UpdatePositionAndMesh();

                counter++;
            }

            if (counter >= numChunksToLoad) //return if we're done pooling chunks to be unloaded
            {
                return;
            }

        }
    }
    
    








}
