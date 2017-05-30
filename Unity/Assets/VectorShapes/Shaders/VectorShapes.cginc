// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#define PI 3.1415926536f
#define MIN_ANGLE_THRESHOLD 0.01745329252f //1 degree
#define MAX_VALUE 999999999999999.0f
#define HALF 0.5f

int _StrokeCornerType;
int _StrokeRenderType;
float _StrokeMiterLimit;

struct VertexInputData
{
	float4 position1;
	float4 position2;
	float4 position3;
	float strokeWidth1;
	float strokeWidth2;
	float strokeWidth3;
	float4 color2;
	int vertexId;
	float2 uv;
};

struct VertexOutputData
{
	float4 position;
	float2 uv;
	float4 color;
};

struct VertexInputDataEncoded
{
	float4 vertex  : POSITION;
	float4 color  : COLOR;
	float4 texcoord0 : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 normal: NORMAL;
	float4 tangent : TANGENT;
};

VertexInputData DecodeVertexData(VertexInputDataEncoded encodedData)
{
	VertexInputData obj;

	obj.position1 = float4( encodedData.normal,1);
	obj.position2 = encodedData.vertex;
	obj.position3 = float4(encodedData.tangent.xyz,1);
	obj.strokeWidth1 = encodedData.texcoord1.y;
	obj.strokeWidth2 = encodedData.texcoord1.z;
	obj.strokeWidth3 = encodedData.texcoord1.w;
	obj.uv = encodedData.texcoord0.xy;
	obj.color2 = encodedData.color;
	obj.vertexId = int(round(encodedData.texcoord1.x));

	return obj;
}

float GetCornerAngle(float3 prevPoint, float3 thisPoint, float3 nextPoint)
{
	float3 vToLastPoint = prevPoint - thisPoint;
	float3 vToNextPoint = nextPoint - thisPoint;
	float angle = (atan2(vToNextPoint.y, vToNextPoint.x) - atan2(vToLastPoint.y, vToLastPoint.x));
	if (angle < 0)
		angle += PI * 2;

	return angle;
}

float3 GetCornerNormal(float3 prevPoint, float3 thisPoint, float3 nextPoint)
{
	float3 vToLastPoint = prevPoint - thisPoint;
	float3 vToNextPoint = nextPoint - thisPoint;
	float angleToNext = atan2(vToNextPoint.y, vToNextPoint.x);
	float angleToLast = atan2(vToLastPoint.y, vToLastPoint.x);
	if (angleToNext < angleToLast)
		angleToNext += PI * 2;

	float angle = lerp(angleToLast, angleToNext,  HALF);

	return float3(cos(angle), sin(angle), 0);
}


float4 GetColor(VertexInputData vertexInputData)
{

	#if !DEBUG_ON
	return vertexInputData.color2;
	#else

	float4 colors [8] = {

		float4(1,0,0,1),
		float4(1,1,0,1),
		float4(0,0,0,1),
		float4(0,1,0,1),
		float4(1,0,1,1),
		float4(1,1,1,1),
		float4(1,0,1,1),
		float4(1,1,0,1)
	};

	return colors[vertexInputData.vertexId];
	#endif
}

void GetCorner_Bevel(VertexInputData vertexInputData, inout VertexOutputData output, float cornerAngle, bool isLeftTurn)
{
	float3 normal1 = cross(normalize(vertexInputData.position1 - vertexInputData.position2), float3(0, 0, 1));
	float3 normal2 = cross(normalize(vertexInputData.position3 - vertexInputData.position2), float3(0, 0, 1));
	float3 leftPoint = vertexInputData.position2 + normal1 * vertexInputData.strokeWidth2 * HALF;
	float3 rightPoint = vertexInputData.position2 - normal1 * vertexInputData.strokeWidth2 * HALF;
	float3 leftPoint2 = vertexInputData.position2 - normal2 * vertexInputData.strokeWidth2 * HALF;
	float3 rightPoint2 = vertexInputData.position2 + normal2 * vertexInputData.strokeWidth2 * HALF;

	if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
	{
		output.position.xyz = leftPoint2;
	}
	else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
	{
		output.position.xyz = rightPoint2;
	}
	else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
	{
		output.position.xyz = rightPoint;
	}
	else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
	{
		output.position.xyz = leftPoint;
	}

	if (!isLeftTurn)
	{
		if (vertexInputData.vertexId == 5 || vertexInputData.vertexId == 6)
		{
			output.uv = float2(vertexInputData.uv.x, 0.5f);
			output.position.xyz = vertexInputData.position2;
		}
	}
	else
	{
		if (vertexInputData.vertexId == 4 || vertexInputData.vertexId == 7)
		{
			output.uv = float2(vertexInputData.uv.x, 0.5f);
			output.position = vertexInputData.position2;
		}
	}
}

