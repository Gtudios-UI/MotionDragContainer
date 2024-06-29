
using Get.Data.Collections;
using Get.Data.Collections.Conversion;
using Get.UI.Controls.Panels;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;
public record struct MotionDragContainerTempalteParts<T>(
    Grid Root,
    Popup Popup,
    UserControl Display,
    OrientedStackForContainer<T> Container
);
partial class MotionDragContainer<T>
{
    public ExternalControlTemplate<MotionDragContainerTempalteParts<T>, MotionDragContainer<T>, Grid> ControlTemplate { get; set; } = DefaultTemplate;
    MotionDragContainerTempalteParts<T> TempalteParts;
    protected override IGDCollection<MotionDragItem<T>> InitializeWithChildren(Grid TemplatedParent)
    {
        TempalteParts = ControlTemplate(this, TemplatedParent);
        // ...
        return new Wrapper(Container.Children.AsGDCollection());
    }
    readonly struct Wrapper(IGDCollection<UIElement> ele) : IGDCollection<MotionDragItem<T>>
    {
        public MotionDragItem<T> this[int index] { get => (MotionDragItem<T>)ele[index]; set => ele[index] = value; }

        public int Count => ele.Count;

        public void Insert(int index, MotionDragItem<T> item)
            => ele.Insert(index, item);

        public void RemoveAt(int index)
            => ele.RemoveAt(index);
    }
    Grid Root => TempalteParts.Root;
    Popup Popup => TempalteParts.Popup;
    UserControl Display => TempalteParts.Display;
    internal OrientedStackForContainer<T> Container => TempalteParts.Container;
}