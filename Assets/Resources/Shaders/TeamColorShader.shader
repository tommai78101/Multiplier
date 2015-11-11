//Shader "Custom/Team Colors" {
//	Properties {
//		_MainTex ("Stripe", 2D) = "blue" {}
//		_Color ("Stripe Color", Color) = (0.5, 0.5, 0.5, 1)
//	}
//	SubShader {
//		/*Pass {
//			Cull Off
//
//			CGPROGRAM
//				#pragma vertex vert
//				#pragma fragment frag
//				
//				#include "UnityCG.cginc"
//
//				uniform sampler2D _MainTex;
//				uniform float4 _Color;
//
//				struct vertexInput{
//					float4 vertex : POSITION;
//					float4 texcoord : TEXCOORD0;
//				};
//				struct vertexOutput{
//					float4 pos : SV_POSITION;
//					float4 tex : TEXCOORD0;
//				};
//
//				inline half3 LightingLambertVectors(half3 normal, half3 lightDir){
//					half diff = max(0, dot(normal, lightDir));
//					return unity_LightColor[0].rgb * (diff * 2);
//				}
//
//				vertexOutput vert(vertexInput input){
//					vertexOutput output;
//					output.tex = input.texcoord;
//					output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
//					return output;
//				}
//
//				fixed4 frag(vertexOutput input) : COLOR {
//					float4 textureColor = tex2D(_MainTex, input.tex.xy);
//					if (textureColor.a < 0.5){
//						textureColor = float4(1, 1, 1, 1);
//					}
//					else {
//						textureColor = _Color;
//					}
//					return textureColor;
//				}
//			ENDCG
//		}*/
//
//		Tags {"RenderType" = "Opaque"}
//
//		CGPROGRAM
//		#pragma surface surf Lambert vertex:vert
//		#pragma target 3.0
//
//		struct vertexInput {
//			float4 vertex : POSITION;
//			float4 texcoord : TEXCOORD0;
//		};
//		struct vertexOutput {
//			float4 pos : SV_POSITION;
//			float4 tex : TEXCOORD0;
//		};
//
//		sampler2D _MainTex;
//		float4 _Color;
//
//		void vert(inout ) {
//			output.tex = input.texcoord;
//			output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
//		}
//
//		void surf(vertexOutput input, inout SurfaceOutput output){
//			float4 textureColor = tex2D(_MainTex, input.tex.xy);
//			if (textureColor.a < 0.5) {
//				textureColor = float4(1, 1, 1, 1);
//			}
//			else {
//				textureColor = _Color;
//			}
//			output.Albedo = textureColor.rgb;
//		}
//
//		ENDCG
//	}
//	Fallback "Diffuse"
//}



Shader "Custom/TeamColors" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_TeamColor("Team Color", Color) = (0.5, 0.5, 0.5, 1)
	}
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 color : COLOR;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _TeamColor;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 sampledColor = tex2D(_MainTex, IN.uv_MainTex);
			fixed4 c = sampledColor * IN.color * _Color;
			if (sampledColor.a < 0.5)
			{
				o.Albedo = c;
			}
			else
			{
				o.Albedo = _TeamColor.rgb;
			}

			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}