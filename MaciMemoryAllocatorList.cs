namespace MaciScript
{
    public class MaciMemoryAllocatorList : IMaciMemoryAllocator
    {
        private readonly List<byte[]> memoryBlocks;
        private readonly Queue<int> freeIndices;

        public MaciMemoryAllocatorList()
        {
            memoryBlocks = [];
            freeIndices = [];
        }

        public int Alloc(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");
            }

            byte[] newBlock = new byte[size];

            // If we have free indices, reuse one of them
            if (freeIndices.Count > 0)
            {
                int index = freeIndices.Dequeue();
                memoryBlocks[index] = newBlock;
                return index;
            }

            // Otherwise, add to the end of the list
            memoryBlocks.Add(newBlock);
            return memoryBlocks.Count - 1;
        }

        public int Realloc(int ptr, int newSize)
        {
            if (ptr < 0 || ptr >= memoryBlocks.Count || memoryBlocks[ptr] == null)
            {
                throw new ArgumentOutOfRangeException(nameof(ptr), "Invalid memory pointer.");
            }

            if (newSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize), "Size must be positive.");
            }

            byte[] currentBlock = memoryBlocks[ptr];
            byte[] newBlock = new byte[newSize];

            // Copy the contents of the old block to the new one
            int copyLength = Math.Min(currentBlock.Length, newSize);
            Array.Copy(currentBlock, newBlock, copyLength);

            // Replace the old block with the new block
            memoryBlocks[ptr] = newBlock;

            return ptr;
        }

        public void Free(int ptr)
        {
            if (ptr < 0 || ptr >= memoryBlocks.Count || memoryBlocks[ptr] == null)
            {
                throw new ArgumentOutOfRangeException(nameof(ptr), "Invalid memory pointer.");
            }

#nullable disable
            // Mark the block as null (but keep the index in the list)
            memoryBlocks[ptr] = null;
#nullable enable

            // Add this index to our free list for reuse
            freeIndices.Enqueue(ptr);
        }

        public byte[] GetMemoryBlock(int ptr)
        {
            if (ptr < 0 || ptr >= memoryBlocks.Count || memoryBlocks[ptr] == null)
            {
                throw new ArgumentOutOfRangeException(nameof(ptr), "Invalid memory pointer.");
            }

            return memoryBlocks[ptr];
        }

        public int GetSize(int ptr)
        {
            if (ptr < 0 || ptr >= memoryBlocks.Count || memoryBlocks[ptr] == null)
            {
                throw new ArgumentOutOfRangeException(nameof(ptr), "Invalid memory pointer.");
            }

            return memoryBlocks[ptr].Length;
        }

        public int AllocatedCount => memoryBlocks.Count - freeIndices.Count;

        public int FreeCount => freeIndices.Count;
    }
}
