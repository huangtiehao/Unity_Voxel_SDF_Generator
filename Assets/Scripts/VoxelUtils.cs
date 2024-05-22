
using System;
using System.IO;
using System.Text;
using UnityEngine;
using VoxelSystem;

public static class VoxelUtils
{
    public static VoxelData loadVoxelInfo(String fullPath)
    {
        if (File.Exists(fullPath))
        {

            // 读取文件的全部字节
            Byte[] data = File.ReadAllBytes(fullPath);
            Debug.Log("File read successfully. Number of bytes: " + data.Length);
            VoxelData voxelData = VoxelData.Deserialize(data);
            return voxelData;
        }
        else
        {
            Debug.LogError("File not found: " + fullPath);
        }

        return new VoxelData();
    }

    public static void saveVoxelInfo(String fullPath, VoxelData voxelData )
    {
        


        byte[] data = voxelData.Serialize();
        string directoryPath = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        // 写入文件
        File.WriteAllBytes(fullPath,data);
        
        Debug.Log("mesh save successfully. bytes: " + data.Length);
        // StringBuilder writeString=new StringBuilder();
        // writeString.Append(voxelData.Width+" "+voxelData.Height+" "+voxelData.Depth+" "+voxelData.UnitLength+"\n");
        //
        // var voxels = voxelData.GetData();
        // for (int i = 0; i < voxels.Length; ++i)
        // {
        //     Voxel voxel = voxels[i];
        //     if (voxel.fill == 0) continue;
        //     writeString.Append(voxel.worldPos[0] + " " + voxel.worldPos[1] +
        //                        " " + voxel.worldPos[2] + '\n');
        //     writeString.Append(voxel.voxelPos[0] + " " + voxel.voxelPos[1] +
        //                        " " + voxel.voxelPos[2] + '\n');
        //     writeString.Append(voxel.uv[0] + " " + voxels[i].uv[1] + "\n");
        //     writeString.Append(voxel.fill + "\n");
        // }
        // String fullPath = path + name;
        // if (!Directory.Exists(path))
        // {
        //     Directory.CreateDirectory(path);
        //     Debug.Log(fullPath);
        // }
        //
        //
        //
        // using (StreamWriter writer = new StreamWriter(fullPath))
        // {
        //     writer.Write(writeString);
        // }
        // Debug.Log("File written at " + fullPath);
    }
}
