//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

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
    using System.Security.Policy;
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
    using static System.Net.WebRequestMethods;

    /// <summary>
    /// 
    /// </summary>
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

        //public const string GlobalResourceFile = "~/DesktopModules/EngagePublish/App_GlobalResources/globalresources.resx";

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
                return
                    versionInfoObject;
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
                    //TODO: this needs changed. We need to know what we're loading in the querystring first, check the ItemID
                    object o = Request.QueryString["pageid"];
                    object c = Request.QueryString["catpageid"];

                    if (o != null && versionInfoObject.ItemTypeId == ItemType.Article.GetId())
                    {
                        pageId = Convert.ToInt32(o, CultureInfo.InvariantCulture);
                    }
                    else if (c != null
                             &&
                             (versionInfoObject.ItemTypeId == ItemType.Category.GetId()
                              || versionInfoObject.ItemTypeId == ItemType.TopLevelCategory.GetId()))
                    {
                        pageId = Convert.ToInt32(c, CultureInfo.InvariantCulture);
                    }
                }
                return pageId;
            }
        }


        //TODO: cache all the HostSetting values
        public bool IsSetup
        {
            get
            {
                string s = HostController.Instance.GetString(Utility.PublishSetup + PortalId);
                string d = HostController.Instance.GetString(Utility.PublishDefaultDisplayPage + PortalId);
                return !String.IsNullOrEmpty(s) && !String.IsNullOrEmpty(d);
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

        public bool IsCommentsEnabled
        {
            get { return IsCommentsEnabledForPortal(PortalId); }
        }

        public bool IsCommentAuthorNotificationEnabled
        {
            get { return IsCommentAuthorNotificationEnabledForPortal(PortalId); }
        }

        public bool AllowAnonymousComments
        {
            get { return AllowAnonymousCommentsForPortal(PortalId); }
        }

        public bool AreCommentsModerated
        {
            get { return AreCommentsModeratedForPortal(PortalId); }
        }

        public bool AutoApproveComments
        {
            get { return AutoApproveCommentsForPortal(PortalId); }
        }

        public bool AreRatingsEnabled
        {
            get { return AreRatingsEnabledForPortal(PortalId); }
        }

        public bool AllowAnonymousRatings
        {
            get { return AllowAnonymousRatingsForPortal(PortalId); }
        }

        public bool IsViewTrackingEnabled
        {
            get { return IsViewTrackingEnabledForPortal(PortalId); }
        }

        public bool EnablePublishFriendlyUrls
        {
            get { return EnablePublishFriendlyUrlsForPortal(PortalId); }
        }

        public bool AllowArticlePaging
        {
            get { return AllowArticlePagingForPortal(PortalId); }
        }



        public bool EnableDisplayNameAsHyperlink
        {
            get { return EnableDisplayNameAsHyperlinkForPortal(PortalId); }
        }

        public bool AllowTags
        {
            get { return AllowTagsForPortal(PortalId); }
        }

        public int PopularTagCount
        {
            get { return PopularTagCountForPortal(PortalId); }
        }

        public int DefaultDisplayTabId
        {
            get { return DefaultDisplayTabIdForPortal(PortalId); }
        }

        public int DefaultTagDisplayTabId
        {
            get { return DefaultTagDisplayTabIdForPortal(PortalId); }
        }


        public bool AllowRichTextDescriptions
        {
            get { return AllowRichTextDescriptionsForPortal(PortalId); }
        }

        public bool DefaultRichTextDescriptions
        {
            get { return DefaultRichTextDescriptionsForPortal(PortalId); }
        }


        public bool UseApprovals
        {
            get { return UseApprovalsForPortal(PortalId); }
        }

        public bool UseEmbeddedArticles
        {
            get { return UseEmbeddedArticlesForPortal(PortalId); }
        }

        public bool ShowItemIds
        {
            get { return ShowItemIdsForPortal(PortalId); }
        }

        public string ThumbnailSubdirectory
        {
            get { return ThumbnailSubdirectoryForPortal(PortalId); }
        }

        public string ThumbnailSelectionOption
        {
            get { return ThumbnailSelectionOptionForPortal(PortalId); }
        }

        public int MaximumRating
        {
            get { return MaximumRatingForPortal(PortalId); }
        }

        public bool IsAdmin
        {
            get
            {
                return Request.IsAuthenticated
                       && (PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAdminRole + PortalId)) || UserInfo.IsSuperUser);
            }
        }

        public bool IsConfigured
        {
            get { return Settings.Contains("DisplayType"); }
        }

        public bool IsAuthor
        {
            get { return Request.IsAuthenticated && PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAuthorRole + PortalId)); }
        }

        public bool IsPingEnabled
        {
            get { return Utility.IsPingEnabledForPortal(PortalId); }
        }

        public bool IsWlwEnabled
        {
            get { return GetWlwSupportForPortal(PortalId); }
        }


        public bool IsPublishCommentType
        {
            get { return IsPublishCommentTypeForPortal(PortalId); }
        }

        public string ForumProviderType
        {
            get { return ForumProviderTypeForPortal(PortalId); }
        }

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
                //someone called the public method and set the ItemID (externally).
                if (externallySetItemId > 0)
                {
                    return externallySetItemId;
                }
                //ItemId has not been set externally now we need to look at settings.

                //if the querystring has the ItemId on it and the settings are to override				
                string i = Request.QueryString["itemId"];
                //we need to look if we're in admin mode, if so forget the reference about IsOverridable, it's always overridable.
                string ctl = Request.QueryString["ctl"];

                int modid = -1;

                object o = Request.Params["modid"];
                if (o != null)
                {
                    modid = Convert.ToInt32(o.ToString(), CultureInfo.InvariantCulture);
                }

                //if (!String.IsNullOrEmpty(ctl) && ctl.Equals(Utility.AdminContainer))
                if (!String.IsNullOrEmpty(ctl) && ctl.ToUpperInvariant().Equals(Utility.AdminContainer.ToUpperInvariant()))
                {
                    if (!String.IsNullOrEmpty(i))
                    {
                        return Convert.ToInt32(i, CultureInfo.InvariantCulture);
                    }
                    if (ItemVersionId > 0)
                    {
                        return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                    }
                }

                var manager = new ItemManager(this);

                //Check if there's a moduleid

                if (modid > 0)
                {
                    if ((Convert.ToInt32(o, CultureInfo.InvariantCulture) == ModuleId || Overrideable))
                    {
                        //if we found the moduleid in the querystring we are trying to force the article here.                      
                        if (!String.IsNullOrEmpty(i))
                        {
                            return Convert.ToInt32(i, CultureInfo.InvariantCulture);
                        }
                        //The local variable ItemVersionId is set so resolve the ItemVersionid to an Itemid
                        if (ItemVersionId > 0)
                        {
                            return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                        }
                        return manager.ResolveId();
                    }
                    //if we don't match the moduleid then we should get the module settings from DNN to use
                    return manager.ResolveId();
                }

                if (!String.IsNullOrEmpty(i) && manager.IsOverrideable)
                {
                    return Convert.ToInt32(i, CultureInfo.InvariantCulture);
                }

                //The local variable ItemVersionId is set so resolve the ItemVersionid to an Itemid
                if (manager.IsOverrideable && ItemVersionId > 0 && modid == ModuleId)
                {
                    return Item.GetItemIdFromVersion(ItemVersionId, PortalId);
                }

                //none of the above have scenarios have been met, need to ask the Manager class to 
                //determine the itemid. The manager contains logic to check module settings and 
                //backward capatibility settings. hk
                return manager.ResolveId();
            }
        }

        //This is the cachetime used by Publish modules
        public int CacheTime
        {
            get
            {
                object o = Settings["CacheTime"];
                if (o != null)
                {
                    return Convert.ToInt32(o.ToString(), CultureInfo.InvariantCulture);
                }
                if (GetDefaultCacheSetting(PortalId) > 0)
                {
                    return GetDefaultCacheSetting(PortalId);
                }
                return 0;
            }
        }

        public int DefaultAdminPagingSize
        {
            get { return GetAdminDefaultPagingSize(PortalId); }
        }

        public static string ApplicationUrl
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Request.ApplicationPath == "/" ? string.Empty : HttpContext.Current.Request.ApplicationPath;
                }
                return string.Empty;
            }
        }

        public int ItemVersionId
        {
            get
            {
                string s = Request.QueryString["VersionId"];
                if (s == null) return -1;
                object o = Request.Params["modid"];
                if (o != null)
                {
                    //check to see if we're on the right module id, otherwise return -1
                    if (Convert.ToInt32(o.ToString(), CultureInfo.InvariantCulture) == ModuleId)
                        return Convert.ToInt32(s, CultureInfo.InvariantCulture);
                    return -1;
                }

                return (s == null ? -1 : Convert.ToInt32(s, CultureInfo.InvariantCulture));
            }
        }

        public int CommentId
        {
            get
            {
                string s = Request.QueryString["CommentId"];
                return (s == null ? -1 : Convert.ToInt32(s, CultureInfo.InvariantCulture));
            }
        }
        public static string DesktopModuleFolderName
        {
            get { return Utility.DesktopModuleFolderName; }
        }

        public static bool ApprovalEmailsEnabled(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEmail + portalId);
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s);
            }
            return false;
        }

        public static bool IsCommentsEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishComment + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool IsCommentAuthorNotificationEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentEmailAuthor + portalId);
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s);
            }
            return false;
        }

        public static bool UseSessionForReturnToList(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishSessionReturnToList + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool AllowAuthorEditCategory(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishAuthorCategoryEdit + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool AllowAnonymousCommentsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentAnonymous + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool AreCommentsModeratedForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentApproval + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool AutoApproveCommentsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCommentAutoApprove + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool AreRatingsEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishRating + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool AllowAnonymousRatingsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishRatingAnonymous + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool IsViewTrackingEnabledForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableViewTracking + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool EnablePublishFriendlyUrlsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnablePublishFriendlyUrls + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }


        public static bool AllowArticlePagingForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableArticlePaging + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool EnableDisplayNameAsHyperlinkForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableDisplayNameAsHyperlink + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool AllowTagsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishEnableTags + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static int PopularTagCountForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishPopularTagCount + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return -1;
        }

        public static int DefaultDisplayTabIdForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultDisplayPage + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return -1;
        }

        public static int DefaultTagDisplayTabIdForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultTagPage + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return -1;
        }



        public static int DefaultCategoryForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultCategory + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return -1;
        }


        public static bool AllowRichTextDescriptionsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishAllowRichTextDescriptions + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool DefaultRichTextDescriptionsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultRichTextDescriptions + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool UseApprovalsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishUseApprovals + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
        }

        public static bool UseEmbeddedArticlesForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishUseEmbeddedArticles + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return false;
        }

        public static bool ShowItemIdsForPortal(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishShowItemId + portalId.ToString(CultureInfo.InvariantCulture));
            if (Utility.HasValue(s))
            {
                return Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            }
            return true;
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
            if (Utility.HasValue(s))
            {
                return int.Parse(s, CultureInfo.InvariantCulture);
            }
            return UserFeedback.Rating.DefaultMaximumRating;
        }

        public static bool IsUserAdmin(int portalId)
        {
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.IsAuthenticated && PortalSecurity.IsInRole(HostController.Instance.GetString(Utility.PublishAdminRole + portalId));
            }
            return false;
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
                return Convert.ToBoolean(supportwlw);
            }
            return false;
        }

        public static string ForumProviderTypeForPortal(int portalId)
        {
            return HostController.Instance.GetString(Utility.PublishForumProviderType + portalId.ToString(CultureInfo.InvariantCulture));
        }

        public static bool UseCachePortal(int portalId)
        {
            if (CacheTimePortal(portalId) > 0)
            {
                return true;
            }
            return false;
        }

        public static int CacheTimePortal(int portalId)
        {
            if (GetDefaultCacheSetting(portalId) > 0)
            {
                return GetDefaultCacheSetting(portalId);
            }
            return 0;
        }

        public static int GetDefaultCacheSetting(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishCacheTime + portalId);
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return 0;
        }

        public static int GetAdminDefaultPagingSize(int portalId)
        {
            string s = HostController.Instance.GetString(Utility.PublishDefaultAdminPagingSize + portalId);
            if (Utility.HasValue(s))
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            }
            return 25;
        }

        public string BuildLinkUrl(string qsParameters)
        {
            return Globals.NavigateURL(TabId, string.Empty, qsParameters);
        }

        public string GetItemLinkUrl(object itemId)
        {

            if (itemId != null)
            {
                //TODO: should we pass TabId from the page, or from the display tab id for the module?

                //we need to check if the PageId is "current" or for a category list, if we're moving from a custom list to an article pageid is being passed, bad things.
                //check to see if the link we're building is for the current item, or another item, if another item we want to use Pageid=1

                int correctPageId = 1;
                if (Convert.ToInt32(itemId) == ItemId)
                    correctPageId = pageId;
                return Utility.GetItemLinkUrl(Convert.ToInt32(itemId, CultureInfo.InvariantCulture), PortalId, TabId, ModuleId, correctPageId, GetCultureName());
            }

            return string.Empty;
        }

        public string GetItemVersionLinkUrl(object itemVersionId)
        {
            Item version = null;
            int i = Item.GetItemIdFromVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
            string currentItemType = Item.GetItemType(i, PortalId);

            if (currentItemType.Equals("article", StringComparison.OrdinalIgnoreCase))
            {
                version = Article.GetArticleVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
            }
            else if (currentItemType.Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                version = Category.GetCategoryVersion(Convert.ToInt32(itemVersionId, CultureInfo.CurrentCulture), PortalId);
            }

            if (itemVersionId != null)
            {
                return Utility.GetItemLinkUrl(version);
            }

            return string.Empty;
        }

        public void SetItemId(int value)
        {
            externallySetItemId = value;
        }

        public string GetItemLinkTarget(object itemId)
        {
            int curItemId;
            Int32.TryParse(itemId.ToString(), out curItemId);
            if (curItemId > 0)
            {
                Item i = BindItemData(curItemId);
                if (i.NewWindow)
                {
                    return "_blank";
                }
                return "_self";
            }
            return "_self";
            //return string.Empty;
        }

        public string GetItemLinkUrl(object itemId, int portalId)
        {
            return Utility.IsDisabled(Convert.ToInt32(itemId, CultureInfo.InvariantCulture), portalId) ? string.Empty : GetItemLinkUrl((itemId));
        }

        public string GetItemLinkUrlExternal(object itemId)
        {
            //if (this.IsShortLinkEnabled)
            //{
            //    return "http://" + this.PortalAlias.HTTPAlias + "/itemlink.aspx?itemId=" + itemId;
            //}
            //return "http://" + this.PortalAlias.HTTPAlias + DesktopModuleFolderName + "itemlink.aspx?itemId=" + itemId;
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
            //TODO: should we also allow for setting the module title here?
            var tp = (CDefault)Page;

            if (AllowTitleUpdate && (tp.Title != versionInfoObject.MetaTitle && tp.Title != versionInfoObject.Name))
            {

                tp.Title = Utility.HasValue(VersionInfoObject.MetaTitle) ? versionInfoObject.MetaTitle : versionInfoObject.Name;
                //open graph title
                var ogTitle = new HtmlGenericControl("meta");
                ogTitle.Attributes["property"] = "og:title";
                ogTitle.Attributes["content"] = tp.Title;
                if (Page.Header.Controls.IndexOf(ogTitle) < 1)
                    Page.Header.Controls.Add(ogTitle);


                var twitterTitle = new HtmlGenericControl("meta");
                twitterTitle.Attributes["property"] = "twitter:title";
                twitterTitle.Attributes["content"] = tp.Title;
                if (Page.Header.Controls.IndexOf(twitterTitle) < 1)
                    Page.Header.Controls.Add(twitterTitle);

                var twitterCard = new HtmlGenericControl("meta");
                twitterCard.Attributes["property"] = "twitter:card";
                twitterCard.Attributes["content"] = "summary";
                if (Page.Header.Controls.IndexOf(twitterCard) < 1)
                    Page.Header.Controls.Add(twitterCard);

                var ogType = new HtmlGenericControl("meta");
                ogType.Attributes["property"] = "og:type";
                ogType.Attributes["content"] = "article";

                if (Page.Header.Controls.IndexOf(ogType) < 1)
                    Page.Header.Controls.Add(ogType);

                if (LogBreadcrumb)
                {
                    AddBreadcrumb(versionInfoObject.Name);
                }

                //do meta tag settings as well
                if (!String.IsNullOrEmpty(VersionInfoObject.MetaDescription))
                {
                    tp.Description = VersionInfoObject.MetaDescription;
                    //open graph description
                    var ogDescription = new HtmlGenericControl("meta");
                    ogDescription.Attributes["property"] = "og:description";
                    ogDescription.Attributes["content"] = VersionInfoObject.MetaDescription;
                    if (Page.Header.Controls.IndexOf(ogDescription) < 1)
                        Page.Header.Controls.Add(ogDescription);

                    //twitter description
                    var twitterDescription = new HtmlGenericControl("meta");
                    twitterDescription.Attributes["property"] = "twitter:description";
                    twitterDescription.Attributes["content"] = VersionInfoObject.MetaDescription;
                    if (Page.Header.Controls.IndexOf(twitterDescription) < 1)
                        Page.Header.Controls.Add(twitterDescription);
                }

                if (!String.IsNullOrEmpty(VersionInfoObject.MetaKeywords))
                {
                    tp.KeyWords = VersionInfoObject.MetaKeywords;
                }

                //tp.SmartNavigation = true;
                Page.SetFocus(tp.ClientID);

                //open graph image using the engage publish thumbnail
                var ogImage = new HtmlGenericControl("meta");
                ogImage.Attributes["property"] = "og:image";
                ogImage.Attributes["content"] = GetThumbnailUrl(VersionInfoObject.Thumbnail);
                //build the full URL

                if (Page.Header.Controls.IndexOf(ogImage) < 1)
                    Page.Header.Controls.Add(ogImage);

                var twitterImage = new HtmlGenericControl("meta");
                twitterImage.Attributes["property"] = "twitter:image";
                twitterImage.Attributes["content"] = GetThumbnailUrl(VersionInfoObject.Thumbnail);
                //build the full URL

                if (Page.Header.Controls.IndexOf(twitterImage) < 1)
                    Page.Header.Controls.Add(twitterImage);

                //set the open graph site name
                var ogSiteName = new HtmlGenericControl("meta");
                ogSiteName.Attributes["property"] = "og:site_name";
                ogSiteName.Attributes["content"] = PortalSettings.PortalName;
                if (Page.Header.Controls.IndexOf(ogSiteName) < 1)
                    Page.Header.Controls.Add(ogSiteName);


                //set the twitter site name
                //TODO: need to save the twitterid of an author somewhere... 
                var twitterSiteName = new HtmlGenericControl("meta");
                twitterSiteName.Attributes["property"] = "twitter:site";
                twitterSiteName.Attributes["content"] = PortalSettings.PortalName;
                if (Page.Header.Controls.IndexOf(twitterSiteName) < 1)
                    Page.Header.Controls.Add(twitterSiteName);

                var twitterCreator = new HtmlGenericControl("meta");
                twitterCreator.Attributes["property"] = "twitter:creator";
                twitterCreator.Attributes["content"] = PortalSettings.PortalName;
                if (Page.Header.Controls.IndexOf(twitterCreator) < 1)
                    Page.Header.Controls.Add(twitterCreator);

                //TODO: need to have a setting for facebook app id

                //set the og url
                var ogUrl = new HtmlGenericControl("meta");
                ogUrl.Attributes["property"] = "og:url";
                ogUrl.Attributes["content"] = GetItemLinkUrl(VersionInfoObject.ItemId);
                if (Page.Header.Controls.IndexOf(ogUrl) < 1)
                    Page.Header.Controls.Add(ogUrl);

            }
            //set canonical tag call
            SetCanonicalTag(GetItemLinkUrl(VersionInfoObject.ItemId));

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

            // Check if a canonical tag already exists
            var canonicalTag = Page.Header.FindControl("canonicalTag") as System.Web.UI.HtmlControls.HtmlLink;

            if (canonicalTag == null)
            {
                canonicalTag = new System.Web.UI.HtmlControls.HtmlLink();
                canonicalTag.ID = "canonicalTag";
                canonicalTag.Attributes.Add("rel", "canonical");
                Page.Header.Controls.Add(canonicalTag);
            }

            canonicalTag.Attributes.Add("href", canonicalUrl);

            // if the canonical url doesn't match the current URL let's send them to canonical
            // Normalize and compare URLs without the scheme and query strings

            string currentUrlWithoutSchemeAndQuery = PortalSettings.PortalAlias.HTTPAlias.TrimEnd('/') + Request.RawUrl.Split('?')[0].ToLower();
            string canonicalUrlWithoutScheme = canonicalUrl.Replace("https://", "").Replace("http://", "").Split('?')[0].ToLower();

            if (currentUrlWithoutSchemeAndQuery == canonicalUrlWithoutScheme)
            {
                // The URLs match (ignoring scheme and query strings), no action needed
            }
            else
            {
                // The URLs differ, redirect to the canonical URL
                Response.Status = "301 Moved Permanently";
                Response.Redirect(canonicalUrl);
            }

            //TODO: add twitter:card info 
        }

        public void SetWlwSupport()
        {
            if (IsWlwEnabled)
            {

                var tp = (CDefault)Page;
                if (tp != null)
                {

                    var lc = new LiteralControl();
                    var lcrsd = new LiteralControl();
                    var sb = new StringBuilder(400);
                    sb.Append("<link rel=\"wlwmanifest\" type=\"application/wlwmanifest+xml\" href=\"");
                    //manifesturl
                    string manifestUrl = "https://" + PortalSettings.PortalAlias.HTTPAlias + DesktopModuleFolderName + "services/wlwmanifest.xml";

                    sb.Append(manifestUrl);
                    sb.Append("\" />");
                    lc.Text = sb.ToString();

                    tp.Header.Controls.Add(lc);

                    var rsd = new StringBuilder(400);
                    rsd.Append("<link rel=\"EditURI\" type=\"application/rsd+xml\" title=\"RSD\" href=\"");

                    string rsdUrl = "https://" + PortalSettings.PortalAlias.HTTPAlias +
                        DesktopModuleFolderName + "services/Publishrsd.aspx?portalid=" + PortalId + "&amp;HomePageUrl=" + HttpUtility.UrlEncode(Request.Url.Scheme + "://" + Request.Url.Host + Request.RawUrl);

                    rsd.Append(rsdUrl);
                    rsd.Append("\" />");
                    lcrsd.Text = rsd.ToString();

                    tp.Header.Controls.Add(lcrsd);
                }
            }
        }

        public void SetRssUrl(string rssUrl, string rssTitle)
        {
            if (rssUrl != null && rssTitle != null)
            {

                var lc = new LiteralControl();
                var sb = new StringBuilder(400);
                sb.Append("<link rel=\"alternate\" type=\"application/rss+xml\" href=\"");
                sb.Append(HttpUtility.HtmlAttributeEncode(rssUrl));
                sb.Append("\" title=\"");
                sb.Append(rssTitle);
                sb.Append("\" />");
                lc.Text = sb.ToString();

                var tp = (CDefault)Page;
                if (tp != null)
                {
                    tp.Header.Controls.Add(lc);
                }
            }
        }

        public void SetExternalRssUrl(string rssUrl, string rssTitle)
        {
            if (rssUrl != null && rssTitle != null)
            {
                var lc = new LiteralControl();
                var sb = new StringBuilder(400);
                sb.Append("<link rel=\"alternate\" type=\"application/rss+xml\" href=\"");
                sb.Append(rssUrl);
                sb.Append("\" title=\"");
                sb.Append(rssTitle);
                sb.Append("\" />");
                lc.Text = sb.ToString();

                var tp = (CDefault)Page;
                if (tp != null)
                {
                    tp.Header.Controls.Add(lc);
                }
            }
        }


        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Compiler doesn't see validation")]
        public string GetThumbnailUrl(object objFileName)
        {
            string fileName = objFileName as string ?? string.Empty;
            if (!Utility.HasValue(fileName))
            {
                return ApplicationUrl + "/images/defaultimage.jpg";
                //return ApplicationUrl + "/images/spacer.gif";
            }
            //check if we're storing another URL, if it doesn't start with a / then we have trouble brewing
            if (fileName.StartsWith(ThumbnailSubdirectory, StringComparison.OrdinalIgnoreCase)
                || (!fileName.StartsWith("/", StringComparison.Ordinal) && !fileName.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
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
            //bool getitem = true;
            if (itemId > 0)
            {
                if (currentItemType.Equals("ARTICLE", StringComparison.OrdinalIgnoreCase))
                {
                    //todo: should we check if allow tags is true?

                    if (ItemVersionId > 0)
                    {
                        i = Article.GetArticleVersion(ItemVersionId, PortalId);
                    }
                    else
                    {
                        i = Article.GetArticle(itemId, PortalId, true, true, true);
                    }
                    if (i != null)
                    {
                        if (AllowTags && i.Tags.Count < 1)
                        {
                            foreach (ItemTag it in ItemTag.GetItemTags(i.ItemVersionId, PortalId))
                            {
                                i.Tags.Add(it);
                            }
                        }
                    }

                    //If an Article can't be created based on the ItemID or the ItemVersionId then we'll create a new one.
                    if (i == null)
                    {
                        i = Article.Create(PortalId);
                    }
                }

                else if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
                {

                    if (ItemVersionId > 0)
                    {
                        i = Category.GetCategoryVersion(ItemVersionId, PortalId);
                    }
                    else
                    {
                        i = Category.GetCategory(itemId, PortalId, true, true, true);
                    }
                    //If a Category can't be created based on the ItemID or the ItemVersionId then we'll create a new one.
                    if (i == null)
                    {
                        i = Category.Create(PortalId);
                    }
                }

                else if (currentItemType.Equals("TOPLEVELCATEGORY", StringComparison.OrdinalIgnoreCase))
                {
                    i = Category.GetCategory(itemId, PortalId);
                    if (ItemVersionId > 0)
                    {
                        i = Category.GetCategoryVersion(ItemVersionId, PortalId);
                    }

                    //If a Category can't be created based on the ItemID or the ItemVersionId then we'll create a new one.
                    if (i == null)
                    {
                        i = Category.Create(PortalId);
                    }
                }

                else
                {
                    i = Article.GetArticle(itemId, PortalId, true, true, true);
                    if (ItemVersionId > 0)
                    {
                        i = Article.GetArticleVersion(ItemVersionId, PortalId);
                    }

                    //If an Article can't be created based on the ItemID or the ItemVersionId then we'll create a new one.
                    if (i == null)
                    {
                        i = Article.Create(PortalId);
                    }
                }
            }
            return i;
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

        /// <summary>
        /// Gets the name of the current culture under which the page and user is operating.
        /// </summary>
        /// <returns>The current culture name, or <see cref="string.Empty"/> if none is found</returns>
        protected string GetCultureName()
        {
            // add to this if necessary, currently we're only looking for language
            string languageValue = Request.QueryString["language"];
            if (languageValue != null)
            {
                // if languages are turned on we should pass the language querystring parameter
                if (UserId > -1 && UserInfo.Profile.PreferredLocale != CultureInfo.CurrentCulture.Name)
                {
                    return UserInfo.Profile.PreferredLocale;
                }

                return languageValue;
            }

            return string.Empty;
        }

        protected string GetEditUrl(string itemId)
        {
            return EditUrl("itemId", itemId, "Edit");
        }

        protected string BuildCategoryListUrl(ItemType type)
        {
            int parentCategoryId = -1;

            if (!VersionInfoObject.IsNew)
            {
                parentCategoryId = VersionInfoObject.ItemTypeId == ItemType.Category.GetId() ? VersionInfoObject.ItemId : Category.GetParentCategory(VersionInfoObject.ItemId, PortalId);
            }

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
            // to quickly build a timebombed package uncomment the following three lines, change the date
            //DateTime di = new DateTime(2009,11,30);
            //if (DateTime.Now.Date > di)
            //{
            //    throw new Exception("Trial license expired");
            //}
            base.OnInit(e);
            //if (AJAX.IsInstalled())
            //{
            //    AJAX.RegisterScriptManager();
            //}
        }

        private void BindNewItem()
        {
            string editControl = Request.QueryString["adminType"];
            if (editControl != null)
            {
                //If the querystring contains a admintype on it then we need to check to see if it's article edit or
                //categoryedit. The reason this is important, we can't use the ItemId property to determine the type of
                //new object to create (article/category) because it looks as settings for the module if needed. So, if you
                //click Add new Article on a Category Display you will get the wrong type of object created. hk
                BindNewItemForEdit();
            }
            else
            {
                BindNewItemByItemType();
            }
        }

        private void BindNewItemForEdit()
        {
            Item i; // = null;
            string editControl = Request.QueryString["adminType"];
            if (editControl.Equals("CATEGORYEDIT", StringComparison.OrdinalIgnoreCase))
            {
                i = Category.Create(PortalId);
            }
            else
            {
                i = Article.Create(PortalId);
            }

            versionInfoObject = i;
        }

        private void BindNewItemByItemType()
        {
            Item i; // = null;
            string currentItemType = Item.GetItemType(ItemId, PortalId);
            if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                i = Category.Create(PortalId);
            }
            else
            {
                i = Article.Create(PortalId);
            }
            versionInfoObject = i;
        }

        private void BindCurrentItem()
        {

            var editControl = Request.QueryString["adminType"];

            //check for version id
            int itemId = ItemId;
            versionInfoObject = BindItemData(itemId);
            if (versionInfoObject.EndDate != null && Convert.ToDateTime(versionInfoObject.EndDate, CultureInfo.InvariantCulture) < DateTime.Now && editControl == null)
            {
                BindNewItem();
            }
        }

        private string GenerateLocalThumbnailUrl(string fileName)
        {
            //DotNetNuke.Entities.Portals.PortalSettings ps = Utility.GetPortalSettings(portalId);
            return Request.Url.Scheme + "://" + Request.Url.Host + PortalSettings.HomeDirectory + fileName;
        }

        public string BuildVersionsUrl()
        {
            //find the location of the ams admin module on the site.
            //DotNetNuke.Entities.Modules.ModuleController objModules = new ModuleController();
            if (ItemId > -1)
            {
                ////string currentItemType = Item.GetItemType(ItemId,PortalId);
                //int itemId = -1;
                //if (!this.VersionInfoObject.IsNew)
                //{
                //    itemId = this.VersionInfoObject.ItemId;
                //}
                return Globals.NavigateURL(TabId, string.Empty, "&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=VersionsList&itemId=" + ItemId.ToString(CultureInfo.InvariantCulture));
            }
            return string.Empty;
        }

        protected string GetItemName()
        {
            if (VersionInfoObject != null)
            {
                return VersionInfoObject.Name;
            }
            return string.Empty;

        }
    }
}