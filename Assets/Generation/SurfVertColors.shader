Shader "Custom/SurfVertColors" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
        Tags{"RenderType" = "Opaque" }
		
		CGPROGRAM
        #include "Assets/Shaders/Includes/Noise.cginc" 

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
            float3 vertexColor;
            float3 worldPos;
		};

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.vertexColor = v.color;    // done here so not per fragment
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo = c.rgb * IN.vertexColor;  // combine normal color with vertex color


            //float f = fbm(float3(IN.uv_MainTex, 0), 3, 5000, 0.5, 3.0)*0.00005;
            //o.Albedo = c.rgb * (0.5 + swag(fbm(float3(IN.worldPos, 0), 3, 20000., 0.5, 4.0))*0.5);

            float f = fbm(IN.worldPos, 3, .01, 0.5, 3.0)*20;
            o.Albedo = c.rgb * _Color.rgb * (0.7 + swag(fbm(IN.worldPos + f, 3, .5, 0.5, 4.0))*0.3);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
