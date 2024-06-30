#nullable enable
using CommunityToolkit.WinUI;
using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Gtudios.UI.MotionDragContainers;
//[DependencyProperty(typeof(bool), "AllowMultipleSelection", GenerateLocalOnPropertyChangedMethod = true, UseNullableReferenceType = true)]
public partial class MotionDragSelectableContainer<T> : MotionDragContainer<T>, ISelectableContainer<T?>
{
    public MotionDragSelectableContainer()
    {
        SelectedIndexProperty.ValueChanged += OnSelectedIndexChanged;
    }
    void OnSelectedIndexChanged(int oldValue, int newValue)
    {
        if (oldValue is >= 0 && ChildContainers[oldValue] is UIElement container)
        {
            if (Canvas.GetZIndex(container) is 2)
                Canvas.SetZIndex(container, 0);
            if (container.FindDescendantOrSelf<MotionDragSelectableItem<T>>() is { } item)
                item.IsPrimarySelected = false;
        }
        if (newValue is >= 0 && ChildContainers[newValue] is UIElement container2)
        {
            if (Canvas.GetZIndex(container2) is 0)
                Canvas.SetZIndex(container2, 2);
            if (container2.FindDescendantOrSelf<MotionDragSelectableItem<T>>() is { } item2)
                item2.IsPrimarySelected = true;
        }
    }
    internal bool RequestPrimarySelect(MotionDragSelectableItem<T> item)
    {
        if (!item.IsSelectableItemKind) throw new NotSupportedException();
        var idx = ChildContainers.IndexOf(item);
        
        if (idx is not -1)
            SelectedIndex = idx;
        return idx is not -1;
    }
    internal bool RequestSelect(MotionDragSelectableItem<T> item) => RequestSelect(item, false, false);
    internal bool RequestSelect(MotionDragSelectableItem<T> item, bool shift, bool ctrl)
    {
        if (!item.IsSelectableItemKind) throw new NotSupportedException();
        if (shift is false && ctrl is false)
        {
            return RequestPrimarySelect(item);
        }
        return false;
    }
    internal bool IsPrimarySelected(MotionDragSelectableItem<T> item)
    {
        return ChildContainers[SelectedIndex]?
            .FindDescendantOrSelf<MotionDragSelectableItem<T>>(
                x => x == item
            ) is not null;
    }
    int cachedSelectedIndex;
    protected override void OnItemDroppingFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {
        cachedSelectedIndex = SelectedIndex;
        SelectedIndex = -1;
        base.OnItemDroppingFromAnotherContainer(sender, item, senderIndex, newIndex);
    }
    protected override void OnItemDropFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {
        // adjust the current selection to the correct item
        if (newIndex <= cachedSelectedIndex)
            SelectedIndex = cachedSelectedIndex + 1;
        else
            SelectedIndex = cachedSelectedIndex;
        // let's see if we need to select the new item or not
        if (sender is ISelectableContainer<T> other && other.SelectedIndex == senderIndex)
        {
            SelectedIndex = newIndex;
            other.SelectedIndex = -1;
        }
        base.OnItemDropFromAnotherContainer(sender, item, senderIndex, newIndex);
    }
    int? newSelectionIndex = null;

    T? ISelectableContainer<T?>.SelectedValue { get => SelectedValue; set => SelfNote.ThrowNotImplemented(); }

    protected override void OnItemMovingInContainer(int oldIndex, int newIndex)
    {
        if (ChildContainers[oldIndex] is { } a &&
            (a.FindDescendantOrSelf<MotionDragSelectableItem<T>>()?.IsPrimarySelected ?? false))
        {
            newSelectionIndex = newIndex;
        } else
        {
            newSelectionIndex = null;
        }
        base.OnItemMovingInContainer(oldIndex, newIndex);
    }
    protected override void OnItemMovedInContainer(int oldIndex, int newIndex)
    {
        if (newSelectionIndex.HasValue)
            SelectedIndex = newIndex;
        base.OnItemMovedInContainer(oldIndex, newIndex);
    }
}
interface ISelectableContainer<T>
{
    int SelectedIndex { get; set; }
    T SelectedValue { get; set; }
}