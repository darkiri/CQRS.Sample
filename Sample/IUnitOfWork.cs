namespace CQRS.Sample
{
    public interface IUnitOfWork {
        void Commit();
    }
}