namespace Avalanche.Shared.Domain.Enumerations
{
    public enum TaskStatuses
    {
        Pending, //when queued and awaiting 1st processing
        Processing, //when running Task
        Failed, //when prior execution failed and pending retry
        Completed, //when completed successfully
    }
}
