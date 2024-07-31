//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Publish
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Xml;
    using Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Utilities;
    using Util;
    using DotNetNuke.Services.FileSystem;

    public partial class EpRss : PageBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _itemType;
        private string _qsTags;
        private ArrayList _tagQuery;

        public bool AllowTags => ModuleBase.AllowTagsForPortal(PortalId);

        public static string ApplicationUrl => HttpContext.Current.Request.ApplicationPath == "/" ? string.Empty : HttpContext.Current.Request.ApplicationPath;

        public int ItemId
        {
            get
            {
                if (int.TryParse(Request.QueryString["itemId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemId))
                {
                    // look up the _itemType if ItemId passed in.
                    ItemType = Item.GetItemType(itemId, PortalId).ToUpperInvariant();
                    return itemId;
                }
                return -1;
            }
        }

        public int ItemTypeId
        {
            get
            {
                if (int.TryParse(Request.QueryString["itemTypeId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int itemTypeId))
                {
                    return itemTypeId;
                }
                return -1;
            }
        }

        public int NumberOfItems
        {
            get
            {
                if (int.TryParse(Request.QueryString["numberOfItems"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int numberOfItems))
                {
                    return numberOfItems;
                }
                return -1;
            }
        }

        public int PortalId
        {
            get
            {
                if (int.TryParse(Request.QueryString["portalId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int portalId))
                {
                    return portalId;
                }
                return -1;
            }
        }

        public string DisplayType => Request.QueryString["DisplayType"];

        public int RelationshipTypeId
        {
            get
            {
                if (int.TryParse(Request.QueryString["RelationshipTypeId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int relationshipTypeId))
                {
                    return relationshipTypeId;
                }
                return -1;
            }
        }

        public string ItemType
        {
            [DebuggerStepThrough]
            get => _itemType;
            [DebuggerStepThrough]
            set => _itemType = value;
        }

        protected override void OnInit(EventArgs e)
        {
            Load += Page_Load;
            base.OnInit(e);

            // TODO: refactor this between here, CustomDisplay.OnInit, TagCloud.LoadTagInfo
            //read the tags querystring parameter and see if we have any to store into the _tagQuery arraylist
            if (AllowTags)
            {
                string tags = Request.QueryString["Tags"];
                if (!string.IsNullOrEmpty(tags))
                {
                    _qsTags = HttpUtility.UrlDecode(tags);
                    char[] separator = { '-' };
                    ArrayList tagList = Tag.ParseTags(_qsTags, PortalId, separator, false);
                    _tagQuery = new ArrayList(tagList.Count);
                    foreach (Tag tg in tagList)
                    {
                        //create a list of tagids to query the database
                        if (_tagQuery.Count < 1)
                        {
                            _tagQuery.Add(tg.TagId);
                        }
                    }
                }
            }
        }

        private void Page_Load(object sender, EventArgs e)
        {
            PortalSettings ps = Utility.GetPortalSettings(PortalId);

            Response.ContentType = "text/xml";
            Response.ContentEncoding = Encoding.UTF8;

            var sw = new StringWriter(CultureInfo.InvariantCulture);
            var wr = new XmlTextWriter(sw);

            wr.WriteStartElement("rss");
            wr.WriteAttributeString("version", "2.0");
            wr.WriteAttributeString("xmlns:dc", "https://purl.org/dc/elements/1.1/");
            wr.WriteAttributeString("xmlns:content", "https://purl.org/rss/1.0/modules/content/");
            wr.WriteAttributeString("xmlns:atom", "https://www.w3.org/2005/Atom");

            wr.WriteStartElement("channel");
            wr.WriteElementString("title", ps.PortalName);
            if (ps.PortalAlias.HTTPAlias.IndexOf("https://", StringComparison.OrdinalIgnoreCase) == -1)
            {
                wr.WriteElementString("link", "https://" + ps.PortalAlias.HTTPAlias);
            }
            else
            {
                wr.WriteElementString("link", ps.PortalAlias.HTTPAlias);
            }
            wr.WriteElementString("description", "RSS Feed for " + ps.PortalName);
            wr.WriteElementString("ttl", "120");

            //TODO: look into options for how to display the "Title" of the RSS feed
            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            if (string.Equals(DisplayType, "ItemListing", StringComparison.OrdinalIgnoreCase) || DisplayType == null)
            {
                dt = ItemId == -1 ? DataProvider.Instance().GetMostRecent(ItemTypeId, NumberOfItems, PortalId) : DataProvider.Instance().GetMostRecentByCategoryId(ItemId, ItemTypeId, NumberOfItems, PortalId);
            }
            else if (string.Equals(DisplayType, "CategoryFeature", StringComparison.OrdinalIgnoreCase))
            {
                DataSet ds = DataProvider.Instance().GetParentItems(ItemId, PortalId, RelationshipTypeId);
                dt = ds.Tables[0];
            }
            else if (string.Equals(DisplayType, "TagFeed", StringComparison.OrdinalIgnoreCase))
            {
                if (AllowTags && _tagQuery != null && _tagQuery.Count > 0)
                {
                    string tagCacheKey = Utility.CacheKeyPublishTag + PortalId + ItemTypeId.ToString(CultureInfo.InvariantCulture) + _qsTags;
                    dt = DataCache.GetCache(tagCacheKey) as DataTable;
                    if (dt == null)
                    {
                        dt = Tag.GetItemsFromTags(PortalId, _tagQuery);
                        DataCache.SetCache(tagCacheKey, dt, DateTime.Now.AddMinutes(5));
                        Utility.AddCacheKey(tagCacheKey, PortalId);
                    }
                }
            }
            if (dt != null)
            {
                DataView dv = dt.DefaultView;
                if (dv.Table.Columns.IndexOf("dateColumn") == -1)
                {
                    dv.Table.Columns.Add("dateColumn", typeof(DateTime));
                    foreach (DataRowView dr in dv)
                    {
                        dr["dateColumn"] = Convert.ToDateTime(dr["startdate"]);
                    }

                    dv.Sort = "dateColumn desc";
                }

                foreach (DataRowView dr in dv)
                {
                    DataRow r = dr.Row;
                    wr.WriteStartElement("item");

                    string title = r["ChildName"].ToString();
                    string description = r["ChildDescription"].ToString();
                    string childItemId = r["ChilditemId"].ToString();
                    string guid = r["itemVersionIdentifier"].ToString();
                    DateTime startDate = (DateTime)r["StartDate"];
                    string thumbnail = r["Thumbnail"].ToString();
                    string author = r["Author"].ToString();

                    if (!Uri.IsWellFormedUriString(thumbnail, UriKind.Absolute) && !string.IsNullOrEmpty(thumbnail))
                    {
                        var thumbnailLink = new Uri(Request.Url, ps.HomeDirectory + thumbnail);
                        thumbnail = thumbnailLink.ToString();
                    }

                    wr.WriteElementString("title", title);

                    if (!Utility.IsDisabled(Convert.ToInt32(childItemId, CultureInfo.InvariantCulture), PortalId))
                    {
                        wr.WriteElementString("link", Utility.GetItemLinkUrl(childItemId, PortalId));
                    }

                    description = Utility.ReplaceTokens(description);
                    wr.WriteElementString("description", Server.HtmlDecode(description));
                    wr.WriteElementString("thumbnail", thumbnail);

                    wr.WriteElementString("dc:creator", author);

                    wr.WriteElementString("pubDate", startDate.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));

                    ItemVersionSetting attachmentSetting = ItemVersionSetting.GetItemVersionSetting(Convert.ToInt32(r["ItemVersionId"].ToString()), "ArticleSettings", "ArticleAttachment", PortalId);
                    if (attachmentSetting != null && attachmentSetting.PropertyValue.Length > 7)
                    {
                        var fileManager = new FileManager();
                        int fileId = Convert.ToInt32(attachmentSetting.PropertyValue.Substring(7));

                        var fi = fileManager.GetFile(fileId);
                        string fileUrl = "https://" + PortalSettings.PortalAlias.HTTPAlias + PortalSettings.HomeDirectory + fi.Folder + fi.FileName;
                        wr.WriteStartElement("enclosure");
                        wr.WriteAttributeString("url", fileUrl);
                        wr.WriteAttributeString("length", fi.Size.ToString());
                        wr.WriteAttributeString("type", fi.ContentType);
                        wr.WriteEndElement();
                    }

                    wr.WriteStartElement("guid");
                    wr.WriteAttributeString("isPermaLink", "false");
                    wr.WriteString(guid);
                    wr.WriteEndElement();

                    wr.WriteEndElement();
                }
            }

            wr.WriteEndElement();
            wr.WriteEndElement();
            Response.Write(sw.ToString());
        }
    }
}
