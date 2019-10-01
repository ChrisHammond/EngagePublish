//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.



namespace Engage.Dnn.Publish.ArticleControls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using Controls;
    using Data;
    using Forum;
    using Util;
    using System.Web.UI;
    using System.Reflection;
    using System.Linq;

    public partial class ArticleDisplay : ModuleBase, IActionable
    {
        private CommentDisplayBase commentDisplay;
        private EmailAFriend ea;
        private PrinterFriendlyButton pf;
        private RelatedArticleLinksBase ral;
        private ArticleDisplay ad;

        public ArticleDisplay()
        {
            DisplayPrinterFriendly = true;
            DisplayRelatedLinks = true;
            DisplayRelatedArticle = true;
            DisplayEmailAFriend = true;
            DisplayTitle = true;
        }

        private const string CommentsControlToLoad = "../Controls/CommentDisplay.ascx";
        private const string EmailControlToLoad = "../Controls/EmailAFriend.ascx";
        private const string PrinterControlToLoad = "../Controls/PrinterFriendlyButton.ascx";
        private const string RelatedArticlesControlToLoad = "../Controls/RelatedArticleLinks.ascx";
        private const string ArticleControlToLoad = "articleDisplay.ascx";

        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            LoadArticle();
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
            PreRender += Page_PreRender;
        }

        public bool DisplayPrinterFriendly
        {
            get;
            set;
        }

        public bool DisplayRelatedLinks
        {
            get;
            set;
        }

        public bool DisplayRelatedArticle
        {
            get;
            set;
        }

        public bool DisplayEmailAFriend
        {
            get;
            set;
        }

        public bool DisplayTitle
        {
            get;
            set;
        }

        public bool ShowAuthor
        {
            get;
            set;
        }

        public bool ShowTags
        {
            get;
            set;
        }




        /// <summary>
        /// Gets the setting value for this module, whether to show ratings or not.
        /// </summary>
        /// <value>
        /// The rating display option.
        /// Defaults to <see cref="Util.RatingDisplayOption.Enable"/> if no setting is defined.
        /// </value>
        private RatingDisplayOption RatingDisplayOption
        {
            get
            {
                if (AreRatingsEnabled)
                {
                    object o = Settings["adEnableRatings"];
                    if (o != null && Enum.IsDefined(typeof(RatingDisplayOption), o))
                    {
                        return (RatingDisplayOption)Enum.Parse(typeof(RatingDisplayOption), o.ToString());
                    }
                    return RatingDisplayOption.Enable;
                }
                return RatingDisplayOption.Disable;
            }
        }

        /// <summary>
        /// Gets whether to display the option to create a comment.
        /// </summary>
        /// <value>
        /// <c>true</c> if the option to create a comment is displayed, otherwise <c>false</c>.
        /// Defaults to <c>true</c> if no setting is defined.
        /// </value>
        private bool DisplayCommentsLink
        {
            get
            {
                if (IsCommentsEnabled)
                {
                    object o = Settings["adCommentsLink"];
                    return (o == null ? true : Convert.ToBoolean(o, CultureInfo.InvariantCulture));
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display comments made on this item.
        /// </summary>
        /// <value>
        /// <c>true</c> if comments should be displayed; otherwise, <c>false</c>.
        /// Defaults to <c>true</c> if no setting is defined.
        /// </value>
        private bool DisplayPublishComments
        {
            get
            {
                if (IsCommentsEnabled)
                {
                    object o = Settings["adCommentsDisplay"];
                    if (o != null)
                    {
                        if (!Convert.ToBoolean(o, CultureInfo.InvariantCulture))
                        {
                            return false;
                        }
                    }
                    //else { o = true; }

                    ItemVersionSetting forumCommentSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "chkForumComments", "Checked", PortalId)
                                                             ?? new ItemVersionSetting
                                                                                                                                                                                       {
                                                                                                                                                                                           ControlName = "chkForumComments",
                                                                                                                                                                                           PropertyName = "Checked",
                                                                                                                                                                                           PropertyValue = false.ToString()
                                                                                                                                                                                       };

                    return IsPublishCommentType || !Convert.ToBoolean(forumCommentSetting.PropertyValue, CultureInfo.InvariantCulture);
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display a link to make comments on this item in a forum post.
        /// </summary>
        /// <value>
        /// <c>true</c> if comments should be made in a forum; otherwise, <c>false</c>.
        /// Defaults to <c>true</c> if no setting is defined.
        /// </value>
        private bool UseForumComments
        {
            get
            {

                if (IsPublishCommentType) return false;
                ItemVersionSetting forumCommentSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "chkForumComments", "Checked", PortalId);
                int? categoryForumId = GetCategoryForumId();
                if (!categoryForumId.HasValue || categoryForumId < 1) return false;
                return (IsCommentsEnabled && !IsPublishCommentType)
                    && (forumCommentSetting != null && Convert.ToBoolean(forumCommentSetting.PropertyValue, CultureInfo.InvariantCulture));
                //{
                //    object o = Settings["adCommentsDisplay"];
                //    return (o == null ? true : Convert.ToBoolean(o, CultureInfo.InvariantCulture));
                //}
                //return false;
            }
        }


        protected void UpdatePanel_Unload(object sender, EventArgs e)
        {
            MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel")).First();
            methodInfo.Invoke(ScriptManager.GetCurrent(Page),
                new object[] { sender as UpdatePanel });
        }

        /// <summary>
        /// Gets the first name collection option, whether to ask for the full name, only the initial, or not to ask for the first name at all.
        /// </summary>
        /// <value>
        /// The first name collection option.
        /// Defaults to <see cref="NameDisplayOption.Full"/> if no setting is defined.
        /// </value>
        private NameDisplayOption FirstNameCollectOption
        {
            get
            {
                object o = Settings["adFirstNameCollectOption"];
                if (o != null && Enum.IsDefined(typeof(NameDisplayOption), o))
                {
                    return (NameDisplayOption)Enum.Parse(typeof(NameDisplayOption), o.ToString());
                }
                return NameDisplayOption.Full;
            }
        }

        /// <summary>
        /// Gets the last name collection option, whether to ask for the full name, only the initial, or not to ask for the last name at all.
        /// </summary>
        /// <value>
        /// The last name collection option.
        /// Defaults to <see cref="NameDisplayOption.Initial"/> if no setting is defined.
        /// </value>
        private NameDisplayOption LastNameCollectOption
        {
            get
            {
                object o = Settings["adLastNameCollectOption"];
                if (o != null && Enum.IsDefined(typeof(NameDisplayOption), o))
                {
                    return (NameDisplayOption)Enum.Parse(typeof(NameDisplayOption), o.ToString());
                }
                return NameDisplayOption.Initial;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display a textbox to collect the email address of the commenter.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the email address should be collected; otherwise, <c>false</c>.
        /// </value>
        private bool CollectEmailAddress
        {
            get
            {
                object o = Settings["adCollectEmailAddress"];

                if (o != null)
                {
                    bool collectEmailAddress;
                    if (bool.TryParse(o.ToString(), out collectEmailAddress))
                    {
                        return collectEmailAddress;
                    }
                }
                return false;
            }
        }

        private bool CollectUrl
        {
            get
            {
                object o = Settings["adCollectUrl"];

                if (o != null)
                {
                    bool value;
                    if (bool.TryParse(o.ToString(), out value))
                    {
                        return value;
                    }
                }
                return true;
            }
        }

        private string LastUpdatedFormat
        {
            get
            {
                object o = Settings["adLastUpdatedFormat"];
                return (o == null ? "MMM yyyy" : o.ToString());
            }
        }

        private int? GetCategoryForumId()
        {
            //TODO: we need to handle items that no longer have a valid parent
            Category pc = Category.GetCategory(Category.GetParentCategory(VersionInfoObject.ItemId, PortalId), PortalId);

            if (pc != null)
            {
                int parentCategoryItemVersionId = pc.ItemVersionId;
                ItemVersionSetting categoryForumSetting = ItemVersionSetting.GetItemVersionSetting(parentCategoryItemVersionId, "CategorySettings", "CommentForumId", PortalId);
                int categoryForumId;
                if (categoryForumSetting == null)
                    return null;
                Int32.TryParse(categoryForumSetting.PropertyValue, out categoryForumId);
                return categoryForumId;
            }
            return null;
        }

        private void LoadArticle()
        {
            try
            {
                BindItemData();
                ConfigureChildControls();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        #region Event Handlers

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    ConfigureSettings();
                    DisplayArticle();
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                ConfigureComments();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        //protected void ajaxRating_Changed(object sender, AjaxControlToolkit.RatingEventArgs e)
        //{
        //    var article = (Article)VersionInfoObject;
        //    article.AddRating(int.Parse(e.Value, CultureInfo.InvariantCulture), UserId == -1 ? null : (int?)UserId);
        //    ajaxRating.ReadOnly = true;
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void btnSubmitComment_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                //TODO: we're allowing anonymous comments, we should have a setting for this.
                var objSecurity = new DotNetNuke.Security.PortalSecurity();
                if (UseForumComments)
                {
                    int? categoryForumId = GetCategoryForumId();
                    if (categoryForumId.HasValue)
                    {
                        int threadId = ForumProvider.GetInstance(PortalId).AddComment(categoryForumId.Value, VersionInfoObject.AuthorUserId,
                            VersionInfoObject.Name, VersionInfoObject.Description, GetItemLinkUrl(VersionInfoObject.ItemId, PortalId),
                            objSecurity.InputFilter(txtComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting), UserId,
                            Request.UserHostAddress);

                        var threadIdSetting = new ItemVersionSetting(Setting.CommentForumThreadId)
                                                  {
                                                      PropertyValue = threadId.ToString(CultureInfo.InvariantCulture),
                                                      ItemVersionId = VersionInfoObject.ItemVersionId
                                                  };
                        threadIdSetting.Save();
                        //VersionInfoObject.VersionSettings.Add(threadIdSetting);
                        //VersionInfoObject.Save(VersionInfoObject.AuthorUserId);
                        Response.Redirect(ForumProvider.GetInstance(PortalId).GetThreadUrl(threadId), true);
                    }
                }
                else
                {
                    if (txtHumanTest.Text.Trim().ToLower() == "human2")
                    {
                        string urlText = txtUrlComment.Text;
                        if (urlText.Trim().Length > 0 && !urlText.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !urlText.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                        {
                            urlText = "http://" + urlText;
                        }

                        int approvalStatusId = ApprovalStatus.Waiting.GetId();
                        if (IsAdmin)
                        {//automatically approve admin comments
                            approvalStatusId = ApprovalStatus.Approved.GetId();
                        }

                        //TODO: format the comment text
                        UserFeedback.Comment.AddComment(VersionInfoObject.ItemVersionId, (UserId == -1 ? null : (int?)UserId),
                            objSecurity.InputFilter(txtComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting), approvalStatusId,
                            null, objSecurity.InputFilter(txtFirstNameComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting),
                            objSecurity.InputFilter(txtLastNameComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting),
                            objSecurity.InputFilter(txtEmailAddressComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting),
                            objSecurity.InputFilter(urlText, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting),
                            DataProvider.ModuleQualifier);

                        //see if comment notification is turned on. Notify the ItemVersion.Author
                        if (IsCommentAuthorNotificationEnabled)
                        {
                            var uc = new UserController();

                            UserInfo ui = uc.GetUser(PortalId, VersionInfoObject.AuthorUserId);

                            if (ui != null)
                            {
                                string emailBody = Localization.GetString("CommentNotificationEmail.Text", LocalResourceFile);
                                emailBody = String.Format(emailBody
                                    , VersionInfoObject.Name
                                    , GetItemLinkUrlExternal(VersionInfoObject.ItemId)
                                    , objSecurity.InputFilter(txtFirstNameComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)
                                    , objSecurity.InputFilter(txtLastNameComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)
                                    , objSecurity.InputFilter(txtEmailAddressComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)
                                    , objSecurity.InputFilter(txtComment.Text, DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting)

                                    );

                                string emailSubject = Localization.GetString("CommentNotificationEmailSubject.Text", LocalResourceFile);
                                emailSubject = String.Format(emailSubject, VersionInfoObject.Name);

                                Mail.SendMail(PortalSettings.Email, ui.Email, string.Empty, emailSubject, emailBody, string.Empty, "HTML", string.Empty, string.Empty, string.Empty, string.Empty);
                            }
                        }


                        ConfigureComments();
                        txtHumanTest.Text = string.Empty;

                        pnlCommentEntry.Visible = false;
                        pnlCommentConfirmation.Visible = true;
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void btnCancelComment_Click(object sender, EventArgs e)
        {
            ClearCommentInput();
            //mpeComment.Hide();
            //mpeForumComment.Hide();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void btnConfirmationClose_Click(object sender, EventArgs e)
        {
            pnlCommentEntry.Visible = true;
            pnlCommentConfirmation.Visible = false;
            //mpeComment.Hide();
        }


        #endregion

        private void ConfigureComments()
        {
            bool showNamePanel = false;
            ClearCommentInput();
            if (mvCommentDisplay.GetActiveView() == vwPublishComments)
            {
                switch (FirstNameCollectOption)
                {
                    case NameDisplayOption.Initial:
                        txtFirstNameComment.MaxLength = 1;
                        txtFirstNameComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.FirstName.Substring(0, 1) : string.Empty;
                        lblFirstNameComment.Text = Localization.GetString("FirstInitial", LocalResourceFile);
                        showNamePanel = true;
                        break;
                    case NameDisplayOption.None:
                        txtFirstNameComment.Visible = false;
                        lblFirstNameComment.Visible = false;
                        rfvFirstNameComment.Enabled = false;
                        
                        //vceFirstNameComment.Enabled = false;
                        break;
                    //case NameDisplayOption.Full:
                    default:
                        txtFirstNameComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.FirstName : string.Empty;
                        lblFirstNameComment.Text = Localization.GetString("FirstName", LocalResourceFile);
                        showNamePanel = true;
                        break;
                }

                switch (LastNameCollectOption)
                {
                    case NameDisplayOption.Initial:
                        txtLastNameComment.MaxLength = 1;
                        txtLastNameComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.LastName.Substring(0, 1) : string.Empty;
                        lblLastNameComment.Text = Localization.GetString("LastInitial", LocalResourceFile);
                        showNamePanel = true;
                        break;
                    case NameDisplayOption.None:
                        txtLastNameComment.Visible = false;
                        lblLastNameComment.Visible = false;
                        rfvLastNameComment.Enabled = false;
                        //vceLastNameComment.Enabled = false;
                        break;
                    //case NameDisplayOption.Full:
                    default:
                        txtLastNameComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.LastName : string.Empty;
                        lblLastNameComment.Text = Localization.GetString("LastName", LocalResourceFile);
                        showNamePanel = true;
                        break;
                }

                pnlEmailAddressComment.Visible = rfvEmailAddressComment.Enabled = CollectEmailAddress;
                pnlNameComment.Visible = showNamePanel;
                pnlUrlComment.Visible = CollectUrl;

                if (pnlEmailAddressComment.Visible)
                {
                    txtEmailAddressComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.Email : string.Empty;
                }
                if (pnlUrlComment.Visible)
                {
                    txtUrlComment.Text = (UserInfo != null && UserInfo.UserID != -1) ? UserInfo.Profile.Website : string.Empty;
                }
            }
            else
            {
                pnlNameComment.Visible = rfvFirstNameComment.Enabled = rfvLastNameComment.Enabled = false;
                pnlEmailAddressComment.Visible = rfvEmailAddressComment.Enabled = false;
                pnlUrlComment.Visible = false;
            }
        }

        private void ConfigureChildControls()
        {
            if (VersionInfoObject.IsNew) return;

            //check if items are enabled.
            if (DisplayEmailAFriend && VersionInfoObject.IsNew == false)
            {
                ea = (EmailAFriend)LoadControl(EmailControlToLoad);
                ea.ModuleConfiguration = ModuleConfiguration;
                ea.ID = Path.GetFileNameWithoutExtension(EmailControlToLoad);
                ea.VersionInfoObject = VersionInfoObject;
                phEmailAFriend.Controls.Add(ea);
            }
            if (DisplayPrinterFriendly && VersionInfoObject.IsNew == false)
            {
                pf = (PrinterFriendlyButton)LoadControl(PrinterControlToLoad);
                pf.ModuleConfiguration = ModuleConfiguration;
                pf.ID = Path.GetFileNameWithoutExtension(PrinterControlToLoad);
                phPrinterFriendly.Controls.Add(pf);
            }

            if (DisplayRelatedLinks)
            {
                ral = (RelatedArticleLinksBase)LoadControl(RelatedArticlesControlToLoad);
                ral.ModuleConfiguration = ModuleConfiguration;
                ral.ID = Path.GetFileNameWithoutExtension(RelatedArticlesControlToLoad);
                phRelatedArticles.Controls.Add(ral);
            }

            if (DisplayRelatedArticle)
            {
                Article a = VersionInfoObject.GetRelatedArticle(PortalId);
                if (a != null)
                {
                    ad = (ArticleDisplay)LoadControl(ArticleControlToLoad);
                    ad.ModuleConfiguration = ModuleConfiguration;
                    ad.ID = Path.GetFileNameWithoutExtension(ArticleControlToLoad);
                    ad.Overrideable = false;
                    ad.UseCache = true;
                    ad.DisplayPrinterFriendly = false;
                    ad.DisplayRelatedArticle = false;
                    ad.DisplayRelatedLinks = false;
                    ad.DisplayEmailAFriend = false;



                    ad.SetItemId(a.ItemId);
                    ad.DisplayTitle = false;
                    phRelatedArticle.Controls.Add(ad);
                    divRelatedArticle.Visible = true;
                }
                else
                {
                    divRelatedArticle.Visible = false;
                }
            }

            if (RatingDisplayOption.Equals(RatingDisplayOption.Enable) || RatingDisplayOption.Equals(RatingDisplayOption.ReadOnly))
            {
                //get the upnlRating setting
                ItemVersionSetting rtSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "upnlRating", "Visible", PortalId);
                if (rtSetting != null)
                {
                    upnlRating.Visible = Convert.ToBoolean(rtSetting.PropertyValue, CultureInfo.InvariantCulture);
                }
                if (upnlRating.Visible)
                {
                    //lblRatingMessage.Visible = true; //TODO: re-enable ratings
                    lblRatingMessage.Visible = false; //TODO: re-enable ratings

                    //ajaxRating.MaxRating = MaximumRating;

                    var avgRating = (int)Math.Round(((Article)VersionInfoObject).AverageRating);
                    //ajaxRating.CurrentRating = (avgRating > MaximumRating ? MaximumRating : (avgRating < 0 ? 0 : avgRating));

                    //ajaxRating.ReadOnly = RatingDisplayOption.Equals(RatingDisplayOption.ReadOnly);
                }
            }

            btnComment.Visible = DisplayCommentsLink;
            if (IsCommentsEnabled)
            {
                btnComment.NavigateUrl = Request.RawUrl + "#CommentEntry";
                if (!UseForumComments || (DisplayPublishComments && !VersionInfoObject.IsNew))
                {
                    pnlComments.Visible = pnlCommentDisplay.Visible = true;
                    commentDisplay = (CommentDisplayBase)LoadControl(CommentsControlToLoad);
                    commentDisplay.ModuleConfiguration = ModuleConfiguration;
                    commentDisplay.ID = Path.GetFileNameWithoutExtension(CommentsControlToLoad);
                    commentDisplay.ArticleId = VersionInfoObject.ItemId;
                    phCommentsDisplay.Controls.Add(commentDisplay);
                }

                if (UseForumComments)
                {
                    pnlComments.Visible = true;
                    mvCommentDisplay.SetActiveView(vwForumComments);
                    ItemVersionSetting forumThreadIdSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "ArticleSetting", "CommentForumThreadId", PortalId);
                    if (forumThreadIdSetting != null)
                    {
                        lnkGoToForum.Visible = true;
                        lnkGoToForum.NavigateUrl = ForumProvider.GetInstance(PortalId).GetThreadUrl(Convert.ToInt32(forumThreadIdSetting.PropertyValue, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        btnForumComment.Visible = true;
                    }
                }
            }
            ConfigureTags();
        }


        private void ConfigureTags()
        {
            //get the upnlRating setting
            ItemVersionSetting tgSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "pnlTags", "Visible", PortalId);
            if (tgSetting != null)
            {
                pnlTags.Visible = Convert.ToBoolean(tgSetting.PropertyValue, CultureInfo.InvariantCulture);
                if (Convert.ToBoolean(tgSetting.PropertyValue, CultureInfo.InvariantCulture))
                {
                    PopulateTagList();
                }
            }
            else
            {
                if (VersionInfoObject.Tags.Count > 0)
                {
                    pnlTags.Visible = true;
                    PopulateTagList();
                }
            }
        }


        private void ConfigureSettings()
        {
            // LogBreadcrumb is true by default.  Check to see if we need to turn it off.
            object o = Settings["LogBreadCrumb"];
            if (o != null)
            {
                bool logBreadCrumb;
                if (bool.TryParse(o.ToString(), out logBreadCrumb))
                {
                    LogBreadcrumb = logBreadCrumb;
                }
            }
        }

        private void DisplayArticle()
        {
            if (VersionInfoObject.IsNew)
            {
                if (IsAdmin || IsAuthor)
                {
                    //Default the text to no approved version. if the module isn't configured or no Categories/Articles exist yet then it will be overwritten.
                    lblArticleText.Text = Localization.GetString("NoApprovedVersion", LocalResourceFile);

                    //Check to see if there are Categories defined. If none are defined this is the first
                    //instance of the Module so we need to notify the user to create categories and articles.
                    int categoryCount = DataProvider.Instance().GetCategories(PortalId).Rows.Count;
                    if (categoryCount == 0)
                    {
                        lblArticleText.Text = Localization.GetString("NoDataToDisplay", LocalResourceFile);
                    }
                    else if (IsConfigured == false)
                    {
                        lnkConfigure.Text = Localization.GetString("UnableToFindAction", LocalResourceFile);
                        lnkConfigure.NavigateUrl = EditUrl("ModuleId", ModuleId.ToString(CultureInfo.InvariantCulture), "Module");
                        lnkConfigure.Visible = true;
                        lblArticleText.Text = Localization.GetString("UnableToFind", LocalResourceFile);
                    }
                }
                return;
            }


            if (Item.GetItemType(VersionInfoObject.ItemId, PortalId) == "Article")
            {
                UseCache = true;

                var article = (Article)VersionInfoObject;
                if (DisplayTitle)
                {
                    SetPageTitle();
                    lblArticleTitle.Text = article.Name;
                    //divArticleTitle.Visible = true;
                    //divLastUpdated.Visible = true;
                }

                article.ArticleText = Utility.ReplaceTokens(article.ArticleText);
                DisplayArticlePaging(article);

                string referrer = string.Empty;
                if (HttpContext.Current.Request.UrlReferrer != null)
                {
                    referrer = HttpContext.Current.Request.UrlReferrer.ToString();
                }
                string url = string.Empty;
                if (HttpContext.Current.Request.RawUrl != null)
                {
                    url = HttpContext.Current.Request.RawUrl;
                }
                article.AddView(UserId, TabId, HttpContext.Current.Request.UserHostAddress);

                DateTime lastUpdated = Convert.ToDateTime(article.LastUpdated, CultureInfo.InvariantCulture);

                DateTime dateCreated = Convert.ToDateTime(article.CreatedDate, CultureInfo.InvariantCulture);

                lblDateCreated.Text = String.Format(Localization.GetString("DateCreated", LocalResourceFile), dateCreated.ToLongDateString());

                if(lastUpdated.Date > dateCreated.Date)
                { 
                    lblLastUpdated.Text = String.Format(Localization.GetString("LastUpdated", LocalResourceFile), lastUpdated.ToShortDateString());
                    lblLastUpdated.Visible = true;
                }
                else
                {
                    lblLastUpdated.Visible = false;
                }

                //get the pnlAuthor setting
                ItemVersionSetting auSetting = ItemVersionSetting.GetItemVersionSetting(article.ItemVersionId, "pnlAuthor", "Visible", PortalId);
                if (auSetting != null)
                {
                    ShowAuthor = Convert.ToBoolean(auSetting.PropertyValue, CultureInfo.InvariantCulture);
                }

                if (ShowAuthor)
                {
                    //pnlAuthor.Visible = true;
                    lblAuthor.Text = article.Author;
                    if (lblAuthor.Text.Trim().Length < 1)
                    {
                        var uc = new UserController();
                        UserInfo ui = uc.GetUser(PortalId, article.AuthorUserId);
                        lblAuthor.Text = ui.DisplayName;
                    }

                    if (lblAuthor.Text.Trim().Length < 1)
                    {
                        //pnlAuthor.Visible = false;
                    }
                }
                else
                {
                    //pnlAuthor.Visible = false;
                }

                //get the pnlPrinterFriendly setting
                ItemVersionSetting pfSetting = ItemVersionSetting.GetItemVersionSetting(article.ItemVersionId, "pnlPrinterFriendly", "Visible", PortalId);
                if (pfSetting != null)
                {
                    phPrinterFriendly.Visible = Convert.ToBoolean(pfSetting.PropertyValue, CultureInfo.InvariantCulture);
                }

                //get the pnlEmailAFriend setting
                ItemVersionSetting efSetting = ItemVersionSetting.GetItemVersionSetting(article.ItemVersionId, "pnlEmailAFriend", "Visible", PortalId);
                if (efSetting != null)
                {
                    phEmailAFriend.Visible = Convert.ToBoolean(efSetting.PropertyValue, CultureInfo.InvariantCulture);
                }

                //get the pnlComments setting
                ItemVersionSetting ctSetting = ItemVersionSetting.GetItemVersionSetting(article.ItemVersionId, "pnlComments", "Visible", PortalId);
                if (ctSetting != null)
                {
                    pnlComments.Visible = Convert.ToBoolean(ctSetting.PropertyValue, CultureInfo.InvariantCulture);
                }


                ////get the upnlRating setting
                //ItemVersionSetting tgSetting = ItemVersionSetting.GetItemVersionSetting(article.ItemVersionId, "pnlTags", "Visible");
                //if (tgSetting != null)
                //{
                //    pnlTags.Visible = Convert.ToBoolean(tgSetting.PropertyValue, CultureInfo.InvariantCulture);
                //    if (Convert.ToBoolean(tgSetting.PropertyValue, CultureInfo.InvariantCulture))
                //    {
                //        PopulateTagList();
                //    }
                //}
                //else
                //{
                //    if (article.Tags.Count > 0)
                //    {
                //        pnlTags.Visible = true;
                //        PopulateTagList();
                //    }
                //}


                DisplayReturnToList(article);
            }
        }

        private void PopulateTagList()
        {
            var article = (Article)VersionInfoObject;
            foreach (ItemTag t in article.Tags)
            {
                var hl = new HyperLink();
                Tag tag = Tag.GetTag(t.TagId, PortalId);
                hl.Text = tag.Name;
                hl.NavigateUrl = DotNetNuke.Common.Globals.NavigateURL(DefaultTagDisplayTabId, string.Empty, "&tags=" + HttpUtility.UrlEncode(tag.Name));
                hl.Attributes.Add("rel", "tag");
                hl.Attributes.Add("class", "btn btn-info btn-sm active"); //bootstrap button class
                hl.Attributes.Add("role", "button");
                var li = new Literal { Text = "&nbsp;" };

                phTags.Controls.Add(hl);
                phTags.Controls.Add(li);
            }
            if (phTags.Controls.Count > 1)
            {
                phTags.Controls.RemoveAt(phTags.Controls.Count - 1);
            }
            else
            {
                pnlTags.Visible = false;
            }
        }


        private void DisplayArticlePaging(Article article)
        {
            //check if we're using paging
            if (AllowArticlePaging && (PageId > 0))
            {
                lblArticleText.Text = article.GetPage(PageId).Replace("[PAGE]", string.Empty);

                //lblArticleText.Text = article.GetPage(PageId).Replace("[PAGE]", "");

                //lnkPreviousPage

                if (PageId > 1)
                {
                    lnkPreviousPage.Text = Localization.GetString("lnkPreviousPage", LocalResourceFile);
                    lnkPreviousPage.NavigateUrl = Utility.GetItemLinkUrl(article.ItemId, PortalId, TabId, ModuleId, PageId - 1, GetCultureName());
                    lnkNextPage.Attributes.Add("rel", "prev");
                }

                if (PageId < article.GetNumberOfPages)
                {
                    lnkNextPage.Text = Localization.GetString("lnkNextPage", LocalResourceFile);
                    lnkNextPage.NavigateUrl = Utility.GetItemLinkUrl(article.ItemId, PortalId, TabId, ModuleId, PageId + 1, GetCultureName());
                    lnkNextPage.Attributes.Add("rel", "next");
                }
            }
            else
            {
                lblArticleText.Text = article.ArticleText.Replace("[PAGE]", string.Empty);
                lnkPreviousPage.Visible = false;
                lnkNextPage.Visible = false;
            }
        }

        private void DisplayReturnToList(Article article)
        {
            //lnkReturnToList
            if (article.DisplayReturnToList())
            {
                //check if there's a "list" in session, if so go back to that URL
                if (Session["PublishListLink"] != null && Utility.HasValue(Session["PublishListLink"].ToString()))
                {
                    lnkReturnToList.NavigateUrl = Session["PublishListLink"].ToString().Trim();
                    lnkReturnToList.Text = String.Format(CultureInfo.CurrentCulture, Localization.GetString("lnkReturnToList", LocalResourceFile), string.Empty);
                }
                else
                {
                    pnlReturnToList.Visible = true;

                    int parentItemId = article.GetParentCategoryId();
                    if (parentItemId > 0)
                    {
                        lnkReturnToList.NavigateUrl = GetItemLinkUrl(parentItemId);

                        //check of the parent category is set to not display on current page, if it isn't, we need to force it to be so here.
                        Category cparent = Category.GetCategory(parentItemId, PortalId);


                        lnkReturnToList.Text = String.Format(CultureInfo.CurrentCulture, Localization.GetString("lnkReturnToList", LocalResourceFile), cparent.Name);
                    }
                    else
                    {
                        pnlReturnToList.Visible = false;
                    }
                }
            }
        }

        private void ClearCommentInput()
        {
            txtComment.Text = string.Empty;
            txtFirstNameComment.Text = string.Empty;
            txtLastNameComment.Text = string.Empty;
            txtEmailAddressComment.Text = string.Empty;
        }

        #region Optional Interfaces

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection
                           {
                                   {
                                           GetNextActionID(),
                                           Localization.GetString("Settings", LocalResourceFile),
                                           DotNetNuke.Entities.Modules.Actions.ModuleActionType.AddContent,
                                           string.Empty, string.Empty, EditUrl("Settings"), false,
                                           DotNetNuke.Security.SecurityAccessLevel.Edit, true, false
                                           }
                           };
            }
        }


        #endregion
    }
}

