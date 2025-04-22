namespace MaciScript
{
    public enum MaciOpcode : byte
    {
        // Data Movement
        Mov,
        Load,
        Store,

        // Arithmetic
        Add,
        Sub,
        Mul,
        Div,

        // Bitwise
        And,
        Or,
        Xor,
        Shl,
        Shr,

        // Comparison
        Cmp,

        // Control Flow
        Jmp,
        Je,
        Jne,
        Jg,
        Jl,
        Call,
        Ret,

        // System
        Syscall
    }
}