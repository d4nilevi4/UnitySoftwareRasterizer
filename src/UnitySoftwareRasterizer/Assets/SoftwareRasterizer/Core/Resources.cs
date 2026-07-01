using System;
using FFS.Libraries.StaticEcs;
using Unity.Collections;
using UnityEngine;

namespace SoftwareRasterizer.Core;

public struct FrameBuffer : IResource, IDisposable
{
    private NativeArray<float> _depthBuffer;
    private NativeArray<Color32> _colorBuffer;

    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;

    public NativeArray<float> DepthBuffer => _depthBuffer;
    public NativeArray<Color32> ColorBuffer => _colorBuffer;

    public FrameBuffer(int width, int height)
    {
        _width = width;
        _height = height;

        _depthBuffer = new NativeArray<float>(width * height, Allocator.Persistent);
        _colorBuffer = new NativeArray<Color32>(width * height, Allocator.Persistent);
    }

    public void ClearColor() => _colorBuffer.AsSpan().Clear();
    public void ClearDepth() => _depthBuffer.AsSpan().Clear();

    public void Dispose()
    {
        if (_depthBuffer.IsCreated) _depthBuffer.Dispose();
        if (_colorBuffer.IsCreated) _colorBuffer.Dispose();
    }
}
