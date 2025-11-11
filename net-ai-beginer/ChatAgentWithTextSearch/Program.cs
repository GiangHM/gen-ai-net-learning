using ChatAgentWithTextSearch;
using ChatAgentWithTextSearch.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using System.ClientModel;

ChatCompletionModelSettings settings = new ChatCompletionModelSettings();

Console.WriteLine("Initialize plugins...");
GoogleTextSearchSettings gitHubSettings = settings.GetSettings<GoogleTextSearchSettings>();
GoogleTextSearchPlugin googleSearchPlugin = new GoogleTextSearchPlugin(gitHubSettings);

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

// Add plugin
kernelBuilder.Plugins.AddFromObject(googleSearchPlugin);

// Add chat completion service
kernelBuilder.AddOpenAIChatCompletion(settings.GitHubModelAI.ChatModel
    , new OpenAIClient(
        new ApiKeyCredential(settings.GitHubModelAI.Token)
        , new OpenAIClientOptions { Endpoint = new Uri(settings.GitHubModelAI.Endpoint) }));

var kernel =kernelBuilder.Build();

// Add agent
Console.WriteLine("Defining agent...");
ChatCompletionAgent writterAgent = new()
{
    Description = "This writer agent takes a request from a user as well as research provider by a web researcher to produce a document.",
    Instructions = "You are an expert copywriter who can take research from a web researcher " +
    " to produce a fun and engaging article that can be used as a magazine article or a blog post." +
    " The goal is to engage the reader and provide them with a fun and informative article." +
    " The article should be between 800 and 1000 words. Use the following instructions as the basis of your article:" +
    " # Research" +
    " {{$research_context}} " +
    " This is Google Web Research" +
    " Use this research to write the article. The research can include entities, web search results, and  news search results." +
    " While it is ok to use the research as a basis for the article, please do not copy and paste the research verbatim." +
    " Instead, use the research to write a fun and engaging article. Do not invent information that is not in the research." +
    " Try to keep your writing short and to the point. The goal is to engage the reader and provide them with\r\n  a fun and informative article." +
    " The article should be between 800 and 1200 words." +
    " Please format the article as markdown but do not include ```markdown``` in the document. ",
    Kernel = kernel,
    Arguments = new KernelArguments(new OpenAIPromptExecutionSettings
    {
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        //WebSearchOptions 
    })
    
};

Console.WriteLine("Agent ready!");

ChatHistoryAgentThread thread = new();

bool isComplete = false;

do
{
    Console.WriteLine();
    Console.Write("> ");
    string input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    // We can use a request model for a Chat Api version
    var message = new ChatMessageContent(AuthorRole.User, input);

    Console.WriteLine();

    DateTime now = DateTime.Now;
    KernelArguments arguments = new()
    {
        { "research_context", message }
    };
    await foreach (ChatMessageContent response in writterAgent.InvokeAsync(message, thread, options: new() { KernelArguments = arguments }))
    {
        // Display response.
        Console.WriteLine($"{response.Content}");
    }

} while (!isComplete);
