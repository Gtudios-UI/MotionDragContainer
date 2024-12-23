namespace Gtudios.UI.MotionDragContainers;
partial class MotionDragItem<TContent>
{
    void InitManipulation()
    {
#if MANIPULATION
        ManipulationStarted += (o, e) => Owner?.MotionDragItemManipulationStarted(o, e);
        ManipulationDelta += (o, e) => Owner?.MotionDragItemManipulationDelta(o, e);
        ManipulationCompleted += (o, e) => Owner?.MotionDragItemManipulationCompleted(o, e);
#endif
    }
}
