using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;


string filePath = Path.GetFullPath("appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(filePath)
    .Build();

// Create a kernel with Azure OpenAI chat completion
// modelId = deploymentName
var modelId = config["DeploymentName"] ?? "Phi-3.5-mini-instruct-generic-cpu";
var endpoint = config["Endpoint"] ?? "";

// Create a kernel and chat completion service
var builder = Kernel.CreateBuilder();

//AddAzureOpenAIChatCompletion => to interact with Azure OpenAI Service| Azure Foundry Service
builder.AddOpenAIChatCompletion(modelId, new Uri(endpoint), null);
var kernel = builder.Build();

#region 1-  Use the chat completion service
//var chat = kernel.GetRequiredService<IChatCompletionService>();

//// Start a chat loop
//Console.WriteLine("Chat with the Semantic Kernel! Type 'exit' to quit.");
//var history = new ChatHistory();
//string prompt = $"""
//You are a highly experienced software engineer. Explain the concept of asynchronous programming to a beginner.
//""";
//while (true)
//{
//    //Console.Write("You: ");
//    //var userInput = Console.ReadLine();
//    //if (string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase)) break;
//    history.AddUserMessage(prompt);
//    var response = await chat.GetChatMessageContentAsync(history);
//    Console.WriteLine($"Bot: {response}");
//    history.AddAssistantMessage(response.ToString());
//}
#endregion

#region 2 - Use the Handlebars template engine


// Alternative way, you can use Handlebars templates in YAML prompts
const string handlebarsTemplate = """
         <message role="system">
         Instructions: You are a career advisor. Analyze the skill gap between 
         the user's current skills and the requirements of the target role.
         </message>
         <message role="user">Target Role:{{targetRole}} </message>
         <message role="user">Current Skills: {{currentSkills}}</message>

         <message role="assistant">
         "Skill Gap Analysis":
         {
             "missingSkills": [],
             "coursesToTake": [],
             "certificationSuggestions": []
         }
         </message>
         """;

var templateFactory = new HandlebarsPromptTemplateFactory();
var promptTemplateConfigs = new PromptTemplateConfig()
{
    Template = handlebarsTemplate,
    TemplateFormat = "handlebars",
    Name = "MissingSkillsPrompt",
};

// arguments can be an user input
var kernelArguments = new KernelArguments
{
    ["targetRole"] = "Game Developer",
    ["currentSkills"] = "Software Engineering, C#, Python, Drawing, Guitar, Dance"
};


var renderedPromptTemplate = await templateFactory.Create(promptTemplateConfigs)
    .RenderAsync(kernel, kernelArguments);

Console.WriteLine($"Rendered Prompt Template: {renderedPromptTemplate}");


var chat = kernel.GetRequiredService<IChatCompletionService>();
var chatHistory = new ChatHistory();

chatHistory.AddUserMessage(renderedPromptTemplate);
await GetReply();


// Get a follow-up prompt from the user
Console.WriteLine("Assistant: How can I help you?");
Console.Write("User: ");
string input = Console.ReadLine()!;
chatHistory.AddUserMessage(input);
await GetReply();

async Task GetReply()
{
    ChatMessageContent chatContent = await chat.GetChatMessageContentAsync(
    chatHistory: chatHistory,
    kernel: kernel);

    Console.WriteLine($"Assistant Response: {chatContent}");
    chatHistory.AddAssistantMessage(chatContent.ToString());

}
#endregion