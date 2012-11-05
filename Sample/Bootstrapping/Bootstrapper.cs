using System.Reflection;
using CQRS.Sample.Bus;
using CQRS.Sample.Store;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using StructureMap;

namespace CQRS.Sample.Bootstrapping
{
    public class Bootstrapper
    {
        public static DocumentStoreConfiguration WithRavenStore()
        {
            return new PersistentStoreConfiguration();
        }

        public static DocumentStoreConfiguration InMemory()
        {
            return new InMemoryStoreConfiguration();
        }
    }

    public abstract class DocumentStoreConfiguration
    {
        protected const string EVENT_STORE_DATABASE = "EventStore";
        protected const string QUERY_STORE_DATABASE = "QueryStore";

        public IDocumentStore EventStore { get; protected set; }
        public IDocumentStore QueryStore { get; protected set; }


        public SubscirptionConfiguration WithAggregatesIn(Assembly assembly)
        {
            return new SubscirptionConfiguration(this, assembly);
        }
    }

    public class PersistentStoreConfiguration : DocumentStoreConfiguration
    {
        public PersistentStoreConfiguration()
        {
            EventStore = new DocumentStore {ConnectionStringName = EVENT_STORE_DATABASE}.Initialize();
            QueryStore = new DocumentStore {ConnectionStringName = QUERY_STORE_DATABASE}.Initialize();
        }
    }

    public class InMemoryStoreConfiguration  : DocumentStoreConfiguration
    {
        public InMemoryStoreConfiguration()
        {
            EventStore = new EmbeddableDocumentStore {RunInMemory = true, DefaultDatabase = EVENT_STORE_DATABASE}.Initialize();
            QueryStore = new EmbeddableDocumentStore {RunInMemory = true, DefaultDatabase = QUERY_STORE_DATABASE}.Initialize();
        }
    }

    public class SubscirptionConfiguration
    {
        readonly DocumentStoreConfiguration _storeConfiguration;
        readonly Assembly _aggregatesAssembly;

        public SubscirptionConfiguration(DocumentStoreConfiguration storeConfiguration, Assembly aggregatesAssembly)
        {
            _storeConfiguration = storeConfiguration;
            _aggregatesAssembly = aggregatesAssembly;
        }

        public void Start()
        {
            ObjectFactory.Initialize(x =>
            {
                x.For<IHandlerRepository>().Use(new HandlerRepository(_aggregatesAssembly));
                x.For<DocumentStoreConfiguration>().Use(_storeConfiguration);
                x.Scan(a =>
                {
                    a.AssemblyContainingType<ServiceBus>();
                    a.Convention<SimpleConvention>();
                });
            });
            ObjectFactory.AssertConfigurationIsValid();
            ObjectFactory.GetInstance<IServiceBus>().Start();
        }
    }
}