//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using DotNetNuke.Entities.Controllers;

namespace Engage.Dnn.Publish.Admin
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Net;
    using System.Runtime.Remoting.Contexts;
    using System.Web.UI.WebControls;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Publish;
    using Util;

    public partial class AdminSettings : ModuleBase
    {
        protected PlaceHolder SettingsTablePlaceHolder;

        #region Event Handlers
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
            lnkUpdate.Click += lnkUpdate_Click;
        }

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizeCollapsePanels();

                if (!IsSetup)
                {
                    //if not setup already display a message notifying they must setup the admin side of things before they can procede.
                    lblMessage.Text = Localization.GetString("FirstTime", LocalResourceFile);
                }

                if (IsPostBack == false)
                {
                    LoadSettings();
                }
                if (IsHostMailConfigured == false)
                {
                    lblMailNotConfigured.Visible = true;
                    lblMailNotConfiguredComment.Visible = true;
                }

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void lnkUpdate_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                
                HostController.Instance.Update(Utility.PublishSetup + PortalId.ToString(CultureInfo.InvariantCulture), "true");
                HostController.Instance.Update(Utility.PublishEmail + PortalId.ToString(CultureInfo.InvariantCulture), chkEmailNotification.Checked.ToString(CultureInfo.InvariantCulture));
               
                HostController.Instance.Update(Utility.PublishEmailNotificationRole + PortalId.ToString(CultureInfo.InvariantCulture), ddlEmailRoles.SelectedValue);
                HostController.Instance.Update(Utility.PublishAdminRole + PortalId.ToString(CultureInfo.InvariantCulture), ddlRoles.SelectedValue);
                HostController.Instance.Update(Utility.PublishAuthorRole + PortalId.ToString(CultureInfo.InvariantCulture), ddlAuthor.SelectedValue);

                HostController.Instance.Update(Utility.PublishAuthorCategoryEdit + PortalId.ToString(CultureInfo.InvariantCulture), chkAuthorCategoryEdit.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishEnableArticlePaging + PortalId.ToString(CultureInfo.InvariantCulture), chkEnablePaging.Checked.ToString(CultureInfo.InvariantCulture));
                
                HostController.Instance.Update(Utility.PublishEnableTags + PortalId.ToString(CultureInfo.InvariantCulture), chkEnableTags.Checked.ToString(CultureInfo.InvariantCulture));
                
                HostController.Instance.Update(Utility.PublishEnableViewTracking + PortalId.ToString(CultureInfo.InvariantCulture), chkEnableViewTracking.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishEnableDisplayNameAsHyperlink + PortalId.ToString(CultureInfo.InvariantCulture), chkEnableDisplayNameAsHyperlink.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishAllowRichTextDescriptions + PortalId.ToString(CultureInfo.InvariantCulture), chkAllowRichTextDescriptions.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultRichTextDescriptions + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultRichTextDescriptions.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishUseApprovals + PortalId.ToString(CultureInfo.InvariantCulture), chkUseApprovals.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishUseEmbeddedArticles + PortalId.ToString(CultureInfo.InvariantCulture), chkUseEmbeddedArticles.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishShowItemId + PortalId.ToString(CultureInfo.InvariantCulture), chkShowItemId.Checked.ToString(CultureInfo.InvariantCulture));

                HostController.Instance.Update(Utility.PublishCacheTime + PortalId.ToString(CultureInfo.InvariantCulture), txtDefaultCacheTime.Text.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultAdminPagingSize + PortalId.ToString(CultureInfo.InvariantCulture), txtAdminPagingSize.Text.ToString(CultureInfo.InvariantCulture));

                HostController.Instance.Update(Utility.PublishDescriptionEditHeight + PortalId.ToString(CultureInfo.InvariantCulture), txtItemDescriptionHeight.Text.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishArticleEditHeight + PortalId.ToString(CultureInfo.InvariantCulture), txtArticleTextHeight.Text.ToString(CultureInfo.InvariantCulture));

                HostController.Instance.Update(Utility.PublishDescriptionEditWidth + PortalId.ToString(CultureInfo.InvariantCulture), txtItemDescriptionWidth.Text.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishArticleEditWidth + PortalId.ToString(CultureInfo.InvariantCulture), txtArticleTextWidth.Text.ToString(CultureInfo.InvariantCulture));
                

                HostController.Instance.Update(Utility.PublishEnablePublishFriendlyUrls + PortalId.ToString(CultureInfo.InvariantCulture), chkEnablePublishFriendlyUrls.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishThumbnailSubdirectory + PortalId.ToString(CultureInfo.InvariantCulture), txtThumbnailSubdirectory.Text.EndsWith("/", StringComparison.Ordinal) ? txtThumbnailSubdirectory.Text : txtThumbnailSubdirectory.Text + "/");
                HostController.Instance.Update(Utility.PublishThumbnailDisplayOption + PortalId.ToString(CultureInfo.InvariantCulture), radThumbnailSelection.SelectedValue.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultDisplayPage + PortalId.ToString(CultureInfo.InvariantCulture), ddlDefaultDisplay.SelectedValue.ToString(CultureInfo.InvariantCulture));

                if (ddlDefaultCategory.SelectedIndex > 0)
                    HostController.Instance.Update(Utility.PublishDefaultCategory + PortalId.ToString(CultureInfo.InvariantCulture), ddlDefaultCategory.SelectedValue.ToString(CultureInfo.InvariantCulture));


                HostController.Instance.Update(Utility.PublishDefaultReturnToList + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultReturnToList.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultRatings + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultArticleRatings.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultComments + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultArticleComments.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishCommentEmailAuthor + PortalId.ToString(CultureInfo.InvariantCulture), chkCommentNotification.Checked.ToString(CultureInfo.InvariantCulture));

                HostController.Instance.Update(Utility.PublishDefaultEmailAFriend + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultEmailAFriend.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultPrinterFriendly+ PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultPrinterFriendly.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishSessionReturnToList + PortalId.ToString(CultureInfo.InvariantCulture), chkReturnToListSession.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultShowAuthor + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultShowAuthor.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishDefaultShowTags + PortalId.ToString(CultureInfo.InvariantCulture), chkDefaultShowTags.Checked.ToString(CultureInfo.InvariantCulture));

                /*ratings*/
                HostController.Instance.Update(Utility.PublishRating + PortalId.ToString(CultureInfo.InvariantCulture), chkEnableRatings.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishRatingMaximum + PortalId.ToString(CultureInfo.InvariantCulture), txtMaximumRating.Text);
                //HostController.Instance.Update(Utility.PublishRatingAnonymous + PortalId.ToString(CultureInfo.InvariantCulture), chkAnonymousRatings.Checked.ToString(CultureInfo.InvariantCulture));
                /*comments*/
                HostController.Instance.Update(Utility.PublishComment + PortalId.ToString(CultureInfo.InvariantCulture), chkEnableComments.Checked.ToString(CultureInfo.InvariantCulture));


                /*Ping Settings*/
                HostController.Instance.Update(Utility.PublishEnablePing + PortalId.ToString(CultureInfo.InvariantCulture), chkEnablePing.Checked.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishPingServers + PortalId.ToString(CultureInfo.InvariantCulture), txtPingServers.Text.ToString(CultureInfo.InvariantCulture));
                HostController.Instance.Update(Utility.PublishPingChangedUrl + PortalId.ToString(CultureInfo.InvariantCulture), txtPingChangedUrl.Text.ToString(CultureInfo.InvariantCulture));

                HostController.Instance.Update(Utility.PublishForumProviderType + PortalId.ToString(CultureInfo.InvariantCulture), ddlCommentsType.SelectedValue);
    
                //HostController.Instance.Update(Utility.PublishCommentApproval + PortalId.ToString(CultureInfo.InvariantCulture), chkCommentApproval.Checked.ToString(CultureInfo.InvariantCulture));
                //HostController.Instance.Update(Utility.PublishCommentAnonymous + PortalId.ToString(CultureInfo.InvariantCulture), chkAnonymousComment.Checked.ToString(CultureInfo.InvariantCulture));
                //HostController.Instance.Update(Utility.PublishCommentAutoApprove + PortalId.ToString(CultureInfo.InvariantCulture), chkCommentAutoApprove.Checked.ToString(CultureInfo.InvariantCulture));

                string returnUrl = Server.UrlDecode(Request.QueryString["returnUrl"]);
                if (!Utility.HasValue(returnUrl) || !Utility.IsLocalURL(returnUrl))
                {
                    Response.Redirect(DotNetNuke.Common.Globals.NavigateURL(TabId, Utility.AdminContainer, "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture)));
                }
                else
                {
                    if (Utility.IsLocalURL(returnUrl))
                    {

                        var response = Response;
                        response.StatusCode = Int32.Parse(HttpStatusCode.OK.ToString());
                        response.Headers["Location"] = returnUrl;

                        //response.Redirect(returnUrl);
                    }
                    else
                    {
                        var response = Response;
                        response.StatusCode = Int32.Parse(HttpStatusCode.OK.ToString());
                        response.Headers["Location"] = "/";
                        //Response.Redirect("/");
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "cv is not an acronym, it is a control prefix"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cvEmailNotification_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (args != null)
            {
                args.IsValid = IsHostMailConfigured || !chkEmailNotification.Checked;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkEnableRatings control,
        /// showing the ratings options if they are enabled, hiding them if not.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void chkEnableRatings_CheckedChanged(object source, EventArgs args)
        {
            //rowAnonymousRatings.Visible = chkEnableRatings.Checked;
            rowMaximumRating.Visible = chkEnableRatings.Checked;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkEnableComments control,
        /// showing the comments options if they are enabled, hiding them if not.
        /// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        //protected void chkEnableComments_CheckedChanged(object source, EventArgs args)
        //{
        //    rowCommentAnonymous.Visible = chkEnableComments.Checked;
        //    rowCommentApproval.Visible = chkEnableComments.Checked;
        //    rowCommentAutoApprove.Visible = chkEnableComments.Checked;

        //    chkCommentAutoApprove.Enabled = chkCommentApproval.Checked && chkAnonymousComment.Checked;
        //    if (!chkCommentAutoApprove.Enabled)
        //    {
        //        chkCommentAutoApprove.Checked = !chkCommentApproval.Checked;
        //    }
        //}

        /// <summary>
        /// Handles the CheckedChanged event of the chkCommentApproval and chkAnonymousComment controls, 
        /// showing rowAutoApprove if both are checked, and hiding it if either aren't checked.
        /// </summary>
        //protected void ShowCommentAutoApprove(object source, EventArgs args)
        //{
        //    chkCommentAutoApprove.Enabled = chkCommentApproval.Checked && chkAnonymousComment.Checked;
        //    if (!chkCommentAutoApprove.Enabled)
        //    {
        //        chkCommentAutoApprove.Checked = !chkCommentApproval.Checked;
        //    }
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member", Justification = "cv is not an acronym, it is a control prefix")]
        protected void cvMaximumRating_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (args != null)
            {
                int max;
                args.IsValid = int.TryParse(args.Value, out max);
            }
        }

        #endregion

        private void LoadSettings()
        {
            FillListControls();

            Utility.SetSettingListValue(Utility.PublishThumbnailDisplayOption, PortalId, radThumbnailSelection);
            Utility.SetSettingListValue(Utility.PublishAdminRole, PortalId, ddlRoles);
            Utility.SetSettingListValue(Utility.PublishAuthorRole, PortalId, ddlAuthor);
            Utility.SetSettingListValue(Utility.PublishEmailNotificationRole, PortalId, ddlEmailRoles);
           
            Utility.SetSettingListValue(Utility.PublishDefaultDisplayPage, PortalId, ddlDefaultDisplay);

            
            Utility.SetSettingListValue(Utility.PublishDefaultCategory, PortalId, ddlDefaultCategory);
            
            Utility.SetSettingListValue(Utility.PublishForumProviderType, PortalId, ddlCommentsType);

            txtDefaultCacheTime.Text = Utility.GetStringPortalSetting(Utility.PublishCacheTime, PortalId, "5");
            txtAdminPagingSize.Text = Utility.GetStringPortalSetting(Utility.PublishDefaultAdminPagingSize, PortalId, "25");

            txtArticleTextHeight.Text = Utility.GetStringPortalSetting(Utility.PublishArticleEditHeight, PortalId, "500");
            txtArticleTextWidth.Text = Utility.GetStringPortalSetting(Utility.PublishArticleEditWidth, PortalId, "500");
            txtItemDescriptionHeight.Text = Utility.GetStringPortalSetting(Utility.PublishDescriptionEditHeight, PortalId, "300");
            txtItemDescriptionWidth.Text = Utility.GetStringPortalSetting(Utility.PublishDescriptionEditWidth, PortalId, "500");
            txtThumbnailSubdirectory.Text = Utility.GetStringPortalSetting(Utility.PublishThumbnailSubdirectory, PortalId, "PublishThumbnails/");
            txtMaximumRating.Text = Utility.GetStringPortalSetting(Utility.PublishRatingMaximum, PortalId, UserFeedback.Rating.DefaultMaximumRating.ToString(CultureInfo.CurrentCulture));
            txtPingServers.Text = Utility.GetStringPortalSetting(Utility.PublishPingServers, PortalId, Localization.GetString("DefaultPingServers", LocalResourceFile));
            txtPingChangedUrl.Text = Utility.GetStringPortalSetting(Utility.PublishPingChangedUrl, PortalId);

            chkEmailNotification.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEmail, PortalId, true);
            chkAuthorCategoryEdit.Checked = Utility.GetBooleanPortalSetting(Utility.PublishAuthorCategoryEdit, PortalId, false);
            chkEnableRatings.Checked = Utility.GetBooleanPortalSetting(Utility.PublishRating, PortalId, false);
            chkEnablePaging.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnableArticlePaging, PortalId, true);

            chkEnableTags.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnableTags, PortalId, false);
            chkEnableViewTracking.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnableViewTracking, PortalId, false);
            chkEnableDisplayNameAsHyperlink.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnableDisplayNameAsHyperlink, PortalId, false);
            chkAllowRichTextDescriptions.Checked = Utility.GetBooleanPortalSetting(Utility.PublishAllowRichTextDescriptions, PortalId, true);
            DefaultRichTextDescriptions();
            chkDefaultRichTextDescriptions.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultRichTextDescriptions, PortalId, false);
            chkUseApprovals.Checked = Utility.GetBooleanPortalSetting(Utility.PublishUseApprovals, PortalId, true);
            chkUseEmbeddedArticles.Checked = Utility.GetBooleanPortalSetting(Utility.PublishUseEmbeddedArticles, PortalId, false);
            chkShowItemId.Checked = Utility.GetBooleanPortalSetting(Utility.PublishShowItemId, PortalId, true);
            chkEnableComments.Checked = Utility.GetBooleanPortalSetting(Utility.PublishComment, PortalId, false);
            chkReturnToListSession.Checked = Utility.GetBooleanPortalSetting(Utility.PublishSessionReturnToList, PortalId, false);
            chkDefaultShowAuthor.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultShowAuthor, PortalId, true);
            chkDefaultShowTags.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultShowTags, PortalId, false);
            chkEnablePing.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnablePing, PortalId, false);

            chkDefaultEmailAFriend.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultEmailAFriend, PortalId, true);
            chkDefaultPrinterFriendly.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultPrinterFriendly, PortalId, true);
            chkDefaultArticleRatings.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultRatings, PortalId, true);
            chkDefaultArticleComments.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultComments, PortalId, true);
            chkCommentNotification.Checked = Utility.GetBooleanPortalSetting(Utility.PublishCommentEmailAuthor, PortalId, true);

            chkDefaultReturnToList.Checked = Utility.GetBooleanPortalSetting(Utility.PublishDefaultReturnToList, PortalId, false);

            

            //default to using Publish URLs, unless FriendlyUrls aren't on, then disable the option and show a message.
            if (HostController.Instance.GetString("UseFriendlyUrls") == "Y")
            {
                chkEnablePublishFriendlyUrls.Checked = Utility.GetBooleanPortalSetting(Utility.PublishEnablePublishFriendlyUrls, PortalId, true);
            }
            else
            {
                lblFriendlyUrlsNotOn.Visible = true;
                chkEnablePublishFriendlyUrls.Checked = false;
                chkEnablePublishFriendlyUrls.Enabled = false;
            }

            DefaultRichTextDescriptions();

            //s = HostController.Instance.GetString(Utility.PublishRatingAnonymous + PortalId.ToString(CultureInfo.InvariantCulture));
            //if (Utility.HasValue(s))
            //{
            //    chkAnonymousRatings.Checked = Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            //}

            //s = HostController.Instance.GetString(Utility.PublishCommentAnonymous + PortalId.ToString(CultureInfo.InvariantCulture));
            //if (Utility.HasValue(s))
            //{
            //    chkAnonymousComment.Checked = Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            //}

            //s = HostController.Instance.GetString(Utility.PublishCommentApproval + PortalId.ToString(CultureInfo.InvariantCulture));
            //if (Utility.HasValue(s))
            //{
            //    chkCommentApproval.Checked = Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            //}
            //else
            //{
            //    chkCommentApproval.Checked = true;
            //}

            //s = HostController.Instance.GetString(Utility.PublishCommentAutoApprove + PortalId.ToString(CultureInfo.InvariantCulture));
            //if (Utility.HasValue(s))
            //{
            //    chkCommentAutoApprove.Checked = Convert.ToBoolean(s, CultureInfo.InvariantCulture);
            //}

            //rowAnonymousRatings.Visible = chkEnableRatings.Checked;
            rowMaximumRating.Visible = chkEnableRatings.Checked;
            //rowCommentApproval.Visible = chkEnableComments.Checked;
            //rowCommentAnonymous.Visible = chkEnableComments.Checked;
            //rowCommentAutoApprove.Visible = chkEnableComments.Checked;

            //chkCommentAutoApprove.Enabled = chkCommentApproval.Checked && chkAnonymousComment.Checked;
            //if (!chkCommentAutoApprove.Enabled)
            //{
            //    chkCommentAutoApprove.Checked = !chkCommentApproval.Checked;
            //}
        }

        private void LoadThumbnailSelectionRadioButtonList()
        {
            //Load thumbnail options
            radThumbnailSelection.Items.Add(new ListItem(Localization.GetString(ThumbnailDisplayOption.DotNetNuke.ToString(), LocalResourceFile), ThumbnailDisplayOption.DotNetNuke.ToString()));
            radThumbnailSelection.Items.Add(new ListItem(Localization.GetString(ThumbnailDisplayOption.EngagePublish.ToString(), LocalResourceFile), ThumbnailDisplayOption.EngagePublish.ToString()));
            //default the setting to DNN
            radThumbnailSelection.SelectedIndex = 0;
        }

        private void FillListControls()
        {
            //Load the roles for admin/author dropdowns
            var portalRoles = (new RoleController()).GetRoles(PortalId);
            //load the display dropdown list
            LoadAdminRolesDropDown(portalRoles);
            LoadNotificationRolesDropDown(portalRoles);
            LoadAuthorRoleDropDown(portalRoles);
            LoadDisplayTabDropDown();
            
            LoadCommentTypesDropDown();
            LoadThumbnailSelectionRadioButtonList();

            LoadDefaultCategoryDropDown();
        }

        private void LoadAdminRolesDropDown(IList<RoleInfo> portalRoles)
        {
            ddlRoles.DataSource = portalRoles;
            ddlRoles.DataBind();
        }

        private void LoadNotificationRolesDropDown(IList<RoleInfo> portalRoles)
        {
            ddlEmailRoles.DataSource = portalRoles;
            ddlEmailRoles.DataBind();
        }

        private void LoadAuthorRoleDropDown(IList<RoleInfo> portalRoles)
        {
            ddlAuthor.DataSource = portalRoles;
            ddlAuthor.DataBind();
        }

        private void LoadCommentTypesDropDown()
        {
            ddlCommentsType.Items.Clear();
            ddlCommentsType.Items.Add(new ListItem(Localization.GetString("PublishCommentType", LocalResourceFile), string.Empty));

            rowCommentsType.Visible = (new ModuleController()).GetModuleByDefinition(PortalId, Utility.ActiveForumsDefinitionModuleName) != null;
            if (rowCommentsType.Visible)
            {
                ddlCommentsType.Items.Add(new ListItem(Localization.GetString("ActiveForumsCommentType", LocalResourceFile), "Engage.Dnn.Publish.Forum.ActiveForumsProvider"));
            }
        }

        private void LoadDisplayTabDropDown()
        {
            var modules = new[] { Utility.DnnFriendlyModuleName };
            DataTable dt = Utility.GetDisplayTabIds(modules);

            ddlDefaultDisplay.Items.Insert(0, new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1"));

            foreach (DataRow dr in dt.Rows)
            {
                var li = new ListItem(dr["TabName"] + " (" + dr["TabID"] + ")", dr["TabID"].ToString());
                ddlDefaultDisplay.Items.Add(li);
            }
        }


        private void LoadDefaultCategoryDropDown()
        {
            ItemRelationship.DisplayCategoryHierarchy(ddlDefaultCategory, -1, PortalId, false);
            var li = new ListItem(Localization.GetString("ChooseOne", LocalSharedResourceFile), "-1");
            ddlDefaultCategory.Items.Insert(0, li);

        }



        private void LocalizeCollapsePanels()
        {

            string expandedImage = ApplicationUrl + Localization.GetString("ExpandedImage.Text", LocalSharedResourceFile).Replace("[L]", "");
            string collapsedImage = ApplicationUrl + Localization.GetString("CollapsedImage.Text", LocalSharedResourceFile).Replace("[L]", "");

            //clpTagSettings.CollapsedText = Localization.GetString("clpTagSettings.CollapsedText", LocalResourceFile);
            //clpTagSettings.ExpandedText = Localization.GetString("clpTagSettings.ExpandedText", LocalResourceFile);
            //clpTagSettings.ExpandedImage = expandedImage;
            //clpTagSettings.CollapsedImage=collapsedImage;

            //clpCommunity.CollapsedText = Localization.GetString("clpCommunity.CollapsedText", LocalResourceFile);
            //clpCommunity.ExpandedText = Localization.GetString("clpCommunity.ExpandedText", LocalResourceFile);
            //clpCommunity.ExpandedImage = expandedImage;
            //clpCommunity.CollapsedImage=collapsedImage;


            //clpArticleEditDefaults.CollapsedText = Localization.GetString("clpArticleEditDefaults.CollapsedText", LocalResourceFile);
            //clpArticleEditDefaults.ExpandedText = Localization.GetString("clpArticleEditDefaults.ExpandedText", LocalResourceFile);
            //clpArticleEditDefaults.ExpandedImage = expandedImage;
            //clpArticleEditDefaults.CollapsedImage = collapsedImage;

            //clpAdminEdit.CollapsedText = Localization.GetString("clpAdminEdit.CollapsedText", LocalResourceFile);
            //clpAdminEdit.ExpandedText = Localization.GetString("clpAdminEdit.ExpandedText", LocalResourceFile);
            //clpAdminEdit.ExpandedImage = expandedImage;
            //clpAdminEdit.CollapsedImage = collapsedImage;

            //clpAddOns.CollapsedText = Localization.GetString("clpAddOns.CollapsedText", LocalResourceFile);
            //clpAddOns.ExpandedText = Localization.GetString("clpAddOns.ExpandedText", LocalResourceFile);
            //clpAddOns.ExpandedImage = expandedImage;
            //clpAddOns.CollapsedImage = collapsedImage;


        }

        protected void chkAllowRichTextDescriptions_CheckedChanged(object sender, EventArgs e)
        {
            DefaultRichTextDescriptions();
        }

        private new void DefaultRichTextDescriptions()
        {
            if (chkAllowRichTextDescriptions.Checked)
            {
                chkDefaultRichTextDescriptions.Visible = true;
                plDefaultRichTextDescriptions.Visible = true;
            }
            else
            {
                chkDefaultRichTextDescriptions.Visible = false;
                plDefaultRichTextDescriptions.Visible = false;
            }
        }

    }
}

