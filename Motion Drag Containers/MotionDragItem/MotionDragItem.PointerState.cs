namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    PointerVisualState _CurrentPointerVisualState;
    protected PointerVisualState CurrentPointerVisualState
    {
        get => _CurrentPointerVisualState;
        private set
        {
            _CurrentPointerVisualState = value;
            OnPointerVisualStateUpdated();
        }
    }
    protected virtual void OnPointerVisualStateUpdated()
    {
        VisualStateManager.GoToState(this, CurrentPointerVisualState.ToString(), true);
    }
    bool isPointerInside = false;
    bool isPressed = false;
    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        isPointerInside = true;
        UpdateState();
        base.OnPointerEntered(e);
    }
    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        isPointerInside = false;
        UpdateState();
        base.OnPointerExited(e);
    }
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        isPressed = true;
        UpdateState();
        base.OnPointerPressed(e);
    }
    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        isPressed = false;
        UpdateState();
        base.OnPointerReleased(e);
    }
    void UpdateState()
    {

        if (isPressed) CurrentPointerVisualState = PointerVisualState.Pressed;
        else
        {
            if (isPointerInside)
                CurrentPointerVisualState = PointerVisualState.PointerOver;
            else
                CurrentPointerVisualState = PointerVisualState.Normal;
        }
    }
    protected enum PointerVisualState
    {
        Normal,
        PointerOver,
        Pressed
    }
}
