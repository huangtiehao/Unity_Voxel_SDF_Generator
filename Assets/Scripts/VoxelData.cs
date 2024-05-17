using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelSystem {

    [Serializable]
    public class VoxelData : ScriptableObject
    {
        
        public int width, height, depth;
        public float unitLength;
        public int arrayLength;
        public Voxel[] voxels;

        int getIndex(int x, int y, int z)
        {
            return x * height * depth + y * depth + z;
        }

        public VoxelData()
        {
            
        }
        public VoxelData(Voxel[] voxelArray,int w, int h, int d, float u,int arrayLen) {
            width = w;
            height = h;
            depth = d;
            unitLength = u;
            arrayLength = arrayLen;
            voxels = new Voxel[arrayLength];
            Debug.Log(arrayLen);
            for (int i = 0; i < arrayLength; ++i)
            {
                voxels[i] = voxelArray[i];
            }
            
        }

        public Byte[] Serialize()
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            // 将字段写入二进制流中
            writer.Write(arrayLength);
            writer.Write(width);
            writer.Write(height);
            writer.Write(depth);
            writer.Write(unitLength);
            for (int i = 0; i < arrayLength; ++i)
            {
                var voxel = voxels[i];
                writer.Write(voxel.fill_VoxelPos);
                writer.Write(voxel.worldPos[0]);
                writer.Write(voxel.worldPos[1]);
                writer.Write(voxel.worldPos[2]);
                writer.Write(voxel.uv[0]);
                writer.Write(voxel.uv[1]);

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
            voxelData.arrayLength = reader.ReadInt32();
            voxelData.width = reader.ReadInt32();
            voxelData.height= reader.ReadInt32();
            voxelData.depth= reader.ReadInt32();
            voxelData.unitLength = reader.ReadSingle();

            voxelData.voxels = new Voxel[voxelData.arrayLength];
            for (int i = 0; i < voxelData.arrayLength; ++i)
            {
                voxelData.voxels[i].fill_VoxelPos = reader.ReadUInt32();
                voxelData.voxels[i].worldPos[0] = reader.ReadSingle();
                voxelData.voxels[i].worldPos[1] = reader.ReadSingle();
                voxelData.voxels[i].worldPos[2] = reader.ReadSingle();
                voxelData.voxels[i].uv[0] = reader.ReadSingle();
                voxelData.voxels[i].uv[1] = reader.ReadSingle();
            }

            reader.Close();
            return voxelData;
        }
        
        public Voxel[] GetData() {
            return voxels;
        }

    }

}