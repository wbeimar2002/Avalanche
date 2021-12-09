namespace Avalanche.Shared.Domain.Enumerations.Media
{
    /// <summary>
    /// Action to perform when a gpio event is triggered
    /// </summary>
    public enum GpioAction
    {
        None = 0,
        StartRecording,
        StopRecording,
        ToggleRecording,
        CaptureImage
    }
}
