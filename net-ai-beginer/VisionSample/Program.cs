// See https://aka.ms/new-console-template for more information

using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Hello, I'm learning AI!");

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var gitHubToken = config["githubtoken"];

IChatClient chatClient = new ChatCompletionsClient(
    endpoint: new Uri("https://models.inference.ai.azure.com")
    , new Azure.AzureKeyCredential(gitHubToken))
    // LLM model have vision capabilities or use a model specialized in vision tasks
    .AsChatClient("gpt-4o-mini"); 

// images
string imgRunningShoes = "running-shoes.jpg";
string imgCarLicense = "license.jpg";
string imgReceipt = "german-receipt.jpg";


// prompts
var promptDescribe = "Describe the image";
var promptAnalyze = "How many red shoes are in the picture? and what other shoes colors are there?";
var promptOcr = "What is the text in this picture? Is there a theme for this?";
var promptReceipt = "I bought the coffee and the sausage. How much do I owe? Add a 18% tip.";

// prompts
string systemPrompt = @"You are a useful assistant that describes images using a direct style.";
var prompt = promptDescribe;
string imageFileName = imgRunningShoes;
string image = Path.Combine(Directory.GetCurrentDirectory(), "images", imageFileName);

List<ChatMessage> messages =
[
    new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, systemPrompt),
    new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, prompt),
];

// Create a chat message with the image content
AIContent imageContent = new DataContent(File.ReadAllBytes(image), "image/jpeg");
var messageWithImage = new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, [imageContent]);
messages.Add(messageWithImage);

var response = await chatClient.GetResponseAsync(messages);
Console.WriteLine($"Prompt: {prompt}");
Console.WriteLine($"Image: {imageFileName}");
Console.WriteLine($"Response: {response.Message}");