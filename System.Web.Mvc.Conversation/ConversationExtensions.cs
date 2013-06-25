using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web.Mvc.Conversation
{
    public static class ConversationExtensions
    {
        public static HttpConversationalSessionState Conversation(this ControllerBase self)
        {
            var items = self.ControllerContext.HttpContext.Items;

            if (!items.Contains(HttpConversationalSessionState.HttpContextItemKey))
            {
                throw new ConversationNotFoundException();
            }

            return (HttpConversationalSessionState)items[HttpConversationalSessionState.HttpContextItemKey];
        }
    }
}
