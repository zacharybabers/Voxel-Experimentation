using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;



public class CulledMeshBuilder : MonoBehaviour
{
    

    public List<Vector3> vertexData;
    public List<int> triangleData;
    public List<Vector2> uvData;
    public UVSet[] uvLookup;

    
    private float timer = 0f;
    
    
    public int length;
    public int width;
    public int height;


    public void InitMeshBuilder()
    {
        vertexData = new List<Vector3>();
        triangleData = new List<int>();
        uvData = new List<Vector2>();
    }

    private void UpdateSurroundingChunks()
    {
        
    }

    public Mesh Build(ChunkData chunkData, ref int[,,] topChunk, ref int[,,] bottomChunk, ref int[,,] leftChunk,
        ref int[,,] rightChunk, ref int[,,] forwardChunk, ref int[,,] backChunk)
    {
        //timer = Time.realtimeSinceStartup;
        
        int[,,] chunkArray = chunkData.voxelAtlas;

        length = chunkArray.GetLength(0);
        width = chunkArray.GetLength(1);
        height = chunkArray.GetLength(2);
        
        vertexData.Clear();
        triangleData.Clear();
        uvData.Clear();
        
        
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    if (chunkArray[i, j, k] != 0)
                    {
                        CreateQuads(0, chunkArray, i, j, k, ref topChunk, ref bottomChunk, ref leftChunk, ref rightChunk, ref forwardChunk, ref backChunk);
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
        
        mesh.vertices = vertexData.ToArray();

        mesh.uv = uvData.ToArray();

        mesh.triangles = triangleData.ToArray();
        
        mesh.RecalculateNormals();

        

        //Debug.Log("time: " + (Time.realtimeSinceStartup - timer));
        
        return mesh;
    }

    private void CreateQuads(int lodLevel, int[,,] chunkArray, int i, int j, int k, ref int[,,] topChunk, ref int[,,] bottomChunk, ref int[,,] leftChunk,
        ref int[,,] rightChunk, ref int[,,] forwardChunk, ref int[,,] backChunk)
    {
        int value = chunkArray[i, j, k];
        int scrollFactor = (int) Mathf.Pow(2, lodLevel);
        
        if((i == 0 && leftChunk[length - 1, j, k] == 0) || (i != 0 && chunkArray[i-scrollFactor, j, k] == 0))     //check back
        {
            CreateBackQuad(lodLevel, i, j, k, ref uvLookup[value].sideUVs);
        }
        
        if((i == length - 1 && rightChunk[0, j, k] == 0) || (i != length - 1 && chunkArray[i+scrollFactor, j, k] == 0))     //check front
        {
            CreateFrontQuad(lodLevel, i, j, k, ref uvLookup[value].sideUVs);
        }       
        
        if((j == 0 && backChunk[i, width - 1, k] == 0) || (j!= 0 && chunkArray[i, j-scrollFactor, k] == 0))     //check left
        {
            CreateLeftQuad(lodLevel, i, j, k, ref uvLookup[value].sideUVs);
        }       
       
        if((j == width - 1 && forwardChunk[i, 0, k] == 0) || (j != width - 1 && chunkArray[i, j+scrollFactor, k] == 0))     //check right
        {
            CreateRightQuad(lodLevel, i, j, k, ref uvLookup[value].sideUVs);
        }       
        
        if( (k == 0 && bottomChunk[i, j, height - 1] == 0) || (k != 0 && chunkArray[i, j, k-scrollFactor] == 0))     //check bottom
        {
            CreateBottomQuad(lodLevel, i, j, k, ref uvLookup[value].bottomUVs);
        }       
        
        if((k == height - 1 && topChunk[i,j, 0] == 0) || (k != height - 1 && chunkArray[i, j, k+scrollFactor] == 0))     //check top
        {
            CreateTopQuad(lodLevel, i, j, k, ref uvLookup[value].topUVs);
        }       
    }

    public Mesh BuildLOD(int lodLevel, ChunkData chunkData, ref int[,,] topChunk, ref int[,,] bottomChunk, ref int[,,] leftChunk,
        ref int[,,] rightChunk, ref int[,,] forwardChunk, ref int[,,] backChunk)
    {
        //timer = Time.realtimeSinceStartup;
        
        int[,,] chunkArray = chunkData.voxelAtlas;

        length = chunkArray.GetLength(0);
        width = chunkArray.GetLength(1);
        height = chunkArray.GetLength(2);
        
        vertexData.Clear();
        triangleData.Clear();
        uvData.Clear();
        
        
        for (int i = 0; i < length; i+= (int) Mathf.Pow(2, lodLevel))
        {
            for (int j = 0; j < width; j+= (int) Mathf.Pow(2, lodLevel))
            {
                for (int k = 0; k < height; k+= (int) Mathf.Pow(2, lodLevel))
                {
                    if (chunkArray[i, j, k] != 0)
                    {
                        CreateQuads(lodLevel, chunkArray, i, j, k, ref topChunk, ref bottomChunk, ref leftChunk, ref rightChunk, ref forwardChunk, ref backChunk);
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
        
        mesh.vertices = vertexData.ToArray();

        mesh.uv = uvData.ToArray();

        mesh.triangles = triangleData.ToArray();
        
        mesh.RecalculateNormals();

        

        //Debug.Log("time: " + (Time.realtimeSinceStartup - timer));
        
        
        return mesh;
    }
   
    private void CreateTopQuad(int lodLevel, int i,int j,int k, ref QuadUVs topUVs)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }

        
        
        Vector3 createdVertex = new Vector3(i + 1,j,k + 1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j + 1,k + 1);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j,k + 1));  //top right
        vertexData.Add(createdVertex2);
       
        //add uvs for upper right triangle
        uvData.Add(topUVs.topLeft);
        uvData.Add(topUVs.topRight);
        uvData.Add(topUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i+1,j+1,k+ 1));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(topUVs.topLeft);
        uvData.Add(topUVs.bottomRight);
        uvData.Add(topUVs.bottomLeft);
    }
    