void GetCorner_Extend(VertexInputData vertexInputData, inout VertexOutputData output, float3 cornerNormal, float normalLength)
{
	float3 leftPoint = vertexInputData.position2 - cornerNormal * normalLength * vertexInputData.strokeWidth2 * HALF;
	float3 rightPoint = vertexInputData.position2 + cornerNormal * normalLength * vertexInputData.strokeWidth2 * HALF;
	float3 leftPoint2 = leftPoint;
	float3 rightPoint2 = rightPoint;

	if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
	{
		output.position.xyz = leftPoint2;
	}
	else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
	{
		output.position.xyz = rightPoint2;
	}
	else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
	{
		output.position.xyz = rightPoint;
	}
	else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
	{
		output.position.xyz = leftPoint;
	}
}
void GetCorner_Miter(VertexInputData vertexInputData, inout VertexOutputData output, float3 cornerNormal, float cornerNormalLength, bool isLeftTurn)
{
	float3 strokeNormal = cross(normalize(vertexInputData.position2 - vertexInputData.position1), float3(0, 0, 1));
	float cornerNormalToStrokeNormalAngle = !isLeftTurn ? GetCornerAngle(cornerNormal, float3(0, 0, 0), strokeNormal) : GetCornerAngle(strokeNormal, float3(0, 0, 0), cornerNormal);

	// outside 
	float outsideNormalLength = _StrokeMiterLimit;
	float3 outsideNormal = cornerNormal * outsideNormalLength;

	// inside
	float insideAngle = (PI * HALF) - cornerNormalToStrokeNormalAngle;
	float insideNormalLength;

	if (abs(insideAngle) > MIN_ANGLE_THRESHOLD)
	{
		insideNormalLength = 1 / sin(abs(insideAngle));
	}
	else
	{
		insideNormalLength = 0;
	}

	float3 insideNormal = -cornerNormal * insideNormalLength * vertexInputData.strokeWidth2 * HALF;
	/*
	float3 projectedMax32 = float3.Project(vertexInputData.position3 - vertexInputData.position2, insideNormal);
	insideNormal = float3.ClampMagnitude(insideNormal, projectedMax32.magnitude);
	float3 projectedMax12 = float3.Project(vertexInputData.position1 - vertexInputData.position2, insideNormal);
	insideNormal = float3.ClampMagnitude(insideNormal, projectedMax12.magnitude);

	float proj23Sqr = (vertexInputData.position3 - vertexInputData.position2).sqrMagnitude + vertexInputData.strokeWidth2 * vertexInputData.strokeWidth2;
	float proj21Sqr = (vertexInputData.position1 - vertexInputData.position2).sqrMagnitude + vertexInputData.strokeWidth2 * vertexInputData.strokeWidth2;
	float insideNormalLengthSqr = insideNormalLength * insideNormalLength;

	if (insideNormalLengthSqr > proj23Sqr || insideNormalLengthSqr > proj21Sqr) 
	{ 
		// special case, normal is longer than line to next or prev point
		if (proj21Sqr < proj23Sqr) {
			insideNormal = (vertexInputData.position1 - vertexInputData.position2);
		} else {
			insideNormal = (vertexInputData.position3 - vertexInputData.position2);
		}
	}*/


	// miter fill
	float miterFillLength = (1 - _StrokeMiterLimit * cos(cornerNormalToStrokeNormalAngle)) / sin(cornerNormalToStrokeNormalAngle);
	float3 miterFillVector = cross(cornerNormal, float3(0, 0, 1)) * -miterFillLength;

	float3 leftPoint = vertexInputData.position2;
	float3 leftPoint2 = vertexInputData.position2;
	float3 rightPoint = vertexInputData.position2;
	float3 rightPoint2 = vertexInputData.position2;
	float3 outsideNormalMinusFillVector = (outsideNormal - miterFillVector) * vertexInputData.strokeWidth2 * HALF;
	float3 outsideNormalPlusFillVector = (outsideNormal + miterFillVector) * vertexInputData.strokeWidth2 * HALF;

	if (isLeftTurn)
	{
		leftPoint += insideNormal;
		rightPoint += outsideNormalMinusFillVector;
		leftPoint2 += insideNormal;
		rightPoint2 += outsideNormalPlusFillVector;

	}
	else
	{
		leftPoint -= outsideNormalPlusFillVector;
		rightPoint -= insideNormal;
		leftPoint2 -= outsideNormalMinusFillVector;
		rightPoint2 -= insideNormal;
	}

	if (vertexInputData.vertexId == 0 || vertexInputData.vertexId == 7)
	{
		output.position.xyz = leftPoint2;
	}
	else if (vertexInputData.vertexId == 1 || vertexInputData.vertexId == 6)
	{
		output.position.xyz = rightPoint2;
	}
	else if (vertexInputData.vertexId == 2 || vertexInputData.vertexId == 5)
	{
		output.position.xyz = rightPoint;
	}
	else if (vertexInputData.vertexId == 3 || vertexInputData.vertexId == 4)
	{
		output.position.xyz = leftPoint;
	}
}

