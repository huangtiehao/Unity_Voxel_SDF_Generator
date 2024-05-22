 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.IO;
 using System.IO.Enumeration;
 using System.Runtime.InteropServices;
 using DefaultNamespace;
 using hth;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Rendering;
 using VoxelSystem;
 
 public class VoxelizationTool : EditorWindow
 {
     private string[] options = new string[] { "voxel", "sdf", "both" };
     private int selectedIndex = 0;
     
     private VoxelGenerator voxelGenerator=new VoxelGenerator();
     private SdfGenerator sdfGenerator=new SdfGenerator();
     // Start is called before the first frame update
     public Mesh mesh;
     private int resolution = 32;
     private float sdfPadding = 0;
     private string processPath="Assets/Res/VoxelSdfData/voxelProcess/";
     private string savePath="Assets/Res/VoxelSdfData/voxelSave/";
     private string loadPath="Assets/Res/VoxelSdfData/VoxelSave/";
     private Texture2D texture;
 
     [MenuItem("Tools/VoxelSdfGenerator")]
     public static void ShowWindow()
     {
         EditorWindow.GetWindow(typeof(VoxelizationTool));
     }

     void OnGUI()
     {
         GUILayout.Label("Select an Option", EditorStyles.boldLabel);
         // 使用Popup来创建下拉选择器
         selectedIndex = EditorGUILayout.Popup("Options", selectedIndex, options);
         GUILayout.Space(10);
         GUILayout.Label("Generator", EditorStyles.boldLabel);
         mesh = EditorGUILayout.ObjectField("Mesh", mesh, typeof(Mesh), false) as Mesh;
         resolution = EditorGUILayout.IntField("resolution", resolution);
         if (selectedIndex != 0)
         {
             sdfPadding = EditorGUILayout.FloatField("sdf padding", sdfPadding);
             sdfGenerator.Padding = sdfPadding;
         }
        
         processPath = EditorGUILayout.TextField("process Path", processPath);
         if (GUILayout.Button("select process Path"))
         {
             processPath = EditorUtility.OpenFolderPanel("select folder", "", "");
             processPath = "Assets" + processPath.Substring(Application.dataPath.Length);;
         }
         savePath = EditorGUILayout.TextField("Save Path", savePath);
         if (GUILayout.Button("select save Path"))
         {
             savePath = EditorUtility.OpenFolderPanel("select folder", "", "");
             savePath = "Assets" + savePath.Substring(Application.dataPath.Length);;
         }

         voxelGenerator.Mesh = mesh;
         voxelGenerator.VoxelResolution = resolution;
         sdfGenerator.Mesh = mesh;
         sdfGenerator.SdfResolution = resolution;
         if (GUILayout.Button("save"))
         {
             generateVoxelSdf_Path();
         }


         if (selectedIndex == 0 || selectedIndex == 2)
         {
             loadPath = EditorGUILayout.TextField("Load Path", loadPath);
             if (GUILayout.Button("Load"))
             {
                 loadVoxels_Path();
             }
         }

         if (GUILayout.Button("Save this mesh"))
         {
             if (selectedIndex == 0 || selectedIndex == 2) saveThisMeshVoxel();
             if (selectedIndex == 1 || selectedIndex == 2) saveThisMeshSdf();
         }
         


     }

     private void OnInspectorUpdate()
     {
         Repaint();
     }
     void saveThisMeshVoxel()
     {
         voxelGenerator.Mesh = mesh;
         // Prompt the user to save the file.
         string path = EditorUtility.SaveFilePanelInProject("Save As", mesh.name + "_Voxel","", "");
         
         VoxelData voxelData = voxelGenerator.generate();
         VoxelUtils.saveVoxelInfo(path, voxelData);
         //VoxelUtils.saveVoxelInfo(savePath+mesh.name,voxelData);
         
     }
     
     void saveThisMeshSdf()
     {
         string path = EditorUtility.SaveFilePanelInProject("Save As", mesh.name + "_Sdf", "asset", "");
         if (path == null || path == "")
         {
             return;
         }
         Texture3D sdfData = sdfGenerator.generate();
         // Prompt the user to save the file.
         AssetDatabase.CreateAsset(sdfData,path);
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
         // Select the SDF in the project view.
         Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
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

     void loadVoxels_Path()
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

                 if (filePath.Length >= 5 && filePath.Substring(filePath.Length - 6, 6) == ".voxel")
                 {
                     // 读取voxel
                     loadVoxel(filePath, ++cnt);
                 }
             }
         }
         else

         {
             Debug.LogError("Directory not found: " + loadPath);
         }
     }
     



     void generateVoxelSdf_Path()
     {
         string fullPath = processPath;
         // 确保目录存在
         if (Directory.Exists(fullPath)) {
             // 获取目录下的所有文件路径
             string[] fileEntries = Directory.GetFiles(fullPath);
             // 遍历所有文件
             int cnt = 0;
             foreach (string filePath in fileEntries) {
                 if(filePath.Substring(filePath.Length-5,5)==".meta")continue;
                 string fileName = Path.GetFileName(filePath);
                 string filenameWithoutExt=Path.GetFileNameWithoutExtension(filePath);
                 mesh = AssetDatabase.LoadAssetAtPath<Mesh>(fullPath + "/" + fileName);
                 if(selectedIndex==0||selectedIndex==2)generateVoxel(filenameWithoutExt);
                 if(selectedIndex==1||selectedIndex==2)generateSdf(filenameWithoutExt);
                 
             }
         }
         else {
             Debug.LogError("Directory not found: " + fullPath);
         }
     }
     void generateSdf(String fileName)
     {
         
         if (mesh != null)
         {
             sdfGenerator.Mesh = mesh;
             Texture3D sdfData = sdfGenerator.generate();
             // Prompt the user to save the file.
             AssetDatabase.CreateAsset(sdfData,savePath+fileName+".asset");
             AssetDatabase.SaveAssets();
             AssetDatabase.Refresh();
             Debug.Log("generate "+savePath+"/"+fileName+" sdf success");
         }
         else
         {
             Debug.Log(processPath + "/" + fileName + " generateSdf failed");
         }
     }
     void generateVoxel(String fileName)
     {

         if (mesh!=null)
         {
             voxelGenerator.Mesh = mesh;
             VoxelData voxelData = voxelGenerator.generate();
             VoxelUtils.saveVoxelInfo(savePath+fileName+".voxel", voxelData);
         }
         else
         {
             Debug.Log(processPath+"/"+fileName+" generateVoxel failed");
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