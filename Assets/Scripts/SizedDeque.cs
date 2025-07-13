using System.Collections.Generic;

public enum End
{
    FRONT,
    BACK
}

public class SizedDeque<T>
{
    private LinkedList<T> list = new LinkedList<T>();
    private int maxSize;

    public SizedDeque(int size)
    {
        maxSize = size;
        for (int i = 0; i < maxSize; i++)
        {
            list.AddLast(default(T));
        }
    }

    public void AddToFront(T item)
    {
        list.AddFirst(item);
        Trim(End.BACK);
    }

    public void AddToBack(T item)
    {
        list.AddLast(item);
        Trim(End.FRONT);
    }

    private void Trim(End end)
    {
        while (list.Count > maxSize)
        {
            if (end == End.FRONT)
            {
                list.RemoveFirst();
            }
            else if (end == End.BACK)
            {
                list.RemoveLast();
            }
            else
            {
                throw new System.ArgumentException("Invalid end specified for trimming.");
            }
        }
    }

    public void Clear()
    {
        list.Clear();
    }

    public IEnumerable<T> Items => list;

    public int Count => list.Count;

    public T[] ToArray()
    {
        T[] array = new T[list.Count];
        list.CopyTo(array, 0);
        return array;
    }
}
