// cs3014-7.cs: `CLSClass.implicit operator CLSClass(byte)' cannot be marked as CLS-compliant because the assembly is not marked as CLS-compliant
// Line: 7

using System;

public abstract class CLSClass {
        [CLSCompliant (true)]
        public static implicit operator CLSClass(byte value) {
                return null;
        }
}