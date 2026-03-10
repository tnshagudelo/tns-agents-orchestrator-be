namespace Domain.Entities
{
    public enum JobStatus
    {
        Pending,
        ReadingRepo,
        GeneratingTests,
        ValidatingTests,
        CreatingPR,
        Completed,
        Failed
    }
}
