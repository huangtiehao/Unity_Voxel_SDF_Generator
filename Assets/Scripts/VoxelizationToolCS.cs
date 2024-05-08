 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.IO;
 using System.Runtime.InteropServices;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Rendering;
 using VoxelSystem;
 
 public class VoxelizationTool : EditorWindow
 {
     // Start is called before the first frame update
     private ComputeShader voxelizer;
     public MeshFilter selectedMesh;
     public GameObject obj;
     private string savePath;
     private string loadPath;
     private Texture2D texture;
 
     [MenuItem("Tools/Voxelizer")]
     public static void ShowWindow()
     {
         EditorWindow.GetWindow(typeof(VoxelizationTool));
     }
     void OnGUI()
     {
         GUILayout.Label("Mesh Generator", EditorStyles.boldLabel);
         obj = EditorGUILayout.ObjectField("GameObject", obj, typeof(GameObject), true) as GameObject;
         selectedMesh = EditorGUILayout.ObjectField("Mesh Filter", selectedMesh, typeof(MeshFilter), true) as MeshFilter;
         savePath = EditorGUILayout.TextField("Save Path", savePath);
         texture = EditorGUILayout.ObjectField("Select Texture", texture, typeof(Texture2D), false) as Texture2D;
 
         if (GUILayout.Button("Save"))
         {
             generateVoxel();
         }
         if (GUILayout.Button("Load"))
         {
             loadPathVoxels();
         }
     }
 
     VoxelData Voxelize(ComputeShader voxelizer, Mesh mesh, int resolution = 32, bool volume = true)
     {

         mesh.RecalculateBounds();
         Bounds bounds = mesh.bounds;
         //顶点数据
         var vertices = mesh.vertices;
         var vertBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf(typeof(Vector3)));
         vertBuffer.SetData(vertices);
			
         //uv数据
         var uvBuffer = new ComputeBuffer(vertBuffer.count, Marshal.SizeOf(typeof(Vector2)));
         if(mesh.uv.Length > 0)
         {
             var uv = mesh.uv;
             uvBuffer.SetData(uv);
         }
			
         //三角形索引数据
         var triangles = mesh.triangles;
         var triBuffer = new ComputeBuffer(triangles.Length, Marshal.SizeOf(typeof(int)));
         triBuffer.SetData(triangles);
         int triangleIndexes = triBuffer.count / 3;
         //得到最长轴
         var maxLength = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
         var unit = maxLength / resolution;
         var hunit = unit * 0.5f;

         //扩展包围盒边界使得体素化的表面更加正确
         var start = bounds.min - new Vector3(hunit, hunit, hunit);
         var end = bounds.max + new Vector3(hunit, hunit, hunit);
         var size = end - start;

            
         int w, h, d;
         w = Mathf.CeilToInt (size.x / unit);
         h = Mathf.CeilToInt (size.y / unit);
         d = Mathf.CeilToInt (size.z / unit);
         
         var voxelBuffer = new ComputeBuffer(w * h * d, Marshal.SizeOf(typeof(Voxel)));
         var voxels = new Voxel[voxelBuffer.count];
         voxelBuffer.SetData(voxels); // initialize voxels explicitly
         
         voxelizer.SetVector("_Start",start);
         voxelizer.SetVector("_End",end);
         voxelizer.SetVector("_Size",size);
         
         voxelizer.SetFloat("_Unit",unit);
         voxelizer.SetFloat("_InvUnit",1f/unit);
         voxelizer.SetFloat("_HalfUnit",hunit);
         voxelizer.SetInt("_Width",w);
         voxelizer.SetInt("_Height",h);
         voxelizer.SetInt("_Depth",d);

         
         voxelizer.SetInt("_TrianglesCount",triBuffer.count);
         voxelizer.SetInt("_TriangleIndexes",triangleIndexes);

         int kernelID = voxelizer.FindKernel("CSMain");
         voxelizer.SetBuffer(kernelID,"_VertBuffer",vertBuffer);
         voxelizer.SetBuffer(kernelID,"_TriBuffer",triBuffer);
         voxelizer.SetBuffer(kernelID, "_UVBuffer",uvBuffer);
         voxelizer.SetBuffer(kernelID,"_VoxelBuffer",voxelBuffer);
         voxelizer.Dispatch(kernelID,triangleIndexes/64,1,1);
         
         voxelBuffer.GetData(voxels);
         voxelBuffer.Release();
         return new VoxelData(voxels, w, h, d, unit);
     }

     void loadVoxel(string path)
     {
         VoxelData voxelData=VoxelUtils.loadVoxelInfo(path);
         GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         // 设置立方体的位置
         cube.transform.position = new Vector3(0, 0, 0);
         cube.GetComponent<MeshFilter>().sharedMesh = VoxelMesh.Build(voxelData.GetData(), voxelData.unitLength, false);
     }
     void loadPathVoxels()
     {
         // 确保目录存在
         if (Directory.Exists(loadPath))
         {
             // 获取目录下的所有文件路径
             string[] fileEntries = Directory.GetFiles(loadPath);
             // 遍历所有文件
             foreach (string filePath in fileEntries)
             {
                 Debug.Log(filePath);
                 // 读取voxel
                 loadVoxel(filePath);
             }
         }
         else
         {
             Debug.LogError("Directory not found: " + loadPath);
         }
         
     }
     
     void generateVoxel()
     {
         voxelizer=Resources.Load("Shaders/VoxelizeCS") as ComputeShader;
         selectedMesh = obj.GetComponent<MeshFilter>();
         // Mesh mesh = selectedMesh.sharedMesh;
         Mesh mesh = Resources.Load("Meshes/mySphere") as Mesh;
         VoxelData voxelData = Voxelize(
             voxelizer, // ComputeShader (Voxelizer.compute)
             mesh, // a target mesh
             32, // # of voxels for largest AABB bounds
             false // flag to fill in volume or not; if set flag to false, sample a surface only
         );
         bool useUV = false;
         string MeshName = "mesh1";
         VoxelUtils.saveVoxelInfo(MeshName,voxelData);
 // build voxel cubes integrated mesh    
         //selectedMesh.sharedMesh = VoxelMesh.Build(voxelData.GetData(), voxelData.unitLength, useUV);
        
 // build 3D texture represent a volume by voxels.
         // RenderTexture volumeTexture = BuildTexture3D(
         //     voxelizer,
         //     data,
         //     texture, // Texture2D to color voxels based on uv coordinates in voxels
         //     RenderTextureFormat.ARGBFloat,
         //     FilterMode.Bilinear
         // );
 
         
     }
     // public static RenderTexture BuildTexture3D(ComputeShader voxelizer, VoxelData data, RenderTextureFormat format, FilterMode filterMode)
     // {
     //     return BuildTexture3D(voxelizer, data, Texture2D.whiteTexture, format, filterMode);
     // }
     //
     // public static RenderTexture BuildTexture3D(ComputeShader voxelizer, VoxelData data, Texture2D texture, RenderTextureFormat format, FilterMode filterMode)
     // {
     //     var volume = CreateTexture3D(data, format, filterMode);
     //
     //     int kernelID = voxelizer.FindKernel("BuildTexture3D");
     //     voxelizer.SetBuffer(kernelID, "_VoxelBuffer", data.Buffer);
     //     voxelizer.SetTexture(kernelID,"_VoxelTexture",volume);
     //     voxelizer.Dispatch(kernelID, data.Width/8+1,data.Height / 8+1, data.Depth / 8+1);
     //
     //     return volume;
     // }
     //
     // static RenderTexture CreateTexture3D(VoxelData data, RenderTextureFormat format, FilterMode filterMode)
     // {
     //     var texture = new RenderTexture(data.Width, data.Height, 0, format, RenderTextureReadWrite.Default);
     //     texture.dimension = TextureDimension.Tex3D;
     //     texture.volumeDepth = data.Depth;
     //     texture.enableRandomWrite = true;
     //     texture.filterMode = filterMode;
     //     texture.wrapMode = TextureWrapMode.Clamp;
     //     texture.Create();
     //
     //     return texture;
     // }
     
 }