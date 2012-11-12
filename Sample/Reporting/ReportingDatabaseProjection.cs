using System.Linq;
using CQRS.Sample.Bootstrapping;
using CQRS.Sample.Events;
using Raven.Client;
using Raven.Client.Indexes;

namespace CQRS.Sample.Reporting
{
    public class ReportingDatabaseProjection
    {
        private readonly IDocumentStore _documentStore;

        public ReportingDatabaseProjection(DocumentStoreConfiguration storeConfig)
        {
            _documentStore = storeConfig.ReportingDatabase;
            Register.Indexes(_documentStore);
        }

        public void Handle(AccountCreated message)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.Store(new AccountDTO
                {
                    StreamId = message.StreamId,
                    Email = message.Email,
                    PasswordHash = message.PasswordHash
                });
                session.SaveChanges();
            }
        }


        public void Handle(PasswordChanged message)
        {
            using (var session = _documentStore.OpenSession())
            {
                var account = session.Query<AccountDTO>().Single(a => a.StreamId == message.StreamId);
                account.PasswordHash = message.PasswordHash;
                session.SaveChanges();
            }
        }
    }


    public static class Register
    {
        public static void Indexes(IDocumentStore store)
        {
            new AccountsByEmail().Execute(store);
        }
    }

    public class AccountsByEmail : AbstractIndexCreationTask<AccountDTO, AccountDTO>
    {
        public AccountsByEmail()
        {
            Map = docs => from doc in docs
                          select new {doc.Email};
        }
    }
}