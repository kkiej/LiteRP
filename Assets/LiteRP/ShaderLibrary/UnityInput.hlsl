#ifndef LITERP_UNITY_INPUT_INCLUDED
#define LITERP_UNITY_INPUT_INCLUDED

CBUFFER_START(UnityPerDraw)
    float4x4 unity_ObjectToWorld;
    float4x4 unity_WorldToObject;
    //在定义（UnityPerDraw）CBuffer时，因为Unity对一组相关数据都归到一个Feature中，即使我们没用到unity_LODFade，
    //我们也需要放到这个CBuffer中来构造一个完整的Feature
    //unity_LODFade供LOD过渡使用，其x值表示当前过渡值（对于fade out的LOD，0代表开始fade out，1代表完全fade out；
    //对于fade in的LOD，-1代表开始fade in，0代表完全fade in），y表示过渡值在16个区间划分内的值（不会使用到）
    float4 unity_LODFade;
    real4 unity_WorldTransformParams;

    float4 unity_RenderingLayer;
    
    float4 unity_ProbesOcclusion;

    float4 unity_OrthoParams;
    float4 _ProjectionParams;
    float4 _ScreenParams;
    float4 _ZBufferParams;
    
    float4 unity_SpecCube0_HDR;
    
    float4 unity_LightmapST;
    float4 unity_DynamicLightmapST;
    
    //球谐函数的所有系数，一共27个，RGB通道每个9个,实际为float3, SH : Spherical Harmonics
    float4 unity_SHAr;
    float4 unity_SHAg;
    float4 unity_SHAb;
    float4 unity_SHBr;
    float4 unity_SHBg;
    float4 unity_SHBb;
    float4 unity_SHC;
    
    //LPPV所需信息
    float4 unity_ProbeVolumeParams;
    float4x4 unity_ProbeVolumeWorldToObject;
    float4 unity_ProbeVolumeSizeInv;
    float4 unity_ProbeVolumeMin;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;

float3 _WorldSpaceCameraPos;

#endif