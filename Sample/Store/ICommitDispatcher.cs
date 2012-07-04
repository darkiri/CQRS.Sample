namespace CQRS.Sample.Store
{
    public interface ICommitDispatcher
    {
        void Dispatch();
    }
}