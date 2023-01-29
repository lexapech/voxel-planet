Shader "Custom/normal"
{
	Properties{
		_TopColor("top", Color) = (1,1,1,1)
		_SideColor("side", Color) = (1,1,1,1)
		_GravityCenter("Gravity Center",Vector) = (0,0,0,0)
		_LightDirection("Light Direction",Vector) = (0,0,0,0)
	}
		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				float4 _TopColor;
				float4 _SideColor;
				float4 _GravityCenter;
				float4 _LightDirection;
				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float illumination : TEXCOORD1;
					float steepness: TEXCOORD2;
				};


				v2f vert(float4 pos : POSITION, float2 uv : TEXCOORD0, float3 normal : NORMAL)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(pos);
					half3 dir = normalize(mul(unity_ObjectToWorld,pos) - _GravityCenter);
					o.steepness = abs(dot(dir, normal));
					o.uv = uv;
					o.illumination = dot(normal,normalize( _LightDirection));
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 top = _TopColor * (i.steepness*4-3);
					fixed4 side = _SideColor * (1.25 - i.steepness*0.5);
					fixed4 c = max(top, side)*min(1,0.2+max(0,i.illumination));
					return c;
					//return max(c,front);
				}
				ENDCG
			}
		}
}