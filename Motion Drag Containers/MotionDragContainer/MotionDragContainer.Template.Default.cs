using Get.Data.Bindings;
using Get.Data.Helpers;
using Get.UI.Controls.Panels;
using Get.UI.Data;
namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragContainer<T>
{
    public static ExternalControlTemplate<MotionDragContainerTempalteParts<T>, MotionDragContainer<T>, Grid> DefaultTemplate { get; } =
        (@this, Root) =>
        {
            Root.HorizontalAlignment = HorizontalAlignment.Left;
            Root.VerticalAlignment = VerticalAlignment.Top;
            Root.Children.Add(new OrientedStack
            {
                OrientationBinding = OneWay(@this.ReorderOrientationProperty),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Children =
                {
                    new OrientedStack
                    {
                        OrientationBinding = OneWay(@this.ReorderOrientationProperty)
                    }.AssignTo(out var Container),
                    new TextBlock().AssignTo(out var tb)
                }
            });
            
            var margin =
                from p in @this.EndPaddingProperty
                from orientation in @this.ReorderOrientationProperty
                select (p, orientation);
            margin.ApplyAndRegisterForNewValue((_, x) =>
            {
                var (p, orientation) = x;
                if (orientation is Orientation.Vertical)
                {
                    tb.Width = 0;
                    tb.Height = p;
                } else
                {
                    tb.Width = p;
                    tb.Height = 0;
                }
            });

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