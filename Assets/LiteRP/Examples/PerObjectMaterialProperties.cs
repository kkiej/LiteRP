using UnityEngine;

namespace LiteRP.Examples
{
    [DisallowMultipleComponent]
    public class PerObjectMaterialProperties : MonoBehaviour
    {
        private static readonly int
            BaseColorId = Shader.PropertyToID("_BaseColor"),
            CutoffId = Shader.PropertyToID("_Cutoff"),
            MetallicId = Shader.PropertyToID("_Metallic"),
            SmoothnessId = Shader.PropertyToID("_Smoothness"),
            EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static MaterialPropertyBlock _block;
        
        [SerializeField]
        private Color baseColor = Color.white;

        [SerializeField, Range(0f, 1f)]
        private float cutoff = 0.5f, metallic, smoothness = 0.5f;

        [SerializeField, ColorUsage(false, true)]
        private Color emissionColor = Color.black;

        private void Awake()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            _block ??= new MaterialPropertyBlock();
            _block.SetColor(BaseColorId, baseColor);
            _block.SetFloat(CutoffId, cutoff);
            _block.SetFloat(MetallicId, metallic);
            _block.SetFloat(SmoothnessId, smoothness);
            _block.SetColor(EmissionColorId, emissionColor);
            GetComponent<Renderer>().SetPropertyBlock(_block);
        }
    }
}
