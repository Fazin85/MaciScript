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
            byte[] bytes = new byte[(arr.Length * sizeof(int)) + sizeof(int)];

            byte[] topBytes = BitConverter.GetBytes(top);
            bytes[0] = topBytes[0];
            bytes[1] = topBytes[1];
            bytes[2] = topBytes[2];
            bytes[3] = topBytes[3];

            for (int i = 4; i < bytes.Length; i += 4)
            {
                byte[] intBytes = BitConverter.GetBytes(arr[i / 4]);
                bytes[i] = intBytes[0];
                bytes[i + 1] = intBytes[1];
                bytes[i + 2] = intBytes[2];
                bytes[i + 3] = intBytes[3];
            }

            return bytes;
        }
    }
}
