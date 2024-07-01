using Get.Data.Bindings.Linq;
using Get.Data.Helpers;
using Get.UI.Data;
namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    public readonly static ExternalControlTemplate<MotionDragItemTempalteParts, MotionDragItem<TContent>, Border> DefaultTemplate =
        (@this, border) =>
        {
            var RootElement = border;
            border.Background = new SolidColorBrush(Colors.Transparent);
            border.BorderBrush = new SolidColorBrush(Colors.White);
            border.BorderThickness = new(0, 1, 0, 1);
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
            border.Child = new ContentBundleControl
            {
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new(5)
            }
            .WithCustomCode(x =>
            {
                x.ContentBundleProperty.Bind(
                    @this.ContentBundleProperty.Select<ContentBundle<TContent, UIElement>, ContentBundle>(x => x),
                    Get.Data.Bindings.ReadOnlyBindingModes.OneWay
                );
            });

            return new(
                RootElement: RootElement,
                RootTransform: RootTransform,
                TranslationResetStoryboard: TranslationResetStoryboard,
                AnimatingTranslationStoryboard: AnimatingTranslationStoryboard,
                AnimatingTranslateX: AnimatingTranslateX,
                AnimatingTranslateY: AnimatingTranslateY
            );
        };
}
