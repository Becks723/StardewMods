namespace JunimoStudio.Input.Gestures
{
    public enum MouseClickStyle
    {
        /// <summary>Trigger click if: Last frame released, this frame pressed.</summary>
        WhenPressed,

        /// <summary>Trigger click if: Last frame pressed, this frame released.</summary>
        WhenReleased
    }
}
