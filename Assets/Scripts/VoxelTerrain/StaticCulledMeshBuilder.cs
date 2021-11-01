using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;

using Unity.Mathematics;

namespace VoxelTerrain
{
    //todo: replace incorrect types (Vector3, List) with blittable types (float3, NativeList)
    //done: replaced floats, todo replace List with NativeList (this involves importing ECS package for nativelist to be added to System.collections
    
    public class StaticCulledMeshBuilder
    {


        

        public static int[] prepareData(ChunkData chunkData)
        {
            int[] flattenedArray = FlattenVoxelAtlas(chunkData.voxelAtlas);
            return flattenedArray;
        }

        public static Mesh Build(NativeArray<int> chunkData)
        {




            int buildLength = WorldInfo.chunkSize;
            int buildWidth = WorldInfo.chunkSize;
            int buildHeight = WorldInfo.chunkSize;

            List<float3> vertexData = new List<float3>();
            List <int> triangleData = new List<int>();

            for (int i = 0; i < buildLength; i++)
            {
                for (int j = 0; j < buildWidth; j++)
                {
                    for (int k = 0; k < buildHeight; k++)
                    {
                        if (chunkData[CoordsToInt(i,j,k)] != 0)
                        {
                            CreateQuads(chunkData, ref vertexData, ref triangleData, i, j, k, buildLength, buildWidth, buildHeight);
                        }

                    }
                }
            }

            Mesh mesh = new Mesh();

            for (int i = 0; i < vertexData.Count; i++)
            {
                Vector3 temp = vertexData[i];
                Vector3 reordered = new Vector3(temp.x, temp.z, temp.y);
                vertexData[i] = reordered;
            }

            //mesh.vertices = vertexData.ToArray();  todo have this be done through some new method? see reddit comment ?

            mesh.triangles = triangleData.ToArray();

            mesh.RecalculateNormals();



           

            return mesh;
        }

        private static void CreateQuads(NativeArray<int> chunkArray, ref List<float3> vertices, ref List<int> triangles, int i, int j, int k, int length, int width, int height)
        {

            if (i == 0 || (chunkArray[CoordsToInt(i - 1, j, k)] == 0)) //check back
            {
                CreateBackQuad(ref vertices, ref triangles, i, j, k);
            }

            if (i == length - 1 || (chunkArray[CoordsToInt(i + 1, j, k)] == 0)) //check front
            {
                CreateFrontQuad(ref vertices, ref triangles, i, j, k);
            }

            if (j == 0 || (chunkArray[CoordsToInt(i, j - 1, k)] == 0)) //check left
            {
                CreateLeftQuad(ref vertices, ref triangles, i, j, k);
            }

            if (j == width - 1 || (chunkArray[CoordsToInt(i, j + 1, k)] == 0)) //check right
            {
                CreateRightQuad(ref vertices, ref triangles, i, j, k);
            }

            if (k == 0 || (chunkArray[CoordsToInt(i, j, k - 1)] == 0)) //check bottom
            {
                CreateBottomQuad(ref vertices, ref triangles, i, j, k);
            }

            if (k == height - 1 || chunkArray[CoordsToInt(i, j, k + 1)] == 0) //check top
            {
                CreateTopQuad(ref vertices, ref triangles, i, j, k);
            }
        }

        private static void CreateTopQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {
            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i + 1, j, k + 1); //top left
            float3 createdVertex2 = new float3(i, j + 1, k + 1); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i, j, k + 1)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i + 1, j + 1, k + 1)); //bottom left
        }

        private static void CreateLeftQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {
            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i + 1, j, k + 1); //top left
            float3 createdVertex2 = new float3(i, j, k); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i + 1, j, k)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i, j, k + 1)); //bottom left
        }

        private static void CreateRightQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {
            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i, j + 1, k + 1); //top left
            float3 createdVertex2 = new float3(i + 1, j + 1, k); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i, j + 1, k)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i + 1, j + 1, k + 1)); //bottom left
        }

        private static void CreateFrontQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {


            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i + 1, j + 1, k + 1); //top left
            float3 createdVertex2 = new float3(i + 1, j, k); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i + 1, j + 1, k)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i + 1, j, k + 1)); //bottom left
        }

        private static void CreateBackQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {
            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i, j, k + 1); //top left
            float3 createdVertex2 = new float3(i, j + 1, k); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i, j, k)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i, j + 1, k + 1)); //bottom left
        }

        private static void CreateBottomQuad(ref List<float3> vertexData, ref List<int> triangleData, int i, int j, int k)
        {
            for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
            {
                triangleData.Add(x);
            }

            float3 createdVertex = new float3(i, j, k); //top left
            float3 createdVertex2 = new float3(i + 1, j + 1, k); //bottom right

            vertexData.Add(createdVertex);
            vertexData.Add(new Vector3(i + 1, j, k)); //top right
            vertexData.Add(createdVertex2);

            vertexData.Add(createdVertex);
            vertexData.Add(createdVertex2);
            vertexData.Add(new Vector3(i, j + 1, k)); //bottom left
        }

        public static int CoordsToInt(int x, int y, int z)
        {
            return (x + y * WorldInfo.chunkSize + z * WorldInfo.chunkSize * WorldInfo.chunkSize);
        }

        public static int[] FlattenVoxelAtlas(int[,,] voxelAtlas)
        {
            int[] flattenedAtlas = new int[WorldInfo.chunkSize * WorldInfo.chunkSize * WorldInfo.chunkSize];
            for (int i = 0; i < WorldInfo.chunkSize; i++)
            {
                for (int j = 0; j < WorldInfo.chunkSize; j++)
                {
                    for (int k = 0; k < WorldInfo.chunkSize; k++)
                    {
                        flattenedAtlas[(i + j * WorldInfo.chunkSize + k * WorldInfo.chunkSize * WorldInfo.chunkSize)] =
                            voxelAtlas[i, j, k];
                    }
                }
            }

            return flattenedAtlas;
        }




    }

}

