using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Threading.RateLimiting;

namespace functioncalling_localfunction
{
    internal sealed class RateLimiterMiddleware(IChatClient next, RateLimiter rateLimiter)
        : DelegatingChatClient(next)
    {
        public override async Task<ChatResponse> GetResponseAsync(IList<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            using var lease = await rateLimiter.AcquireAsync(1, cancellationToken)
                .ConfigureAwait(false);
            if (!lease.IsAcquired)
                throw new InvalidOperationException("Rate limit exceeded. Please try again later.");

            return await base.GetResponseAsync(chatMessages, options, cancellationToken)
                .ConfigureAwait(false);
        }
        public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IList<ChatMessage> chatMessages
            , ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var lease = await rateLimiter.AcquireAsync(1, cancellationToken)
                .ConfigureAwait(false);
            if (!lease.IsAcquired)
                throw new InvalidOperationException("Rate limit exceeded. Please try again later.");
            await foreach (var response in base.GetStreamingResponseAsync(chatMessages, options, cancellationToken)
                .ConfigureAwait(false))
            {
                yield return response;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                rateLimiter.Dispose();

            base.Dispose(disposing);
        }
    }

    internal static class RateLimiterChatClientExtentsions
    {
        internal static ChatClientBuilder UseRateLimiting(this ChatClientBuilder builder
            , RateLimiter? rateLimiter = null)
        {
            return builder.Use((innerClient, service)
                => new RateLimiterMiddleware(innerClient
                , service.GetRequiredService<RateLimiter>()));
        }
    }
}
