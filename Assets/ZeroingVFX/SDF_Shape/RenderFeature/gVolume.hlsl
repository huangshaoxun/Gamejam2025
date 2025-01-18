#ifndef gVolume
#define gVolume


struct Shape
{
    float3 position;
    float3 size;
    float4 colour;
    float4 custom;
    int shapeType;
    int operation;
    float blendStrength;
    int numChildren;
};

//https://iquilezles.org/articles/distfunctions/

float opUnion(float d1, float d2) { return min(d1, d2); }

float opSubtraction(float d1, float d2) { return max(-d1, d2); }

float opIntersection(float d1, float d2) { return max(d1, d2); }


float opSmoothUnion(float d1, float d2, float k)
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) - k * h * (1.0 - h);
}

float opSmoothSubtraction(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    return lerp(d2, -d1, h) + k * h * (1.0 - h);
}

float opSmoothIntersection(float d1, float d2, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return lerp(d2, d1, h) + k * h * (1.0 - h);
}

float4 opUnionCol(float d1, float d2, float3 colA, float3 colB)
{
    float4 cold = float4(colA, d1);
    if (d2 < d1)
    {
        cold = float4(colB, d2);
    }
    return cold;
}

float4 opSubtractionCol(float d1, float d2, float3 colA, float3 colB)
{
    // return max(-d1,d2);
    float4 cold = float4(colA, -d1);
    if (d2 > -d1)
    {
        cold = float4(colB, d2);
    }
    return cold;
}

float4 opIntersectionCol(float d1, float d2, float3 colA, float3 colB)
{
    // return max(d1,d2);
    float4 cold = float4(colA, d1);
    if (d2 > d1)
    {
        cold = float4(colB, d2);
    }
    return cold;
}

float4 opSmoothUnionCol(float d1, float d2, float3 colA, float3 colB, float k)
{
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    float blendDst = lerp(d2, d1, h) - k * h * (1.0 - h);
    float3 blendCol = lerp(colB, colA, h);
    return float4(blendCol, blendDst);
}

float4 opSmoothSubtractionCol(float d1, float d2, float3 colA, float3 colB, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 + d1) / k, 0.0, 1.0);
    float blendDst = lerp(d2, -d1, h) + k * h * (1.0 - h);
    float3 blendCol = lerp(colB, colA, h);
    return float4(blendCol, blendDst);
}

float4 opSmoothIntersectionCol(float d1, float d2, float3 colA, float3 colB, float k)
{
    float h = clamp(0.5 - 0.5 * (d2 - d1) / k, 0.0, 1.0);
    float blendDst = lerp(d2, d1, h) + k * h * (1.0 - h);
    float3 blendCol = lerp(colB, colA, h);
    return float4(blendCol, blendDst);
}


float sdSphere(float3 p, float s)
{
    return length(p) - s;
}

float sdBox(float3 p, float3 b)
{
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

float sdCylinder(float3 p, float3 c)
{
    return length(p.xz - c.xy) - c.z;
}

float sdCapsule(float3 p, float3 a, float3 b, float r)
{
    float3 pa = p - a;
    float3 ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}


float4 CombineDistanceColor(float dstold, float dstnew, float3 colold, float3 colnew, Shape newShape)
{
    if (newShape.operation == 0)
    {
        //smooth blend
        return opSmoothUnionCol(dstnew, dstold, colnew, colold, newShape.blendStrength);
    }
    else if (newShape.operation == 1)
    {
        return opSmoothSubtractionCol(dstnew, dstold, colnew, colold, newShape.blendStrength);
    }
    else if (newShape.operation == 2)
    {
        return opSmoothIntersectionCol(dstnew, dstold, colnew, colold, newShape.blendStrength);
    }
    else if (newShape.operation == 3)
    {
        //sharp
        return opUnionCol(dstnew, dstold, colnew, colold);
    }
    else if (newShape.operation == 4)
    {
        return opSubtractionCol(dstnew, dstold, colnew, colold);
    }
    else if (newShape.operation == 5)
    {
        return opIntersectionCol(dstnew, dstold, colnew, colold);
    }
    return opUnion(dstnew, dstold);
}

float CombineDistance(float dstold, float dstnew, Shape newShape)
{
    if (newShape.operation == 0)
    {
        //smooth blend
        return opSmoothUnion(dstnew, dstold, newShape.blendStrength);
    }
    else if (newShape.operation == 1)
    {
        return opSmoothSubtraction(dstnew, dstold, newShape.blendStrength);
    }
    else if (newShape.operation == 2)
    {
        return opSmoothIntersection(dstnew, dstold, newShape.blendStrength);
    }
    else if (newShape.operation == 3)
    {
        //sharp
        return opUnion(dstnew, dstold);
    }
    else if (newShape.operation == 4)
    {
        return opSubtraction(dstnew, dstold);
    }
    else if (newShape.operation == 5)
    {
        return opIntersection(dstnew, dstold);
    }
    return opUnion(dstnew, dstold);
}


#endif
