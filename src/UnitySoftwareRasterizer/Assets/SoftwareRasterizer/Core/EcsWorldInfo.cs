using FFS.Libraries.StaticEcs;

namespace SoftwareRasterizer.Core;

public struct WorldType : IWorldType {}
public sealed class W : World<WorldType> {}