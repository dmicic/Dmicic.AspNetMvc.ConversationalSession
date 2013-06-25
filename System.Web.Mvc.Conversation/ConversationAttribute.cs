using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web.Mvc.Conversation
{
    public class ConversationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Name of the conversation. (Optional)
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// If set to <c>true</c>, all data of the current conversation will be cleared after the action execution.
        /// </summary>
        public bool End { get; set; }

        /// <summary>
        /// Handles conversation related logic
        /// </summary>
        private ConversationContext Context { get; set; }

        /// <summary>
        /// Create conversation
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.Context = ConversationContext.CreateConversationContext(filterContext, this);
        }

        /// <summary>
        /// End conversation if conversation is ended.
        /// Add conversation parameter routed data if conversation isn't ended.
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (this.Context != null)
                this.Context.EndConversation();
        }
    }
}
