using System;

namespace Generators
{
    [Flags]
    public enum ClassFlags
    {
        None = 0,
        Static = 1,
        Partial = 2,
        Sealed = 4
    };
}
