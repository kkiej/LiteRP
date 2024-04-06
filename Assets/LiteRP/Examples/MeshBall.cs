using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace LiteRP.Examples
{
    public class MeshBall : MonoBehaviour
    {
        private static readonly int
            BaseColorId = Shader.PropertyToID("_BaseColor"),
            MetallicId = Shader.PropertyToID("_Metallic"),
            SmoothnessId = Shader.PropertyToID("_Smoothness");

        [SerializeField] private Mesh mesh = default;

        [SerializeField] private Material material = default;

        [SerializeField] private LightProbeProxyVolume lightProbeVolume = null;

        private readonly Matrix4x4[] _matrices = new Matrix4x4[1023];
        private readonly Vector4[] _baseColors = new Vector4[1023];

        private readonly float[]
            _metallic = new float[1023],
            _smoothness = new float[1023];

        private MaterialPropertyBlock block;

        private void Awake()
        {
            for (int i = 0; i < _matrices.Length; i++)
            {
                _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f,
                    Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                    Vector3.one * Random.Range(0.5f, 1.5f));
                _baseColors[i] = new Vector4(Random.value, Random.value, Random.value,
                    Random.Range(0.5f, 1f));
                _metallic[i] = Random.value < 0.25f ? 1f : 0f;
                _smoothness[i] = Random.Range(0.05f, 0.95f);
            }
        }

        private void Update()
        {
            if (block == null)
            {
                block = new MaterialPropertyBlock();
                block.SetVectorArray(BaseColorId, _baseColors);
                block.SetFloatArray(MetallicId, _metallic);
                block.SetFloatArray(SmoothnessId, _smoothness);

                if (!lightProbeVolume)
                {
                    var positions = new Vector3[1023];
                    for (int i = 0; i < _matrices.Length; i++)
                    {
                        positions[i] = _matrices[i].GetColumn(3);
                    }

                    var lightProbes = new SphericalHarmonicsL2[1023];
                    var occlusionProbes = new Vector4[1023];
                    LightProbes.CalculateInterpolatedLightAndOcclusionProbes(positions, lightProbes, occlusionProbes);
                    block.CopySHCoefficientArraysFrom(lightProbes);
                    block.CopyProbeOcclusionArrayFrom(occlusionProbes);
                }
            }
            Graphics.DrawMeshInstanced(mesh, 0 , material, _matrices, 1023, block,
                ShadowCastingMode.On, true, 0, null,
                lightProbeVolume ? LightProbeUsage.UseProxyVolume : LightProbeUsage.CustomProvided, lightProbeVolume);
        }
    }
}