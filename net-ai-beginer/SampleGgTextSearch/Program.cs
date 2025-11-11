using Google.Apis.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;


var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var apiKey = Environment.GetEnvironmentVariable("GoogleApiKey") ?? config["GoogleApiKey"];
var searchEngineId = Environment.GetEnvironmentVariable("SearchEngineId") ?? config["SearchEngineId"];

var textSearch = new GoogleTextSearch(
    initializer: new() { ApiKey = apiKey },
    searchEngineId: searchEngineId);

var query = "What is the Semantic Kernel?";

// Search and return results as string items
KernelSearchResults<string> stringResults = await textSearch.SearchAsync(query, new() { Top = 4, Skip = 0 });
Console.WriteLine("——— String Results ———\n");
await foreach (string result in stringResults.Results)
{
    Console.WriteLine(result);
}

// Search and return results as TextSearchResult items
KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4, Skip = 4 });
Console.WriteLine("\n——— Text Search Results ———\n");
await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}

// Search and return results as Google.Apis.CustomSearchAPI.v1.Data.Result items
KernelSearchResults<object> fullResults = await textSearch.GetSearchResultsAsync(query, new() { Top = 4, Skip = 8 });
Console.WriteLine("\n——— Google Web Page Results ———\n");
await foreach (Google.Apis.CustomSearchAPI.v1.Data.Result result in fullResults.Results)
{
    Console.WriteLine($"Title:       {result.Title}");
    Console.WriteLine($"Snippet:     {result.Snippet}");
    Console.WriteLine($"Link:        {result.Link}");
    Console.WriteLine($"DisplayLink: {result.DisplayLink}");
    Console.WriteLine($"Kind:        {result.Kind}");
}