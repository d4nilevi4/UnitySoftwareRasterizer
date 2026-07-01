using FFS.Libraries.StaticEcs;
using SoftwareRasterizer.Core;
using SoftwareRasterizer.Core.Systems;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SoftwareRasterizer.Demo
{
    public sealed class SoftwareRendererBehaviour : MonoBehaviour
    {
        public PixelScreen screen;
        
        public Mesh[] meshes;

        public Color32 color = new Color32(0, 200, 255, 255);
        
        private NativeArray<float3> _vertArray;
        private NativeArray<int> _triangleArray;

        private void Start()
        {
            W.Create(WorldConfig.Default());
            W.Types()
                .Component<ObjectMesh>()
                .Component<ObjectColor>();
            W.Initialize();

            W.SetResource(new FrameBuffer(screen.Width, screen.Height));

            DemoSys.Create();
            DemoSys.Add(new DrawSystem(), order: 0);
            DemoSys.Initialize();

            for (int i = 0; i < meshes.Length; i++)
            {
                var entity = W.NewEntity<Default>();
                entity.Set(new ObjectColor { Value = color });
                entity.Set(ObjectMesh.FromUnityMesh(meshes[i]));
            }
        }

        private void LateUpdate()
        {
            var frameBuffer = W.GetResource<FrameBuffer>();
            frameBuffer.ClearColor();
            frameBuffer.ClearDepth();
            DemoSys.Update();
            screen.SetPixels(frameBuffer.ColorBuffer.AsReadOnlySpan());
            screen.Apply();
        }

        private void OnDestroy()
        {
            _vertArray.Dispose();
            _triangleArray.Dispose();
            
            DemoSys.Destroy();
            W.GetResource<FrameBuffer>().Dispose();
            W.Destroy();
        }
    }
}