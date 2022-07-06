// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Vector Shapes/Stroke Alpha Blend"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		//[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[MaterialToggle] Antialiasing ("Antialiasing", Int) = 0
		[MaterialToggle] Debug("Debug Mode", Int) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile _ PIXELSNAP_ON
			#pragma shader_feature ETC1_EXTERNAL_ALPHA
			#pragma multi_compile_local _ DEBUG_ON
			#pragma multi_compile_local _ ANTIALIASING_ON
			#pragma multi_compile_local STROKE_CORNER_BEVEL STROKE_CORNER_EXTEND_OR_CUT STROKE_CORNER_EXTEND_OR_MITER
			#pragma multi_compile_local STROKE_RENDER_SCREEN_SPACE_PIXELS STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT STROKE_RENDER_SHAPE_SPACE STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA

			#include "UnityCG.cginc"
			#include "VectorShapes.cginc"



			struct v2f
			{
				float4 vertex  : SV_POSITION;

				// better color gradients
				#if SHADER_API_GLCORE || SHADER_API_D3D11
				noperspective 
				#endif
				fixed4 color  : COLOR;

				float2 texcoord : TEXCOORD0;
				#if ANTIALIASING_ON
				float pixelSizeInUV : TEXCOORD1;
				#endif
			};
			
			fixed4 _Color;

			v2f vert(VertexInputDataEncoded IN)
			{
				v2f OUT;

				VertexInputData vertexInputData = DecodeVertexData(IN);

				// add one pixel to antialiased stroke, to compensate the weight loss
				#if ANTIALIASING_ON
				#if STROKE_RENDER_SCREEN_SPACE_PIXELS
				vertexInputData.strokeWidth2 += 1;
				#elif STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT
				vertexInputData.strokeWidth2 += 1 / _ScreenParams.y;
				#endif
				#endif
				VertexOutputData vertexOutputData = GetCornerVertex(vertexInputData);

				OUT.vertex = vertexOutputData.position;
				OUT.texcoord = vertexOutputData.uv;
				OUT.color = vertexOutputData.color * _Color;

				// calculate how big one pixel in uv space is
				#if ANTIALIASING_ON
				#if STROKE_RENDER_SCREEN_SPACE_PIXELS

				OUT.pixelSizeInUV = 1 / vertexInputData.strokeWidth2;

				#elif STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT

				OUT.pixelSizeInUV = 1 / (vertexInputData.strokeWidth2 * _ScreenParams.y);

				#else
				// a bit of a hack, could probably be done in a nicer way..
				float4 normalPos = UnityObjectToClipPos(IN.vertex);
				float2 clipSpaceNormalVector = normalPos.xy/normalPos.w-vertexOutputData.position.xy/vertexOutputData.position.w;
				float2 screenSpaceNormalVector = clipSpaceNormalVector * _ScreenParams.xy / 2;
				float strokeWidthInPixels = length(screenSpaceNormalVector);

				/*
				//use corner normal??
				float3 cn = GetCornerNormal(vertexInputData.position1,vertexInputData.position2,vertexInputData.position3);
				float4 cornerNormalTransformed = mul(UNITY_MATRIX_MVP,float4(cn,0));
				float2 cornerNormalPixels = cornerNormalTransformed.xy * _ScreenParams.xy / 2;
				float strokeWidthInPixels = length( cornerNormalPixels) * vertexInputData.strokeWidth2;
				*/
				OUT.pixelSizeInUV = 1 / strokeWidthInPixels;

				#endif
				#endif

				//#ifdef PIXELSNAP_ON
				//OUT.vertex = UnityPixelSnap (OUT.vertex);
				//#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
				// get the color from an external texture (usecase: Alpha support for ETC1 on android)
				color.a = tex2D (_AlphaTex, uv).r;
#endif //ETC1_EXTERNAL_ALPHA

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				
                #ifdef UNITY_COLORSPACE_GAMMA
                fixed4 color = IN.color;
                #else
                fixed4 color = fixed4(GammaToLinearSpace(IN.color.rgb), IN.color.a);
                #endif
				fixed4 c = SampleSpriteTexture (IN.texcoord) * color;

				#if ANTIALIASING_ON
				float a = saturate( ( 1 - IN.texcoord.y ) / IN.pixelSizeInUV) * saturate( IN.texcoord.y / IN.pixelSizeInUV) ;
				c.a *= a;
				#endif

				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}
