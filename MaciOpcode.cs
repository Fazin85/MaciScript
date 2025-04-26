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

        Fadd,
        Fsub,
        Fmul,
        Fdiv,

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
        Jmpf,
        Jef,
        Jnef,
        Jgf,
        Jlf,
        Call,
        Ret,

        // System
        Syscall,
        Ldstr,
        Push,
        Pop
    }
}
