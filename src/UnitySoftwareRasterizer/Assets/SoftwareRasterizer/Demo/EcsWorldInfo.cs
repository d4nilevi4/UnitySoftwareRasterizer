using FFS.Libraries.StaticEcs;
using SoftwareRasterizer.Core;

namespace SoftwareRasterizer.Demo
{
    public struct DemoSystems : ISystemsType
    {
    }

    public abstract class DemoSys : W.Systems<DemoSystems>
    {
    }
}