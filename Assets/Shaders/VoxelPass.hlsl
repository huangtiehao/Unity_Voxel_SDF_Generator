#ifndef CUSTOM_VOXEL_PASS_INCLUDED
#define CUSTOM_VOXEL_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Voxel {
    float3 position;
    float2 uv;
    bool fill;
    bool front;
};

bool is_front_voxel(Voxel v)
{
    return v.fill && v.front;
}

bool is_back_voxel(Voxel v)
{
    return v.fill && !v.front;
}

bool is_empty_voxel(Voxel v)
{
    return !v.fill;
}
RWStructuredBuffer<Voxel>voxels;

int w,h,d;
float4x4 PX;
float4x4 PY;
float4x4 PZ;
float4x4 VX;
float4x4 VY;
float4x4 VZ;
float3 start;
float3 end;
float unit;
TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

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
    float4 positionCS :SV_POSITION;
    float3 positionWS : VAR_POSITION;
    float3 normalWS: VAR_NORMAL;
    float2 baseUV : VAR_BASE_UV;
};
int getIndex(int x,int y,int z)
{
    return x*h*d+y*d+z;
}
Attributes VoxelPassVertex(Attributes input)
{
     return input;
     // Varyings output;
     // output.positionWS.xyz=TransformObjectToWorld(input.positionOS);
     // output.positionCS=TransformWorldToHClip(output.positionWS);
     // float4 viewPos=mul(VX,float4(output.positionWS.xyz,1));
     // //float4 viewPos=float4(TransformWorldToView(output.positionWS),1.0);
     // //output.positionCS=TransformWViewToHClip(viewPos);
     // output.positionCS = mul(PX,viewPos); 
     // return output;
    
}

[maxvertexcount(3)]
void geo(triangle Attributes input[3], inout TriangleStream<Varyings> stream)
{
    stream.RestartStrip();
    float3 pos0WS = TransformObjectToWorld(input[0].positionOS);
    float3 pos1WS = TransformObjectToWorld(input[1].positionOS);
    float3 pos2WS = TransformObjectToWorld(input[2].positionOS);
    float3 p1 = pos1WS-pos0WS;
    float3 p2 = pos2WS - pos0WS;
    float3 faceNormal = normalize(cross(p1, p2));
    float LenX = abs(faceNormal.x);
    float LenY = abs(faceNormal.y);
    float LenZ = abs(faceNormal.z);
    float4x4 viewM;
    float4x4 pM;
    if( LenX > LenY && LenX > LenZ )
    {
        viewM=VX;
        pM=PX;
    }
    else if( LenY > LenX && LenY > LenZ  )
    {
        viewM=VY;
        pM=PY;
    }
    else
    {
        viewM=VZ;
        pM=PZ;
    }

    float4 viewPos;
    Varyings output;
    output.positionWS.xyz=TransformObjectToWorld(input[0].positionOS);
    output.normalWS=TransformObjectToWorldNormal(input[0].normalOS);
    viewPos=mul(viewM,float4(output.positionWS.xyz,1));
    output.positionCS=mul(pM,viewPos);
    //output.positionCS=TransformWorldToHClip(output.positionWS);
    output.baseUV=input[0].baseUV;
    stream.Append(output);
    
    output.positionWS.xyz=TransformObjectToWorld(input[1].positionOS);
    output.normalWS=TransformObjectToWorldNormal(input[1].normalOS);
    viewPos=mul(viewM,float4(output.positionWS.xyz,1));
    output.positionCS=mul(pM,viewPos);
    //output.positionCS=TransformWorldToHClip(output.positionWS);
    output.baseUV=input[1].baseUV;
    stream.Append(output);
    
    output.positionWS.xyz=TransformObjectToWorld(input[2].positionOS);
    output.normalWS=TransformObjectToWorldNormal(input[2].normalOS);
    viewPos=mul(viewM,float4(output.positionWS.xyz,1));
    output.positionCS=mul(pM,viewPos);
    //output.positionCS=TransformWorldToHClip(output.positionWS);
    output.baseUV=input[2].baseUV;
    stream.Append(output);
}



float4 VoxelPassFragment (Varyings input):SV_TARGET
{
    //input.positionCS=ComputeScreenPos(input.positionCS);
    int x,y,z;
    x=int((input.positionWS.x-start.x)/unit);
    y=int((input.positionWS.y-start.y)/unit);
    z=int((input.positionWS.z-start.z)/unit);
    int index=getIndex(x,y,z);
    voxels[index].fill=1;
    voxels[index].position=float3(start.x + unit * (x+0.5),
    start.y + unit * (y+0.5),
    start.z + unit * (z+0.5));
    //return float4(1,1,1,1.0);
    //return float4(VX[3][0],VX[3][1],VX[3][2],VX[3][3]);
    //input.positionCS =TransformWorldToHClip(input.positionWS);
    return float4(1.0f,1.0f,1.0f,1.0f);
}
#endif
