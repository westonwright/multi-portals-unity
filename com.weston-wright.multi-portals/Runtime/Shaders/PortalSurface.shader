Shader "Portal/PortalSurface"
{
	Properties
	{
		_Color("Fade Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "black" {}
	}
	SubShader
	{
		
		Tags { "RenderType" = "Transparent" }
		ZWrite Off
		//Blend One OneMinusSrcAlpha
		
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

			float _ColorStrength;

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
				fixed4 col = lerp(tex2D(_MainTex, textureCoordinate), _Color, _ColorStrength);
				return col;
			}
			ENDCG
		}
	}
}
