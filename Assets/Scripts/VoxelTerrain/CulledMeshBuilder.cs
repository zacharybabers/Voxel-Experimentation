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


    

    public Mesh Build(ChunkData chunkData)
    {
        timer = Time.realtimeSinceStartup;
        
        int[,,] chunkArray = chunkData.voxelAtlas;

        length = chunkArray.GetLength(0);
        width = chunkArray.GetLength(1);
        height = chunkArray.GetLength(2);
        
        vertexData = new List<Vector3>();
        triangleData = new List<int>();
        uvData = new List<Vector2>();
        
        
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < height; k++)
                {
                    if (chunkArray[i, j, k] != 0)
                    {
                        CreateQuads(chunkArray, i, j, k);
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

        

        //Debug.Log(Time.realtimeSinceStartup - timer);
        
        return mesh;
    }

    private void CreateQuads(int[,,] chunkArray, int i, int j, int k)
    {
        int value = chunkArray[i, j, k];
        
        if(i ==0 || (chunkArray[i-1, j, k] == 0))     //check back
        {
            CreateBackQuad(i, j, k, ref uvLookup[value].sideUVs);
        }
        
        if(i == length - 1 || (chunkArray[i+1, j, k] == 0))     //check front
        {
            CreateFrontQuad(i, j, k, ref uvLookup[value].sideUVs);
        }       
        
        if(j == 0 || (chunkArray[i, j-1, k] == 0))     //check left
        {
            CreateLeftQuad(i, j, k, ref uvLookup[value].sideUVs);
        }       
       
        if(j == width - 1 || (chunkArray[i, j+1, k] == 0))     //check right
        {
            CreateRightQuad(i, j, k, ref uvLookup[value].sideUVs);
        }       
        
        if( k == 0 || (chunkArray[i, j, k-1] == 0))     //check bottom
        {
            CreateBottomQuad(i, j, k, ref uvLookup[value].bottomUVs);
        }       
        
        if(k == height - 1 || chunkArray[i, j, k+1] == 0)     //check top
        {
            CreateTopQuad(i, j, k, ref uvLookup[value].topUVs);
        }       
    }
   
    private void CreateTopQuad(int i,int j,int k, ref QuadUVs topUVs)
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
    
    private void CreateLeftQuad(int i,int j,int k, ref QuadUVs sideUVs)
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
    private void CreateRightQuad(int i,int j,int k, ref QuadUVs sideUVs)
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
    private void CreateFrontQuad(int i,int j,int k, ref QuadUVs sideUVs)
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
    private void CreateBackQuad(int i,int j,int k, ref QuadUVs sideUVs)
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
    private void CreateBottomQuad(int i,int j,int k, ref QuadUVs bottomUVs)
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
