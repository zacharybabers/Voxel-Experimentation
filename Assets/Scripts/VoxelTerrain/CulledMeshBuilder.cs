using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;



public class CulledMeshBuilder : MonoBehaviour
{
    

    public List<Vector3> vertexData;
    public List<int> triangleData;
 

    private float timer = 0f;
    
    
    public int length;
    public int width;
    public int height;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Mesh Build(ChunkData chunkData)
    {
        timer = Time.realtimeSinceStartup;
        
        int[,,] chunkArray = chunkData.voxelAtlas;

        length = chunkArray.GetLength(0);
        width = chunkArray.GetLength(1);
        height = chunkArray.GetLength(2);
        
        vertexData = new List<Vector3>();
        triangleData = new List<int>();
        
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

        mesh.triangles = triangleData.ToArray();
        
        mesh.RecalculateNormals();

        

        //Debug.Log(Time.realtimeSinceStartup - timer);
        
        return mesh;
    }

    private void CreateQuads(int[,,] chunkArray, int i, int j, int k)
    {
        
        if(i ==0 || (chunkArray[i-1, j, k] == 0))     //check back
        {
            CreateBackQuad(i, j, k);
        }
        
        if(i == length - 1 || (chunkArray[i+1, j, k] == 0))     //check front
        {
            CreateFrontQuad(i, j, k);
        }       
        
        if(j == 0 || (chunkArray[i, j-1, k] == 0))     //check left
        {
            CreateLeftQuad(i, j, k);
        }       
       
        if(j == width - 1 || (chunkArray[i, j+1, k] == 0))     //check right
        {
            CreateRightQuad(i, j, k);
        }       
        
        if( k == 0 || (chunkArray[i, j, k-1] == 0))     //check bottom
        {
            CreateBottomQuad(i, j, k);
        }       
        
        if(k == height - 1 || chunkArray[i, j, k+1] == 0)     //check top
        {
            CreateTopQuad(i, j, k);
        }       
    }
   
    private void CreateTopQuad(int i,int j,int k)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i + 1,j,k + 1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j + 1,k + 1);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j,k + 1));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i+1,j+1,k+ 1));   //bottom left
    }
    
    private void CreateLeftQuad(int i,int j,int k)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i+ 1,j,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j,k);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+1,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j,k+1));   //bottom left
    }
    private void CreateRightQuad(int i,int j,int k)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j+ 1,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j+ 1,k);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j+ 1,k));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i +1,j+ 1,k+1));   //bottom left
    }
    private void CreateFrontQuad(int i,int j,int k)
    {
       
        
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i+ 1,j+ 1,k+ 1);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j,k);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+ 1,j+1,k));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i+ 1,j,k+1));   //bottom left
    }
    private void CreateBackQuad(int i,int j,int k)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j,k+1);     //top left
        Vector3 createdVertex2 = new Vector3(i,j+1,k);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j+1,k+1));   //bottom left
    }
    private void CreateBottomQuad(int i,int j,int k)
    {
        for (int x = vertexData.Count; x < vertexData.Count + 6; x++)
        {
            triangleData.Add(x);
        }
        
        Vector3 createdVertex = new Vector3(i,j,k);     //top left
        Vector3 createdVertex2 = new Vector3(i+ 1,j+ 1,k);   //bottom right
        
        vertexData.Add(createdVertex);
        vertexData.Add(new Vector3(i+1,j,k));  //top right
        vertexData.Add(createdVertex2);
        
        vertexData.Add(createdVertex);
        vertexData.Add(createdVertex2);
        vertexData.Add(new Vector3(i,j+1,k));   //bottom left
    }

    
}
