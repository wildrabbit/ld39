using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<T>
    where T: class
{
    PairingHeap<T> heap;

    public PriorityQueue()
    {

    }

    public void Enqueue(T item, int priority)
    {
        heap.Insert(item, priority);
    }

    public T Dequeue()
    {
        return heap.RemoveTop();
    }

    public T Peek()
    {
        return heap.Peek();
    }

    public void Clear()
    {
        heap.Clear();
    }

    public int Count()
    {
        return heap.Count;
    }
}
