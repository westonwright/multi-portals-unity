Shader "Unlit/UniltSST"
{
	Properties
	{
		_Color("Main Color (A=Opacity)", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		
		Tags { "RenderType" = "Transparent" }
		ZWrite Off
		Blend One OneMinusSrcAlpha
		//LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 screenPosition : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				//convert the vertex positions from object space to clip space so they can be rendered
				o.position = UnityObjectToClipPos(v.vertex);
				o.screenPosition = ComputeScreenPos(o.position);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
				fixed4 col = tex2D(_MainTex, textureCoordinate);
				col *= _Color;
				return col;
			}
			ENDCG
		}
	}
}
