Shader "EAUploader/AlwaysShow" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+2" }
        LOD 200

        ZTest Always

        CGPROGRAM
        #pragma surface surf Lambert alpha

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutput o) {
            fixed4 c = _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Emission = c.rgb;
        }
        ENDCG
    } 
    FallBack "Diffuse"
}
