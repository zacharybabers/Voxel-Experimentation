using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRender : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Vector3> vertices;
    public List<int> triangles;

    [SerializeField] private MeshFilter meshFilter;
    
    
    
    
    void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        BuildCube();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildCube()
    {
      BuildTopQuad();
      BuildLeftQuad();
      BuildBottomQuad();
      BuildBackQuad();
      BuildFrontQuad();
      BuildRightQuad();
      
      for (int i = 0; i < vertices.Count; i++)
      {
          Vector3 temp = vertices[i];
          Vector3 reordered = new Vector3(temp.y, temp.z, temp.x);
          vertices[i] = reordered;
      }
      
      Mesh mesh = new Mesh();
      mesh.vertices = vertices.ToArray();

      mesh.triangles = triangles.ToArray();
      
      mesh.RecalculateNormals();

      meshFilter.mesh = mesh;
    }
    

     void BuildTopQuad()
    {
        Vector3 createdVertex = new Vector3(1, 0, 1); //i2
        
        vertices.Add(createdVertex);

        int index = vertices.LastIndexOf(createdVertex);
        
        triangles.Add(index);
        triangles.Add(index +1);
        triangles.Add(index + 2);
        
        
        vertices.Add(new Vector3(1,1,1)); //i11
        vertices.Add(new Vector3(0,1,1)); //i3

        triangles.Add(index + 3);
        triangles.Add(index + 4);
        triangles.Add(index + 5);

        vertices.Add(createdVertex);
        vertices.Add(new Vector3(0,1,1)); //i3
        vertices.Add(new Vector3(0,0,1)); //i12
    }

     void BuildLeftQuad()
     {
         Vector3 createdVertex = new Vector3(1, 0, 1);
        
         vertices.Add(createdVertex);

         int index = vertices.LastIndexOf(createdVertex);
        
         triangles.Add(index);
         triangles.Add(index +1);
         triangles.Add(index + 2);
        
        
         vertices.Add(new Vector3(0,0,1));
         vertices.Add(new Vector3(0,0,0));

         triangles.Add(index + 3);
         triangles.Add(index + 4);
         triangles.Add(index + 5);

         vertices.Add(createdVertex);
         vertices.Add(new Vector3(0,0,0));
         vertices.Add(new Vector3(1,0,0));
     }

     private void BuildBottomQuad()
     {
         Vector3 createdVertex = new Vector3(0, 0, 0);
        
         vertices.Add(createdVertex);

         int index = vertices.LastIndexOf(createdVertex);
        
         triangles.Add(index);
         triangles.Add(index +1);
         triangles.Add(index + 2);
        
        
         vertices.Add(new Vector3(0,1,0));
         vertices.Add(new Vector3(1,1,0));

         triangles.Add(index + 3);
         triangles.Add(index + 4);
         triangles.Add(index + 5);

         vertices.Add(createdVertex);
         vertices.Add(new Vector3(1,1,0));
         vertices.Add(new Vector3(1,0,0));
     }

     private void BuildBackQuad()
     {
         Vector3 createdVertex = new Vector3(0, 0, 1);
        
         vertices.Add(createdVertex);

         int index = vertices.LastIndexOf(createdVertex);
        
         triangles.Add(index);
         triangles.Add(index +1);
         triangles.Add(index + 2);
        
        
         vertices.Add(new Vector3(0,1,1));
         vertices.Add(new Vector3(0,1,0));

         triangles.Add(index + 3);
         triangles.Add(index + 4);
         triangles.Add(index + 5);

         vertices.Add(createdVertex);
         vertices.Add(new Vector3(0,1,0));
         vertices.Add(new Vector3(0,0,0));
     }

     private void BuildFrontQuad()
     {
         Vector3 createdVertex = new Vector3(1, 1, 1);
        
         vertices.Add(createdVertex);

         int index = vertices.LastIndexOf(createdVertex);
        
         triangles.Add(index);
         triangles.Add(index +1);
         triangles.Add(index + 2);
        
        
         vertices.Add(new Vector3(1,0,1));
         vertices.Add(new Vector3(1,0,0));

         triangles.Add(index + 3);
         triangles.Add(index + 4);
         triangles.Add(index + 5);

         vertices.Add(createdVertex);
         vertices.Add(new Vector3(1,0,0));
         vertices.Add(new Vector3(1,1,0));
     }

     private void BuildRightQuad()
     {
         Vector3 createdVertex = new Vector3(0, 1, 1);
        
         vertices.Add(createdVertex);

         int index = vertices.LastIndexOf(createdVertex);
        
         triangles.Add(index);
         triangles.Add(index +1);
         triangles.Add(index + 2);
        
        
         vertices.Add(new Vector3(1,1,1));
         vertices.Add(new Vector3(1,1,0));

         triangles.Add(index + 3);
         triangles.Add(index + 4);
         triangles.Add(index + 5);

         vertices.Add(createdVertex);
         vertices.Add(new Vector3(1,1,0));
         vertices.Add(new Vector3(0,1,0));
     }

   
        
    
}
