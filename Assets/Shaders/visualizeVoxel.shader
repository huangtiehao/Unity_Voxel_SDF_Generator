Shader "GI/visualizeVoxel"
{

    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("BaseColor",Color)=(0.5,1,1.0,1.0)
    }
    SubShader
    {
        Pass
        {

            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex VoxelPassVertex
            #pragma fragment VoxelPassFragment
            #include "visualizeVoxelPass.hlsl"
            #pragma enable_d3d11_debug_symbols
            ENDHLSL
        }
    }
}
