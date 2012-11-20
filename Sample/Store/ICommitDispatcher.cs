namespace CQRS.Sample.Store
{
    /// <summary>
    /// For additional post-commit actions
    /// </summary>
    public interface ICommitDispatcher
    {
        void Dispatch();
    }
}