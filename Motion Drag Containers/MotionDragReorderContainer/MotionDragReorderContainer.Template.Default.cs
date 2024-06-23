using Get.Data.Helpers;
using Get.UI.Controls.Panels;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragContainer<T>
{
    static readonly ExternalControlTemplate<MotionDragContainerTempalteParts, MotionDragContainer<T>, Grid> DefaultTemplate =
        (@this, Root) =>
        {
            Root.HorizontalAlignment = HorizontalAlignment.Left;
            Root.VerticalAlignment = VerticalAlignment.Top;
            Root.Children.Add(new OrientedStackForContainer
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            }.AssignTo(out var Container));
            Root.Children.Add(new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Children =
                {
                    new Popup
                    {
                        ShouldConstrainToRootBounds = false,
                        Child = new UserControl
                        {
                            IsHitTestVisible = false
                        }.AssignTo(out var Display)
                    }.AssignTo(out var Popup)
                }
            });
            return new(
                Root: Root,
                Popup: Popup,
                Display: Display,
                Container: Container
            );
        };
}