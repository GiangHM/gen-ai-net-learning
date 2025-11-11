using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using RAG_sample_1;
using System.Text;

Console.WriteLine("Hello, I'm studying AI");

var endpoint = new Uri("http://localhost:11434/");
var modelId = "all-minilm";

// Using the correct client from the Microsoft Semantic Kernel package
IEmbeddingGenerator<string, Embedding<float>> generator = new OllamaEmbeddingGenerator(
    endpoint: endpoint.ToString(),
    modelId: modelId);

var moviesData = MovieVectorStore.CreateMoviesAsync();

var movieStore = new InMemoryVectorStore();
var movieCollection = movieStore.GetCollection<int, MovieVector>("movies");
await movieCollection.CreateCollectionIfNotExistsAsync();

foreach (var movie in moviesData)
{
    var vector = await generator.GenerateEmbeddingVectorAsync(movie.Description);
    movie.Vector = vector;
    await movieCollection.UpsertAsync(movie);
}

// Add chat completion client code here
IChatClient client = new OllamaChatClient(
    new Uri("http://localhost:11434/"), "phi4-mini");

var query = "I want to see family friendly movie";

var chatHistory = new List<ChatMessage>();

var chatOption = new ChatOptions
{
    Temperature = 0.5f,
    TopP = 0.9f,
    ResponseFormat = ChatResponseFormat.Text
};

var systemMessage = new ChatMessage(ChatRole.System,
    "You are a useful movie suggestor agent. You know all kind of movies." +
    " If you don't know an answer, say 'I don't know!'." +
    " Always reply in a funny way. Use emojis if possible." +
    "Just provide the information in short" +
    "1. Name of the movie" +
    "2. When was the movie published?" +
    "3. List of actors/actress");

chatHistory.Add(systemMessage);
chatHistory.Add(new ChatMessage(ChatRole.User, query));

// Perform a sematic search
var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);
var searchOptions = new VectorSearchOptions
{
    Top = 1,
    VectorPropertyName = nameof(MovieVector.Vector)
};

var results = await movieCollection.VectorizedSearchAsync(queryEmbedding, searchOptions);

Console.WriteLine("Search Results:");
await foreach (var result in results.Results)
{
    Console.WriteLine($"Title: {result.Record.Title}");
    Console.WriteLine($"Description: {result.Record.Description}");
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine();
}

// Add search results to the chat history as user messages
await foreach (var result in results.Results)
{
    chatHistory.Add(new ChatMessage(ChatRole.User, result.Record.Description));
}

// Iteract with chat model to get the final answer
Console.WriteLine("AI Response");
var sb = new StringBuilder();
var response = client.GetStreamingResponseAsync(chatHistory, chatOption);
await foreach (var item in response)
{
    sb.Append(item.Text);
    Console.Write(item.Text);
}
Console.WriteLine();

chatHistory.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));