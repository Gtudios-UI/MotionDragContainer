using CommunityToolkit.WinUI;
using Gtudios.UI.MotionDragContainers;

namespace Get.UI.Controls.Panels;

public partial class OrientedStackForContainer<T> : OrientedStack
{
    public OrientedStackForContainer()
    {
        Loaded += OrientedStackForContainer_Loaded;
        Unloaded += OrientedStackForContainer_Unloaded;
    }

    private void OrientedStackForContainer_Unloaded(object sender, RoutedEventArgs e)
    {
        parentCached.ReorderOrientationProperty.ValueChanged -= ReorderOrientationProperty_ValueChanged;
    }
    MotionDragContainer<T> parentCached;
    private void OrientedStackForContainer_Loaded(object sender, RoutedEventArgs e)
    {
        var parent = this.FindAscendant<MotionDragContainer<T>>();
        if (parent is not null)
        {
            Orientation = parent.ReorderOrientationProperty.Value;
            parent.ReorderOrientationProperty.ValueChanged -= ReorderOrientationProperty_ValueChanged;
            parentCached = parent;
            parent.ReorderOrientationProperty.ValueChanged += ReorderOrientationProperty_ValueChanged;
        }
    }

    private void ReorderOrientationProperty_ValueChanged(Orientation oldValue, Orientation newValue)
    {
        Orientation = newValue;
    }
}