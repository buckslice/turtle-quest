﻿Shader "Custom/RockShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

            CGPROGRAM
        #include "Assets/Shaders/Includes/Noise.cginc" 

            // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

            // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        struct Input {
            float3 objPos;
            float3 worldPos;
        };

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objPos = v.vertex;
        }

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
            UNITY_INSTANCING_BUFFER_END(Props)

            void surf(Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            //o.Albedo = _Color * worley(IN.objPos + float3(0,0,_Time.x), 4, 1, 0.5, 3.0,1.5,1.5);

            o.Albedo = _Color * (1.-(0.5+worley(IN.objPos + float3(0, _Time.x*.7,0), 2, 2, 0.5, 2.0, .5, 3.5)*.5));
            //o.Albedo *= worleyCell(IN.objPos*5. + float3(_Time.x*3,_Time.x*7,_Time.x*5));
            o.Albedo *= ripples(IN.worldPos);

            //float3 cell;
            //o.Albedo = _Color * worleyCell(IN.objPos + float3(0, 0, _Time.x), 4, 1, cell);
            //o.Albedo.r += cell.x;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
