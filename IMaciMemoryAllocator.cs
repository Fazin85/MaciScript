namespace MaciScript
{
    public interface IMaciMemoryAllocator
    {
        int Alloc(int size);
        int Realloc(int ptr, int newSize);
        void Free(int ptr);
    }
}
