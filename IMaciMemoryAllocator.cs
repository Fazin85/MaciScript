namespace MaciScript
{
    public interface IMaciMemoryAllocator
    {
        int Alloc(int size);
        void Realloc(int index, int newSize);
        void Free(int index);
    }
}
