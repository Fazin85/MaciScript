using System.Runtime.InteropServices;

namespace MaciScript
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MaciOperand
    {
        [FieldOffset(0)]
        private byte _flags;

        public bool IsReg
        {
            readonly get => (_flags & 0b0001) != 0;
            set => _flags = value ? (byte)(_flags | 0b0001) : (byte)(_flags & ~0b0001);
        }

        public bool IsSysReg
        {
            readonly get => (_flags & 0b0010) != 0;
            set => _flags = value ? (byte)(_flags | 0b0010) : (byte)(_flags & ~0b0010);
        }

        public bool IsImmediate
        {
            readonly get => (_flags & 0b0100) != 0;
            set => _flags = value ? (byte)(_flags | 0b0100) : (byte)(_flags & ~0b0100);
        }

        public bool IsFloat
        {
            readonly get => (_flags & 0b1000) != 0;
            set => _flags = value ? (byte)(_flags | 0b1000) : (byte)(_flags & ~0b1000);
        }

        [FieldOffset(4)]
        public int Value;
    }
}
