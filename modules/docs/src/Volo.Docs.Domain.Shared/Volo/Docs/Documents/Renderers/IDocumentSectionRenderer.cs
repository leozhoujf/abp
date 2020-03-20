using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Volo.Docs.Documents.Renderers
{
    public interface IDocumentSectionRenderer: ITransientDependency
    {
        Task<string> RenderAsync(string document, DocumentRenderParameters parameters = null, List<DocumentPartialTemplateContent> partialTemplates = null);

        Task<Dictionary<string, List<string>>> GetAvailableParametersAsync(string document);

        Task<List<DocumentPartialTemplateWithValuesDto>> GetPartialTemplatesInDocumentAsync(string documentContent);
    }
}
