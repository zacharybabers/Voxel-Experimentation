using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInfo : MonoBehaviour
{
    public Dictionary<Vector3, TerrainChunk> loadedChunkDictionary;
    public Queue<Vector3> chunksToLoad;
    public List<ChunkData> chunkPool;
    public List<TerrainChunk> unloadedChunks;

    [SerializeField] private int chunkPoolSize = 9;
    [SerializeField] private int nonLoadedChunkSize = 9;

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
        loadedChunkDictionary = new Dictionary<Vector3, TerrainChunk>();
        chunksToLoad = new Queue<Vector3>();
        chunkPool = new List<ChunkData>();
        unloadedChunks = new List<TerrainChunk>();
        for (int i = 0; i < nonLoadedChunkSize; i++)
        {
            GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
            var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
            terrainChunk.SetLoaded();
            unloadedChunks.Add(terrainChunk);
        }

        InitializeWorld();
    }

    private void Update()
    {
        UpdateChunks();
    }

    private TerrainChunk GetChunkFromCoordinates(Vector3 chunkCoord)
    {
        GameObject chunkObject = Instantiate(chunkPrefab, chunkParent);
        var terrainChunk = chunkObject.AddComponent<TerrainChunk>();
        chunkObject.name = "chunk (" + chunkCoord.x + ", " + chunkCoord.y + ")";

        terrainChunk.CreateChunkData(chunkCoord, GenerateChunkAtlas(chunkCoord));
        terrainChunk.chunkData.chunkMesh = meshBuilder.Build(terrainChunk.chunkData);
        terrainChunk.chunkCoord = chunkCoord;


        chunkObject.transform.position =
            new Vector3(chunkCoord.x * chunkSize, chunkCoord.z * chunkSize, chunkCoord.y * chunkSize);

        terrainChunk.BuildMesh();
        terrainChunk.SetLoaded();
        return terrainChunk;
    }


    private int[,,] GenerateChunkAtlas(Vector3 chunkCoord)
    {
        int[,,] ints = new int[chunkSize, chunkSize, chunkSize];

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

    private int[,] GenerateHeightMap(Vector3 chunkCoord)
    {
        int[,] initMap = new int[chunkSize, chunkSize];

        for (int i = 0; i < chunkSize; i++)
        {
            for (int j = 0; j < chunkSize; j++)
            {
                float temp =
                    fastNoiseUnity.fastNoise.GetNoise(chunkSize * chunkCoord.x + i, chunkSize * chunkCoord.y + j) *
                    (mapScale / 2f);
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
        var pos = targetTransform.position;
        int currentChunkX = (int) pos.x / chunkSize;
        int currentChunkY = (int) pos.z / chunkSize;
        int currentChunkZ = (int) pos.y / chunkSize;

        int chunksInLinearDist = (int) drawDistance / chunkSize;

        //loop through x,y,z... if dist(player,(x,y,z)) < radius, do our check for coords being in dictionary, if not, add to chunksToLoad... this method would allow implementation of steps as well
        for (int i = currentChunkX - chunksInLinearDist;
            i <= currentChunkX + chunksInLinearDist;
            i++) //edit this loop in order to create steps, for example, per frame we only go through one x and repeat every set number of frames ( this would probably be done for y generally but no real difference )
        {
            for (int j = currentChunkY - chunksInLinearDist; j <= currentChunkY + chunksInLinearDist; j++)
            {
                if (!loadedChunkDictionary.ContainsKey(new Vector3(i, j, 0)) &&
                    !chunksToLoad.Contains(new Vector3(i, j, 0)))
                {
                    var distSquared = Mathf.Pow((float) (i - currentChunkX) * chunkSize, 2) +
                                      Mathf.Pow((float) (j - currentChunkY) * chunkSize, 2) + Mathf.Pow((float) (0 - currentChunkZ) * chunkSize, 2);
                    if (distSquared < viewDistSquared)
                    {
                        chunksToLoad.Enqueue(new Vector3(i, j, 0));
                    }
                }
            }
        }

        var myChunk = new Vector3(currentChunkX, currentChunkY, 0);

        if (chunksToLoad.Contains(myChunk) && chunksToLoad.Peek() != myChunk)
        {
            List<Vector3> tempList = new List<Vector3>();
            tempList.Add(myChunk);
            tempList.AddRange(chunksToLoad);
            var index = 0;
            for (int i = 1; i < tempList.Count; i++)
            {
                if (tempList[i].Equals(myChunk))
                {
                    index = i;
                }
            }

            tempList.RemoveAt(index);
            chunksToLoad = new Queue<Vector3>(tempList);
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
            terrainChunk.SetLoaded();
        }
    }

    public void UpdateChunks()
    {
        FindInitialChunksToLoad();
        UnloadChunks();

        if (chunksToLoad.Count > 0)
        {
            RefreshOneChunk();
        }
    }


    public void UnloadChunks()
    {
        var viewDistSquared = drawDistance * drawDistance;

        var pos = targetTransform.position;
        int currentChunkX = (int) pos.x / chunkSize;
        int currentChunkY = (int) pos.z / chunkSize;
        int currentChunkZ = (int) pos.y / chunkSize;

        TerrainChunk[] terrainChunks = new TerrainChunk[loadedChunkDictionary.Count];
        loadedChunkDictionary.Values.CopyTo(terrainChunks, 0);

        for (int chunkNum = 0; chunkNum < loadedChunkDictionary.Count; chunkNum++)
        {
            //calculate view distance, check which chunks are outside of it, then add them to pool and unload them


            var distSquared = Mathf.Pow((float) (terrainChunks[chunkNum].chunkCoord.x - currentChunkX) * chunkSize, 2) +
                              Mathf.Pow((float) (terrainChunks[chunkNum].chunkCoord.y - currentChunkY) * chunkSize, 2) +
                              Mathf.Pow((float) (terrainChunks[chunkNum].chunkCoord.z - currentChunkZ) * chunkSize, 2);
            if (distSquared >= viewDistSquared)
            {
                var terrainChunk = terrainChunks[chunkNum];

                terrainChunk.SetUnloaded();
                loadedChunkDictionary.Remove(terrainChunk.chunkCoord);
                unloadedChunks.Add(terrainChunk);

                chunkPool.Add(terrainChunk.chunkData);
                if (chunkPool.Count > chunkPoolSize)
                {
                    chunkPool.RemoveAt(0);
                }
            }
        }
    }

    public void RefreshOneChunk()
    {
        var terrainChunk = unloadedChunks[0];
        unloadedChunks.RemoveAt(0);

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

        terrainChunk.SetLoaded();
        terrainChunk.UpdatePositionAndMesh();

        loadedChunkDictionary.Add(chunkToLoad, terrainChunk);
    }
}