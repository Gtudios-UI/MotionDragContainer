using Get.UI.Data;
namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    public ExternalControlTemplate<MotionDragItemTempalteParts, MotionDragItem<TContent>, Border> ControlTemplate { get; set; } = DefaultTemplate;
    public record struct MotionDragItemTempalteParts(
        Border RootElement,
        CompositeTransform RootTransform,
        Storyboard TranslationResetStoryboard,
        Storyboard AnimatingTranslationStoryboard,
        DoubleAnimation AnimatingTranslateX,
        DoubleAnimation AnimatingTranslateY
    );
    protected override void Initialize(Border rootElement)
    {
        TempalteParts = ControlTemplate(this, rootElement);
        if (TempalteParts.RootTransform == null) Debugger.Break();
        RootElement.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
        InitManipulation();
    }
    MotionDragItemTempalteParts TempalteParts;
    Border RootElement => TempalteParts.RootElement;
    CompositeTransform RootTransform => TempalteParts.RootTransform;
    Storyboard TranslationResetStoryboard => TempalteParts.TranslationResetStoryboard;
    Storyboard? AnimatingTranslationStoryboard => TempalteParts.AnimatingTranslationStoryboard;
    DoubleAnimation? AnimatingTranslateX => TempalteParts.AnimatingTranslateX;
    DoubleAnimation? AnimatingTranslateY => TempalteParts.AnimatingTranslateY;
}
