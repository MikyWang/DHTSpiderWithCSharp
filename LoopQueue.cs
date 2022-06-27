namespace DHT;

public class LoopQueue<T>
{
    private int _head;
    private int _tail;
    private int _size;
    private T[] _array;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;
    public int Capcity => _size;

    public LoopQueue() : this(20) { }
    public LoopQueue(int size)
    {
        _size = size;
        _array = new T[size];
        _head = 0;
        _tail = 0;
        Count = 0;
    }

    public void EnQueue(T element)
    {
        Count++;
        Glow();
        _array[_tail] = element;
        _tail = (_tail + 1) % _size;
    }

    public T DeQueue()
    {
        if (IsEmpty) throw new IndexOutOfRangeException();

        _head++;
        Count--;
        return _array[_head - 1];
    }

    private void Glow()
    {
        if (Count < _size) return;

        var newSize = _size * 2;
        var extArray = new T[newSize];
        Array.Copy(_array, extArray, _size);
        _size = newSize;
        _array = extArray;
    }
}
