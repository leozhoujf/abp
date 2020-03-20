using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Scriban;

namespace Volo.Docs.Documents.Renderers
{
    public class ScribanDocumentSectionRenderer : IDocumentSectionRenderer
    {
        private const string JsonOpener = "````json";
        private const string JsonCloser = "````";
        private const string DocsParam = "//[doc-params]";
        private const string DocsTemplates = "//[doc-template]";

        public ILogger<ScribanDocumentSectionRenderer> Logger { get; set; }

        public ScribanDocumentSectionRenderer()
        {
            Logger = NullLogger<ScribanDocumentSectionRenderer>.Instance;
        }

        public async Task<string> RenderAsync(
            string document,
            DocumentRenderParameters parameters = null,
            List<DocumentPartialTemplateContent> partialTemplates = null)
        {
            if (partialTemplates != null && partialTemplates.Any())
            {
                document = SetPartialTemplates(document, partialTemplates);
            }

            var scribanTemplate = Template.Parse(document);

            if (parameters == null)
            {
                return await scribanTemplate.RenderAsync();
            }

            var result = await scribanTemplate.RenderAsync(parameters);

            return RemoveOptionsJson(result);
        }


        public async Task<Dictionary<string, List<string>>> GetAvailableParametersAsync(string document)
        {
            try
            {
                if (!document.Contains(JsonOpener) || !document.Contains(DocsParam))
                {
                    return new Dictionary<string, List<string>>();
                }

                var jsonIndexResult = GetJsonBeginEndIndexesAndPureJson(document);

                if (jsonIndexResult.JsonBeginningIndex < 0 || jsonIndexResult.JsonEndingIndex <= 0 || string.IsNullOrWhiteSpace(jsonIndexResult.InsideJsonSection))
                {
                    return new Dictionary<string, List<string>>();
                }

                var pureJson = jsonIndexResult.InsideJsonSection.Replace(DocsParam, "").Trim();

                var result = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(pureJson);

                return await Task.FromResult(result);
            }
            catch (Exception)
            {
                Logger.LogWarning("Unable to parse parameters of document.");
                return await Task.FromResult(new Dictionary<string, List<string>>());
            }
        }

        private static string RemoveOptionsJson(string document)
        {
            var orgDocument = document;
            try
            {
                if (!document.Contains(JsonOpener) || !document.Contains(DocsParam))
                {
                    return orgDocument;
                }

                var jsonIndexResult = GetJsonBeginEndIndexesAndPureJson(document);

                if (jsonIndexResult.JsonBeginningIndex < 0 ||
                    jsonIndexResult.JsonEndingIndex <= 0 ||
                    string.IsNullOrWhiteSpace(jsonIndexResult.InsideJsonSection))
                {
                    return orgDocument;
                }

                var length = (jsonIndexResult.JsonEndingIndex + JsonCloser.Length) -
                             (jsonIndexResult.JsonBeginningIndex - JsonOpener.Length);

                return document.Remove(
                    jsonIndexResult.JsonBeginningIndex - JsonOpener.Length,
                    length
                );
            }
            catch (Exception)
            {
                return orgDocument;
            }
        }

        private static JsonIndexResult GetJsonBeginEndIndexesAndPureJson(string document)
        {
            var searchedIndex = 0;

            while (searchedIndex < document.Length)
            {
                int jsonBeginningIndex = document.Substring(searchedIndex)
                                             .IndexOf(JsonOpener, StringComparison.Ordinal) + JsonOpener.Length + searchedIndex;

                if (jsonBeginningIndex < 0)
                {
                    return new JsonIndexResult(-1, -1, string.Empty);
                }

                var jsonEndingIndex = document.Substring(jsonBeginningIndex).IndexOf(JsonCloser, StringComparison.Ordinal) + jsonBeginningIndex;
                var insideJsonSection = document.Substring(jsonBeginningIndex, jsonEndingIndex);

                if (insideJsonSection.IndexOf(DocsParam, StringComparison.Ordinal) < 0)
                {
                    searchedIndex = jsonEndingIndex + JsonCloser.Length;
                    continue;
                }

                return new JsonIndexResult(jsonBeginningIndex, jsonEndingIndex, insideJsonSection);
            }

            return new JsonIndexResult(-1, -1, string.Empty);
        }

