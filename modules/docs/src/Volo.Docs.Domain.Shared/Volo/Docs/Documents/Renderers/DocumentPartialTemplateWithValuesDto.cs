using System.Collections.Generic;

namespace Volo.Docs.Documents.Renderers
{
    public class DocumentPartialTemplateWithValuesDto
    {
        public string Path { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}