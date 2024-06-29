using CommunityToolkit.WinUI;
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.UI.Data;
using Gtudios.UI.MotionDrag;
using Get.EasyCSharp;
namespace Gtudios.UI.MotionDragContainers;

public partial class MotionDragContainer<T> : TwoWaySelectableItemsTemplatedControl<Grid, T, MotionDragItem<T>>
{
    public Property<Orientation> ReorderOrientationProperty { get; } = new(default);
    public Property<MotionDragConnectionContext<T>?> ConnectionContextProperty { get; } = new(new());
    public MotionDragConnectionContext<T>? ConnectionContext
    {
        get => ConnectionContextProperty.Value;
        set => ConnectionContextProperty.Value = value;
    }
    public MotionDragContainer()
    {
        AllowDrop = true;
        AnimationController = new(this);
        ConnectionContextProperty.ValueChanging += OnConnectionContextChanged;
        Loaded += MotionDragContainer_Loaded;
        Unloaded += MotionDragContainer_Unloaded;
    }

    

    //int curItemIndex;
    //int curHoverItemIndex;
    //double tranOffset;
    internal int SafeIndexFromContainer(DependencyObject? obj)
    {
        var eles = obj.FindAscendants().GetEnumerator();
        DependencyObject cur, next;
        do
        {
            if (!eles.MoveNext()) return -1;
            cur = eles.Current;
            if (!eles.MoveNext()) return -1;
            next = eles.Current;
        } while (next != Container);
        if (cur is not UIElement ele) return -1;
        return Container.Children.IndexOf(ele);
    }
    internal int SafeIndexFromMotionDragItem(MotionDragItem<T>? ele) => SafeIndexFromContainer(ele);
    //bool DidSetZIndex = false;
    MotionDragItem<T>? CurrentManipulationItem;
    void OnConnectionContextChanged(MotionDragConnectionContext<T>? oldValue, MotionDragConnectionContext<T>? newValue)
    {

        if (oldValue is not null)
            MotionDragConnectionContext<T>.UnsafeRemove(oldValue, this);
        if (IsLoaded && newValue is not null)
            MotionDragConnectionContext<T>.UnsafeAdd(newValue, this);
    }
    readonly MotionDragReorderContainerController<T> AnimationController;
    internal T? ObjectFromSRI(MotionDragItem<T> item)
    {
        var itemIdx = SafeIndexFromContainer(item);
        if (itemIdx >= 0)
            return ItemsSourceProperty[itemIdx];
        return default;
    }
}
partial class MotionDragReorderContainerController<T>(MotionDragContainer<T> self)
{
    [Property(OnChanged = nameof(UpdateAnimated))]
    int startRemoveIndex;
    [Property(OnChanged = nameof(UpdateAnimated))]
    int startShiftIndex;
    [Property(OnChanged = nameof(UpdateAnimated))]
    double shiftAmount;
    double[]? positions;
    double[]? positionsremoved;
    public Point PositionOfItemAtIndex(int index)
    {
        bool afterLast = false;
        if (index >= self.ItemsSourceProperty.Count)
        {
            afterLast = true;
            index = self.ItemsSourceProperty.Count - 1;
        }
        if (self.ChildContainers[index] is not { } curItem || curItem.FindDescendantOrSelf<MotionDragItem<T>>() is not { } st)
            throw new InvalidOperationException();
        if (positions is null) UpdateAnimated();
        var position = curItem.TransformToVisual(self.Container).TransformPoint(default);
        if (afterLast)
        {
            return self.ReorderOrientationProperty.Value is Orientation.Horizontal ?
                new(position.X + curItem.ActualSize.X, positions[index]) :
                new(positions[index], position.Y + curItem.ActualSize.Y);
        }
        else
        {
            return self.ReorderOrientationProperty.Value is Orientation.Horizontal ?
                new(positions[index], position.Y) :
                new(position.X, positions[index]);
        }
    }
    [MemberNotNull(nameof(positions))]
    [MemberNotNull(nameof(positionsremoved))]
    void UpdateAnimated()
    {
        var orientation = self.ReorderOrientationProperty.Value;

        Point PointAt(double tran) => orientation is Orientation.Horizontal ? new(tran, 0) : new(0, tran);
        double TranAt(Point pt) => orientation is Orientation.Horizontal ? pt.X : pt.Y;

        var shift = PointAt(shiftAmount);
        var itemCount = self.ItemsSourceProperty.Count;
        if (positions is null || positions.Length != itemCount + 1)
            positions = new double[itemCount + 1];
        if (positionsremoved is null || positionsremoved.Length != itemCount + 1)
            positionsremoved = new double[itemCount + 1];
        for (int i = 0; i < itemCount; i++)
        {
            // do not translate the one we are showing on popup
            //if (i == startRemoveIndex)
            //{
            //    continue;
            //}
            if (self.ChildContainers[i] is not { } curItem || curItem.FindDescendantOrSelf<MotionDragItem<T>>() is not { } st)
                continue;

            var position = curItem.TransformToVisual(self.Container).TransformPoint(default);


            Point translationAmount = default;
            //if (i > 0 && i > startRemoveIndex)
            //    if (self.Containers[i - 1] is { } prevItem)
            //    {
            //        var d = curItem.TransformToVisual(prevItem).TransformPoint(default);
            //        translationAmount = translationAmount.Subtract(d);
            //    }
            positions[i] = positionsremoved[i] = TranAt(position) + TranAt(translationAmount);
            // positions[i] = TranAt(position) + TranAt(translationAmount);
        }
        if (self.ChildContainers[itemCount - 1] is { } lastItem)
        {
            positions[itemCount] = positions[itemCount - 1] +
                    (orientation is Orientation.Horizontal ? lastItem.ActualSize.X : lastItem.ActualSize.Y);
            positionsremoved[itemCount] = positionsremoved[itemCount - 1] +
                (orientation is Orientation.Horizontal ? lastItem.ActualSize.X : lastItem.ActualSize.Y);
        }
        // iterate backwards so that we can read the previous value
        for (int i = itemCount; i > startRemoveIndex; i--)
        {
            positions[i] -= positions[i] - positions[i - 1];
            positionsremoved[i] -= positionsremoved[i] - positionsremoved[i - 1];
        }
        for (int i = startShiftIndex; i < positions.Length; i++)
        {
            positions[i] += shiftAmount;
        }
        for (int i = 0; i < itemCount; i++)
        {
            if (i == startRemoveIndex)
                // do not play animation for the item we are dragging
                continue;
            if (self.ChildContainers[i] is not { } curItem || curItem.FindDescendantOrSelf<MotionDragItem<T>>() is not { } st)
                continue;
            var position = curItem.TransformToVisual(self.Container).TransformPoint(default);
            var translationAmount = PointAt(positions[i]).Point() - position;
            st.TemporaryAnimateTranslation(translationAmount.X, translationAmount.Y);
        }
    }
    public void Reset()
    {
        startRemoveIndex = startShiftIndex = self.ItemsSourceProperty.Count;
        shiftAmount = 0;
        positions = positionsremoved = null;
        int i = 0;
        while (self.ChildContainers[i++]?.FindDescendantOrSelf<MotionDragItem<T>>() is { } st2)
        {
            st2.ResetTranslationImmedietly();
        }
    }
    public int IndexOfItemAt(double posX, double posY)
    {
        //if (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) is Windows.UI.Core.CoreVirtualKeyStates.Down)
        //{
        //    Debugger.Break();
        //}
        if (positionsremoved is null) UpdateAnimated();
        SelfNote.DebugBreakOnShift();
        var pos = self.ReorderOrientationProperty.Value is Orientation.Vertical ? posY : posX;
        var idx = Array.BinarySearch(positionsremoved, pos);
        if (idx < 0)
        {
            idx = ~idx;
        }
        int clampedIdx = idx >= positionsremoved.Length ? positionsremoved.Length - 1 : idx;
        if (idx > 0 && pos < positionsremoved[clampedIdx])
        {
            clampedIdx--;
            idx = clampedIdx;
        }
        if (clampedIdx > 0 && positionsremoved[clampedIdx] == positionsremoved[clampedIdx - 1])
        {
            clampedIdx--;
            idx = clampedIdx;
        if (idx >= 1 && idx + 1 < positionsremoved.Length)
        {
            if (pos >= (positionsremoved[idx] + positionsremoved[idx + 1]) / 2)
            {
                idx++;
            }
            }
        }
        if (idx == startRemoveIndex + 2)
        {
            if (pos < (positionsremoved[idx] + positionsremoved[idx + 1]) / 2)
                idx--;
            //if (pos <= (positionsremoved[idx + 1] + positionsremoved[idx + 2]) / 2)
            //    idx--;
        }
        if (idx >= positionsremoved.Length) idx = positionsremoved.Length;
        return idx;
    }
}