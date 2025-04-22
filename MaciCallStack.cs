namespace MaciScript
{
    public struct MaciCallStack(int maxSize)
    {
        private readonly int[] arr = new int[maxSize];
        private int top = -1;

        public void Push(int value)
        {
            if (top >= arr.Length - 1)
            {
                throw new Exception("Stack overflow");
            }

            arr[++top] = value;
        }

        public int Pop()
        {
            if (top < 0)
            {
                throw new Exception("Stack underflow");
            }

            return arr[top--];
        }

        public readonly byte[] ToByteArray()
        {
            int totalSize = sizeof(int) + sizeof(int) + (arr.Length * sizeof(int));
            byte[] result = new byte[totalSize];

            BitConverter.GetBytes(top).CopyTo(result, 0);

            BitConverter.GetBytes(arr.Length).CopyTo(result, sizeof(int));

            for (int i = 0; i < arr.Length; i++)
            {
                BitConverter.GetBytes(arr[i]).CopyTo(result, (i * sizeof(int)) + (2 * sizeof(int)));
            }

            return result;
        }

        public static MaciCallStack FromByteArray(byte[] data)
        {
            int top = BitConverter.ToInt32(data, 0);
            int arrayLength = BitConverter.ToInt32(data, sizeof(int));

            MaciCallStack stack = new(arrayLength)
            {
                top = top
            };

            for (int i = 0; i < arrayLength; i++)
            {
                stack.arr[i] = BitConverter.ToInt32(data, (i * sizeof(int)) + (2 * sizeof(int)));
            }

            return stack;
        }
    }
}
