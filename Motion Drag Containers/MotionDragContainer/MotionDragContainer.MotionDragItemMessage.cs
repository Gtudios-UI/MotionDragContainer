using Get.Data.Collections;
using Gtudios.UI.MotionDrag;
using System.Threading.Tasks;

namespace Gtudios.UI.MotionDragContainers;

partial class MotionDragContainer<T>
{
    Vector3 translation = default;
    Point initmousePos = default;
    Point mousePos = default;
    Point translationRoot = default;
    Point translationXamlRoot = default;
    Rect itemRect;
    int ItemDragIndex = -1;
    T DraggingObject = default!;
    bool isCanceled = false;
    DateTime startMani;
    internal void MotionDragItemManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
    {
        startMani = DateTime.Now;
        e.Handled = true;
        isCanceled = false;
        if (sender is not UIElement ele) return;
        Display.Width = ele.ActualSize.X;
        Display.Height = ele.ActualSize.Y;

        var eleVisual = ElementCompositionPreview.GetElementVisual(ele);
        var compositor = eleVisual.Compositor;
        ElementCompositionPreview.SetElementChildVisual(
            Display,
            compositor.CreateRedirectVisual(eleVisual)
        );
        eleVisual.IsVisible = false;
        translationRoot = ele.TransformToVisual(Root).TransformPoint(default);
        initmousePos = e.Position.Point() + translationRoot;
        translationXamlRoot = ele.TransformToVisual(Root.XamlRoot.Content).TransformPoint(e.Position);
        itemRect = new(translationRoot, new Size(ele.ActualSize.X, ele.ActualSize.Y));
        _globalRectangle = GlobalContainerRect.GetFromContainer(this);
        var idx = ChildContainers.IndexOf((MotionDragItem<T>)ele);
        AnimationController.Reset();
        AnimationController.StartRemoveIndex = ItemDragIndex = idx;
        DraggingObject = TargetCollection[idx];

        Popup.Translation = new((float)translationRoot.X, (float)translationRoot.Y, 0);
        Popup.IsOpen = true;
        set = false;
    }
    bool set;
    internal void MotionDragItemManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
    {
        SelfNote.DebugBreakOnShift();
        e.Handled = true;
        if (isCanceled) return;
        if (WinWrapper.Input.Keyboard.IsKeyDown(WinWrapper.Input.VirtualKey.ESCAPE))
        {
            isCanceled = true;
            Popup.IsOpen = false;
            if (sender is UIElement ele)
            {
                var eleVisual = ElementCompositionPreview.GetElementVisual(ele);
                eleVisual.IsVisible = true;
            }
            AnimationController.Reset();
            return;
        }
        DragPosition dp = new(GlobalRectangle, itemRect, initmousePos, e.Cumulative.Translation);
        if (!set)
        {
            var mousePos = dp.MousePositionToScreen;
            var window = WinWrapper.Windowing.Window.FromLocation((int)mousePos.X, (int)mousePos.Y);
            if (window.Class.Name is
#if WINDOWS_UWP
                "Xaml_WindowedPopupClass"
#else
                "Microsoft.UI.Content.PopupWindowSiteBridge"
#endif
            )
            {
                window.SetTopMost();
                window[WinWrapper.Windowing.WindowExStyles.Layered] = true;
                window[WinWrapper.Windowing.WindowExStyles.Transparent] = true;
                set = true;
            }
        }
        translation = new Vector3(
            (float)(e.Cumulative.Translation.X + translationRoot.X),
            (float)(e.Cumulative.Translation.Y + translationRoot.Y),
        0);
        var localTranslation = translation;
        if (ConnectionContext is { } cc)
            MotionDragConnectionContext<T>.UnsafeSendDragEvent(
                cc, this, DraggingObject, ItemDragIndex, new(GlobalRectangle, itemRect, initmousePos, e.Cumulative.Translation), ref localTranslation
            );
        Popup.Translation = localTranslation;
        //var hwnd = Popup.XamlRoot.ContentIslandEnvironment.AppWindowId;
        //System.Drawing.Point pt = default;
        //_ = PInvoke.MapWindowPoints(new((nint)hwnd.Value), default, &pt, 1);
        //var scale = Popup.XamlRoot.RasterizationScale;
        //testValue.Text = $"{(e.Cumulative.Translation.X + translationXamlRoot.X) * scale + pt.X}, {(e.Cumulative.Translation.Y + translationXamlRoot.Y) * scale + pt.Y}";
    }
    class MotionDragReorderContainerDropManager : DropManager
    {
        public required Func<Task> RemoveItemFunc;
        bool isAlreadyCalled = false;
        public override Task RemoveItemFromHostAsync()
        {
            if (isAlreadyCalled) return Task.CompletedTask;
            isAlreadyCalled = true;
            return RemoveItemFunc();
        }
    }
    internal async void MotionDragItemManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
    {
        e.Handled = true;
        if (DateTime.Now - startMani < TimeSpan.FromMilliseconds(200))
        {
            // probably was not intentional
            isCanceled = true;
            Popup.IsOpen = false;
            var eleVisual = ElementCompositionPreview.GetElementVisual(ChildContainers[ItemDragIndex]);
            eleVisual.IsVisible = true;
            AnimationController.Reset();
        }
        if (isCanceled)
        {
            isCanceled = false;
            return;
        }
        var dropManager = new MotionDragReorderContainerDropManager()
        {
            RemoveItemFunc = RemoveItemAsync
        };
        MotionDragConnectionContext<T>.UnsafeSendDropEvent(
            ConnectionContext, this, DraggingObject, ItemDragIndex, new(GlobalRectangle, itemRect, initmousePos, e.Cumulative.Translation), dropManager
        );
        await dropManager.WaitForDeferralAsync();
        if (sender is UIElement ele)
        {
            var eleVisual = ElementCompositionPreview.GetElementVisual(ele);
            eleVisual.IsVisible = true;
        }
        Popup.IsOpen = false;

        Task RemoveItemAsync()
        {
            TargetCollection.RemoveAt(ItemDragIndex);
            AnimationController.Reset();
            return Task.CompletedTask;
        }
        AnimationController.Reset();
        //var hwnd = Popup.XamlRoot.ContentIslandEnvironment.AppWindowId;
        //System.Drawing.Point pt = default;
        //_ = PInvoke.MapWindowPoints(new((nint)hwnd.Value), default, &pt, 1);
        //var scale = Popup.XamlRoot.RasterizationScale;
        //var pos = new System.Drawing.Point(
        //    (int)((e.Cumulative.Translation.X //+ translationXamlRoot.X
        //                                      ) * scale + pt.X),
        //    (int)((e.Cumulative.Translation.Y //+ translationXamlRoot.Y
        //                                      ) * scale + pt.Y)
        //    );
        //AppWindow.GetFromWindowId(hwnd).Move(new() { X = pos.X, Y = pos.Y });
    }
    public void ResetAnimation()
    {
        //if (!isCanceled) {
        //    isCanceled = true;
        //    Popup.IsOpen = false;
        //    var eleVisual = ElementCompositionPreview.GetElementVisual(ChildContainers[ItemDragIndex]);
        //    eleVisual.IsVisible = true;
        //    AnimationController.Reset();
        //}
    }
}