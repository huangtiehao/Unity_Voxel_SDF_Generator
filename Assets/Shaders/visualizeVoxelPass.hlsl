#include <HLSLSupport.cginc>
#ifndef CUSTOM_VOXEL_PASS_INCLUDED
#define CUSTOM_VOXEL_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)

struct Attributes
{
    float4 positionOS : POSITION;
    float4 normalOS: NORMAL;
    float2 baseUV : TEXCOORD0;
};

struct Varyings
{
    float4 color;
    float4 positionCS :SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS: VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
};
Varyings VoxelPassVertex(Attributes input)
{
    
    Varyings output;
    output.positionWS.xyz=TransformObjectToWorld(input.positionOS);
    output.positionCS=TransformWorldToHClip(output.positionWS);
    output.normalWS=TransformObjectToWorldNormal(input.normalOS);
    output.baseUV=input.baseUV;

    float randomValue1 = frac(sin(dot(output.positionWS.xy, float2(12.9898, 78.233))) * 43758.5453);
    float randomValue2 = frac(sin(dot(output.positionWS.yz, float2(93.9898, 67.345))) * 24634.6345);
    float randomValue3 = frac(sin(dot(output.positionWS.xz, float2(43.3321, 12.4254))) * 56445.2345);
    output.color=float4(1,1,1,1);
        //float4 viewPos=mul(VX,float4(output.positionWS.xyz,1));
        ////float4 viewPos=float4(TransformWorldToView(output.positionWS),1.0);
        /////output.positionCS=TransformWViewToHClip(viewPos);
        /////output.positionCS = mul(PX,viewPos); 
     return output;
    
}



float4 VoxelPassFragment (Varyings input):SV_TARGET
{
    return input.color;
}
#endif
