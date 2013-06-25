using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Web.Mvc.Conversation
{
    /// <summary>
    /// Conversation session. Manages the reads/writes from/to a conversational session.
    /// All data are stored in the ASP.NET session under a specified key naming.
    /// </summary>
    public class HttpConversationalSessionState
    {
        /// <summary>
        /// Session prefix
        /// </summary>
        internal static readonly string SessionPrefix = "Conversation";
        internal static readonly string HttpContextItemKey = "ConversationState";

        /// <summary>
        /// Session key delimiter
        /// </summary>
        internal static readonly string Delimiter = ":";

        /// <summary>
        /// Current conversation context
        /// </summary>
        internal ConversationContext Context { get; set; }

        internal HttpConversationalSessionState(ConversationContext ctx)
        {
            this.Context = ctx;
        }

        /// <summary>
        /// Conversation data getter/setter
        /// </summary>
        /// <param name="key">Existing key of current conversation</param>
        /// <returns>Appropriate value out of the current conversation</returns>
        public object this[string key]
        {
            get { return this.GetValue(key); }
            set { this.SetValue(key, value); }
        }

        /// <summary>
        /// Get value out of conversation
        /// </summary>
        /// <param name="key">Conversation key</param>
        /// <returns><c>Object</c> or <c>null</c></returns>
        private object GetValue(string key)
        {
            if (this.Context.Session == null)
                return null;

            // Get value
            return this.Context.Session[this.Context.GetKey(key)];
        }

        /// <summary>
        /// Set value in current conversation
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        private void SetValue(string key, object value)
        {
            if (this.Context.Session == null)
                throw new NullReferenceException("Session");

            this.Context.Session[this.Context.GetKey(key)] = value;
        }

        /// <summary>
        /// End current conversation and clear data after action execution.
        /// </summary>
        public void End()
        {
            this.Context.ClearDataAndMarkAsEnded();
        }
    }
}
