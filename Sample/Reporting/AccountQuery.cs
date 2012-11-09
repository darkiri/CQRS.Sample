using System;
using System.Linq;
using CQRS.Sample.Bootstrapping;
using Raven.Client;

namespace CQRS.Sample.Reporting
{
    public class AccountQuery
    {
        readonly IDocumentStore _store;

        public AccountQuery(DocumentStoreConfiguration storeConfig)
        {
            _store = storeConfig.ReportingDatabase;
        }

        public AccountDTO[] Execute()
        {
            using (var session = _store.OpenSession())
            {
                return session.Query<AccountDTO>().ToArray();
            }
        } 

        public AccountDTO[] Execute(string email)
        {
            using (var session = _store.OpenSession())
            {
                return session
                    .Query<AccountDTO>()
                    .Where(a => a.Email == email)
                    .ToArray();
            }
        }
    }
}