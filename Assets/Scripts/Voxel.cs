using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelSystem
{

    [StructLayout(LayoutKind.Sequential)]
    public struct Voxel
    {
        public Vector3 worldPos;
        public uint fill_VoxelPos;//存储了fill，在高位的第八个位置，后面三个字节每个字节分别存储voxelPos的xyz
        public Vector2 uv;

        public bool IsFill()
        {
            return (((fill_VoxelPos >> 24) & 1) == 1);//右移24位将fill移到最低位
        }
    }

}