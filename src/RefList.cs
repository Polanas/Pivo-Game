using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class RefList<T>
{

    public int Count => _size;

    public T[] Items => _items;

    private T[] _items;

    private int _size;

    public RefList()
    {
        _items = new T[1];
    }

    public ref T this[int i]
    {
        get { return ref _items[i]; }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_size < _items.Length)
        {
            _items[_size] = item;
            _size++;

            return;
        }

        AddWithResize(item);
    }
    
    public void Remove(T item)
    {
        int index = IndexOf(item);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        _size--;
        _items[index] = default(T);

        Array.Copy(_items, index+1, _items, index, _size - index);
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, _size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Sort(IComparer<T> comparer)
    {
        Array.Sort(_items, comparer);
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (_size > 0)
            {
                Array.Clear(_items, 0, _size);
                return;
            }
        }
        else _size = 0;
    }

    private void AddWithResize(T item)
    {
        int newSize = _size * 2;

        Array.Resize(ref _items, newSize);
        _items[_size] = item;

        _size++;
    }
}