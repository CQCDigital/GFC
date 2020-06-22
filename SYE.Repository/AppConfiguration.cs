
using System;
using System.Security;
using Microsoft.Azure.Documents.Client;

namespace SYE.Repository
{

    public interface ILocationConfiguration
    {
        string Endpoint { get; set; }
        string Key { get; set; }
        ConnectionPolicy Policy { get; set; }
    }
    public class LocationConfiguration : ILocationConfiguration
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public ConnectionPolicy Policy { get; set; }
    }

    public interface IDocClient
    {
        string Endpoint { get; set; }
        SecureString Key { get; set; }
        ConnectionPolicy Policy { get; set; }
    }
    public class DocClient : IDocClient
    {
        public string Endpoint { get; set; }
        public SecureString Key { get; set; }
        public ConnectionPolicy Policy { get; set; }
    }



    public interface ILocationConfig<T> where T : class
    {
        string DatabaseId { get; set; }
        string CollectionId { get; set; }
    }
    public class LocationConfig<T> : ILocationConfig<T> where T : class
    {
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
    }

    public interface ISearchConfiguration
    {
        string SearchServiceName { get; set; }
        string SearchApiKey { get; set; }
        string IndexName { get; set; }
    }

    public interface ICosmosConnection
    {
        string Endpoint { get; set; }
        string Key { get; set; }
    }

    public class CosmosConnection : ICosmosConnection
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
    }

    public interface IBlobStorageConnection
    {
        string ConnectionString { get; set; }
        string ContainerName { get; set; }
    }

    public class BlobStorageConnection : IBlobStorageConnection
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }
    }

    public interface IAppConfiguration<T> where T : class
    {
        string DatabaseId { get; set; }
        string CollectionId { get; set; }
        string ConfigRecordId { get; set; }
    }
    public class AppConfiguration<T> : IAppConfiguration<T> where T : class
    {
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
        public string ConfigRecordId { get; set; }
    }

    public class SearchConfiguration : ISearchConfiguration
    {
        public string SearchServiceName { get; set; }
        public string SearchApiKey { get; set; }
        public string IndexName { get; set; }
    }

    public interface IEmailFieldMapping
    {
        string Name { get; set; }
        string TemplateField { get; set; }
        string FormField { get; set; }
    }

    public class EmailFieldMapping : IEmailFieldMapping
    {
        public string Name { get; set; }
        public string TemplateField { get; set; }
        public string FormField { get; set; }
    }

}
