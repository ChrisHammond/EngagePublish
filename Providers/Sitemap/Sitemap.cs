using System;
using System.Collections.Generic;
using DotNetNuke.Services.Sitemap;
using Engage.Dnn.Publish.Util;

namespace Engage.Dnn.Publish.Providers.Sitemap
{
    public class Sitemap : SitemapProvider
    {
        public override List<SitemapUrl> GetUrls(int portalId, DotNetNuke.Entities.Portals.PortalSettings ps, string version)
        {
            var listOfUrls = new List<SitemapUrl>();

            foreach (var a in Article.GetAllArticlesList(portalId))
            {

                var pageUrl = new SitemapUrl
                                  {
                                      Url = Utility.GetItemLinkUrl(a.ItemId, portalId),
                                      Priority = (float)0.5,
                                      LastModified = Convert.ToDateTime(a.LastUpdated),
                                      ChangeFrequency = SitemapChangeFrequency.Daily
                                  };
                listOfUrls.Add(pageUrl);

            }
            return listOfUrls;


        }
    }
}
