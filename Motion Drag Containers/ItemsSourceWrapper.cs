
using System.Linq;
using System.Runtime.CompilerServices;

namespace Get.UI.Controls.Containers;

readonly struct ItemsSourceWrapper(object? itemsSource, Action notify) : IList<object?>
{
    static readonly IList<object?> Default = Array.Empty<object?>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IList? GetIList() => itemsSource as IList;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IList<object?> GetIListObject() => itemsSource as IList<object?> ?? Default;
    public object? this[int index]
    {
        get
        {
            if (GetIList() is { } l) return l[index];
            else return GetIListObject()[index];
        }
        set
        {
            if (GetIList() is { } l) l[index] = value;
            else GetIListObject()[index] = value;
            Notify();
        }
    }

    public bool IsFixedSize => GetIList()?.IsFixedSize ?? false;
    public bool IsReadOnly => GetIList()?.IsReadOnly ?? GetIListObject().IsReadOnly;
    public int Count => GetIList()?.Count ?? GetIListObject().Count;
    void Notify()
    {
        if (itemsSource is not null && itemsSource is not INotifyCollectionChanged)
        {
            notify();
        }
    }

    public void Add(object? value)
    {
        var toReturn = GetIList()?.Add(value);
        if (!toReturn.HasValue)
        {
            var iListObj = GetIListObject();
            iListObj.Add(value);
            toReturn = iListObj.Count;
        }
        Notify();
    }

    public void Clear()
    {
        if (GetIList() is { } l) l.Clear();
        else GetIListObject().Clear();
        Notify();
    }

    public bool Contains(object? value)
    {
        return GetIList()?.Contains(value) ?? GetIListObject().Contains(value);
    }

    public void CopyTo(object?[] array, int index)
    {
        if (GetIList() is { } l) l.CopyTo(array, index);
        else GetIListObject().CopyTo(array, index);
    }

    public IEnumerator GetEnumerator()
    {
        return GetIList()?.GetEnumerator() ?? GetIListObject().GetEnumerator();
    }

    public int IndexOf(object? value)
    {
        return GetIList()?.IndexOf(value) ?? GetIListObject().IndexOf(value);
    }

    public void Insert(int index, object? value)
    {
        if (GetIList() is { } l) l.Insert(index, value);
        else GetIListObject().Insert(index, value);
        Notify();
    }

    public bool Remove(object? value)
    {
        bool toReturn = true;
        if (GetIList() is { } l)
        {
            l.Remove(value);
        }
        else toReturn = GetIListObject().Remove(value);
        Notify();
        return toReturn;
    }

    public void RemoveAt(int index)
    {
        if (GetIList() is { } l) l.RemoveAt(index);
        else GetIListObject().RemoveAt(index);
        Notify();
    }



    IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
        => GetIListObject().GetEnumerator() ?? Convert(GetIList());
    static IEnumerator<object?> Convert(IEnumerable? objects)
    {
        if (objects is null) yield break;
        foreach (var a in objects)
        {
            yield return a;
        }
    }
}