using FFS.Libraries.StaticEcs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SoftwareRasterizer.Core.Systems;

public struct DrawSystem : ISystem
{
    public void Update()
    {
        var frameBuffer = W.GetResource<FrameBuffer>();

        W.Query().For(frameBuffer, static (
            ref FrameBuffer frameBuffer,
            in ObjectMesh objectMesh,
            in ObjectColor objectColor
        ) =>
        {
            new RasterizeJob
            {
                Vertices = objectMesh.Vertices,
                Triangles = objectMesh.Triangles,
                Color = objectColor.Value,
                Angle = Time.time,
                DepthBuffer = frameBuffer.DepthBuffer,
                ColorBuffer = frameBuffer.ColorBuffer,
                Width = frameBuffer.Width,
                Height = frameBuffer.Height,
            }.Schedule().Complete();
        });
    }
}

[BurstCompile]
public struct RasterizeJob : IJob
{
    [ReadOnly] public NativeArray<float3> Vertices;
    [ReadOnly] public NativeArray<int> Triangles;

    public Color32 Color;

    public float Angle;

    public NativeArray<float> DepthBuffer;
    public NativeArray<Color32> ColorBuffer;

    public int Width;
    public int Height;

    public void Execute()
    {
        int halfWidth = Width / 2;
        int halfHeight = Height / 2;
        
        for (int t = 0; t < Triangles.Length; t += 3)
        {
            float3 va = Vertices[Triangles[t]];
            float3 vb = Vertices[Triangles[t + 1]];
            float3 vc = Vertices[Triangles[t + 2]];

            float3 ra = RotateXZ(va, Angle) * 15;
            float3 rb = RotateXZ(vb, Angle) * 15;
            float3 rc = RotateXZ(vc, Angle) * 15;

            float3 a = new float3(ra.x * halfWidth + halfWidth, ra.y * halfHeight + halfHeight, (ra.z + 1) / 2);
            float3 b = new float3(rb.x * halfWidth + halfWidth, rb.y * halfHeight + halfHeight, (rb.z + 1) / 2);
            float3 c = new float3(rc.x * halfWidth + halfWidth, rc.y * halfHeight + halfHeight, (rc.z + 1) / 2);

            int bbminx = math.max(0, (int)math.round(math.min(a.x, math.min(b.x, c.x))));
            int bbminy = math.max(0, (int)math.round(math.min(a.y, math.min(b.y, c.y))));
            int bbmaxx = math.min(Width - 1, (int)math.round(math.max(a.x, math.max(b.x, c.x))));
            int bbmaxy = math.min(Height - 1, (int)math.round(math.max(a.y, math.max(b.y, c.y))));

            int2 ia = (int2)math.round(a.xy);
            int2 ib = (int2)math.round(b.xy);
            int2 ic = (int2)math.round(c.xy);

            float tcp = math.determinant(new float2x2(ia - ib, ic - ib));

            if (tcp == 0f)
                continue;

            for (int x = bbminx; x <= bbmaxx; x++)
            {
                for (int y = bbminy; y <= bbmaxy; y++)
                {
                    int2 xy = new int2(x, y);
                    float a1 = math.determinant(new float2x2(xy - ib, ic - xy)) / tcp;
                    float a2 = math.determinant(new float2x2(xy - ic, ia - xy)) / tcp;
                    float a3 = math.determinant(new float2x2(xy - ia, ib - xy)) / tcp;

                    if (a1 < 0 || a2 < 0 || a3 < 0)
                        continue;

                    float z = a.z * a1 + b.z * a2 + c.z * a3;

                    int index = y * Width + x;
                    if (DepthBuffer[index] > z)
                        continue;

                    DepthBuffer[index] = z;
                    ColorBuffer[index] = new Color32((byte)(Color.r * z), (byte)(Color.g * z), (byte)(Color.b * z), Color.a);
                }
            }
        }

        float3 RotateXZ(float3 v, float angle)
        {
            float cos = math.cos(angle);
            float sin = math.sin(angle);
            
            return new float3(
                v.x * cos - v.z * sin,
                v.y,
                v.x * sin + v.z * cos
            );
        }
    }
}