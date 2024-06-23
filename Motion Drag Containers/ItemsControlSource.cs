
using System;
using System.Runtime.CompilerServices;

namespace Get.UI.Controls.Containers;

readonly struct ItemsControlSource(ItemsControl itemsControl) : IList
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ItemsControlSource FromContainer(ItemsControl container)
        => new(container);
    
    ItemCollection Items
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => itemsControl.Items;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IList? GetIList() => itemsControl.ItemsSource as IList;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IList<object?> GetIListObject() => itemsControl.ItemsSource as IList<object?> ?? Items;
    public object? this[int index] {
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
        var itemsSource = itemsControl.ItemsSource;
        if (itemsSource is not null && itemsSource is not INotifyCollectionChanged)
        {
            // we need to notify
            itemsControl.ItemsSource = null;
            itemsControl.ItemsSource = itemsSource;
        }
    }

    public bool IsSynchronized => GetIList()?.IsSynchronized ?? itemsControl
#if WINDOWS_UWP
        .Dispatcher
#else
        .DispatcherQueue
#endif
        .HasThreadAccess;

    public object SyncRoot => GetIList()?.SyncRoot ?? itemsControl
#if WINDOWS_UWP
        .Dispatcher
#else
        .DispatcherQueue
#endif
        ;

    public int Add(object? value)
    {
        var toReturn = GetIList()?.Add(value);
        if (!toReturn.HasValue)
        {
            var iListObj = GetIListObject();
            iListObj.Add(value);
            toReturn = iListObj.Count;
        }
        Notify();
        return toReturn.Value;
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

    public void CopyTo(Array array, int index)
    {
        if (GetIList() is { } l) l.CopyTo(array, index);
        else GetIListObject().CopyTo((object?[])array, index);
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

    public void Remove(object? value)
    {
        if (GetIList() is { } l) l.Remove(value);
        else GetIListObject().Remove(value);
        Notify();
    }

    public void RemoveAt(int index)
    {
        if (GetIList() is { } l) l.RemoveAt(index);
        else GetIListObject().RemoveAt(index);
        Notify();
    }
}