void GetCorner_Cut(VertexInputData vertexInputData,inout VertexOutputData output)
{
	float3 normal1 = cross(normalize(vertexInputData.position1 - vertexInputData.position2), float3(0, 0, 1));
	float3 normal2 = cross(normalize(vertexInputData.position3 - vertexInputData.position2), float3(0, 0, 1));

	float3 leftPoint = vertexInputData.position2 + normal1 * vertexInputData.strokeWidth2 * HALF;
	float3 rightPoint = vertexInputData.position2 - normal1 * vertexInputData.strokeWidth2 * HALF;
	float3 leftPoint2 = vertexInputData.position2 - normal2 * vertexInputData.strokeWidth2 * HALF;
	float3 rightPoint2 = vertexInputData.position2 + normal2 * vertexInputData.strokeWidth2 * HALF;

	if (vertexInputData.vertexId == 0)
	{
		output.position.xyz = leftPoint2;
	}
	else if (vertexInputData.vertexId == 1)
	{
		output.position.xyz = rightPoint2;
	}
	else if (vertexInputData.vertexId == 2)
	{
		output.position.xyz = rightPoint;
	}
	else if (vertexInputData.vertexId == 3)
	{
		output.position.xyz = leftPoint;
	}
	else
	{
		output.position.xyz = vertexInputData.position2;
	}
}

VertexOutputData GetCornerVertexLocalSpace(VertexInputData vertexInputData)
{
	VertexOutputData output;
	output.uv = vertexInputData.uv;
	output.position = float4(0,0,0,1);
	output.color = GetColor(vertexInputData);

	float cornerAngle = GetCornerAngle(vertexInputData.position1, vertexInputData.position2, vertexInputData.position3);
	bool isLeftTurn = cornerAngle > PI;

	#if STROKE_CORNER_BEVEL

	GetCorner_Bevel(vertexInputData, output, cornerAngle, isLeftTurn);

	#elif STROKE_CORNER_EXTEND_OR_CUT || STROKE_CORNER_EXTEND_OR_MITER

	float3 cornerNormal = GetCornerNormal(vertexInputData.position1, vertexInputData.position2, vertexInputData.position3);
	float cornerNormalLength;
	if (abs(cornerAngle) > MIN_ANGLE_THRESHOLD)
		cornerNormalLength = 1 / sin(cornerAngle * HALF);
	else
		cornerNormalLength = MAX_VALUE;

	bool miterLimitReached = cornerNormalLength > _StrokeMiterLimit;

	// do extend
	if( miterLimitReached )
	{
		#if STROKE_CORNER_EXTEND_OR_MITER
		GetCorner_Miter(vertexInputData, output, cornerNormal, cornerNormalLength, isLeftTurn);
		#elif STROKE_CORNER_EXTEND_OR_CUT
		GetCorner_Cut(vertexInputData, output);
		#endif
	}
	else
	{
		GetCorner_Extend(vertexInputData, output, cornerNormal, cornerNormalLength);
	}

	#endif

	output.position.w = saturate( 1 / vertexInputData.strokeWidth2);
	output.position.xyz *= output.position.w;

	return output;
}


