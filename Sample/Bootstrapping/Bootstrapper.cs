using System;
using System.Reflection;
using CQRS.Sample.Bus;
using CQRS.Sample.Store;
using Raven.Client;
using Raven.Client.Embedded;
using StructureMap;

namespace CQRS.Sample.Bootstrapping
{
    public class Bootstrapper
    {
        public static DocumentStoreConfiguration WithRavenStore()
        {
            return new DocumentStoreConfiguration(new EmbeddableDocumentStore().Initialize());
        }

        public static DocumentStoreConfiguration InMemory()
        {
            return new DocumentStoreConfiguration(new EmbeddableDocumentStore {RunInMemory = true}.Initialize());
        }
    }

    public class DocumentStoreConfiguration
    {
        readonly IDocumentStore _documentStore;

        public DocumentStoreConfiguration(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        public IDocumentStore DocumentStore
        {
            get { return _documentStore; }
        }


        public SubscirptionConfiguration WithAggregatesIn(Assembly assembly)
        {
            return new SubscirptionConfiguration(this, assembly);
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
                x.For<IDocumentStore>().Use(_storeConfiguration.DocumentStore);
                x.For<IHandlerRepository>().Use(new HandlerRepository(_aggregatesAssembly));
                x.For<IPersister>().Use<RavenPersister>();
                x.Scan(a =>
                {
                    a.AssemblyContainingType<ServiceBus>();
                    a.Convention<SimpleConvention>();
                });
            });
            ObjectFactory.GetInstance<IServiceBus>().Start();
            ObjectFactory.AssertConfigurationIsValid();
        }
    }
}