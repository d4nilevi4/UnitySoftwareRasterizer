using System;
using Unity.Collections;
using UnityEngine;

namespace SoftwareRasterizer.Demo
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class PixelScreen : MonoBehaviour
    {
        [Header("Resolution")] 
        [Min(1)] [SerializeField] private int width = 64;

        [Min(1)] [SerializeField] private int height = 64;

        [Header("Geometry")] 
        [Min(0.0001f)] [SerializeField] private float pixelSize = 0.1f;

        [SerializeField] private bool centered = true;
        [SerializeField] private bool flipY = false;

        [Header("Behaviour")] 
        [SerializeField] private Material material;

        private Mesh _mesh;
        private Texture2D _texture;

        public int Width => width;
        public int Height => height;

        private void Awake() => Rebuild();

        private void OnDestroy()
        {
            Destroy(_mesh);
            Destroy(_texture);
            _mesh = null;
            _texture = null;
        }

        public void SetPixels(ReadOnlySpan<Color32> buffer)
        {
            buffer.CopyTo(_texture.GetRawTextureData<Color32>().AsSpan());
        }

        public void Apply()
        {
            _texture.Apply(false, false);
        }

        public void Rebuild()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            BuildTexture();
            BuildMesh();
            EnsureMaterial();
        }

        private void BuildTexture()
        {
            if (_texture == null || _texture.width != width || _texture.height != height)
            {
                _texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false, linear: false)
                {
                    name = "PixelScreen",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
            }

            NativeArray<Color32> raw = _texture.GetRawTextureData<Color32>();
            for (int i = 0; i < raw.Length; i++) 
                raw[i] = new Color32();
            _texture.Apply(false, false);
        }

        private void BuildMesh()
        {
            float w = width * pixelSize;
            float h = height * pixelSize;
            float originX = centered ? -w * 0.5f : 0f;
            float originY = centered ? -h * 0.5f : 0f;

            var vertices = new Vector3[4]
            {
                new Vector3(originX, originY, 0f), // 0: low-left
                new Vector3(originX + w, originY, 0f), // 1: low-right
                new Vector3(originX, originY + h, 0f), // 2: top-left
                new Vector3(originX + w, originY + h, 0f), // 3: top-right
            };

            float vBottom = flipY ? 1f : 0f;
            float vTop = flipY ? 0f : 1f;
            var uvs = new Vector2[4]
            {
                new Vector2(0f, vBottom),
                new Vector2(1f, vBottom),
                new Vector2(0f, vTop),
                new Vector2(1f, vTop),
            };

            var indices = new int[6] { 0, 2, 1, 2, 3, 1 };

            if (_mesh == null) _mesh = new Mesh { name = "PixelScreen" };
            else _mesh.Clear();

            _mesh.SetVertices(vertices);
            _mesh.SetUVs(0, uvs);
            _mesh.SetTriangles(indices, 0);
            _mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        private void EnsureMaterial()
        {
            if (material == null)
            {
                var shader = Shader.Find("SoftwareRasterizer/FramebufferBlit");
                if (shader != null)
                    material = new Material(shader) { name = "PixelScreen (auto)" };
                else
                    Debug.LogWarning("PixelScreen: shader \"SoftwareRasterizer/FramebufferBlit\" not found; " +
                                     "assign a textured material manually.", this);
            }

            if (material != null)
            {
                material.mainTexture = _texture;
                GetComponent<MeshRenderer>().sharedMaterial = material;
            }
        }
    }
}