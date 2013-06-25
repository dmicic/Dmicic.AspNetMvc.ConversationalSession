using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace System.Web.Mvc.Conversation
{

    internal class ConversationContext
    {
        /// <summary>
        /// Length of the URL parameter
        /// </summary>
        internal static readonly int IdentifierLength = 5;

        /// <summary>
        /// URL parameter name
        /// </summary>
        internal static readonly string ParameterName = "__cnv";

        /// <summary>
        /// Allowed chars for the identifier
        /// </summary>
        internal static readonly string AllowedKeyChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Name of the conversation.
        /// </summary>
        internal string Scope { get; private set; }

        /// <summary>
        /// If set to <c>true</c>, all data of the current conversation will be cleared after the action execution.
        /// </summary>
        internal bool End { get; set; }

        /// <summary>
        /// Current identifier
        /// </summary>
        internal string ConversationIdentifier { get; set; }

        /// <summary>
        /// Caller conversation identifier
        /// </summary>
        internal string CallerConversationIdentifier { get; set; }

        internal HttpSessionStateBase Session
        {
            get { return this.HttpContext.Session; }
        }

        internal HttpContextBase HttpContext
        {
            get; set;
        }

        private ControllerContext ControllerContext { get; set; }

        internal ConversationContext(HttpContextBase httpContext, string scope, string identifier, bool end, string callerIdentifier = null)
        {
            this.HttpContext = httpContext;
            this.HttpContext.Items.Add(HttpConversationalSessionState.HttpContextItemKey, new HttpConversationalSessionState(this));
            this.Scope = scope;
            this.ConversationIdentifier = identifier;
            this.End = end;
        }

        /// <summary>
        /// Create instance of <c>ConversationContext</c>
        /// </summary>
        /// <param name="filterContext">Execution filter</param>
        /// <param name="actionAttr">The called conversation attribute</param>
        /// <returns>New instance of <c>ConversationContext</c></returns>
        internal static ConversationContext CreateConversationContext(ActionExecutingContext filterContext, ConversationAttribute actionAttr)
        {
            string scope = ConversationContext.GetScope(filterContext, actionAttr);
            string identifier = ConversationContext.GetOrCreateIdentifier(filterContext.HttpContext, scope);

            return new ConversationContext(filterContext.HttpContext, scope, identifier, actionAttr.End);
        }

        /// <summary>
        /// Gets the scope name of the current conversation 
        /// </summary>
        /// <param name="filterContext"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static string GetScope(ActionExecutingContext filterContext, ConversationAttribute attr)
        {
            string scope = attr.Scope;

            // Check if Scope property of conversationattribute is set.
            if (string.IsNullOrWhiteSpace(scope))
            {
                // If not, check if an conversation is defined at class level. 
                ConversationAttribute parent = ConversationContext.GetParentConversation(filterContext.Controller);

                // If conversation found, get scope name or use controller name as scope.
                if (parent != null)
                {
                    scope = parent.Scope;

                    // If empty, take controller name as scope. -> Controller scoped conversation
                    if (string.IsNullOrWhiteSpace(scope))
                    {
                        scope = filterContext.Controller.GetType().Name;
                    }
                }
                else
                {
                    // If no class attribute is defined, take controller + action name as scope.
                    scope = filterContext.Controller.GetType().Name + HttpConversationalSessionState.Delimiter + filterContext.ActionDescriptor.ActionName;
                }
            }
            return scope;
        }

        /// <summary>
        /// Get <c>ConversationAttribute</c> of controller (class).
        /// </summary>
        /// <param name="controller">Controller with ConversationAttribute</param>
        /// <returns>ConversationAttribute or null</returns>
        private static ConversationAttribute GetParentConversation(ControllerBase controller)
        {
            return controller.GetType().GetCustomAttributes(typeof(ConversationAttribute), true).FirstOrDefault() as ConversationAttribute;
        }

        /// <summary>
        /// Get the conversation identifier out of the query string. 
        /// If no parameter found, create new identifier.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scope"></param>
        /// <param name="isExistingConversation"></param>
        /// <returns></returns>
        internal static string GetOrCreateIdentifier(HttpContextBase context, string scope)
        {
            var isExistingConversation = false;

            if (context.Request.Params.AllKeys.Any(x => x.Equals(ConversationContext.ParameterName, StringComparison.OrdinalIgnoreCase)))
            {
                string urlId = context.Request.Params[ConversationContext.ParameterName];
                string scopeIdentifier = scope + HttpConversationalSessionState.Delimiter + urlId;

                foreach (string key in context.Session.Keys)
                {
                    if (key.IndexOf(scopeIdentifier, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        isExistingConversation = true;
                        break;
                    }
                }

                if (isExistingConversation)
                    return urlId;
                else
                {
                    return ConversationContext.GenerateRandomIdentifier();
                }
            }
            else
                return ConversationContext.GenerateRandomIdentifier();
        }

        /// <summary>
        /// Ends conversation if End property set to <c>true</c>
        /// </summary>
        internal void EndConversation()
        {
            if (this.End)
            {
                this.ClearDataAndMarkAsEnded();
            }
        }

        /// <summary>
        /// Generate random string, which will be used as identifer
        /// </summary>
        /// <returns></returns>
        internal static string GenerateRandomIdentifier()
        {
            var id = new char[ConversationContext.IdentifierLength];
            var rand = new Random();

            for (int x = 0; x < ConversationContext.IdentifierLength; x++)
            {
                id[x] = ConversationContext.AllowedKeyChars[rand.Next(0, ConversationContext.AllowedKeyChars.Length)];
            }

            return new string(id);
        }

        /// <summary>
        /// Get conversation key
        /// </summary>
        /// <param name="key">key (i.e. abc)</param>
        /// <returns>Key: (i.e. Conversation:{Identifier}:{Scope}:{key}</returns>
        internal string GetKey(string key = null)
        {
            key = string.IsNullOrWhiteSpace(key) ? string.Empty : key;
            string scope = this.Scope;
            return HttpConversationalSessionState.SessionPrefix + HttpConversationalSessionState.Delimiter +
                    scope + HttpConversationalSessionState.Delimiter +
                    this.ConversationIdentifier + HttpConversationalSessionState.Delimiter +
                    key;
        }

        /// <summary>
        /// Clear current conversation data and mark conversation as ended.
        /// </summary>
        internal void ClearDataAndMarkAsEnded()
        {
            string fullScope = this.GetKey();

            for (var x = 0; x < this.Session.Keys.Count; x++)
            {
                if (this.Session.Keys[x].StartsWith(fullScope, StringComparison.OrdinalIgnoreCase))
                    this.Session.RemoveAt(x);
                else
                    x++;
            }

            this.ConversationIdentifier = null;
            this.End = true;
        }
    }
}
