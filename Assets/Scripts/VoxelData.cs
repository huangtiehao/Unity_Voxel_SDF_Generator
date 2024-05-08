using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelSystem {

    public class VoxelData {
        

        public int width, height, depth;
        public float unitLength;
        
        public Voxel[] voxels;

        int getIndex(int x, int y, int z)
        {
            return x * height * depth + y * depth + z;
        }

        public VoxelData()
        {
            
        }
        public VoxelData(Voxel[] voxelArray,int w, int h, int d, float u) {
            width = w;
            height = h;
            depth = d;
            unitLength = u;
            voxels = new Voxel[w*h*d];
            Debug.Log(w+" "+h+" "+d);
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    for (int k = 0; k < d; ++k)
                    {
                        int index = getIndex(i, j, k);
                        voxels[index].worldPos = voxelArray[index].worldPos;
                        voxels[index].voxelPos = voxelArray[index].voxelPos;
                        voxels[index].uv = voxelArray[index].uv;
                        voxels[index].fill = voxelArray[index].fill;
                        voxels[index].front = voxelArray[index].front;
                    }
                }
            }
            
        }

        public Byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            // 将字段写入二进制流中
            writer.Write(width);
            writer.Write(height);
            writer.Write(depth);
            writer.Write(unitLength);
            for (int i = 0; i < voxels.Length; ++i)
            {
                var voxel = voxels[i];
                writer.Write(voxel.worldPos[0]);
                writer.Write(voxel.worldPos[1]);
                writer.Write(voxel.worldPos[2]);
                writer.Write(voxel.voxelPos[0]);
                writer.Write(voxel.voxelPos[1]);
                writer.Write(voxel.voxelPos[2]);
                writer.Write(voxel.uv[0]);
                writer.Write(voxel.uv[1]);
                writer.Write(voxel.fill);
                writer.Write(voxel.front);
            }
            writer.Close();
            return stream.ToArray();

        }
        
        public static VoxelData Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryReader reader = new BinaryReader(stream);
                
            VoxelData voxelData=new VoxelData();   
            // 从二进制流中读取字段
            voxelData.width = reader.ReadInt32();
            voxelData.height= reader.ReadInt32();
            voxelData.depth= reader.ReadInt32();
            voxelData.unitLength = reader.ReadSingle();

            int len = voxelData.width * voxelData.height * voxelData.depth;
            voxelData.voxels = new Voxel[len];
            for (int i = 0; i < len; ++i)
            {
                voxelData.voxels[i].worldPos[0] = reader.ReadSingle();
                voxelData.voxels[i].worldPos[1] = reader.ReadSingle();
                voxelData.voxels[i].worldPos[2] = reader.ReadSingle();
                voxelData.voxels[i].voxelPos[0] = reader.ReadInt32();
                voxelData.voxels[i].voxelPos[1] = reader.ReadInt32();
                voxelData.voxels[i].voxelPos[2] = reader.ReadInt32();
                voxelData.voxels[i].uv[0] = reader.ReadSingle();
                voxelData.voxels[i].uv[1] = reader.ReadSingle();
                voxelData.voxels[i].fill = reader.ReadUInt32();
                voxelData.voxels[i].front = reader.ReadUInt32();
            }

            reader.Close();
            return voxelData;
        }
        
        public Voxel[] GetData() {
            return voxels;
        }

    }

}