        public async Task<List<DocumentPartialTemplateWithValuesDto>> GetPartialTemplatesInDocumentAsync(string documentContent)
        {
            var templates = new List<DocumentPartialTemplateWithValuesDto>();

            while (documentContent.Contains(JsonOpener))
            {
                var afterJsonOpener = documentContent.Substring(
                    documentContent.IndexOf(JsonOpener, StringComparison.Ordinal) + JsonOpener.Length);

                var betweenJsonOpenerAndCloser = afterJsonOpener.Substring(0,
                    afterJsonOpener.IndexOf(JsonCloser, StringComparison.Ordinal));

                documentContent = afterJsonOpener.Substring(
                    afterJsonOpener.IndexOf(JsonCloser, StringComparison.Ordinal) + JsonCloser.Length);

                if (!betweenJsonOpenerAndCloser.Contains(DocsTemplates))
                {
                    continue;
                }

                var json = betweenJsonOpenerAndCloser.Substring(betweenJsonOpenerAndCloser.IndexOf(DocsTemplates, StringComparison.Ordinal) + DocsTemplates.Length);

                var template = JsonConvert.DeserializeObject<DocumentPartialTemplateWithValuesDto>(json);

                templates.Add(template);
            }

            return await Task.FromResult(templates);
        }

        private static string SetPartialTemplates(string document, List<DocumentPartialTemplateContent> templates)
        {
            var newDocument = new StringBuilder();

            while (document.Contains(JsonOpener))
            {
                var beforeJson = document.Substring(0,
                    document.IndexOf(JsonOpener, StringComparison.Ordinal) + JsonOpener.Length);

                var afterJsonOpener = document.Substring(
                    document.IndexOf(JsonOpener, StringComparison.Ordinal) + JsonOpener.Length);

                var betweenJsonOpenerAndCloser = afterJsonOpener.Substring(0,
                    afterJsonOpener.IndexOf(JsonCloser, StringComparison.Ordinal));

                if (!betweenJsonOpenerAndCloser.Contains(DocsTemplates))
                {
                    document = afterJsonOpener.Substring(
                        afterJsonOpener.IndexOf(JsonCloser, StringComparison.Ordinal) + JsonCloser.Length
                    );

                    newDocument.Append(beforeJson + betweenJsonOpenerAndCloser + JsonCloser);
                    continue;
                }

                var json = betweenJsonOpenerAndCloser
                    .Substring(betweenJsonOpenerAndCloser
                                   .IndexOf(DocsTemplates, StringComparison.Ordinal) + DocsTemplates.Length);

                var templatePath = JsonConvert.DeserializeObject<DocumentPartialTemplateWithValuesDto>(json)?.Path;

                var template = templates.FirstOrDefault(t => t.Path == templatePath);

                var beforeTemplate = document.Substring(0,
                    document.IndexOf(JsonOpener, StringComparison.Ordinal));

                newDocument.Append(beforeTemplate + template?.Content + JsonCloser);

                document = afterJsonOpener.Substring(
                    afterJsonOpener.IndexOf(JsonCloser, StringComparison.Ordinal) + JsonCloser.Length);
            }

            newDocument.Append(document);

            return newDocument.ToString();
        }

        public class JsonIndexResult
        {
            public int JsonBeginningIndex { get; set; }
            public int JsonEndingIndex { get; set; }
            public string InsideJsonSection { get; set; }

            public JsonIndexResult()
            {

            }

            public JsonIndexResult(int jsonBeginningIndex, int jsonEndingIndex, string insideJsonSection)
            {
                JsonBeginningIndex = jsonBeginningIndex;
                JsonEndingIndex = jsonEndingIndex;
                InsideJsonSection = insideJsonSection;
            }
        }
    }
}
