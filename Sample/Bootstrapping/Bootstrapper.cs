using System;
using System.Collections.Generic;
using System.Reflection;
using CQRS.Sample.Bus;
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

    public abstract class DocumentStoreConfiguration : IDisposable
    {
        protected const string EVENT_STORE_DATABASE = "EventStore";
        protected const string REPORTING_DATABASE = "Reporting";

        public IDocumentStore EventStore { get; protected set; }
        public IDocumentStore ReportingDatabase { get; protected set; }


        public SubscriptionConfiguration WithAggregatesIn(Assembly assembly)
        {
            return new SubscriptionConfiguration(this, assembly);
        }

        public void Dispose()
        {
            EventStore.Dispose();
            ReportingDatabase.Dispose();
        }
    }

    public class PersistentStoreConfiguration : DocumentStoreConfiguration
    {
        public PersistentStoreConfiguration()
        {
            EventStore = new DocumentStore {ConnectionStringName = EVENT_STORE_DATABASE}.Initialize();
            ReportingDatabase = new DocumentStore {ConnectionStringName = REPORTING_DATABASE}.Initialize();
        }
    }

    public class InMemoryStoreConfiguration  : DocumentStoreConfiguration
    {
        public InMemoryStoreConfiguration()
        {
            EventStore = new EmbeddableDocumentStore {RunInMemory = true}.Initialize();
            ReportingDatabase = new EmbeddableDocumentStore {RunInMemory = true}.Initialize();
        }
    }

    public class SubscriptionConfiguration : IDisposable
    {
        readonly DocumentStoreConfiguration _storeConfiguration;
        readonly Assembly _aggregatesAssembly;
        readonly IList<Assembly> _pluginAssemblies = new List<Assembly>();

        public SubscriptionConfiguration(DocumentStoreConfiguration storeConfiguration, Assembly aggregatesAssembly)
        {
            _storeConfiguration = storeConfiguration;
            _aggregatesAssembly = aggregatesAssembly;
        }

        public SubscriptionConfiguration WithPluginsIn(Assembly pluginAssembly)
        {
            _pluginAssemblies.Add(pluginAssembly);
            return this;
        }

        public IDisposable Start()
        {
            ObjectFactory.Initialize(x =>
            {
                x.For<IHandlerRepository>().Use(new HandlerRepository(_aggregatesAssembly));
                x.For<DocumentStoreConfiguration>().Use(_storeConfiguration);
                x.Scan(a =>
                {
                    a.AssemblyContainingType<Bootstrapper>();
                    foreach (var assembly in _pluginAssemblies)
                    {
                        a.Assembly(assembly);
                    }
                    a.Convention<SimpleConvention>();
                });
            });
            ObjectFactory.AssertConfigurationIsValid();
            ObjectFactory.GetInstance<IServiceBus>().Start();

            return this;
        }

        public void Dispose()
        {
            _storeConfiguration.Dispose();
        }
    }
}