Shader "SDF/Visualize" {
  Properties {
    [Header(Optimization)]

    [Header(SDF)]
    [NoScaleOffset]
    _SDF ("SDF", 3D) = "white" {}

    [Header(Mesh)]
    _MinExtents("Minimum", Vector) = (-1, -1, -1)
    _MaxExtents("Maximum", Vector) = (1, 1, 1)
    _Center("Center", Vector) = (0,0,0)
  }
  SubShader {
    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
    LOD 100

    Blend SrcAlpha OneMinusSrcAlpha

    Cull Back

    Pass {
      CGPROGRAM
      
      #pragma vertex vert
      #pragma fragment frag
      // make fog work
      #pragma multi_compile_fog

      #pragma target 4.0

      #pragma shader_feature VERTEX_LIT

      #include "UnityCG.cginc"

      #define STEPS 64

      struct appdata {
        float4 vertex : POSITION;
      };

      struct v2f {
        float4 vertex : SV_POSITION;
        float3 localPos : TEXCOORD0;
      };

      sampler3D _SDF;
      float _Density;
      float3 _MinExtents;
      float3 _MaxExtents;
      float3 _Center;
      
      float3 Remap(float3 v, float3 fromMin, float3 fromMax, float3 toMin, float3 toMax) {
          return (v-fromMin)/(fromMax-fromMin)*(toMax-toMin)+toMin;
      }
      
      float3 LocalPosToUVW(float3 localPos) {
          return Remap(localPos-_Center, _MinExtents, _MaxExtents, 0, 1);
      }
      
      v2f vert (appdata v) {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.localPos = v.vertex;
          return o;
      }

      
      float4 frag (v2f i) : SV_Target {
          
          float3 localViewPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1.0)).xyz;
          float3 localViewDir = normalize(i.localPos - localViewPos);
          
          // This is the longest any ray throughout the mesh could be
          //float maxDist = length(_MaxExtents - _MinExtents) + 0.01;
          
          float3 position=i.localPos+0*localViewDir ;
          
          [unroll(STEPS)]
          for (uint j = 0; j < STEPS; j++) {

              float3 texcoord = LocalPosToUVW(position);
              float4 samp = tex3D(_SDF, texcoord);
              if (texcoord.x < 0.0 || texcoord.x > 1.0||texcoord.y < 0.0 || texcoord.y > 1.0||texcoord.z < 0.0 || texcoord.z > 1.0)
              {
                  return float4(0,0,0,1);
              }
              float sdf=samp.r;
              position+=sdf*localViewDir;
              if(abs(sdf)<0.01)
              {
                  return float4(1,1,1, 1);
              }
          }
          return float4(0,0,0, 1);
      }
  
      ENDCG
    }  // Pass
  }  // SubShader
}  // Shader
