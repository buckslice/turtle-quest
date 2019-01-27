Shader "Custom/KelpShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CULL OFF

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

            v.vertex = v.vertex + float4(1, 0, 0, 0) * sin(_Time.y*2.0 + v.vertex.x + v.vertex.z + v.vertex.y*2.0)*0.3;
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
            o.Albedo = _Color.rgb + ridged(IN.objPos + float3(0, 0, _Time.x), 2, 1, 0.5, 3.0)*.2;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
        FallBack "Diffuse"
}
