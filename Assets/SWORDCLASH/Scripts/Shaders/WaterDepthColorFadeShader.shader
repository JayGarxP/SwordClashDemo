Shader "Sprites/WaterDepthColorFadeShader"
{
/*
	Image Shader to make the back flipping Tentacle Tip look cool;
	WaterFX  -  Color fade with depth; Red fade at 5Meter O at 8M Y at 15M Green at 25M Blue at 35M
	Sprite Scaling -  Shrink sprite as it goes deeper in the ocean, grow as it rises closer to surface
*/

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WaterDepth ("Water Depth", Range(0.0, 50.0)) = 1
		_RedDepth ("Red Light Fade Depth", Float) = 0.0
		_GrnDepth ("Green Light Fade Depth", Float) = 20.0
		_BluDepth ("Blue Light Fade Depth", Float) = 35.0

	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			// where do i put my own functions??? do I need separate ENDCG tags or ??/
			// Calculate value to subtract from a color channel value based on depth underwater
			float CalcDepthAdjustment (float dep : CURRENTDEPTH, float coldep : COLORFADEDEPTH, float maxdep )
			{
				float TracyChapman = 0.0f;

				if (dep >= coldep)
				{
					   TracyChapman = ((dep - coldep) / maxdep);
				}

				return TracyChapman; 
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				// resize somewhere in vertex func here

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _WaterDepth;
			float _RedDepth, _GrnDepth, _BluDepth;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				
				/// Adjust RGB channels depending on depth.
				float depth = _WaterDepth;
				float maxDepth = 50.0f;

				//col.r = 0; // just turns reds to black
				//col.r = col.r - (depth - _RedDepth / maxDepth);
				col.r = col.r - CalcDepthAdjustment(depth, _RedDepth, maxDepth);
				col.g = col.g - CalcDepthAdjustment(depth, _GrnDepth, maxDepth);
				col.b = col.b - CalcDepthAdjustment(depth, _BluDepth, maxDepth);


				return col;
			}

			

			ENDCG
		}
	}
}
