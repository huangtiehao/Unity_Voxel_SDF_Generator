using System;
using Unity.Mathematics;
using UnityEngine;

    public class particleTest : MonoBehaviour
    {
        struct ParticleData {
            float3 pos;
            float4 color;
        };

        public ComputeShader computeShader;
        public Material material;
        private ComputeBuffer particleBuffer;
        private const int particleNum = 20000;
        private int kernelID;
        private void Start()
        {
            particleBuffer = new ComputeBuffer(particleNum,28);
            ParticleData[] particleDatas = new ParticleData[particleNum];
            particleBuffer.SetData(particleDatas);
            kernelID = computeShader.FindKernel("CSMain");
            
        }

        private void Update()
        {
            computeShader.SetBuffer(kernelID,"ParticleBuffers",particleBuffer);
            computeShader.SetFloat("Time",Time.time);
            computeShader.Dispatch(kernelID,particleNum/1000,1,1);
            material.SetBuffer("_particleDataBuffer", particleBuffer);
        }
        void OnRenderObject()
        {
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, particleNum);
        }
        void OnDestroy()
        {
            particleBuffer.Release();
        }
    }
    