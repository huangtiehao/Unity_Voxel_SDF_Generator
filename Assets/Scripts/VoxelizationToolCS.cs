 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.IO;
 using System.IO.Enumeration;
 using System.Runtime.InteropServices;
 using Unity.VisualScripting;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Rendering;
 using VoxelSystem;
 
 public class VoxelizationTool : EditorWindow
 {
     // Start is called before the first frame update
     private ComputeShader voxelizer;
     private MeshFilter selectedMesh;
     private GameObject obj;
     private string voxelizePath="Meshes";//只能voxelize Resources下的目录
     private string savePath="Assets/Resources/VoxelInfo/";
     private string voxelizeSavePath="Assets/Resources/VoxelInfo/";
     private string loadPath="Assets/Resources/VoxelInfo/";
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
         voxelizePath = EditorGUILayout.TextField("Voxelize Path", voxelizePath);
         voxelizeSavePath = EditorGUILayout.TextField("Voxelize Save Path", voxelizeSavePath);
         if (GUILayout.Button("Voxelize"))
         {
             generateVoxels_Path();
         }
         loadPath = EditorGUILayout.TextField("Load Path", loadPath);
         if (GUILayout.Button("Load"))
         {
             loadPathVoxels();
         }
         savePath = EditorGUILayout.TextField("Save Path", savePath);
         if (GUILayout.Button("Save this mesh"))
         {
             saveThisMesh();
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
         Debug.Log(w+" "+h+" "+d);
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
         voxelizer.Dispatch(kernelID,triangleIndexes/64+1,1,1);
         
         voxelBuffer.GetData(voxels);
         int fillVoxels = 0;
         for (int i = 0; i < voxels.Length; ++i)
         {
             if (voxels[i].IsFill())
             {
                 voxels[fillVoxels++] = voxels[i];
             }
         }
         vertBuffer.Release();
         uvBuffer.Release();
         triBuffer.Release();
         voxelBuffer.Release();
         return new VoxelData(voxels, w, h, d, unit,fillVoxels);
     }
     void saveThisMesh()
     {
         voxelizer=Resources.Load("Shaders/VoxelizeCS") as ComputeShader;
         selectedMesh = obj.GetComponent<MeshFilter>();
         Mesh mesh = selectedMesh.sharedMesh;
         VoxelData voxelData = Voxelize(
             voxelizer, // ComputeShader (Voxelizer.compute)
             mesh, // a target mesh
             32, // # of voxels for largest AABB bounds
             false // flag to fill in volume or not; if set flag to false, sample a surface only
         );
         bool useUV = false;
         
         Debug.Log(savePath);
         VoxelUtils.saveVoxelInfo(savePath+obj.name,voxelData);
     }
     void loadVoxel(string path,int num)
     {
         VoxelData voxelData=VoxelUtils.loadVoxelInfo(path);
         GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         // 设置立方体的位置
         cube.transform.position = new Vector3(2*num, 0, 0);
         cube.name = "Voxel" + num;
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
             int cnt = 0;
             foreach (string filePath in fileEntries)
             {
                 
                 if (filePath.Length>=5&&filePath.Substring(filePath.Length - 5, 5) == ".meta") continue;
                 // 读取voxel
                 loadVoxel(filePath, ++cnt);
             }
         }
         else

         {
             Debug.LogError("Directory not found: " + loadPath);


         }
     }

     void generateVoxels_Path()
     {
         string fullPath = "Assets/Resources/" + voxelizePath;
         // 确保目录存在
         if (Directory.Exists(fullPath)) {
             // 获取目录下的所有文件路径
             string[] fileEntries = Directory.GetFiles(fullPath);
             // 遍历所有文件
             int cnt = 0;
             foreach (string filePath in fileEntries) {
                 if(filePath.Substring(filePath.Length-5,5)==".meta")continue;
                 string fileName = Path.GetFileNameWithoutExtension(filePath);
                 generateVoxel(fileName);

             }
         }
         else {
             Debug.LogError("Directory not found: " + fullPath);
         }
     }
     void generateVoxel(String fileName)
     {
         //shader路径
         voxelizer = Resources.Load("Shaders/VoxelizeCS") as ComputeShader;
         //只能加载Resource下的资源
         Mesh mesh = Resources.Load(voxelizePath+"/"+fileName) as Mesh;
         if (mesh!=null)
         {
             VoxelData voxelData = Voxelize(
                 voxelizer, // ComputeShader (Voxelizer.compute)
                 mesh, // a target mesh
                 32, // # of voxels for largest AABB bounds
                 false // flag to fill in volume or not; if set flag to false, sample a surface only
             );
             bool useUV = false;
             VoxelUtils.saveVoxelInfo(voxelizeSavePath+fileName, voxelData);
         }
         else
         {
             Debug.Log(voxelizePath+"/"+fileName+" generateVoxel failed");
         }


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