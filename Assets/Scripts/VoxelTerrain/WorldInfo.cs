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

    private void Awake()
    {
        meshBuilder = gameObject.AddComponent<CulledMeshBuilder>();
        loadedChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        chunksToLoad = new Queue<Vector2>();
        chunkPool = new List<ChunkData>();
        
       InitializeWorld();
    }

    private void Update()
    {
        UpdateChunks();
    } 

    private TerrainChunk GetChunkFromCoordinates(Vector2 chunkCoord)
    {
        GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
        var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
        chunkObject.name = "chunk (" + chunkCoord.x + ", " + chunkCoord.y + ")";
        
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
                if (!loadedChunkDictionary.ContainsKey(new Vector2(i, j)) && !chunksToLoad.Contains(new Vector2(i, j)))
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
        FindInitialChunksToLoad();
        
        if (chunksToLoad.Count > 0)
        {
            RefreshOneChunk();
        }

        
      

    }

    public void RefreshOneChunk()
    {
        //calculate view distance, check which chunks are outside of it, then add them to pool
        var viewDistSquared = drawDistance * drawDistance;
        int currentChunkX = (int) targetTransform.position.x / chunkSize;
        int currentChunkY = (int) targetTransform.position.z / chunkSize;
        
        
        TerrainChunk[] terrainChunks = new TerrainChunk[loadedChunkDictionary.Count];
        loadedChunkDictionary.Values.CopyTo(terrainChunks, 0);

        for (int chunkNum = 0; chunkNum < loadedChunkDictionary.Count; chunkNum++)
        {
            var foundChunkToUnload = false;
            
            var distSquared = Mathf.Pow((float) (terrainChunks[chunkNum].chunkCoord.x - currentChunkX) * chunkSize, 2) + Mathf.Pow((float) (terrainChunks[chunkNum].chunkCoord.y - currentChunkY) * chunkSize, 2);
            if (distSquared >= viewDistSquared)
            {
                var terrainChunk = terrainChunks[chunkNum];
                
                var originalCoord = terrainChunk.chunkCoord;
                
                chunkPool.Add(terrainChunk.chunkData);
                if (chunkPool.Count > chunkPoolSize)
                {
                    chunkPool.RemoveAt(0);
                }

                var chunkToLoad = chunksToLoad.Dequeue();

                bool gotChunk = false;

                for (int i = chunkPool.Count - 1; i >= 0; i--)
                {
                    bool finished = false;
                    if (chunkPool[i].chunkCoord.Equals(chunkToLoad))
                    {
                        terrainChunk.SetChunkData(chunkPool[i]);
                        chunkPool.RemoveAt(i);
                        gotChunk = true;
                        finished = true;
                    }

                    if (finished)
                    {
                        break;
                    }
                }

                if (!gotChunk)
                {
                    terrainChunk.CreateChunkData(chunkToLoad, GenerateChunkAtlas(chunkToLoad));
                }

                terrainChunk.UpdatePositionAndMesh();

                terrainChunk.gameObject.name =
                    "chunk (" + terrainChunk.chunkCoord.x + ", " + terrainChunk.chunkCoord.y + ")";

                loadedChunkDictionary.Add(chunkToLoad, terrainChunk);
                loadedChunkDictionary.Remove(originalCoord);

                foundChunkToUnload = true;
            }
            
            if (foundChunkToUnload)
            {
                break;
            }

        }
    }
}
