using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GDSHelpers.Models.FormSchema;
using Microsoft.AspNetCore.Hosting;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Repository;
using Newtonsoft.Json;
using System.IO;
using DocumentFormat.OpenXml.CustomXmlSchemaReferences;
using Microsoft.Extensions.Configuration;

namespace SYE.Services
{
    /// <summary>
    /// Gets the appropriate form from the schema DB.
    /// If in local environment, can set appsettings.json to use local schema version in Content/service-feedback.json
    /// </summary>
    public interface IFormService
    {
        Task<FormVM> GetLatestForm();
        Task<FormVM> GetLatestFormByName(string formName);
        Task<FormVM> GetFormById(string id);
        Task<FormVM> FindByName(string formName);
        Task<FormVM> FindByVersion(string version);
        Task<FormVM> FindByNameAndVersion(string formName, string version);
    }

    /// <inheritdoc cref="IFormService"/>
    [LifeTime(LifeTime.Scoped)]
    public class FormService : IFormService
    {
        private readonly IGenericRepository<FormVM> _repo;
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly bool _useLocalForm;

        /// <summary>
        /// FormService constructor
        /// </summary>
        /// <param name="repo">Repository class that talks to database</param>
        /// <param name="env">Hosting environment; only used to check if local</param>
        /// <param name="config">appsettings.json values, source of whether to use the local schema in local dev environment</param>
        public FormService(IGenericRepository<FormVM> repo, IHostingEnvironment env, IConfiguration config)
        {
            _repo = repo;

            _env = env;
            _config = config;
            _useLocalForm = _env.IsEnvironment("Local") && _config.GetSection("LocalFeatureFlags").GetValue<bool>("UseLocalFormSchema");
        }

        /// <summary>
        /// Method to pass through local version of the forms in Content folder, instead of the version in the database. Only available in local environment
        /// </summary>
        /// <param name="formName">Form schema to return</param>
        /// <returns></returns>
        private Task<FormVM> getLocalForm(string formName = "form-schema")
        {
            if (formName == "give-feedback-on-care") formName = "form-schema"; //Handles discrepancy between in-form name field and file name

            var schemaFile = File.ReadAllText($@"{_env.ContentRootPath}\Content\{formName}.json");
            return Task.FromResult(JsonConvert.DeserializeObject<FormVM>(schemaFile));
        }
        
        public Task<FormVM> GetLatestForm()
        {
            return _useLocalForm ? getLocalForm() : _repo.GetAsync(null, null, (x => x.LastModified));
        }

        public Task<FormVM> GetLatestFormByName(string formName)
        {
            return _useLocalForm ? getLocalForm(formName) : _repo.GetAsync((x => x.FormName == formName), null, (x => x.LastModified));
        }

        public Task<FormVM> GetFormById(string id)
        {
            return _useLocalForm ? getLocalForm() : _repo.GetByIdAsync(id);
        }

        public async Task<FormVM> FindByName(string formName)
        {
            return _useLocalForm ? getLocalForm(formName).Result : _repo.FindByAsync(m => m.FormName == formName).Result.FirstOrDefault();
        }

        public async Task<FormVM> FindByVersion(string version)
        {
            return _useLocalForm ? getLocalForm().Result : _repo.FindByAsync(m => m.Version == version).Result.FirstOrDefault();
        }

        public async Task<FormVM> FindByNameAndVersion(string formName, string version)
        {
            return _useLocalForm ? getLocalForm(formName).Result : _repo.FindByAsync(m => m.FormName == formName && m.Version == version).Result.FirstOrDefault();
        }

    }
}
