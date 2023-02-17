using Ethanol.ContextBuilder.Context;
using System.Net;
using static Ethanol.ContextBuilder.Builders.HostContextBuilder;

namespace Ethanol.ContextBuilder.Builders
{
    /// <summary>
    ///  Represents a collection of flows related to the host.
    /// </summary>
    public record HostConversations
    {
        public IPAddress Host { get; set; }
        public Conversation[] Conversations { get; set; }
    }

    /// <summary>
    /// Represents a bidirectional flow, i.e., Conversation.
    /// </summary>
    public class Conversation
    {
        public FlowKey ConversationKey => UpFlow?.FlowKey ?? DownFlow?.FlowKey;
        public IpFlow UpFlow { get; set; }
        public IpFlow DownFlow { get; set; }
    }
}
