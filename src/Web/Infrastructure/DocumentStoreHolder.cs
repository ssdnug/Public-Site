﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using Raven.Client.MvcIntegration;

namespace Web.Infrastructure
{
    public static class DocumentStoreHolder
    {
        private static IDocumentStore documentStore;

        public static IDocumentStore DocumentStore
        {
            get
            {
                if (documentStore != null)
                    return documentStore;

                lock (typeof(DocumentStoreHolder))
                {
                    if (documentStore != null)
                        return documentStore;

                    var parser = ConnectionStringParser<RavenConnectionStringOptions>.FromConnectionStringName("RavenDB");
                    
                    parser.Parse();

                    documentStore = new DocumentStore
                    {
                        ConnectionStringName = ConnectionStringName,
                        ApiKey = parser.ConnectionStringOptions.ApiKey,
                        Url = parser.ConnectionStringOptions.Url,
                    };


                    documentStore.Initialize();

                    IndexCreation.CreateIndexes(typeof(DocumentStoreHolder).Assembly, documentStore);
                    RavenProfiler.InitializeFor(documentStore);
                }

                return documentStore;
            }
        }

        private static string ConnectionStringName
        {
            get
            {
                var customConnection = ConfigurationManager.ConnectionStrings[Environment.MachineName] != null;
                var connectionStringName = customConnection ? Environment.MachineName : "RavenDB";
                return connectionStringName;
            }
        }
    }
}