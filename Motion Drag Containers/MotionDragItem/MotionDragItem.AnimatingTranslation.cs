﻿using System.Threading.Tasks;

namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    internal void TemporaryAnimateTranslation(double x, double y)
    {
        if (AnimatingTranslateX is not { } aniTranX) return;
        if (AnimatingTranslateY is not { } aniTranY) return;
        if (AnimatingTranslationStoryboard is not { } storyboard) return;
        aniTranX.To = x;
        aniTranY.To = y;
        storyboard.Begin();
    }
    internal async Task TemporaryAnimateTranslationAsync(double x, double y)
    {
        if (AnimatingTranslateX is not { } aniTranX) return;
        if (AnimatingTranslateY is not { } aniTranY) return;
        if (AnimatingTranslationStoryboard is not { } storyboard) return;
        aniTranX.To = x;
        aniTranY.To = y;
        TaskCompletionSource t = new();
        void handler(object? sender, object e)
        {
            t.SetResult();
            storyboard.Completed -= handler;
        }
        storyboard.Completed += handler;
        storyboard.Begin();
        await t.Task;
    }
    internal void ResetTranslationImmedietly()
    {
        AnimatingTranslationStoryboard?.Stop();
        if (RootTransform is not null)
        {
            RootTransform.TranslateX = 0;
            RootTransform.TranslateY = 0;
        }
    }
}
