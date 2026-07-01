// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable
using FFS.Libraries.StaticEcs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SoftwareRasterizer.Core;

public struct ObjectColor : IComponent
{
    public Color32 Value;
}

public readonly struct ObjectMesh : IComponent
{
    public readonly NativeArray<float3> Vertices;
    public readonly NativeArray<int> Triangles;

    public ObjectMesh(NativeArray<float3> vertices, NativeArray<int> triangles)
    {
        Vertices = vertices;
        Triangles = triangles;
    }

    public void OnDelete<TWorld>(World<TWorld>.Entity self, HookReason reason) where TWorld : struct, IWorldType
    {
        Vertices.Dispose();
        Triangles.Dispose();
    }

    public static ObjectMesh FromUnityMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        NativeArray<float3> vertArray = new NativeArray<float3>(vertices.Length, Allocator.Persistent);
        NativeArray<int> triangleArray = new NativeArray<int>(triangles.Length, Allocator.Persistent);

        vertArray.Reinterpret<Vector3>().CopyFrom(vertices);
        triangleArray.CopyFrom(triangles);

        return new ObjectMesh(vertArray, triangleArray);
    }
}