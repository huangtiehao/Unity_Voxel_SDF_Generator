using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using VoxelSystem;

namespace hth
{   
    
    public class VoxelGenerator
    {
        private int voxelResolution;
        
        public int VoxelResolution {
            get { return voxelResolution; }
            set { voxelResolution = value; }
        }
        private Mesh mesh;
        public Mesh Mesh {
            get { return mesh; }
            set { mesh = value; }
        }
        
        public VoxelData generate()
        {
             ComputeShader computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>("Packages/com.funplus.xrender/Shaders/VoxelSdfGen/GenVoxel.compute");
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
             var unit = maxLength / voxelResolution;
             var hunit = unit * 0.5f;

             //扩展包围盒边界使得体素化的表面更加正确
             var start = bounds.min - new Vector3(hunit, hunit, hunit);
             var end = bounds.max + new Vector3(hunit, hunit, hunit);
             var size = end - start;
                
             int w, h, d;
             w = Mathf.CeilToInt (size.x / unit);
             h = Mathf.CeilToInt (size.y / unit);
             d = Mathf.CeilToInt (size.z / unit);
             Debug.Log(w+" "+h+" "+d);
             var voxelBuffer = new ComputeBuffer(w * h * d, Marshal.SizeOf(typeof(Voxel)));
             var voxels = new Voxel[voxelBuffer.count];
             Debug.Log(w+" "+h+" "+d);
             voxelBuffer.SetData(voxels); // initialize voxels explicitly
             
             computeShader.SetVector("_Start",start);
             computeShader.SetVector("_End",end);
             computeShader.SetVector("_Size",size);
             
             computeShader.SetFloat("_Unit",unit);
             computeShader.SetFloat("_InvUnit",1f/unit);
             computeShader.SetFloat("_HalfUnit",hunit);
             computeShader.SetInt("_Width",w);
             computeShader.SetInt("_Height",h);
             computeShader.SetInt("_Depth",d);

             
             computeShader.SetInt("_TrianglesCount",triBuffer.count);
             computeShader.SetInt("_TriangleIndexes",triangleIndexes);

             int kernelID = computeShader.FindKernel("CSMain");
             computeShader.SetBuffer(kernelID,"_VertBuffer",vertBuffer);
             computeShader.SetBuffer(kernelID,"_TriBuffer",triBuffer);
             computeShader.SetBuffer(kernelID, "_UVBuffer",uvBuffer);
             computeShader.SetBuffer(kernelID,"_VoxelBuffer",voxelBuffer);
             computeShader.Dispatch(kernelID,triangleIndexes/64+1,1,1);
             
             voxelBuffer.GetData(voxels);
             int fillVoxels = 0;
             for (int i = 0; i < voxels.Length; ++i)
             {
                 if (voxels[i].IsFill())
                 {
                     voxels[fillVoxels++] = voxels[i];
                 }
             }
             Debug.Log(fillVoxels);
             vertBuffer.Release();
             uvBuffer.Release();
             triBuffer.Release();
             voxelBuffer.Release();
             return new VoxelData(voxels, w, h, d, unit,fillVoxels);
        }
    }
}