
using Get.Data.Collections;
using Get.Data.Collections.Conversion;
using Get.UI.Controls.Panels;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;
public record struct MotionDragContainerTempalteParts(
    Grid Root,
    Popup Popup,
    UserControl Display,
    OrientedStackForContainer Container
);
partial class MotionDragContainer<T>
{
    public ExternalControlTemplate<MotionDragContainerTempalteParts, MotionDragContainer<T>, Grid> ControlTemplate { get; set; } = DefaultTemplate;
    MotionDragContainerTempalteParts TempalteParts;
    protected override IGDCollection<UIElement> InitializeWithChildren(Grid TemplatedParent)
    {
        TempalteParts = ControlTemplate(this, TemplatedParent);
        // ...
        return Container.Children.AsGDCollection();
    }
    Grid Root => TempalteParts.Root;
    Popup Popup => TempalteParts.Popup;
    UserControl Display => TempalteParts.Display;
    internal OrientedStackForContainer Container => TempalteParts.Container;
}