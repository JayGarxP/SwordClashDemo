Shader "Unlit/HologramShader"
{
	/*
	HologramShader sets alpha to half, and moves vertices side to side in a sin wave,
	to simulate scanlines of a glitchy hologram,
	A script can use the entry points in the properties below by changing them at runtime via
	Renderer ref -> Material . setFloat("_Speed", urSpeed);
	*/

	// public properties available under material->shader in Game Object Property window
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	    _TintColor ("Tint Color", Color) = (1,1,1,1)
		_Transparency ("Transparency", Range(0.0, 0.50)) = 0.25
		_CutOutThresh ("CutOut Threshold", Range(0.0, 1.0)) = 0.2 
		_Distance ("Distance", Float) = 1
		_Amplitude ("Amplitude", Float) = 1
		_Speed ("Speed", Float) = 1
		_Amount ("Amount", Range(0.0, 1.0)) = 0.5
	}

	SubShader
	{
			// Opaque default to Transparent; also need renderQ to allow alpha blending
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 100
			// do not write to depth buffer
		ZWrite Off
			// see ShaderLab blend factors in docs
			Blend SrcAlpha OneMinusSrcAlpha
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			// float4 good for color rgba
			float4 _TintColor;
			// public property alpha multiplier
			float _Transparency;
			float _CutOutThresh;
			// hologram flicker properties of sin wave tranpose
			float _Distance, _Amplitude, _Speed, _Amount;
			
			v2f vert (appdata v)
			{
				v2f o;
				// _Time xyzw is a float4 buffer with different time ticks; .y is seconds
				// transpose sideways all vertices in sin function (jitters / warps material)
				v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance *_Amount;
				
				// convert vertices to object/local space
				o.vertex = UnityObjectToClipPos(v.vertex);

				// convert vertices to screen space
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture, to get col color rgba
				fixed4 col = tex2D(_MainTex, i.uv);
				col += _TintColor; // additive tint
				col.a = _Transparency; // set alpha from public property value
				// cut out pixels beyond red threshold; clip is same as "discard"
				clip(col.r - _CutOutThresh);

				return col;
			}
			ENDCG
		}
	}
}
