using Get.Data.Bindings.Linq;
using Get.Data.Bundles;
using Get.Data.Helpers;
using Get.UI.Data;
namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    public static ExternalControlTemplate<MotionDragItemTempalteParts, MotionDragItem<TContent>, Border> DefaultTemplate { get; } =
        (@this, border) =>
        {
            var RootElement = border;
            border.Background = new SolidColorBrush(Colors.Transparent);
            border.RenderTransform = new CompositeTransform().AssignTo(out var RootTransform);
            var TranslationResetStoryboard = new Storyboard
            {
                Children =
                {
                    new DoubleAnimation
                    {
                        Duration = new(TimeSpan.FromSeconds(0.25)),
                        To = 0,
                        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
                    },
                }
            };
            TranslationResetStoryboard.Children.Add(new DoubleAnimation
            {
                Duration = new(TimeSpan.FromSeconds(0.25)),
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            }.WithCustomCode(x =>
            {
                Storyboard.SetTarget(x, RootTransform);
                Storyboard.SetTargetProperty(x, nameof(RootTransform.TranslateX));
            }));
            TranslationResetStoryboard.Children.Add(new DoubleAnimation
            {
                Duration = new(TimeSpan.FromSeconds(0.25)),
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            }.WithCustomCode(x =>
            {
                Storyboard.SetTarget(x, RootTransform);
                Storyboard.SetTargetProperty(x, nameof(RootTransform.TranslateY));
            }));
            var AnimatingTranslationStoryboard = new Storyboard();
            AnimatingTranslationStoryboard.Children.Add(new DoubleAnimation
            {
                Duration = new(TimeSpan.FromSeconds(0.25)),
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            }.WithCustomCode(x =>
            {
                Storyboard.SetTarget(x, RootTransform);
                Storyboard.SetTargetProperty(x, nameof(RootTransform.TranslateX));
            }).AssignTo(out var AnimatingTranslateX));
            AnimatingTranslationStoryboard.Children.Add(new DoubleAnimation
            {
                Duration = new(TimeSpan.FromSeconds(0.25)),
                To = 0,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            }.WithCustomCode(x =>
            {
                Storyboard.SetTarget(x, RootTransform);
                Storyboard.SetTargetProperty(x, nameof(RootTransform.TranslateY));
            }).AssignTo(out var AnimatingTranslateY));

            border.Child = new UIElementContentControl
            {
                Foreground = new SolidColorBrush(Colors.White),
                ContentBinding = OneWay(@this.ContentProperty)
            };

            return new(
                RootElement: RootElement,
                RootTransform: RootTransform,
                TranslationResetStoryboard: TranslationResetStoryboard,
                AnimatingTranslationStoryboard: AnimatingTranslationStoryboard,
                AnimatingTranslateX: AnimatingTranslateX,
                AnimatingTranslateY: AnimatingTranslateY
            );
        };
    public static ExternalControlTemplate<MotionDragItemTempalteParts, MotionDragItem<TContent>, Border> DefaultTemplateStyled { get; } =
        (@this, border) =>
        {
            var x = DefaultTemplate(@this, border);
            border.BorderBrush = Solid(Colors.White);
            border.BorderThickness = new(0, 1, 0, 1);
            ((FrameworkElement)border.Child).Margin = new(5);
            return x;
        };


}
