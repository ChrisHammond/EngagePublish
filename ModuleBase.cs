//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System.Web.UI.HtmlControls;
using DotNetNuke.Entities.Controllers;
using Newtonsoft.Json;

namespace Engage.Dnn.Publish
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using Util;

    public class ModuleBase : PortalModuleBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool allowTitleUpdate = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int externallySetItemId = -1;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool logBreadcrumb = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool overrideable = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int pageId;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool useCache = true;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool useUrls;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Item versionInfoObject;

        public string LocalSharedResourceFile
        {
            get
            {
                return "~" + DesktopModuleFolderName + DotNetNuke.Services.Localization.Localization.LocalResourceDirectory + "/" + DotNetNuke.Services.Localization.Localization.LocalSharedResourceFile;
            }
        }

        public bool UseUrls
        {
            [DebuggerStepThrough]
            get { return useUrls; }
            [DebuggerStepThrough]
            set
            {
                useUrls = value;
            }
        }

        public Item VersionInfoObject
        {
            [DebuggerStepThrough]
            get
            {
                return versionInfoObject;
            }
            [DebuggerStepThrough]
            set { versionInfoObject = value; }
        }

        public bool LogBreadcrumb
        {
            [DebuggerStepThrough]
            get { return logBreadcrumb; }
            [DebuggerStepThrough]
            set { logBreadcrumb = value; }
        }

        public bool Overrideable
        {
            [DebuggerStepThrough]
            get { return overrideable; }
            [DebuggerStepThrough]
            set { overrideable = value; }
        }

        public bool UseCache
        {
            get { return useCache && CacheTime > 0; }
            [DebuggerStepThrough]
            set { useCache = value; }
        }

        public bool AllowTitleUpdate
        {
            get
            {
                object o = Settings["AllowTitleUpdate"];
                if (o == null || !bool.TryParse(o.ToString(), out allowTitleUpdate))
                {
                    allowTitleUpdate = true;
                }
                return allowTitleUpdate;
            }
        }

        public int PageId
        {
            get
            {
                pageId = 1;
                if (versionInfoObject != null)
                {
                    string pageIdStr = Request.QueryString["pageid"];
                    string catPageIdStr = Request.QueryString["catpageid"];

                    if (!string.IsNullOrEmpty(pageIdStr) && versionInfoObject.ItemTypeId == ItemType.Article.GetId())
                    {
                        if (int.TryParse(pageIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedPageId))
                        {
                            pageId = parsedPageId;
                        }
                    }
                    else if (!string.IsNullOrEmpty(catPageIdStr)
                             && (versionInfoObject.ItemTypeId == ItemType.Category.GetId()
                                 || versionInfoObject.ItemTypeId == ItemType.TopLevelCategory.GetId()))
                    {
                        if (int.TryParse(catPageIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedCatPageId))
                        {
                            pageId = parsedCatPageId;
                        }
                    }
                }
                return pageId;
            }
        }

        public bool IsSetup
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishSetup + PortalId);
                string d = HostController.Instance.GetString(Utility.PublishDefaultDisplayPage + PortalId);
                return !string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(d);
            }
        }

        public static bool IsHostMailConfigured
        {
            get
            {
                string s = HostController.Instance.GetString("SMTPServer");
                return Utility.HasValue(s);
            }
        }

        public int ArticleEditWidth
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishArticleEditWidth + PortalId);
                if (Utility.HasValue(s))
                {
                    return Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                return 500;
            }
        }

        public int ArticleEditHeight
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishArticleEditHeight + PortalId);
                if (Utility.HasValue(s))
                {
                    return Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                return 400;
            }
        }

        public int ItemEditDescriptionHeight
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishDescriptionEditHeight + PortalId);
                if (Utility.HasValue(s))
                {
                    return Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                return 300;
            }
        }

        public int ItemEditDescriptionWidth
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishDescriptionEditWidth + PortalId);
                if (Utility.HasValue(s))
                {
                    return Convert.ToInt32(s, CultureInfo.InvariantCulture);
                }
                return 500;
            }
        }

        public bool IsCommentsEnabled => IsCommentsEnabledForPortal(PortalId);

        public bool IsCommentAuthorNotificationEnabled => IsCommentAuthorNotificationEnabledForPortal(PortalId);

        public bool AllowAnonymousComments => AllowAnonymousCommentsForPortal(PortalId);

        public bool AreCommentsModerated => AreCommentsModeratedForPortal(PortalId);

        public bool AutoApproveComments => AutoApproveCommentsForPortal(PortalId);

        public bool AreRatingsEnabled => AreRatingsEnabledForPortal(PortalId);

        public bool AllowAnonymousRatings => AllowAnonymousRatingsForPortal(PortalId);

        public bool IsViewTrackingEnabled => IsViewTrackingEnabledForPortal(PortalId);

        public bool EnablePublishFriendlyUrls => EnablePublishFriendlyUrlsForPortal(PortalId);

        public bool AllowArticlePaging => AllowArticlePagingForPortal(PortalId);

        public bool EnableDisplayNameAsHyperlink => EnableDisplayNameAsHyperlinkForPortal(PortalId);

        public bool AllowTags => AllowTagsForPortal(PortalId);

        public int PopularTagCount => PopularTagCountForPortal(PortalId);

        public int DefaultDisplayTabId => DefaultDisplayTabIdForPortal(PortalId);

        public int DefaultTagDisplayTabId => DefaultTagDisplayTabIdForPortal(PortalId);

        public bool AllowRichTextDescriptions => AllowRichTextDescriptionsForPortal(PortalId);

        public bool DefaultRichTextDescriptions => DefaultRichTextDescriptionsForPortal(PortalId);

        public bool UseApprovals => UseApprovalsForPortal(PortalId);

        public bool UseEmbeddedArticles => UseEmbeddedArticlesForPortal(PortalId);

        public bool ShowItemIds => ShowItemIdsForPortal(PortalId);

        public string ThumbnailSubdirectory => ThumbnailSubdirectoryForPortal(PortalId);

        public string ThumbnailSelectionOption => ThumbnailSelectionOptionForPortal(PortalId);

        public int MaximumRating => MaximumRatingForPortal(PortalId);

        public bool IsAdmin
        {
            get
            {
                return Request.IsAuthenticated
                       && (PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAdminRole + PortalId)) || UserInfo.IsSuperUser);
            }
        }

        public bool IsConfigured => Settings.Contains("DisplayType");

        public bool IsAuthor
        {
            get
            {
                return Request.IsAuthenticated && PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAuthorRole + PortalId));
            }
        }

        public bool IsPingEnabled => Utility.IsPingEnabledForPortal(PortalId);

        public bool IsWlwEnabled => GetWlwSupportForPortal(PortalId);

        public bool IsPublishCommentType => IsPublishCommentTypeForPortal(PortalId);

        public string ForumProviderType => ForumProviderTypeForPortal(PortalId);

        public ItemType TypeOfItem
        {
            get
            {
                int typeId = Item.GetItemTypeId(ItemId, PortalId);
                return ItemType.GetFromId(typeId, typeof(ItemType));
            }
        }

        public int ItemId
        {
            get
            {
                if (externallySetItemId > 0)
                {
                    return externallySetItemId;
                }

                string itemIdStr = Request.QueryString["itemId"];
                string ctl = Request.QueryString["ctl"];

                int itemId = -1;
                int modid = -1;

                if (!string.IsNullOrEmpty(ctl) && ctl.Equals(Utility.AdminContainer, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(itemIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemId))
                    {
                        return itemId;
                    }
                    if (ItemVersionId > 0)
                    {
                        return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                    }
                }

                if (Request.Params["modid"] != null && int.TryParse(Request.Params["modid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out modid))
                {
                    var manager = new ItemManager(this);
                    if (modid > 0 && (modid == ModuleId || Overrideable))
                    {
                        if (int.TryParse(itemIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemId))
                        {
                            return itemId;
                        }
                        if (ItemVersionId > 0)
                        {
                            return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                        }
                        return manager.ResolveId();
                    }
                    return manager.ResolveId();
                }

                if (int.TryParse(itemIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out itemId) && new ItemManager(this).IsOverrideable)
                {
                    return itemId;
                }

                if (new ItemManager(this).IsOverrideable && ItemVersionId > 0 && modid == ModuleId)
                {
                    return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                }

                return new ItemManager(this).ResolveId();
            }
        }

        public int CacheTime
        {
            get
            {
                if (Settings["CacheTime"] != null && int.TryParse(Settings["CacheTime"].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int cacheTime))
                {
                    return cacheTime;
                }
                return GetDefaultCacheSetting(PortalId);
            }
        }

        public int DefaultAdminPagingSize => GetAdminDefaultPagingSize(PortalId);

        public static string ApplicationUrl
        {
            get
            {
                return HttpContext.Current != null ? (HttpContext.Current.Request.ApplicationPath == "/" ? string.Empty : HttpContext.Current.Request.ApplicationPath) : string.Empty;
            }
        }

        public int ItemVersionId
        {
            get
            {
                int versionId = -1;
                string versionIdStr = Request.QueryString["VersionId"];
                if (versionIdStr == null) return -1;

                if (Request.Params["modid"] != null && int.TryParse(Request.Params["modid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int modid) && modid == ModuleId)
                {
                    return int.TryParse(versionIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out versionId) ? versionId : -1;
                }

                return int.TryParse(versionIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out versionId) ? versionId : -1;
            }
        }

        public int CommentId
        {
            get
            {
                return int.TryParse(Request.QueryString["CommentId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out int commentId) ? commentId : -1;
            }
        }

        public static string DesktopModuleFolderName => Utility.DesktopModuleFolderName;

        public static bool ApprovalEmailsEnabled(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEmail + portalId);
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool IsCommentsEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishComment + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool IsCommentAuthorNotificationEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentEmailAuthor + portalId);
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool UseSessionForReturnToList(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishSessionReturnToList + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AllowAuthorEditCategory(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishAuthorCategoryEdit + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AllowAnonymousCommentsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentAnonymous + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AreCommentsModeratedForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentApproval + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AutoApproveCommentsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentAutoApprove + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AreRatingsEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishRating + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AllowAnonymousRatingsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishRatingAnonymous + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool IsViewTrackingEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableViewTracking + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool EnablePublishFriendlyUrlsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnablePublishFriendlyUrls + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AllowArticlePagingForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableArticlePaging + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool EnableDisplayNameAsHyperlinkForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableDisplayNameAsHyperlink + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool AllowTagsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableTags + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static int PopularTagCountForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishPopularTagCount + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : -1;
        }

        public static int DefaultDisplayTabIdForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultDisplayPage + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : -1;
        }

        public static int DefaultTagDisplayTabIdForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultTagPage + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : -1;
        }

        public static int DefaultCategoryForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultCategory + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : -1;
        }

        public static bool AllowRichTextDescriptionsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishAllowRichTextDescriptions + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool DefaultRichTextDescriptionsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultRichTextDescriptions + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool UseApprovalsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishUseApprovals + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool UseEmbeddedArticlesForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishUseEmbeddedArticles + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static bool ShowItemIdsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishShowItemId + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) && Convert.ToBoolean(s, CultureInfo.InvariantCulture);
        }

        public static string ThumbnailSubdirectoryForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishThumbnailSubdirectory + portalId.ToString(CultureInfo.InvariantCulture));
            if (!Utility.HasValue(s))
            {
                s = "PublishThumbnails/";
            }
            else if (s.StartsWith("/", StringComparison.Ordinal))
            {
                s = s.Substring(1);
            }

            return s;
        }

        public static string ThumbnailSelectionOptionForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishThumbnailDisplayOption + portalId.ToString(CultureInfo.InvariantCulture));
            return s;
        }

        public static int MaximumRatingForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishRatingMaximum + portalId.ToString(CultureInfo.InvariantCulture));
            return Utility.HasValue(s) ? int.Parse(s, CultureInfo.InvariantCulture) : UserFeedback.Rating.DefaultMaximumRating;
        }

        public static bool IsUserAdmin(int portalId)
        {
            return HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated && PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAdminRole + portalId));
        }

        public static bool IsPublishCommentTypeForPortal(int portalId)
        {
            return string.IsNullOrEmpty(HostController.Instance.GetString(Utility.PublishForumProviderType + portalId.ToString(CultureInfo.InvariantCulture)));
        }

        public bool GetWlwSupportForPortal(int portalId)
        {
            if (Settings.Contains("SupportWLW"))
            {
                string supportwlw = Settings["SupportWLW"].ToString();
                return Convert.ToBoolean(supportwlw, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static string ForumProviderTypeForPortal(int portalId)
        {
            return HostController.Instance.GetString(Utility.PublishForumProviderType + portalId.ToString(CultureInfo.InvariantCulture));
        }

        public static bool UseCachePortal(int portalId)
        {
            return CacheTimePortal(portalId) > 0;
        }

        public static int CacheTimePortal(int portalId)
        {
            return GetDefaultCacheSetting(portalId);
        }

        public static int GetDefaultCacheSetting(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCacheTime + portalId);
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : 0;
        }

        public static int GetAdminDefaultPagingSize(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultAdminPagingSize + portalId);
            return Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : 25;
        }

        public string BuildLinkUrl(string qsParameters)
        {
            return Globals.NavigateURL(TabId, string.Empty, qsParameters);
        }

        public string GetItemLinkUrl(object itemId)
        {
            if (itemId != null)
            {
                int itemIdInt = Convert.ToInt32(itemId, CultureInfo.InvariantCulture);
                int correctPageId = itemIdInt == ItemId ? pageId : 1;
                return Utility.GetItemLinkUrl(itemIdInt, PortalId, TabId, ModuleId, correctPageId, GetCultureName());
            }
            return string.Empty;
        }

        public string GetItemVersionLinkUrl(object itemVersionId)
        {
            if (itemVersionId != null)
            {
                int itemId = Item.GetItemIdFromVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
                string currentItemType = Item.GetItemType(itemId, PortalId);

                Item version = null;

                if (currentItemType.Equals("article", StringComparison.OrdinalIgnoreCase))
                {
                    version = Article.GetArticleVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
                }
                else if (currentItemType.Equals("category", StringComparison.OrdinalIgnoreCase))
                {
                    version = Category.GetCategoryVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
                }

                if (version != null)
                {
                    return Utility.GetItemLinkUrl(version);
                }
            }

            return string.Empty;
        }


        public void SetItemId(int value)
        {
            externallySetItemId = value;
        }

        public string GetItemLinkTarget(object itemId)
        {
            if (itemId != null && int.TryParse(itemId.ToString(), out int curItemId) && curItemId > 0)
            {
                Item i = BindItemData(curItemId);
                return i.NewWindow ? "_blank" : "_self";
            }
            return "_self";
        }

        public string GetItemLinkUrl(object itemId, int portalId)
        {
            return Utility.IsDisabled(Convert.ToInt32(itemId, CultureInfo.InvariantCulture), portalId) ? string.Empty : GetItemLinkUrl(itemId);
        }

        public string GetItemLinkUrlExternal(object itemId)
        {
            return Utility.GetItemLinkUrl(itemId, PortalId);
        }

        public static string GetRssLinkUrl(object itemId, int maxDisplayItems, int itemTypeId, int portalId, string displayType)
        {
            var url = new StringBuilder(128);

            url.Append(ApplicationUrl);
            url.Append(DesktopModuleFolderName);
            url.Append("eprss.aspx?itemId=");
            url.Append(itemId);
            url.Append("&numberOfItems=");
            url.Append(maxDisplayItems);
            url.Append("&itemtypeid=");
            url.Append(itemTypeId);
            url.Append("&portalid=");
            url.Append(portalId);
            url.Append("&DisplayType=");
            url.Append(displayType);

            return url.ToString();
        }

        public static string GetRssLinkUrl(int portalId, string displayType, string tags)
        {
            var url = new StringBuilder(128);

            url.Append(ApplicationUrl);
            url.Append(DesktopModuleFolderName);
            url.Append("eprss.aspx?");
            url.Append("portalid=");
            url.Append(portalId);
            url.Append("&DisplayType=");
            url.Append(displayType);
            url.Append("&Tags=");
            url.Append(HttpUtility.UrlEncode(tags));

            return url.ToString();
        }

        public void SetPageTitle()
        {
            var tp = (CDefault)Page;

            if (AllowTitleUpdate && (tp.Title != versionInfoObject.MetaTitle && tp.Title != versionInfoObject.Name))
            {
                tp.Title = Utility.HasValue(VersionInfoObject.MetaTitle) ? versionInfoObject.MetaTitle : versionInfoObject.Name;

                AddMetaTag("og:title", tp.Title);
                AddMetaTag("twitter:title", tp.Title);
                AddMetaTag("twitter:card", "summary");
                AddMetaTag("og:type", "article");

                if (LogBreadcrumb)
                {
                    AddBreadcrumb(versionInfoObject.Name);
                }

                if (!string.IsNullOrEmpty(VersionInfoObject.MetaDescription))
                {
                    tp.Description = VersionInfoObject.MetaDescription;
                    AddMetaTag("og:description", VersionInfoObject.MetaDescription);
                    AddMetaTag("twitter:description", VersionInfoObject.MetaDescription);
                }

                if (!string.IsNullOrEmpty(VersionInfoObject.MetaKeywords))
                {
                    tp.KeyWords = VersionInfoObject.MetaKeywords;
                }

                Page.SetFocus(tp.ClientID);

                string thumbnailUrl = GetThumbnailUrl(VersionInfoObject.Thumbnail);
                AddMetaTag("og:image", thumbnailUrl);
                AddMetaTag("twitter:image", thumbnailUrl);
                AddMetaTag("og:site_name", PortalSettings.PortalName);
                AddMetaTag("twitter:site", PortalSettings.PortalName);
                AddMetaTag("twitter:creator", PortalSettings.PortalName);
                AddMetaTag("og:url", GetItemLinkUrl(VersionInfoObject.ItemId));
            }

            if (pageId > 1)
            {
                tp.Title += $" - Page ({pageId})";
            }
        }

        private void AddMetaTag(string property, string content)
        {
            var metaTag = new HtmlGenericControl("meta");
            metaTag.Attributes["property"] = property;
            metaTag.Attributes["content"] = content;

            if (Page.Header.Controls.IndexOf(metaTag) < 1)
            {
                Page.Header.Controls.Add(metaTag);
            }
        }

        public void GenerateJsonLd(
            string headline,
            string imageUrl,
            string authorName,
            string authorUrl,
            string publisherName,
            string publisherLogoUrl,
            DateTime datePublished,
            DateTime dateModified,
            string description,
            string articleBody)
        {
            // Remove all HTML tags
            string noHtml = Regex.Replace(articleBody, "<.*?>", string.Empty);

            // Decode HTML entities
            string decoded = HttpUtility.HtmlDecode(noHtml);

            // Remove any remaining newline characters
            string cleaned = decoded.Replace("\n", "").Replace("\r", "").Trim();
            articleBody = cleaned;
            // Set default image if imageUrl is empty
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = "/portals/0/defaultimage.jpg"; // Specify the path to your default image, this is currently hardcoded
            }

            var jsonLd = new Dictionary<string, object>
    {
        { "@context", "https://schema.org" },
        { "@type", "BlogPosting" },
        { "name", headline },
        { "headline", headline },
        { "image", imageUrl },
        { "author", new Dictionary<string, string>
            {
                { "@type", "Person" },
                { "name", authorName },
                { "url", authorUrl }
            }
        },
        { "publisher", new Dictionary<string, object>
            {
                { "@type", "Organization" },
                { "name", publisherName },
                { "logo", new Dictionary<string, string>
                    {
                        { "@type", "ImageObject" },
                        { "url", publisherLogoUrl }
                    }
                }
            }
        },
        { "datePublished", datePublished.ToString("yyyy-MM-ddTHH:mm:ssZ") },
        { "dateModified", dateModified.ToString("yyyy-MM-ddTHH:mm:ssZ") },
        { "description", HttpUtility.HtmlDecode(HtmlUtils.StripTags(description, true)).Replace("\n", "").Replace("\r", "") },
        { "articleBody", HttpUtility.HtmlDecode(HtmlUtils.StripTags(articleBody, true).Replace("\n", "").Replace("\r", "")) }
    };

            // Create an HtmlGenericControl to hold the script tag
            HtmlGenericControl scriptTag = new HtmlGenericControl("script");
            scriptTag.Attributes["type"] = "application/ld+json";
            scriptTag.InnerHtml = JsonConvert.SerializeObject(jsonLd, Newtonsoft.Json.Formatting.Indented);

            // Check if the script tag is already present in the header
            bool scriptExists = false;
            foreach (Control control in Page.Header.Controls)
            {
                if (control is HtmlGenericControl existingScript && existingScript.Attributes["type"] == "application/ld+json")
                {
                    if (existingScript.InnerHtml == scriptTag.InnerHtml)
                    {
                        scriptExists = true;
                        break;
                    }
                }
            }

            // Add the script tag only if it doesn't already exist
            if (!scriptExists)
            {
                Page.Header.Controls.Add(scriptTag);
            }
        }


        public void SetCanonicalTag(string canonicalUrl)
        {
            if (string.IsNullOrEmpty(canonicalUrl))
                return;

            var canonicalTag = Page.Header.FindControl("canonicalTag") as HtmlLink;
            if (canonicalTag == null)
            {
                canonicalTag = new HtmlLink
                {
                    ID = "canonicalTag",
                    Attributes =
                    {
                        ["rel"] = "canonical",
                        ["href"] = canonicalUrl
                    }
                };
                Page.Header.Controls.Add(canonicalTag);
            }
            else
            {
                canonicalTag.Attributes["href"] = canonicalUrl;
            }

            string currentUrlWithoutSchemeAndQuery = PortalSettings.PortalAlias.HTTPAlias.TrimEnd('/') + Request.RawUrl.Split('?')[0].ToLower();
            string canonicalUrlWithoutScheme = canonicalUrl.Replace("https://", "").Replace("http://", "").Split('?')[0].ToLower();

            if (currentUrlWithoutSchemeAndQuery != canonicalUrlWithoutScheme)
            {
                Response.Status = "301 Moved Permanently";
                Response.Redirect(canonicalUrl);
            }
        }

        public void SetWlwSupport()
        {
            if (IsWlwEnabled)
            {
                var tp = (CDefault)Page;
                if (tp != null)
                {
                    string manifestUrl = $"https://{PortalSettings.PortalAlias.HTTPAlias}{DesktopModuleFolderName}services/wlwmanifest.xml";
                    AddLinkTag(tp, "wlwmanifest", "application/wlwmanifest+xml", manifestUrl);

                    string rsdUrl = $"https://{PortalSettings.PortalAlias.HTTPAlias}{DesktopModuleFolderName}services/Publishrsd.aspx?portalid={PortalId}&amp;HomePageUrl={HttpUtility.UrlEncode(Request.Url.Scheme + "://" + Request.Url.Host + Request.RawUrl)}";
                    AddLinkTag(tp, "EditURI", "application/rsd+xml", rsdUrl);
                }
            }
        }

        private void AddLinkTag(CDefault tp, string rel, string type, string href)
        {
            var linkTag = new LiteralControl
            {
                Text = $"<link rel=\"{rel}\" type=\"{type}\" href=\"{href}\" />"
            };
            tp.Header.Controls.Add(linkTag);
        }

        public void SetRssUrl(string rssUrl, string rssTitle)
        {
            if (!string.IsNullOrEmpty(rssUrl) && !string.IsNullOrEmpty(rssTitle))
            {
                var lc = new LiteralControl
                {
                    Text = $"<link rel=\"alternate\" type=\"application/rss+xml\" href=\"{HttpUtility.HtmlAttributeEncode(rssUrl)}\" title=\"{rssTitle}\" />"
                };

                var tp = (CDefault)Page;
                tp?.Header.Controls.Add(lc);
            }
        }

        public void SetExternalRssUrl(string rssUrl, string rssTitle)
        {
            if (!string.IsNullOrEmpty(rssUrl) && !string.IsNullOrEmpty(rssTitle))
            {
                var lc = new LiteralControl
                {
                    Text = $"<link rel=\"alternate\" type=\"application/rss+xml\" href=\"{rssUrl}\" title=\"{rssTitle}\" />"
                };

                var tp = (CDefault)Page;
                tp?.Header.Controls.Add(lc);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Compiler doesn't see validation")]
        public string GetThumbnailUrl(object objFileName)
        {
            string fileName = objFileName as string ?? string.Empty;
            if (!Utility.HasValue(fileName))
            {
                return $"{ApplicationUrl}/images/defaultimage.jpg";
            }

            if (fileName.StartsWith(ThumbnailSubdirectory, StringComparison.OrdinalIgnoreCase) ||
                (!fileName.StartsWith("/", StringComparison.Ordinal) && !fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
            {
                return GenerateLocalThumbnailUrl(fileName);
            }
            return fileName;
        }

        protected void AddBreadcrumb(string pageName)
        {
            Breadcrumb.Add(pageName, GetItemLinkUrl(ItemId));
        }

        protected Item BindItemData(int itemId)
        {
            Item i = null;
            string currentItemType = Item.GetItemType(itemId, PortalId);

            if (itemId > 0)
            {
                i = GetItemByType(currentItemType, itemId);
                if (i == null)
                {
                    i = CreateItemByType(currentItemType);
                }
            }

            return i;
        }

        private Item GetItemByType(string currentItemType, int itemId)
        {
            if (currentItemType.Equals("ARTICLE", StringComparison.OrdinalIgnoreCase))
            {
                return ItemVersionId > 0
                    ? Article.GetArticleVersion(ItemVersionId, PortalId)
                    : Article.GetArticle(itemId, PortalId, true, true, true);
            }

            if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                return ItemVersionId > 0
                    ? Category.GetCategoryVersion(ItemVersionId, PortalId)
                    : Category.GetCategory(itemId, PortalId, true, true, true);
            }

            if (currentItemType.Equals("TOPLEVELCATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                return ItemVersionId > 0
                    ? Category.GetCategoryVersion(ItemVersionId, PortalId)
                    : Category.GetCategory(itemId, PortalId);
            }

            return Article.GetArticle(itemId, PortalId, true, true, true);
        }

        private Item CreateItemByType(string currentItemType)
        {
            if (currentItemType.Equals("ARTICLE", StringComparison.OrdinalIgnoreCase))
            {
                return Article.Create(PortalId);
            }

            if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                return Category.Create(PortalId);
            }

            return Category.Create(PortalId);
        }

        protected void BindItemData(bool createNew)
        {
            if (createNew || ItemId < 1)
            {
                BindNewItem();
            }
            else
            {
                BindCurrentItem();
            }
        }

        protected void BindItemData()
        {
            BindItemData(false);
        }

        protected string GetCultureName()
        {
            string languageValue = Request.QueryString["language"];
            return !string.IsNullOrEmpty(languageValue) ? languageValue : UserId > -1 && UserInfo.Profile.PreferredLocale != CultureInfo.CurrentCulture.Name ? UserInfo.Profile.PreferredLocale : string.Empty;
        }

        protected string GetEditUrl(string itemId)
        {
            return EditUrl("itemId", itemId, "Edit");
        }

        protected string BuildCategoryListUrl(ItemType type)
        {
            int parentCategoryId = !VersionInfoObject.IsNew
                ? VersionInfoObject.ItemTypeId == ItemType.Category.GetId()
                    ? VersionInfoObject.ItemId
                    : Category.GetParentCategory(VersionInfoObject.ItemId, PortalId)
                : -1;

            return Globals.NavigateURL(
                TabId,
                string.Empty,
                "ctl=" + Utility.AdminContainer,
                "mid=" + ModuleId.ToString(CultureInfo.InvariantCulture),
                "adminType=" + type.Name + "list",
                "categoryId=" + parentCategoryId.ToString(CultureInfo.InvariantCulture));
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        private void BindNewItem()
        {
            string editControl = Request.QueryString["adminType"];
            versionInfoObject = editControl != null ? BindNewItemForEdit(editControl) : BindNewItemByItemType();
        }

        private Item BindNewItemForEdit(string editControl)
        {
            Item newItem = null;
            if (editControl.Equals("CATEGORYEDIT", StringComparison.OrdinalIgnoreCase))
            {
                newItem = Category.Create(PortalId);
            }
            else
            {
                newItem = Article.Create(PortalId);
            }

            return newItem;
        }

        private Item BindNewItemByItemType()
        {
            string currentItemType = Item.GetItemType(ItemId, PortalId);
            Item newItem = null;

            if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                newItem = Category.Create(PortalId);
            }
            else
            {
                newItem = Article.Create(PortalId);
            }

            return newItem;
        }

        private void BindCurrentItem()
        {
            int itemId = ItemId;
            versionInfoObject = BindItemData(itemId);
            if (versionInfoObject.EndDate != null && Convert.ToDateTime(versionInfoObject.EndDate, CultureInfo.InvariantCulture) < DateTime.Now && Request.QueryString["adminType"] == null)
            {
                BindNewItem();
            }
        }

        private string GenerateLocalThumbnailUrl(string fileName)
        {
            return $"{Request.Url.Scheme}://{Request.Url.Host}{PortalSettings.HomeDirectory}{fileName}";
        }

        public string BuildVersionsUrl()
        {
            return ItemId > -1 ? Globals.NavigateURL(TabId, string.Empty, "&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=VersionsList&itemId=" + ItemId.ToString(CultureInfo.InvariantCulture)) : string.Empty;
        }

        protected string GetItemName()
        {
            return VersionInfoObject?.Name ?? string.Empty;
        }
    }
}
