using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Web.Mvc.Conversation.View
{
    public static class LinkExtensions
    {
        /// <summary>
        /// a href link regex
        /// </summary>
        private static readonly string LinkRegex = "href=\"(?<url>.+?)\"";

        /// <summary>
        /// Prases the a-tags and adds the conversation identifier to the href url
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static MvcHtmlString AsConversational(this MvcHtmlString self)
        {
            var htmlString = self.ToHtmlString();
            var rx = new Regex(LinkRegex);
            string url = string.Empty;

            var matches = rx.Match(htmlString);

            // > 1 (default group)
            if (matches.Groups.Count > 1)
            {
                var conversationLink = rx.Replace(self.ToHtmlString(), m =>
                {
                    if (rx.GetGroupNames().Contains("url"))
                    {
                        url = matches.Groups["url"].Value;
                        url += (url.Contains("?")) ? "&amp;" : "?";
                        url += CommonExtensions.GetConversationUrlParameter(null);

                        return m.Value.Replace(matches.Groups["url"].Value, url);
                    }
                    return m.Value;
                });

                return new MvcHtmlString(conversationLink);
            }
            return self;
        }
    }
}
