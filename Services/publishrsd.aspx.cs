namespace Engage.Dnn.Publish.Services
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using DotNetNuke.Entities.Portals;
    using Util;

    public partial class Publishrsd : System.Web.UI.Page
    {
        public string EngineName { get; private set; }
        public string EngineUrl { get; private set; }
        public string HomePageUrl { get; private set; }
        public string ApiLink { get; private set; }

        public int PortalId
        {
            get
            {
                string portalIdStr = this.Request.QueryString["portalId"];
                if (int.TryParse(portalIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int portalId))
                {
                    return portalId;
                }
                return -1;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.ContentType = "text/xml";

            PortalSettings ps = Utility.GetPortalSettings(this.PortalId);

            string homePageUrlParam = Request.QueryString["HomePageUrl"];
            if (!string.IsNullOrEmpty(homePageUrlParam))
            {
                EngineName = "EngagePublish";
                EngineUrl = "https://github.com/chrishammond/engagepublish";

                // Assume HTTPS for ApiLink and sanitize inputs
                string portalAlias = HttpUtility.HtmlEncode(ps.PortalAlias.HTTPAlias);
                string moduleFolder = HttpUtility.HtmlEncode(ModuleBase.DesktopModuleFolderName);
                ApiLink = $"https://{portalAlias}{moduleFolder}services/Metaweblog.ashx";

                // Sanitize HomePageUrl
                HomePageUrl = HttpUtility.HtmlEncode(homePageUrlParam);
            }

            var responseStream = new StringBuilder(1000);
            responseStream.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?><rsd version=\"1.0\" xmlns=\"https://github.com/danielberlinger/rsd\">");
            responseStream.Append("<service><engineName>");
            responseStream.Append(HttpUtility.HtmlEncode(EngineName));
            responseStream.Append("</engineName><engineLink>");
            responseStream.Append(HttpUtility.HtmlEncode(EngineUrl));
            responseStream.Append("</engineLink><homePageLink>");
            responseStream.Append(HttpUtility.HtmlEncode(HomePageUrl));
            responseStream.Append("</homePageLink><apis><api name=\"MetaWeblog\" preferred=\"true\" apiLink=\"");
            responseStream.Append(HttpUtility.HtmlEncode(ApiLink));
            responseStream.Append("\" blogID=\"0\" /></apis></service></rsd>");
            Response.Write(responseStream.ToString());
        }
    }
}
