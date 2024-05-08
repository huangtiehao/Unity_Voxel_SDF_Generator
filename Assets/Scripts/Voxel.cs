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
        public int3 voxelPos;
        public Vector2 uv;
        public uint fill;
        public uint front;

        public bool IsFrontFace()
        {
            return fill > 0 && front > 0;
        }

        public bool IsBackFace()
        {
            return fill > 0 && front < 1;
        }

        public bool IsEmpty()
        {
            return fill < 1;
        }
    }

}