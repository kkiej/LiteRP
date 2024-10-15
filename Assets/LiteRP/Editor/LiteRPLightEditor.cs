using LiteRP.Runtime;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace LiteRP.Editor
{
    [CanEditMultipleObjects]
    [CustomEditorForRenderPipeline(typeof(Light), typeof(LiteRenderPipelineAsset))]
    public class LiteRPLightEditor : LightEditor
    {
        private static GUIContent renderingLayerMaskLabel =
            new GUIContent("Rendering Layer Mask", "Functional version of above property.");
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawRenderingLayerMask();
            
            if (!settings.lightType.hasMultipleDifferentValues &&
                (LightType)settings.lightType.enumValueIndex == LightType.Spot)
            {
                settings.DrawInnerAndOuterSpotAngle();
            }
            
            settings.ApplyModifiedProperties();

            var light = target as Light;
            if (light.cullingMask != -1)
            {
                EditorGUILayout.HelpBox(
                    light.type == LightType.Directional
                        ? "Culling Mask only affects shadows."
                        : "Culling Mask only affects shadow unless Use Lights Per Objects is on.", MessageType.Warning);
            }
        }
        
        void DrawRenderingLayerMask ()
        {
            SerializedProperty property = settings.renderingLayerMask;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int mask = property.intValue;
            mask = EditorGUILayout.MaskField(
                renderingLayerMaskLabel, mask,
                GraphicsSettings.currentRenderPipeline.renderingLayerMaskNames
            );
            if (EditorGUI.EndChangeCheck()) {
                property.intValue = mask;
            }
            EditorGUI.showMixedValue = false;
        }
    }
}