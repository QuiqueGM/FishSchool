Shader "SOS/Fish Movement" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpAmount ("Normal Amount", Range(-2, 2)) = 1
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5

		_Speed ("Speed", Range(0, 50.0)) = 20
        _Frequency ("Frequency", Range(0, 25.0)) = 1
        _Amplitude ("Amplitude", Range(-0.5, 0.5)) = 1
		_Start ("Start Effect", Range(-1, 1)) = 0
		_Offset ("Offset", Range(-2, 2)) = 0
	}

	SubShader
	{
		Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 100

		CGPROGRAM
		#pragma surface surf Lambert alphatest:_Cutoff vertex:vert

		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _BumpAmount;
		fixed4 _Color;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		float _Speed;
		float _Frequency;
		float _Amplitude;
		float _Start;
		float _Offset;

		void vert (inout appdata_full v) 
		{
			  v.vertex.x += ((cos( _Time.y * _Speed + v.vertex.z * _Frequency ) * _Amplitude) * (v.vertex.z + _Start)) / (v.vertex.z + _Offset);
		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap)* _BumpAmount) ;
		}
		ENDCG
	}

	FallBack "Legacy Shaders/Transparent/Cutout/Diffuse"
}
