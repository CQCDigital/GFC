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
    /// <summary>
    /// CRUD operations on a class that maps to database via IAppConfiguration&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">Class this repository interface maps to</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<T> CreateAsync(T item);
        Task DeleteAsync(string id);
        Task<T> GetByIdAsync(string id);
        Task<T> GetAsync<TKey>(Expression<Func<T, bool>> predicate, Expression<Func<T, TKey>> ascKeySelector, Expression<Func<T, TKey>> descKeySelector);
        Task<IEnumerable<T>> FindByAsync(Expression<Func<T, bool>> predicate);
        Task<T> UpdateAsync(string id, T item);
        T GetDocumentId(string guid);
    }

    /// <inheritdoc cref="IGenericRepository{T}"/>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly string _databaseId;
        private readonly string _collectionId;
        private readonly IDocumentClient _client;

        public GenericRepository(IAppConfiguration<T> appConfig, IDocumentClient client)
        {
            _databaseId = appConfig?.DatabaseId;
            _collectionId = appConfig?.CollectionId;
            _client = client;
        }

        public async Task<T> GetByIdAsync(string id)
        {
            var param = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
            return await _client.ReadDocumentAsync<T>(param).ConfigureAwait(false);
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
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            var options = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }; // -1 here is a dynamic item count.

            var query = _client.CreateDocumentQuery<T>(collectionUri, options)
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
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            var result = await _client.CreateDocumentAsync(collectionUri, item).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result.Resource.ToString());
        }

        public T GetDocumentId(string guid)
        {
            var query = new SqlQuerySpec($"SELECT DocumentId(c) as DocumentId FROM c WHERE c.id = @id");
            query.Parameters = new SqlParameterCollection();
            query.Parameters.Add(new SqlParameter("@id", guid));

            var options = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };
            var collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
            var results = _client.CreateDocumentQuery<Document>(collectionUri, query, options).AsEnumerable().ToList();

            return JsonConvert.DeserializeObject<T>(results.FirstOrDefault().ToString());
        }

        public async Task<T> UpdateAsync(string id, T item)
        {
            var documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
            var result = await _client.ReplaceDocumentAsync(documentUri, item).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result.Resource.ToString());
        }

        public async Task DeleteAsync(string id)
        {
            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, id)).ConfigureAwait(false);
        }
    }
}
