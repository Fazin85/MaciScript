using System.Diagnostics;

namespace MaciScript
{
    public class MaciMemoryAllocator<T> : IMaciMemoryAllocator where T : struct
    {
        private readonly Dictionary<int, T[]> allocations = [];
        private readonly Stack<int> freeIndicies = [];
        private int nextIndex = 0;

        public int Alloc(int size)
        {
            int index;
            if (freeIndicies.Count > 0)
            {
                index = freeIndicies.Pop();
            }
            else
            {
                index = nextIndex++;
            }

            allocations.Add(index, new T[size]);

            return index;
        }

        public void Free(int index)
        {
            if (allocations.Remove(index))
            {
                freeIndicies.Push(index);
            }
            else
            {
                throw new Exception($"Failed to remove memory at index {index}");
            }
        }

        public void Realloc(int index, int newSize)
        {
            T[] array = allocations[index];
            T[] newArray = new T[newSize];

            Debug.Assert(newArray.Length >= array.Length);

            Array.Copy(array, newArray, array.Length);

            allocations[index] = newArray;
        }
    }
}
