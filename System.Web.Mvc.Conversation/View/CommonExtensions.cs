using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace System.Web.Mvc.Conversation.View
{

    public static class CommonExtensions
    {
        /// <summary>
        /// Get the current conversation identifier/key
        /// </summary>
        /// <param name="self"></param>
        /// <returns>Identifier/Key</returns>
        public static string GetConversationIdentifier(this HtmlHelper self)
        {
            return CommonExtensions.GetConversationIdentifier();
        }

        /// <summary>
        /// Get the conversation parameter name 
        /// </summary>
        /// <param name="self"></param>
        /// <returns>Conversation Parameter name</returns>
        public static string GetConversationParameterName(this HtmlHelper self)
        {
            return ConversationContext.ParameterName;
        }

        /// <summary>
        /// Generate conversation url parameter
        /// </summary>
        /// <param name="self"></param>
        /// <returns>Conversation url parameter: (__cnv=123)</returns>
        public static string GetConversationUrlParameter(this HtmlHelper self)
        {
            return ConversationContext.ParameterName + "=" + CommonExtensions.GetConversationIdentifier();
        }

        /// <summary>
        /// Create route values and add the current conversation identifier
        /// </summary>
        /// <param name="self"></param>
        /// <param name="routeValues">Route values</param>
        /// <returns>Route values with conversation identifier</returns>
        public static RouteValueDictionary CreateRouteValues(this HtmlHelper self, object routeValues = null)
        {
            var values = (routeValues == null) ? new RouteValueDictionary() : new RouteValueDictionary(routeValues);
            values.Add(CommonExtensions.GetConversationParameterName(self), CommonExtensions.GetConversationIdentifier());
            return values;
        }


        private static string GetConversationIdentifier()
        {
            var items = HttpContext.Current.Items;
            var conversation = HttpContext.Current.Items[HttpConversationalSessionState.HttpContextItemKey] as HttpConversationalSessionState;

            if (conversation == null)
                throw new ConversationNotFoundException();

            return conversation.Context.ConversationIdentifier;
        }
    }
}
