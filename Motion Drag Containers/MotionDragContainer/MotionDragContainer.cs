using CommunityToolkit.WinUI;
using Get.Data.Bundles;
namespace Gtudios.UI.MotionDragContainers;
[AutoProperty]
public partial class MotionDragContainer<T> : TemplateControl<Grid>
{
    public IProperty<double> EndPaddingProperty { get; } = Auto(0d);
    public IProperty<IUpdateCollection<T>> TargetCollectionProperty { get; }
        = Auto<IUpdateCollection<T>>(new UpdateCollection<T>());
    public IProperty<IDataTemplate<T, MotionDragItem<T>>> ItemTemplateProperty { get; }
        = Auto<IDataTemplate<T, MotionDragItem<T>>>(new DataTemplate<T, MotionDragItem<T>>(x => 
            new MotionDragItem<T>
            {
                ContentBundle = new ContentBundle<T, UIElement>(x.CurrentValue)
                {
                    ContentTemplate = DataTemplates.TextBlockUIElement<T>(),
                }
            }
            .WithCustomCode(
                mdi =>
                    mdi.ContentBundle!.ContentProperty.BindOneWay(x)
            )
        ));
    public IProperty<Orientation> ReorderOrientationProperty { get; } = Auto<Orientation>(default);
    public IProperty<MotionDragConnectionContext<T>?> ConnectionContextProperty { get; }
        = Auto<MotionDragConnectionContext<T>?>(new());
    public MotionDragContainer()
    {
        AllowDrop = true;
        AnimationController = new(this);
        ConnectionContextProperty.ValueChanging += OnConnectionContextChanged;
        Loaded += MotionDragContainer_Loaded;
        Unloaded += MotionDragContainer_Unloaded;
        var ChildContainers = new UpdateCollection<MotionDragItem<T>>();
        var itemsBundle = new ItemsBundle<T, MotionDragItem<T>>();
        itemsBundle.ItemsSourceProperty.BindOneWay(TargetCollectionProperty);
        itemsBundle.ItemTemplateProperty!.BindOneWay(ItemTemplateProperty);
        this.ChildContainers = itemsBundle.OutputContent;
    }

    //int curItemIndex;
    //int curHoverItemIndex;
    //double tranOffset;
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
        var itemIdx = ChildContainers.IndexOf(item);
        if (itemIdx >= 0)
            return TargetCollection[itemIdx];
        return default;
    }
    public IGDReadOnlyCollection<MotionDragItem<T>> ChildContainers { get; }
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
    double[]? positionsreal;
    double[]? positionsthreshold;
    public Point PositionOfItemAtIndex(int index)
    {
        bool afterLast = false;
        if (index >= self.TargetCollection.Count)
        {
            afterLast = true;
            index = self.TargetCollection.Count - 1;
        }
        if (self.ChildContainers[index] is not { } curItem || curItem.FindDescendantOrSelf<MotionDragItem<T>>() is not { } st)
            throw new InvalidOperationException();
        if (positions is null) UpdateAnimated();
        var position = curItem.TransformToVisual(self.Container).TransformPoint(default);
        if (afterLast)
        {
            return self.ReorderOrientation is Orientation.Horizontal ?
                new(position.X + curItem.ActualSize.X, positions[index]) :
                new(positions[index], position.Y + curItem.ActualSize.Y);
        }
        else
        {
            return self.ReorderOrientation is Orientation.Horizontal ?
                new(positions[index], position.Y) :
                new(position.X, positions[index]);
        }
    }
    [MemberNotNull(nameof(positions))]
    [MemberNotNull(nameof(positionsremoved))]
    void UpdateAnimated()
    {
        var orientation = self.ReorderOrientation;

        Point PointAt(double tran) => orientation is Orientation.Horizontal ? new(tran, 0) : new(0, tran);
        double TranAt(Point pt) => orientation is Orientation.Horizontal ? pt.X : pt.Y;

        var shift = PointAt(shiftAmount);
        var itemCount = self.TargetCollection.Count;
        if (positions is null || positions.Length != itemCount + 1)
            positions = new double[itemCount + 1];
        if (positionsremoved is null || positionsremoved.Length != itemCount + 1)
            positionsremoved = new double[itemCount + 1];
        if (positionsthreshold is null || positionsthreshold.Length != itemCount - 1)
            positionsthreshold = new double[itemCount== 0 ? 0 : itemCount - 1];
        if (positionsreal is null || positionsreal.Length != itemCount)
            positionsreal = new double[itemCount];
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
            positions[i] = positionsremoved[i] = positionsreal[i] = TranAt(position) + TranAt(translationAmount);
            // positions[i] = TranAt(position) + TranAt(translationAmount);
        }
        if (itemCount >= 1 && self.ChildContainers[itemCount - 1] is { } lastItem)
        {
            positions[itemCount] = positions[itemCount - 1] +
                    (orientation is Orientation.Horizontal ? lastItem.ActualSize.X : lastItem.ActualSize.Y);
            positionsremoved[itemCount] = positionsremoved[itemCount - 1] +
                (orientation is Orientation.Horizontal ? lastItem.ActualSize.X : lastItem.ActualSize.Y);
        }
        {
            // positions = real position here
            // update the threshold based on the real position
            if (startRemoveIndex >= itemCount)
            {
                for (int i = positionsthreshold.Length - 1; i >= 0; i--)
                {
                    positionsthreshold[i] = (positions[i] + positions[i + 1]) / 2;
                }
            } else
            {
                for (int i = startRemoveIndex - 1; i >= 0; i--)
                {
                    positionsthreshold[i] = (positions[i] + positions[i + 1]) / 2;
                }
                for (int i = startRemoveIndex; i < positionsthreshold.Length; i++)
                {
                    positionsthreshold[i] = (positions[i + 1] + positions[i + 2]) / 2;
                }
            }
        }
        // iterate backwards so that we can read the previous value
        for (int i = itemCount; i > startRemoveIndex; i--)
        {
            positions[i] -= positions[i] - positions[i - 1];
            positionsremoved[i] -= positionsremoved[i] - positionsremoved[i - 1];
        }
        for (int i = startRemoveIndex >= itemCount ? (startShiftIndex) : (
            startRemoveIndex > startShiftIndex ? startShiftIndex : startShiftIndex + 1
        ); i < positions.Length; i++)
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
        startRemoveIndex = startShiftIndex = self.TargetCollection.Count;
        shiftAmount = 0;
        positions = positionsremoved = positionsthreshold = null;
        int i = 0;
        while (i < self.ChildContainers.Count && self.ChildContainers[i++].FindDescendantOrSelf<MotionDragItem<T>>() is { } st2)
        {
            st2.ResetTranslationImmedietly();
        }
    }
    public int IndexOfItemAt(double posX, double posY)
    {
        if (positionsthreshold is null) UpdateAnimated();
        SelfNote.DebugBreakOnShift();
        var pos = self.ReorderOrientation is Orientation.Vertical ? posY : posX;
        var idx = Array.BinarySearch(positionsthreshold!, pos);
        if (idx < 0)
        {
            idx = ~idx;
        }
        return idx;
    }
}