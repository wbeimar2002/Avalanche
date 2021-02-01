namespace Avalanche.Shared.Domain.Enumerations
{
    /// <summary>
    /// Mode of operation for timeout
    /// </summary>
    public enum TimeoutMode
    {
        /// <summary>
        /// Timeout is a pdf file
        /// </summary>
        PdfFile = 0,

        /// <summary>
        /// Timeout is a fullscreen video source, typically nurse pc
        /// </summary>
        VideoSource = 1
    }
}
