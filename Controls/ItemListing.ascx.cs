//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Publish.Controls
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using Data;
    using Util;


    public partial class ItemListing : ModuleBase, IActionable
    {
        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            Load += Page_Load;
            base.OnInit(e);
        }

        private void Page_Load(object sender, EventArgs e)
        {
            //try
            //{
                if (ItemTypeId == -1)
                {
                    lblMessage.Text = Localization.GetString("SetupItemType", LocalResourceFile);
                    lblMessage.Visible = true;
                    return;
                }

                lstItems.DataSource = GetData();
                lstItems.DataBind();
            //}
            //catch (Exception exc)
            //{
            //    Exceptions.ProcessModuleLoadException(this, exc);
            //}
        }

        #endregion

        private DataTable GetData()
        {
            int itemId = ItemId;  //Since figuring out the Item ID is so expensive, do it only once.
            //this obviously needs refactoring

            string cacheKey = Utility.CacheKeyPublishCategory + "ItemListing_" + DataType.Replace(" ", "") + CategoryId; // +"PageId";
            var dt = DataCache.GetCache(cacheKey) as DataTable;

            if (dt == null)
            {
                switch (DataType)
                {
                    case "Item Listing":
                        if (CategoryId != -1)
                        {
                            dt = DataProvider.Instance().GetChildrenInCategory(CategoryId, ItemTypeId, MaxDisplayItems, PortalId);
                        }
                        else
                        {
                            DataSet ds = DataProvider.Instance().GetItems(TopLevelCategoryItemType.Category.GetId(), PortalId, Util.RelationshipType.CategoryToTopLevelCategory.GetId(), ItemTypeId);
                            dt = ds.Tables[0];
                            dt = FormatDataTable(dt);
                        }
                        break;
                    case "Most Popular":
                        dt = DataProvider.Instance().GetMostPopular(CategoryId, ItemTypeId, MaxDisplayItems, PortalId);
                        break;
                    case "Most Recent":
                        if (EnableRss)
                        {
                            lnkRss.Visible = true;
                            string rssImage = Localization.GetString("rssImage", LocalResourceFile);
#if DEBUG
                            rssImage = rssImage.Replace("[L]", "");
#endif

                            lnkRss.ImageUrl = ApplicationUrl + rssImage; //"/images/xml.gif";
                            lnkRss.Attributes.Add("alt", Localization.GetString("rssAlt", LocalResourceFile));
                            lnkRss.NavigateUrl = GetRssLinkUrl(itemId, MaxDisplayItems, ItemTypeId, PortalId, "ItemListing");

                            SetRssUrl(lnkRss.NavigateUrl, Localization.GetString("rssText", LocalResourceFile));
                        }
                        dt = itemId > -1 ? DataProvider.Instance().GetMostRecentByCategoryId(itemId, ItemTypeId, MaxDisplayItems, PortalId) : DataProvider.Instance().GetMostRecent(ItemTypeId, MaxDisplayItems, PortalId);
                        break;
                    default:
                        dt = DataProvider.Instance().GetChildrenInCategory(CategoryId, ItemTypeId, MaxDisplayItems, PortalId);
                        break;
                }

                //Set the object into cache
                if (dt != null)
                {
                    DataCache.SetCache(cacheKey, dt, DateTime.Now.AddMinutes(CacheTime));
                    Utility.AddCacheKey(cacheKey, PortalId);
                }
            }

            if (EnableRss && DataType == "Most Recent")
            {
                lnkRss.Visible = true;
                string rssImage = Localization.GetString("rssImage", LocalResourceFile);
#if DEBUG
                rssImage = rssImage.Replace("[L]", "");
#endif

                lnkRss.ImageUrl = ApplicationUrl + rssImage; //"/images/xml.gif";
                lnkRss.Attributes.Add("alt", Localization.GetString("rssAlt", LocalResourceFile));
                lnkRss.NavigateUrl = GetRssLinkUrl(itemId, MaxDisplayItems, ItemTypeId, PortalId, "ItemListing");

                SetRssUrl(lnkRss.NavigateUrl, Localization.GetString("rssText", LocalResourceFile));
            }

            return dt;
        }

        private static DataTable FormatDataTable(DataTable dt)
        {
            var newDataTable = new DataTable(dt.TableName, dt.Namespace) {Locale = CultureInfo.InvariantCulture};
            newDataTable.Columns.Add("CategoryName", typeof(string), "");
            newDataTable.Columns.Add("Thumbnail");
            newDataTable.Columns.Add("ChildName");
            newDataTable.Columns.Add("ChildItemId", typeof(int));
            newDataTable.Columns.Add("ChildDescription");

            foreach (DataRow row in dt.Rows)
	        {
                DataRow newRow = newDataTable.NewRow();

                newRow["Thumbnail"] = row["Thumbnail"];
                newRow["ChildName"] = row["Name"];
                newRow["ChildItemId"] = Convert.ToInt32(row["ItemId"], CultureInfo.InvariantCulture);
                newRow["ChildDescription"] = row["Description"];
                newDataTable.Rows.Add(newRow);
        	}

            return newDataTable;
        }

        private int ItemTypeId
        {
            get
            {
                object o = Settings["ilItemTypeId"];
                return (o == null ? -1 : Convert.ToInt32(o, CultureInfo.InvariantCulture));
            }
        }

        private int CategoryId
        {
            get
            {
                object o = Settings["ilCategoryId"];
                return (o == null ? -1 : Convert.ToInt32(o, CultureInfo.InvariantCulture));
            }
        }

        private int MaxDisplayItems
        {
            get
            {
                object o = Settings["ilMaxDisplayItems"];
                return (o == null ? -1 : Convert.ToInt32(o, CultureInfo.InvariantCulture));
            }
        }

        private bool EnableRss
        {
            get
            {
                object o = Settings["ilEnableRss"];
                return (o == null ? false : Convert.ToBoolean(o, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this module is set to show the item's parent category.
        /// </summary>
        /// <value><c>true</c> if the parent category should be shown; otherwise, <c>false</c>.</value>
        private bool ShowParent
        {
            get
            {
                object o = Settings["ilShowParent"];
                return (o == null ? false : Convert.ToBoolean(o, CultureInfo.InvariantCulture));
            }
        }

        //private int MaxDisplaySubItems
        //{
        //    get
        //    {
        //        object o = Settings["ilMaxDisplaySubItems"];
        //        return (o == null ? -1 : Convert.ToInt32(o));
        //    }
        //}

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection
                           {
                                   {
                                           GetNextActionID(),
                                           Localization.GetString("Settings", LocalResourceFile),
                                           ModuleActionType.AddContent, "", "", EditUrl("Settings"), false,
                                           SecurityAccessLevel.Edit, true, false
                                           }
                           };
            }
        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "0#", Justification = "Interface Implementation"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Interface Implementation"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "modInfo", Justification = "Interface implementation")]
        public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo)
        {
            // included as a stub only so that the core knows this module Implements Entities.Modules.ISearchable
            return null;
        }

        protected ArticleViewOption DataDisplayFormat
        {
            get
            {
                object o = Settings["ilDataDisplayFormat"];
                return Enum.IsDefined(typeof(ArticleViewOption), o)
                               ? (ArticleViewOption)Enum.Parse(typeof(ArticleViewOption), o.ToString(), true)
                               : ArticleViewOption.Abstract;
            }
        }

        protected string DataType
        {
            get
            {
                object o = Settings["ilDataType"];
                return (o == null ? "Item Listing" : o.ToString());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void lstItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null)
            {
                var pnlThumbnail = (Panel)e.Item.FindControl("pnlThumbnail");
                var pnlCategory = (Panel)e.Item.FindControl("pnlCategory");
                //Panel pnlTitle = (Panel)e.Item.FindControl("pnlTitle");
                var pnlDescription = (Panel)e.Item.FindControl("pnlDescription");
                var pnlReadMore = (Panel)e.Item.FindControl("pnlReadMore");
                var lnkTitle = (HyperLink)e.Item.FindControl("lnkTitle");

                if (pnlThumbnail != null)
                {
                    pnlThumbnail.Visible = (DataDisplayFormat == ArticleViewOption.Thumbnail || DataDisplayFormat == ArticleViewOption.TitleAndThumbnail);
                }
                if (pnlDescription != null)
                {
                    pnlDescription.Visible = (DataDisplayFormat == ArticleViewOption.Thumbnail || DataDisplayFormat == ArticleViewOption.Abstract);
                }
                if (pnlReadMore != null)
                {
                    pnlReadMore.Visible = (DataDisplayFormat == ArticleViewOption.Thumbnail || DataDisplayFormat == ArticleViewOption.Abstract);
                }
                if (pnlCategory != null)
                {
                    pnlCategory.Visible = ShowParent;
                }

                //This code makes sure that an article that is disabled does not get a link.
                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    DataRow dr = ((DataRowView)e.Item.DataItem).Row;
                    var childItemId = (int)dr["ChildItemId"];

                    if (Utility.IsDisabled(childItemId, PortalId))
                    {
                        lnkTitle.NavigateUrl = "";
                        if (pnlReadMore != null)
                        {
                            pnlReadMore.Visible = false;
                        }
                    }

                    if (pnlThumbnail != null && (dr["Thumbnail"] == null || !Utility.HasValue(dr["Thumbnail"].ToString())))
                    {
                        pnlThumbnail.CssClass += " item_listing_nothumbnail";
                    }
                }
            }
        }
    }
}

