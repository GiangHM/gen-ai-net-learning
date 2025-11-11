using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ChatAgentWithTextSearch
{
    public class ChatCompletionModelSettings
    {
        private readonly IConfigurationRoot configRoot;

        private GitHubModel githubModelAI;

        public GitHubModel GitHubModelAI => this.githubModelAI ??= this.GetSettings<ChatCompletionModelSettings.GitHubModel>();
        public class GitHubModel
        {
            public string ChatModel { get; set; } = string.Empty;
            public string Endpoint { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }
        public TSettings GetSettings<TSettings>() =>
        this.configRoot.GetRequiredSection(typeof(TSettings).Name).Get<TSettings>()!;

        public ChatCompletionModelSettings()
        {
            this.configRoot =
                new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
                    .Build();
        }
    }
}
