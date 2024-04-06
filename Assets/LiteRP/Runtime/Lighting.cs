using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public class Lighting
    {
        private const string BufferName = "Lighting";

        private readonly CommandBuffer buffer = new CommandBuffer()
        {
            name = BufferName
        };

        private const int MaxDirLightCount = 4, maxOtherLightCount = 64;

        private static readonly int
            //方向光的属性名
            DirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
            DirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
            DirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections"),
            DirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData"),
            //其它光的属性名
            otherLightCountId = Shader.PropertyToID("_OtherLightCount"),
            otherLightColorsId = Shader.PropertyToID("_OtherLightColors"),
            otherLightPositionsId = Shader.PropertyToID("_OtherLightPositions"),
            otherLightDirectionsId = Shader.PropertyToID("_OtherLightDirections"),
            otherLightSpotAnglesId = Shader.PropertyToID("_OtherLightSpotAngles"),
            otherLightShadowDataId = Shader.PropertyToID("_OtherLightShadowData");

        private static readonly Vector4[]
            //方向光属性数组
            DirLightColors = new Vector4[MaxDirLightCount],
            DirLightDirections = new Vector4[MaxDirLightCount],
            DirLightShadowData = new Vector4[MaxDirLightCount],
            //其他光属性数组
            otherLightColors = new Vector4[maxOtherLightCount],
            otherLightPositions = new Vector4[maxOtherLightCount],
            otherLightDirections = new Vector4[maxOtherLightCount],
            otherLightSpotAngles = new Vector4[maxOtherLightCount],
            otherLightShadowData = new Vector4[maxOtherLightCount];

        private CullingResults cullingResults;

        private readonly Shadows shadows = new Shadows();

        private static readonly string LightsPerObjectKeyword = "_LIGHTS_PER_OBJECT";

        public void Setup(ScriptableRenderContext context, CullingResults cullingResults,
            ShadowSettings shadowSettings, bool useLightsPerObject)
        {
            this.cullingResults = cullingResults;
            buffer.BeginSample(BufferName);
            shadows.Setup(context, cullingResults, shadowSettings);
            SetupLights(useLightsPerObject);
            shadows.Render();
            buffer.EndSample(BufferName);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        private void SetupLights(bool useLightsPerObject)
        {
            //从剔除结果检索灯光索引图
            NativeArray<int> indexMap = useLightsPerObject ?
                cullingResults.GetLightIndexMap(Allocator.Temp) : default;
            //从剔除结果获取可见光
            NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

            int dirLightCount = 0, otherLightCount = 0;
            int i;
            for (i = 0; i < visibleLights.Length; i++)
            {
                int newIndex = -1;
                VisibleLight visibleLight = visibleLights[i];
                switch (visibleLight.lightType)
                {
                    case LightType.Directional:
                        if (dirLightCount < MaxDirLightCount)
                        {
                            SetupDirectionalLight(dirLightCount++, i, ref visibleLight);
                        }
                        break;
                    case LightType.Point:
                        if (otherLightCount < maxOtherLightCount)
                        {
                            newIndex = otherLightCount;
                            SetupPointLight(otherLightCount++, i, ref visibleLight);
                        }
                        break;
                    case LightType.Spot:
                        if (otherLightCount < maxOtherLightCount)
                        {
                            newIndex = otherLightCount;
                            SetupSpotLight(otherLightCount++, i, ref visibleLight);
                        }
                        break;
                }

                if (useLightsPerObject)
                {
                    indexMap[i] = newIndex;
                }
            }
            
            if (useLightsPerObject)
            {
                //归零其他不可见光源
                for (; i < indexMap.Length; i++)
                {
                    indexMap[i] = -1;
                }
                //将重映射后的每物体光源索引告诉Unity
                cullingResults.SetLightIndexMap(indexMap);
                //释放indexMap内存
                indexMap.Dispose();
                //激活PerObjectLight关键字
                Shader.EnableKeyword(LightsPerObjectKeyword);
            }
            else
            {
                Shader.DisableKeyword(LightsPerObjectKeyword);
            }
            
            buffer.SetGlobalInt(DirLightCountId, dirLightCount);
            if (dirLightCount > 0)
            {
                buffer.SetGlobalVectorArray(DirLightColorsId, DirLightColors);
                buffer.SetGlobalVectorArray(DirLightDirectionsId, DirLightDirections);
                buffer.SetGlobalVectorArray(DirLightShadowDataId, DirLightShadowData);
            }
            
            buffer.SetGlobalInt(otherLightCountId, otherLightCount);
            if (otherLightCount > 0)
            {
                buffer.SetGlobalVectorArray(otherLightColorsId, otherLightColors);
                buffer.SetGlobalVectorArray(otherLightPositionsId, otherLightPositions);
                buffer.SetGlobalVectorArray(otherLightDirectionsId, otherLightDirections);
                buffer.SetGlobalVectorArray(otherLightSpotAnglesId, otherLightSpotAngles);
                buffer.SetGlobalVectorArray(otherLightShadowDataId, otherLightShadowData);
            }
        }

        private void SetupDirectionalLight(int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            DirLightColors[index] = visibleLight.finalColor;
            DirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
            DirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, visibleIndex);
        }

        private void SetupPointLight(int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            otherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            otherLightPositions[index] = position;

            otherLightSpotAngles[index] = new Vector4(0f, 1f);

            Light light = visibleLight.light;
            otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
        }
        
        private void SetupSpotLight(int index, int visibleIndex, ref VisibleLight visibleLight)
        {
            otherLightColors[index] = visibleLight.finalColor;
            Vector4 position = visibleLight.localToWorldMatrix.GetColumn(3);
            position.w = 1f / Mathf.Max(visibleLight.range * visibleLight.range, 0.00001f);
            otherLightPositions[index] = position;
            otherLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);

            Light light = visibleLight.light;
            float innerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * light.innerSpotAngle);
            float outerCos = Mathf.Cos(Mathf.Deg2Rad * 0.5f * visibleLight.spotAngle);
            float angleRangeInv = 1f / Mathf.Max(innerCos - outerCos, 0.001f);
            otherLightSpotAngles[index] = new Vector4(angleRangeInv, -outerCos * angleRangeInv);
            otherLightShadowData[index] = shadows.ReserveOtherShadows(light, visibleIndex);
        }

        public void Cleanup()
        {
            shadows.Cleanup();
        }
    }
}