namespace QS
{
	public class RingBuffer<T>
	{
        private readonly T[] buffer;
        private readonly int size, lastIndex;
        private int index;

        public RingBuffer(int length)
        {
            index = -1;
            size = length;
            lastIndex = size - 1;
            buffer = new T[size];
        }

        public void Add(T item)
        {
            if (++index < lastIndex)            
                buffer[index] = item;
            else
            {
                for (int i = 1; i < size; i++)
                    buffer[i - 1] = buffer[i];
                buffer[lastIndex] = item;
            }
        }

        public bool Get(out T item)
        {
            item = buffer[0];
            return (index >= 0);
        }
	}
}