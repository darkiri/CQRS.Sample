using CQRS.Sample.Events;
using Raven.Client;

namespace CQRS.Sample.Reporting
{
    public class ReportingDatabase
    {
        private readonly IDocumentStore _documentStore;

        public ReportingDatabase(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public void Handle(AccountCreated message)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(new AccountDTO
                {
                    StreamId = message.StreamId,
                    Version = message.Version,
                    Email = message.Email,
                    PasswordHash = message.PasswordHash
                });
                session.SaveChanges();
            }
        }
    }
}