VertexOutputData GetCornerVertex(VertexInputData vertexInputData)
{
	#if STROKE_RENDER_SCREEN_SPACE_PIXELS || STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT || STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA

	#if STROKE_RENDER_SCREEN_SPACE_PIXELS
	float strokeWidthMulti = 1 / _ScreenParams.y;
	vertexInputData.strokeWidth1 *= strokeWidthMulti;
	vertexInputData.strokeWidth2 *= strokeWidthMulti;
	vertexInputData.strokeWidth3 *= strokeWidthMulti;
	#endif

	vertexInputData.strokeWidth1 *= 2;
	vertexInputData.strokeWidth2 *= 2;
	vertexInputData.strokeWidth3 *= 2;

	#if STROKE_RENDER_SCREEN_SPACE_PIXELS || STROKE_RENDER_SCREEN_SPACE_RELATIVE_TO_SCREEN_HEIGHT
	vertexInputData.position1 = UnityObjectToClipPos(vertexInputData.position1);
	vertexInputData.position2 = UnityObjectToClipPos(vertexInputData.position2);
	vertexInputData.position3 = UnityObjectToClipPos(vertexInputData.position3);
	vertexInputData.position1 /= vertexInputData.position1.w;
	vertexInputData.position2 /= vertexInputData.position2.w;
	vertexInputData.position3 /= vertexInputData.position3.w;
	#endif
			
	#if STROKE_RENDER_SHAPE_SPACE_FACING_CAMERA
	vertexInputData.position1 = mul(UNITY_MATRIX_MV,vertexInputData.position1);
	vertexInputData.position2 = mul(UNITY_MATRIX_MV,vertexInputData.position2);
	vertexInputData.position3 = mul(UNITY_MATRIX_MV,vertexInputData.position3);
	vertexInputData.strokeWidth1 /= 1-(vertexInputData.position1.z / vertexInputData.position1.w);
	vertexInputData.strokeWidth2 /= 1-(vertexInputData.position2.z / vertexInputData.position2.w);
	vertexInputData.strokeWidth3 /= 1-(vertexInputData.position3.z / vertexInputData.position3.w);
	vertexInputData.position1 = mul(UNITY_MATRIX_P,vertexInputData.position1);
	vertexInputData.position2 = mul(UNITY_MATRIX_P,vertexInputData.position2);
	vertexInputData.position3 = mul(UNITY_MATRIX_P,vertexInputData.position3);
	vertexInputData.position1 /= vertexInputData.position1.w;
	vertexInputData.position2 /= vertexInputData.position2.w;
	vertexInputData.position3 /= vertexInputData.position3.w;
	#endif

	float4 aspectRatioCorrection = float4 (_ScreenParams.x/_ScreenParams.y,1,1,1);
	vertexInputData.position1 *= aspectRatioCorrection;
	vertexInputData.position2 *= aspectRatioCorrection;
	vertexInputData.position3 *= aspectRatioCorrection;
	//vertexInputData.position1.z = 0;
	//vertexInputData.position2.z = 0;
	//vertexInputData.position3.z = 0;

	VertexOutputData output = GetCornerVertexLocalSpace(vertexInputData);

	output.position /= aspectRatioCorrection;
	#endif

	#if STROKE_RENDER_SHAPE_SPACE
	VertexOutputData output = GetCornerVertexLocalSpace(vertexInputData);
	output.position = UnityObjectToClipPos(output.position);
	#endif

	return output;
}