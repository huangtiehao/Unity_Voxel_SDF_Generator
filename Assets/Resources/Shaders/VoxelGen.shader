Shader "GI/Voxelizer"
{

    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("BaseColor",Color)=(0.5,0.5,1.0,1.0)
    }
    SubShader
    {
        ZTest Always
        ZWrite off
        Cull off
        Pass
        {

            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex VoxelPassVertex
            #pragma geometry geo
            #pragma fragment VoxelPassFragment
            #include "VoxelPass.hlsl"
            #pragma enable_d3d11_debug_symbols
            ENDHLSL
        }
    }
}
