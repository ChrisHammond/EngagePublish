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
        public string EngineName;
        public string EngineUrl;
        public string HomePageUrl;
        public string ApiLink;

        public int PortalId
        {
            get
            {
                string i = this.Request.Params["portalId"];
                if (i != null)
                {
                    return Convert.ToInt32(i, CultureInfo.InvariantCulture);
                }
                return -1;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.Response.Clear();
            this.Response.ContentType = "text/xml";

            PortalSettings ps = Utility.GetPortalSettings(this.PortalId);

            object o = Request.QueryString["HomePageUrl"];
            if (o != null)
            {
                EngineName = "EngagePublish";
                EngineUrl = "https://github.com/chrishammond/engagepublish";

                // Assume HTTPS for ApiLink
                ApiLink = "https://" + HttpUtility.HtmlEncode(ps.PortalAlias.HTTPAlias) + HttpUtility.HtmlEncode(ModuleBase.DesktopModuleFolderName) + "services/Metaweblog.ashx";

                // Sanitize HomePageUrl
                HomePageUrl = HttpUtility.HtmlEncode(o.ToString());
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
