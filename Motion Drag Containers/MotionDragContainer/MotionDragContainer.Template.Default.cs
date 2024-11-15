using Get.Data.Bindings;
using Get.Data.Helpers;
using Get.UI.Controls.Panels;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragContainer<T>
{
    static readonly ExternalControlTemplate<MotionDragContainerTempalteParts<T>, MotionDragContainer<T>, Grid> DefaultTemplate =
        (@this, Root) =>
        {
            Root.HorizontalAlignment = HorizontalAlignment.Left;
            Root.VerticalAlignment = VerticalAlignment.Top;
            Root.Children.Add(new OrientedStack
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            }.AssignTo(out var Container));
            Container.OrientationProperty.Bind(@this.ReorderOrientationProperty, ReadOnlyBindingModes.OneWay);
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