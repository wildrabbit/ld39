using System.Collections;
using System.Collections.Generic;

public class PairingHeap<T>
    where T: class
{
    class Entry
    {
        public T item;
        public int priority;

        public Entry(T _item, int _priority)
        {
            item = _item;
            priority = _priority;
        }
    }

    Entry root = null;
    List<PairingHeap<T>> children = null;

    public bool Empty
    {
        get
        {
            return Count == 0;
        }
    }

    public int Count
    {
        get
        {
            if (root == null)
            {
                return 0;
            }
            int aggregate = 1;
            for (int i = 0; i < children.Count; ++i)
            {
                aggregate += children[i].Count;
            }
            return aggregate;
        }
    }

    public PairingHeap()
    {
        root = null;
        children = null;
    }

    public PairingHeap(T item, int priority)
    {
        Entry e = new Entry(item, priority);
        children = new List<PairingHeap<T>>();
    }

    public void Insert(T item, int priority)
    {
        PairingHeap<T> aux = new PairingHeap<T>(item, priority);
        Merge(aux);
    }

    void Merge(PairingHeap<T> other)
    {
        if (Empty && !other.Empty)
        {
            root = other.root;
            children = other.children;
        }
        else if (!Empty && !other.Empty)
        {
            int ourPrio = root.priority;
            int otherPrio = other.root.priority;
            if (ourPrio < otherPrio)
            {
                children.Add(other);
            }
            else
            {
                PairingHeap<T> us = new PairingHeap<T>(root.item, root.priority);
                us.children = new List<PairingHeap<T>>(children);
                
                root.item = other.root.item;
                root.priority = other.root.priority;
                children = other.children;
                children.Add(us);
            }
        }        
    }

    public T Peek()
    {
        if (Count == 0)
        {
            return default(T);
        }
        return root.item;
    }

    public T RemoveTop()
    {
        if (Count == 0)
        {
            return default(T);
        }
        else
        {
            MergePairs(children);
            return Peek();
        }
    }

    public void UpdateKey(T item, int newPrio)
    {
        if (item == root.item && newPrio < root.priority)
        {
            root.priority = newPrio;
        }
        for (int i = 0; i < children.Count; ++i)
        {
            if (children[i].root.item == item)
            {
                if (newPrio >= children[i].root.priority) continue;
                PairingHeap<T> child = children[i];
                children.Remove(child);
                child.root.priority = newPrio;
                Merge(child);
                return;
            }
            else
            {
                children[i].UpdateKey(item, newPrio);
            }
        }
    }

    void MergePairs(List<PairingHeap<T>> l)
    {
        if (l.Count == 0)
        {
            return;
        }
        else if (l.Count == 1)
        {
            root.item = l[0].root.item;
            root.priority = l[0].root.priority;

            children = new List<PairingHeap<T>>(l[0].children);
        }
        else
        {
            l[0].Merge(l[1]);
            l[0].MergePairs(l.GetRange(2, l.Count - 2));
        }
    }

    public void Clear()
    {
        root = null;
        for (int i = 0; i < children.Count; ++i)
        {
            children.Clear();
        }
        children = null;
    }

}