    private void CreateLeftQuad(int lodLevel, int i,int j,int k, ref QuadUVs sideUVs)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i+ 1,j,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j,k);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+1,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        //add uvs for upper right triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.topRight);
        uvData.Add(sideUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j,k+1));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.bottomRight);
        uvData.Add(sideUVs.bottomLeft);
    }
    private void CreateRightQuad(int lodLevel, int i,int j,int k, ref QuadUVs sideUVs)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j+ 1,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j+ 1,k);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j+ 1,k));  //top right
        vertexData.Add(createdVertex2);
        
        //add uvs for upper right triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.topRight);
        uvData.Add(sideUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i +1,j+ 1,k+1));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.bottomRight);
        uvData.Add(sideUVs.bottomLeft);
    }
    private void CreateFrontQuad(int lodLevel, int i,int j,int k, ref QuadUVs sideUVs)
    {
       
        
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i+ 1,j+ 1,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j,k);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+ 1,j+1,k));  //top right
        vertexData.Add(createdVertex2);
        
        //add uvs for upper right triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.topRight);
        uvData.Add(sideUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i+ 1,j,k+1));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.bottomRight);
        uvData.Add(sideUVs.bottomLeft);
    }
    private void CreateBackQuad(int lodLevel, int i,int j,int k, ref QuadUVs sideUVs)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j,k+1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j+1,k);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        //add uvs for upper right triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.topRight);
        uvData.Add(sideUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j+1,k+1));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(sideUVs.topLeft);
        uvData.Add(sideUVs.bottomRight);
        uvData.Add(sideUVs.bottomLeft);
    }
    private void CreateBottomQuad(int lodLevel, int i,int j,int k, ref QuadUVs bottomUVs)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j,k);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j+ 1,k);   //bottom right
        
        //add vertices for upper right triangle
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+1,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        //add uvs for upper right triangle
        uvData.Add(bottomUVs.topLeft);
        uvData.Add(bottomUVs.topRight);
        uvData.Add(bottomUVs.bottomRight);
        
        //add vertices for bottom left triangle
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j+1,k));   //bottom left
        
        //add uvs for bottom left triangle
        uvData.Add(bottomUVs.topLeft);
        uvData.Add(bottomUVs.bottomRight);
        uvData.Add(bottomUVs.bottomLeft);
    }

    
}

