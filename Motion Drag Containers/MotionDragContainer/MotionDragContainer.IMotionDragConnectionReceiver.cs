using Gtudios.UI.MotionDrag;
namespace Gtudios.UI.MotionDragContainers;

partial class MotionDragContainer<T> : IMotionDragConnectionReceiver<T>
{
    bool IMotionDragConnectionReceiver<T>.IsVisibleAt(Point pt)
    {
        SelfNote.HasDisallowedPInvoke();
        if (!(XamlRoot.IsHostVisible && Visibility is Visibility.Visible))
            return false;
        var ptScreen = GlobalRectangle.WindowPosOffset.Point() + pt;
        return WinWrapper.Windowing.Window.FromLocation(
            (int)ptScreen.X,
            (int)ptScreen.Y
        ).Root == 
        WinWrapper.Windowing.Window.FromWindowHandle(Windowing.Window.GetFromXamlRoot(XamlRoot).WindowHandle).Root;
    }

    bool useCached = false; // warning: doesn't work, fix before turning this back to true
    GlobalContainerRect _globalRectangle;
    GlobalContainerRect GlobalRectangle => useCached ? _globalRectangle : GlobalContainerRect.GetFromContainer(this);
    GlobalContainerRect IMotionDragConnectionReceiver<T>.GlobalRectangle => GlobalRectangle;
    void IMotionDragConnectionReceiver<T>.DragEnter(object? sender, T item, int senderIndex, DragPosition dragPositionIn, ref Point itemOffset)
    {
        if (!ReferenceEquals(sender, this))
            AnimationController.Reset();
        var dragPosition = dragPositionIn.ToNewContainer(GlobalRectangle);
        //if (!ReferenceEquals(sender, this)) Debugger.Break();
        DragDelta(sender, item, dragPosition, ref itemOffset);
    }

    void DragDelta(object? sender, object? item, DragPosition dragPositionIn, ref Point itemOffset)
    {
        var dragPosition = dragPositionIn.ToNewContainer(GlobalRectangle);
        SnapDrag(dragPosition, dragPositionIn, ref itemOffset);
        AnimationController.ShiftAmount =
            ReorderOrientation is Orientation.Horizontal ?
            dragPosition.OriginalItemRect.Width :
            dragPosition.OriginalItemRect.Height;
        AnimationController.StartShiftIndex =
            AnimationController.IndexOfItemAt(
                dragPosition.MousePositionToContainer.X,
                dragPosition.MousePositionToContainer.Y
            );
    }
    void IMotionDragConnectionReceiver<T>.DragDelta(object? sender, T item, int senderIndex, DragPosition dragPosition, ref Point itemOffset)
        => DragDelta(sender, item, dragPosition, ref itemOffset);

    void IMotionDragConnectionReceiver<T>.DragLeave(object? sender, T item, int senderIndex)
    {
        AnimationController.StartShiftIndex = TargetCollection.Count;
    }
    async void IMotionDragConnectionReceiver<T>.Drop(object? sender, T item, int senderIndex, DragPosition dragPosition, DropManager dropManager)
    {
        //var pt = dragPosition.ItemPositionToScreen;
        //var hwnd = Popup.XamlRoot.ContentIslandEnvironment.AppWindowId;
        //AppWindow.GetFromWindowId(hwnd).Move(new() { X = (int)pt.X, Y = (int)pt.Y });
        if (ReferenceEquals(sender, this))
        {
            var newIdx = AnimationController.StartShiftIndex;
            var def = dropManager.GetDeferral();
            //if (SafeContainerFromIndex(ItemDragIndex) is { } curItem && curItem.FindDescendantOrSelf<MotionDragItem>() is { } st)
            //{
            //    var pt = AnimationController.PositionOfItemAtIndex(newIdx);
            //    await st.TemporaryAnimateTranslationAsync(pt.X, pt.Y);
            //}
            AnimationController.Reset();
            newIdx = Math.Min(newIdx, TargetCollection.Count - 1);
            if (newIdx != ItemDragIndex)
            {
                //int i = 0;
                //while (SafeContainerFromIndex(i++)?.FindDescendantOrSelf<MotionDragItem>() is { } st2)
                //{
                //    st2.ResetTranslationImmedietly();
                //}
                OnItemMovingInContainer(ItemDragIndex, newIdx);
                TargetCollection.RemoveAt(ItemDragIndex);
                TargetCollection.Insert(newIdx, item);
                OnItemMovedInContainer(ItemDragIndex, newIdx);
            }

            // We wanted to remove the item but since we are interacting with the same object we know what's going on
            def.Complete();
        } else
        {
            var newIdx = AnimationController.StartShiftIndex;
            AnimationController.Reset();
            var def = dropManager.GetDeferral();
            //await Task.Delay(1000);
            //if (SafeContainerFromIndex(ItemDragIndex) is { } curItem && curItem.FindDescendantOrSelf<MotionDragItem>() is { } st)
            //{
            //    var pt = AnimationController.PositionOfItemAtIndex(newIdx);
            //    await st.TemporaryAnimateTranslationAsync(pt.X, pt.Y);
            //}
            //int i = 0;
            //while (SafeContainerFromIndex(i++)?.FindDescendantOrSelf<MotionDragItem>() is { } st2)
            //{
            //    st2.ResetTranslationImmedietly();
            //}
            OnItemDroppingFromAnotherContainer(sender, item, senderIndex, newIdx);
            await ((MotionDragReorderContainerDropManager)dropManager).RemoveItemFromHostAsync();
            if (newIdx > TargetCollection.Count) newIdx = TargetCollection.Count;
            TargetCollection.Insert(newIdx, item);
            OnItemDropFromAnotherContainer(sender, item, senderIndex, newIdx);
            def.Complete();
        }
    }
    void SnapDrag(DragPosition dragPosition, DragPosition dragPositionOriginal, ref Point itemOffset)
    {
        mousePos = dragPosition.MousePositionToContainer;
        if (mousePos.X > 0 && mousePos.X < ActualWidth && mousePos.Y > 0 && mousePos.Y < ActualHeight)
        {
            if (ReorderOrientation is Orientation.Vertical)
            {
                itemOffset.X -= dragPosition.MousePositionToContainer.X - (dragPositionOriginal.MouseOffset.X - dragPositionOriginal.OriginalItemRect.X);
            }
            else
            {
                itemOffset.Y -= dragPosition.MousePositionToContainer.Y - (dragPositionOriginal.MouseOffset.Y - dragPositionOriginal.OriginalItemRect.Y);
            }
        }
    }
    protected virtual void OnItemDroppingFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {

    }
    protected virtual void OnItemDropFromAnotherContainer(object? sender, T item, int senderIndex, int newIndex)
    {

    }
    protected virtual void OnItemMovingInContainer(int oldIndex, int newIndex)
    {

    }
    protected virtual void OnItemMovedInContainer(int oldIndex, int newIndex)
    {

    }
    private void MotionDragContainer_Unloaded(object sender, RoutedEventArgs e)
    {

        if (ConnectionContext is { } cc)
            MotionDragConnectionContext<T>.UnsafeRemove(cc, this);
    }

    private void MotionDragContainer_Loaded(object sender, RoutedEventArgs e)
    {
        if (ConnectionContext is { } cc)
            MotionDragConnectionContext<T>.UnsafeAdd(cc, this);
    }
}