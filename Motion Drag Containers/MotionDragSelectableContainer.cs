#nullable enable
using CommunityToolkit.WinUI;
using Get.Data.Bindings.Linq;
using Get.Data.Collections;
using Get.Data.Collections.Update;

namespace Gtudios.UI.MotionDragContainers;
//[DependencyProperty(typeof(bool), "AllowMultipleSelection", GenerateLocalOnPropertyChangedMethod = true, UseNullableReferenceType = true)]
[AutoProperty]
public partial class MotionDragSelectableContainer<T> : MotionDragContainer<T>, ISelectableContainer<T?>
{
    public IProperty<SelectionManagerMutable<T>> SelectionManagerProperty { get; } = Auto<SelectionManagerMutable<T>>(new());
    int SelectedIndex
    {
        get => SelectionManager.SelectedIndex;
        set => SelectionManager.SelectedIndex = value;
    }
    int ISelectableContainer<T?>.SelectedIndex
    {
        get => SelectedIndex;
        set => SelectedIndex = value;
    }
    public MotionDragSelectableContainer()
    {
        TargetCollectionProperty.BindOneWay(SelectionManagerProperty.Select(x => x.Collection));
        TargetCollectionProperty.ValueChanged += (_, newVal) =>
        {
            if (newVal != SelectionManager.Collection)
            {
                throw new InvalidOperationException(
                    "Collection should not be modified manually. " +
                    "It is automatically set to SelectionManager.Collection.");
            }
        };
        SelectionManagerProperty.SelectPath(x => x.SelectedIndexProperty).ValueChanged += OnSelectedIndexChanged;
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
        var idx = SelectedIndex;
        if (idx < 0) return false;
        if (idx > ChildContainers.Count) return false;
        return ChildContainers[SelectedIndex]?
            .FindDescendantOrSelf<MotionDragSelectableItem<T>>(
                x => x == item
            ) is not null;
    }
    protected override void OnItemDroppingFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {
        //cachedSelectedIndex = SelectedIndex;
        //SelectedIndex = -1;
        base.OnItemDroppingFromAnotherContainer(sender, item, senderIndex, newIndex);
    }
    protected override void OnItemDropFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {
        // let's see if we need to select the new item or not
        if (sender is ISelectableContainer<T> other && other.SelectedIndex == senderIndex)
        {
            SelectedIndex = newIndex;
            other.SelectedIndex = -1;
        }
        base.OnItemDropFromAnotherContainer(sender, item, senderIndex, newIndex);
    }
    bool shouldSelect;
    protected override void OnItemMovingInContainer(int oldIndex, int newIndex)
    {
        shouldSelect = SelectedIndex == oldIndex;
        if (shouldSelect)
            PausePreferAlwaysSelectItemProperty = true;
        base.OnItemMovingInContainer(oldIndex, newIndex);
    }
    protected override void OnItemMovedInContainer(int oldIndex, int newIndex)
    {
        if (shouldSelect) SelectedIndex = newIndex;
        if (shouldSelect)
            PausePreferAlwaysSelectItemProperty = false;
    }
    bool PausePreferAlwaysSelectItemProperty;

    T? ISelectableContainer<T?>.SelectedValue { get => SelectionManager.SelectedValue; set => SelfNote.ThrowNotImplemented(); }
}
interface ISelectableContainer<T>
{
    int SelectedIndex { get; set; }
    T SelectedValue { get; set; }
}