using Get.Data.Properties;
using Get.UI.Data;

namespace Gtudios.UI.MotionDragContainers;
public partial class MotionDragItem<TContent> : TypedTemplateContentControl<TContent, Border>
{
    MotionDragContainer? Owner => this.FindAscendant<MotionDragContainer>();

    public Property<bool> ReorderTranslationAnimationProperty { get; } = new(true);
    ///// <summary>
    ///// Tells whether this tab kind is supposed to be selectable.
    ///// Useful if you want to create spacial kind of tab where
    ///// the tab itself is supposed to be used as a display-only
    ///// and do not supposed to act as a tab.
    ///// </summary>
    //public virtual bool IsSelectableTabKind => true;
    //public virtual bool IsTabGroupKind => false;
    
}
