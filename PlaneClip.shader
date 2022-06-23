Shader "PlaneClip"{
    //show values to edit in inspector
    Properties{
        _Color("Tint", Color) = (0, 0, 0, 1)
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metalness", Range(0, 1)) = 0
        [HDR]_Emission("Emission", color) = (0,0,0)

        //[HDR]_CutoffColor("Cutoff Color", Color) = (0,0,0,0)
    }

        SubShader{
            //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
            //Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
            Tags{"RenderType"="Opaque" "Queue"="Geometry"}
            LOD 200
            // render faces regardless if they point towards the camera or away from it
            //Cull Off

            CGPROGRAM
            //the shader is a surface shader, meaning that it will be extended by unity in the background 
            //to have fancy lighting and other features
            //our surface shader function is called surf and we use our custom lighting model
            //fullforwardshadows makes sure unity adds the shadow passes the shader might need
            //vertex:vert makes the shader use vert as a vertex shader function
            #pragma surface surf Standard addshadow
            #pragma target 3.0

            sampler2D _MainTex;
            fixed4 _Color;
            half _Smoothness;
            half _Metallic;
            half3 _Emission;

            int _PlaneCount;
            float4 _Planes[10];

            //float4 _CutoffColor;

            //input struct which is automatically filled by unity
            struct Input {
                float2 uv_MainTex;
                float3 worldPos;
                float facing : VFACE;
            };

            //the surface shader function which sets parameters the lighting function then uses
            void surf(Input i, inout SurfaceOutputStandard o) {
                //calculate signed distance to plane
                for (int j = 0; j < _PlaneCount; j++) {
                    float distance = dot(i.worldPos, _Planes[j].xyz);
                    distance = distance + _Planes[j].w;
                    //discard surface above plane
                    clip(-distance);
                }
                float facing = i.facing * 0.5 + 0.5;
                //normal color stuff
                fixed4 col = tex2D(_MainTex, i.uv_MainTex);
                col *= _Color;
                o.Albedo = col.rgb * facing;
                o.Metallic = _Metallic * facing;
                o.Smoothness = _Smoothness * facing;
                o.Emission = _Emission * facing;
                //o.Emission = lerp(_CutoffColor, _Emission, facing);
            }
            ENDCG
        }
            FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}