using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TestCube : MonoBehaviour
{
    public ComputeShader computeShader;
    public Material material;
    // Start is called before the first frame update
    void Start()
    {
        RenderTexture mRenderTexture = new RenderTexture(256, 256, 16);
        mRenderTexture.enableRandomWrite=true;
        mRenderTexture.Create();
        material.mainTexture = mRenderTexture;
        int kernelIndex = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelIndex,"Result",mRenderTexture);
        computeShader.Dispatch(kernelIndex, 256 / 8, 256 / 8, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
