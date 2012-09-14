namespace CQRS.Sample
{
    public interface IUnitOfWork {
        void Commit();
        void Cancel();
    }
}