using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Auth;
//using Microsoft.WindowsAzure.Storage.Blob;
//using Microsoft.WindowsAzure.Storage.Table;
using SnowMaker;
using SYE.Repository;

namespace SYE.Services
{
    public interface IUidGeneratorService
    {
        long GetNextId(string dataStoreKey);
    }

    //Local-only test version of class, for work without azure integration
    public class LocalTestGeneratorAsync : IUidGeneratorService
    {
        private string _dataStorePath;
        private IOptimisticDataStore _dataStore;
        private IUniqueIdGenerator _generator;

        public LocalTestGeneratorAsync()
        {
            _dataStorePath = @"C:\Local\SnowMakerDatastore";

            _dataStore = new DebugOnlyFileDataStore(_dataStorePath);
            _generator = new UniqueIdGenerator(_dataStore) {BatchSize = 10};
        }

        public long GetNextId(string dataStoreKey)
        {
            long nextId = _generator.NextIdAsync(dataStoreKey).Result;
            return nextId;
        }
    }

    public class CoreIdGenerator : IUidGeneratorService
    {
        private Microsoft.Azure.Storage.CloudStorageAccount _cloudStorageAccount;
        private string _containerName;
        private IOptimisticDataStore _dataStore;
        private IUniqueIdGenerator _generator;

        public CoreIdGenerator(IConfiguration config)
        {
            var storageConnection = config.GetSection("ConnectionStrings:AzureBlobStorage").Get<BlobStorageConnection>();
            var batchSize = config.GetValue<int>("IdGeneratorBatchSize");

            _cloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(storageConnection.ConnectionString);
            _containerName = storageConnection.ContainerName;

            _dataStore = BlobOptimisticDataStore.CreateAsync(_cloudStorageAccount, _containerName).Result;
            _generator = new UniqueIdGenerator(_dataStore) {BatchSize = batchSize};
        }

        public long GetNextId(string dataStoreKey)
        {
            long nextId = _generator.NextIdAsync(dataStoreKey).Result;
            return nextId;
        }
    }
}