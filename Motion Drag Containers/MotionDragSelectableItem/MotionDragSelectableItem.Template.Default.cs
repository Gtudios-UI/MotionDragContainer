using Get.Data.Bindings.Linq;
using Get.Data.Helpers;
using Get.Data.Properties;
using Get.UI.Controls.Panels;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;

partial class MotionDragSelectableItem<TContent>
{
    readonly static ExternalControlTemplate<MotionDragItemTempalteParts, MotionDragItem<TContent>, Border> DefaultTemplate =
        (thisuncated, border) =>
        {
            var @this = (MotionDragSelectableItem<TContent>)thisuncated;
            var a = MotionDragItem<TContent>.DefaultTemplate(@this, border);
            border.Child = new Border
            {
                Child = border.Child
            }.AssignTo(out var BackgroundPlace);
            Border.BackgroundProperty.AsProperty<Border, SolidColorBrush>(BackgroundPlace)
            .Bind(@this.IsSelectedProperty.Select<bool, SolidColorBrush>(x => x ? new(Colors.Red) : new(Colors.Transparent)),
                Get.Data.Bindings.ReadOnlyBindingModes.OneWay
            );
            return a;
        };
}