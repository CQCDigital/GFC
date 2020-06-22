using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace SYE.Repository
{
    public interface ILocationRepository<T> where T : class
    {
        Task<T> CreateAsync(T item);
        Task DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        Task<T> GetAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> ascKeySelector, Expression<Func<T, TKey>> descKeySelector);
        Task<IEnumerable<T>> FindByAsync(Expression<Func<T, bool>> predicate);
        Task<T> UpdateAsync(string id, T item);
    }

    public class LocationRepository<T> : ILocationRepository<T> where T : class
    {
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly IDocClient _locConfig;
        private readonly DocumentClient _client;

        public LocationRepository(ILocationConfig<T> appConfig, IDocClient locConfig)
        {
            _databaseId = appConfig?.DatabaseId;
            _collectionId = appConfig?.CollectionId;
            _locConfig = locConfig;
            _client = new DocumentClient(new Uri(_locConfig.Endpoint), _locConfig.Key, _locConfig.Policy);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var param = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
            return await _client.ReadDocumentAsync<T>(param,
                new RequestOptions()
                {
                    PartitionKey = new PartitionKey("id")

                }).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> ascKeySelector, Expression<Func<T, TKey>> descKeySelector)
        {
            return await Task.Run(() =>
            {
                var endPoint = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
                IQueryable<T> query = _client.CreateDocumentQuery<T>(endPoint, new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true });
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
                if (ascKeySelector != null)
                {
                    query = query.OrderBy(ascKeySelector);
                }
                if (descKeySelector != null)
                {
                    query = query.OrderByDescending(descKeySelector);
                }
                return query?.AsEnumerable()?.FirstOrDefault();
            }).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> FindByAsync(Expression<Func<T, bool>> predicate)
        {
            var query = _client.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(predicate)
                .AsDocumentQuery();
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var items = await query.ExecuteNextAsync<T>();
                if (items?.Count > 0)
                {
                    results.AddRange(await query.ExecuteNextAsync<T>());
                }
            }
            return results;
        }

        public async Task<T> CreateAsync(T item)
        {
            var result = await _client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId), item).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result.Resource.ToString());
        }

        public async Task<T> UpdateAsync(string id, T item)
        {
            var result = await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id), item).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result.Resource.ToString());
        }

        public async Task DeleteAsync(string id)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id)).ConfigureAwait(false);
        }
    }
}
