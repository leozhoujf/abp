using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace Volo.Docs.Documents.FullSearch.Elastic
{
    public class DocumentChangedEventHandler : ILocalEventHandler<EntityCreatedEventData<Document>>, 
        ILocalEventHandler<EntityUpdatedEventData<Document>>,
        ILocalEventHandler<EntityDeletedEventData<Document>>,
        ITransientDependency
    {
        private readonly DocsElasticSearchOptions _options;
        private readonly IDocumentFullSearch _documentFullSearch;

        public DocumentChangedEventHandler(IDocumentFullSearch documentFullSearch, IOptions<DocsElasticSearchOptions> options)
        {
            _documentFullSearch = documentFullSearch;
            _options = options.Value;
        }

        public async Task HandleEventAsync(EntityCreatedEventData<Document> eventData)
        {
            await AddOrUpdateToSearchIndex(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Document> eventData)
        {
            await AddOrUpdateToSearchIndex(eventData.Entity);
        }

        public async Task HandleEventAsync(EntityDeletedEventData<Document> eventData)
        {
            if (_options.Enable)
            {
                await _documentFullSearch.DeleteAsync(eventData.Entity.Id);
            }
        }
        
        private async Task AddOrUpdateToSearchIndex(Document document)
        {
            if (_options.Enable)
            {
                await _documentFullSearch.AddOrUpdateAsync(document);
            }
        }
    }
}