Shader "Custom/Shader"
{
    Properties
    {
        _WaterTex("Water Texture", 2D) = "white" {}
        _SandTex("Sand Texture", 2D) = "white" {}
        _GrassTex("Grass Texture", 2D) = "white" {}
        _RockTex("Rock Texture", 2D) = "white" {}
        _SnowTex("Snow Texture", 2D) = "white" {}
        
        _WaterHeight("Water Height", Range(0,1)) = 0.0
        _SandHeight("Sand Height", Range(0,1)) = 0.3
        _GrassHeight("Grass Height", Range(0,1)) = 0.6
        _RockHeight("Rock Height", Range(0,1)) = 0.8
        _SnowHeight("Snow Height", Range(0,1)) = 1.0

        _MapHeight("Map Height", Float) = 50.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1; 
            };

            TEXTURE2D(_WaterTex); 
            SAMPLER(sampler_WaterTex);
            
            TEXTURE2D(_SandTex); 
            SAMPLER(sampler_SandTex);

            TEXTURE2D(_GrassTex); 
            SAMPLER(sampler_GrassTex);

            TEXTURE2D(_RockTex); 
            SAMPLER(sampler_RockTex);

            TEXTURE2D(_SnowTex); 
            SAMPLER(sampler_SnowTex);

            // Heights
            CBUFFER_START(UnityPerMaterial)
                float _WaterHeight;
                float _SandHeight;
                float _GrassHeight;
                float _RockHeight;
                float _SnowHeight;
                float _MapHeight;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;

            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Normalize height based on Y position
                float height = saturate(IN.worldPos.y / _MapHeight); 

                // Sample each texture
                half4 water = SAMPLE_TEXTURE2D(_WaterTex, sampler_WaterTex, IN.uv);
                half4 sand = SAMPLE_TEXTURE2D(_SandTex, sampler_SandTex, IN.uv);
                half4 grass = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, IN.uv);
                half4 rock = SAMPLE_TEXTURE2D(_RockTex, sampler_RockTex, IN.uv);
                half4 snow = SAMPLE_TEXTURE2D(_SnowTex, sampler_SnowTex, IN.uv);

                // Blend based on height thresholds
                half4 colour;

                if (height > _RockHeight)
                {
                    float t = smoothstep(_RockHeight, _SnowHeight, height);
                    colour = lerp(rock, snow, t);
                }
                else if (height > _GrassHeight)
                {
                    float t = smoothstep(_GrassHeight, _RockHeight, height);
                    colour = lerp(grass, rock, t);
                }
                else if (height > _SandHeight)
                {
                    float t = smoothstep(_SandHeight, _GrassHeight, height);
                    colour = lerp(sand, grass, t);
                }
                else if (height > _WaterHeight)
                {
                    float t = smoothstep(_WaterHeight, _SandHeight, height);
                    colour = lerp(water, sand, t);
                }

                return colour;
            }
            ENDHLSL
        }
    }
}
