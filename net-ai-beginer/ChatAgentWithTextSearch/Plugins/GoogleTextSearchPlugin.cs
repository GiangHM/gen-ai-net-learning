using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatAgentWithTextSearch.Plugins
{
    internal sealed class GoogleTextSearchSettings
    {
        public string ApiKey { get; set; } = "";

        public string SearchEngineId { get; set; } = string.Empty;
    }
    internal sealed class GoogleTextSearchPlugin
    {
        private readonly GoogleTextSearch? _googleTextSearch;
        public GoogleTextSearchPlugin(GoogleTextSearchSettings settings)
        {
            _googleTextSearch = new GoogleTextSearch(
                initializer: new() { ApiKey = settings.ApiKey },
                searchEngineId: settings.SearchEngineId);
        }

        [KernelFunction]
        public async Task<string> GetRepositoryAsync(string searchText)
        {
            if (searchText == null)
                throw new ArgumentNullException(nameof(searchText));
            if (_googleTextSearch == null)
                return "No research found because of the error";

            KernelSearchResults<string> stringResults = await _googleTextSearch.SearchAsync(searchText, new() { Top = 1, Skip = 0 });

            StringBuilder finalResult = new StringBuilder(); 
            await foreach (string result in stringResults.Results)
            {
                finalResult.Append(result);
            }

            return finalResult.ToString();
        }
    }
}
