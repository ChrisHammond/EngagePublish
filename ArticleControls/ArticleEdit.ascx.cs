//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Tabs;

namespace Engage.Dnn.Publish.ArticleControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;
    using Controls;
    using Data;
    using Tags;
    using Util;


    public partial class ArticleEdit : ModuleBase
    {
        //the relationship controls don't have ID's set in the code below because they will fail if you set them all to the same id.
        //The local resource file for each of the relationship controls is set in the code below.

        #region Controls
        protected TextEditor TeArticleText;
        private ItemRelationships parentCategoryRelationship; // item parent category
        private ItemRelationships relatedCategoryRelationships; // item related categories
        //private ItemRelationships relatedArticlesRelationships; //related articles // CJH 5/30/2018 removed for non-use
        //private ItemRelationships embeddedArticlesRelationships; //article links // CJH 5/30/2018 removed for non-use
        private ItemApproval itemApprovalStatus; //item approval status
        private ItemEdit itemEditControl; //item edit control
        private TagEntry tagEntryControl; //tag entry control

        #endregion

        #region Private Const
        private readonly string ItemrelationshipResourceFile = "~" + DesktopModuleFolderName + "Controls/App_LocalResources/ItemRelationships";
        private const string ApprovalControlToLoad = "../controls/ItemApproval.ascx";
        private const string ItemControlToLoad = "../Controls/itemEdit.ascx";

        private const string TagControlToLoad = "../Tags/TagEntry.ascx";
        #endregion

        #region Properties
        private int ParentId
        {
            get
            {
                string s = Request.QueryString["parentid"];
                return (s == null ? -1 : Convert.ToInt32(s, CultureInfo.InvariantCulture));
            }
        }
        #endregion

        #region Event Handlers
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            LoadControlType();
            base.OnInit(e);
            LoadSharedResources();
            //ctlUrlSelection.Url = 
            ItemVersionSetting attachmentSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "ArticleSettings", "ArticleAttachment", PortalId);
            if (attachmentSetting != null)
            {
                ctlUrlSelection.Url = attachmentSetting.PropertyValue;
            }
        }

        private void LoadSharedResources()
        {
            lblPublishOverrideable.Text = Localization.GetString("lblPublishOverrideable.Text", LocalSharedResourceFile);
        }

        private void InitializeComponent()
        {
            cmdUpdate.Click += CmdUpdateClick;
            cmdCancel.Click += CmdCancelClick;
            Load += Page_Load;
            PreRender += Page_PreRender;
        }

        private void LoadControlType()
        {

            UseCache = false;
            if (ItemVersionId == -1)
            {
                BindItemData(true);
                //trArticleId.Visible = false;
                cmdDelete.Visible = false;
            }
            else
            {
                BindItemData();
                cmdDelete.Visible = IsAdmin;
            }

            var av = (Article)VersionInfoObject;

            //Item Edit
            itemEditControl = (ItemEdit)LoadControl(ItemControlToLoad);
            itemEditControl.ModuleConfiguration = ModuleConfiguration;
            itemEditControl.ID = Path.GetFileNameWithoutExtension(ItemControlToLoad);
            itemEditControl.VersionInfoObject = VersionInfoObject;
            phControls.Controls.Add(itemEditControl);

            //Article Text Editor
            TeArticleText = (TextEditor)LoadControl("~/controls/TextEditor.ascx");
            TeArticleText.HtmlEncode = false;
            TeArticleText.TextRenderMode = "Raw";
            //TeArticleText.Width = ArticleEditWidth; //default values for the editor
            TeArticleText.Height = ArticleEditHeight; //default values for the editor
            TeArticleText.ChooseMode = true;
            phArticleText.Controls.Add(TeArticleText);
            TeArticleText.Text = av.ArticleText;

            //Parent Category Relationship
            parentCategoryRelationship = (ItemRelationships)LoadControl("../controls/ItemRelationships.ascx");
            parentCategoryRelationship.ModuleConfiguration = ModuleConfiguration;

            parentCategoryRelationship.LocalResourceFile = ItemrelationshipResourceFile;
            parentCategoryRelationship.VersionInfoObject = VersionInfoObject;
            parentCategoryRelationship.ListRelationshipTypeId = Util.RelationshipType.ItemToParentCategory.GetId();
            parentCategoryRelationship.CreateRelationshipTypeId = Util.RelationshipType.ItemToParentCategory.GetId();
            parentCategoryRelationship.AvailableSelectionMode = ListSelectionMode.Single;
            parentCategoryRelationship.IsRequired = true;
            parentCategoryRelationship.FlatView = true;
            parentCategoryRelationship.ItemTypeId = ItemType.Category.GetId();
            phParentCategory.Controls.Add(parentCategoryRelationship);

            //Related Category Relationship
            relatedCategoryRelationships = (ItemRelationships)LoadControl("../controls/ItemRelationships.ascx");
            relatedCategoryRelationships.ModuleConfiguration = ModuleConfiguration;
            relatedCategoryRelationships.LocalResourceFile = ItemrelationshipResourceFile;
            relatedCategoryRelationships.VersionInfoObject = VersionInfoObject;
            relatedCategoryRelationships.ListRelationshipTypeId = Util.RelationshipType.ItemToParentCategory.GetId();
            relatedCategoryRelationships.CreateRelationshipTypeId = Util.RelationshipType.ItemToRelatedCategory.GetId();
            relatedCategoryRelationships.AvailableSelectionMode = ListSelectionMode.Multiple;
            relatedCategoryRelationships.IsRequired = false;
            relatedCategoryRelationships.FlatView = true;
            relatedCategoryRelationships.ItemTypeId = ItemType.Category.GetId();
            phRelatedCategories.Controls.Add(relatedCategoryRelationships);


            //load approval status
            itemApprovalStatus = (ItemApproval)LoadControl(ApprovalControlToLoad);
            itemApprovalStatus.ModuleConfiguration = ModuleConfiguration;
            itemApprovalStatus.ID = Path.GetFileNameWithoutExtension(ApprovalControlToLoad);
            itemApprovalStatus.VersionInfoObject = VersionInfoObject;
            phApproval.Controls.Add(itemApprovalStatus);


            if (AllowTags)
            {
                var tagList = new StringBuilder(255);
                foreach (ItemTag it in VersionInfoObject.Tags)
                {
                    tagList.Append(Tag.GetTag(it.TagId, PortalId).Name);
                    tagList.Append(";");
                }

                tagEntryControl = (TagEntry)LoadControl(TagControlToLoad);
                tagEntryControl.ModuleConfiguration = ModuleConfiguration;
                tagEntryControl.ID = Path.GetFileNameWithoutExtension(TagControlToLoad);


                tagEntryControl.TagList = tagList.ToString();
                phTagEntry.Controls.Add(tagEntryControl);

            }
            else
            {
            }
        }

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                LocalizeCollapsePanels();

                DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteConfirm", LocalResourceFile));
                var av = (Article)VersionInfoObject;
                if (!Page.IsPostBack)
                {
                    //check to see if we're dealing with a new Item, if so set the parentid based on the querystring
                    if (av.IsNew)
                    {
                        if (ParentId != -1)
                        {
                            Category parent = Category.GetCategory(ParentId, PortalId);// = null;
                            parentCategoryRelationship.AddToSelectedItems(parent);
                        }
                    }

                    //trArticleId.Visible = ShowItemIds;

                    txtArticleId.Text = VersionInfoObject.ItemId.ToString(CultureInfo.CurrentCulture) == "-1" ? Localization.GetString("NewArticle", LocalResourceFile) : VersionInfoObject.ItemId.ToString(CultureInfo.CurrentCulture);
                    txtVersionNumber.Text = av.VersionNumber;
                    TeArticleText.Text = av.ArticleText;
                    txtPreviousVersionDescription.Text = av.VersionDescription;

                    rblDisplayOnCurrentPage.Items.Add(new ListItem(Localization.GetString("CurrentPage", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
                    rblDisplayOnCurrentPage.Items.Add(new ListItem(Localization.GetString("SpecificPage", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));

                    //get the pnlPrinterFriendly setting
                    ItemVersionSetting pfSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "pnlPrinterFriendly", "Visible", PortalId);
                    if (pfSetting != null)
                    {
                        chkPrinterFriendly.Checked = Convert.ToBoolean(pfSetting.PropertyValue, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string hostPrinterFriendlySetting = HostController.Instance.GetString(Utility.PublishDefaultPrinterFriendly + PortalId.ToString(CultureInfo.InvariantCulture));
                        chkPrinterFriendly.Checked = !Utility.HasValue(hostPrinterFriendlySetting) || Convert.ToBoolean(hostPrinterFriendlySetting, CultureInfo.InvariantCulture);
                    }

                    //get the pnlEmailAFriend setting
                    ItemVersionSetting efSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "pnlEmailAFriend", "Visible", PortalId);
                    if (efSetting != null)
                    {
                        chkEmailAFriend.Checked = Convert.ToBoolean(efSetting.PropertyValue, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string hostEmailFriendSetting = HostController.Instance.GetString(Utility.PublishDefaultEmailAFriend + PortalId.ToString(CultureInfo.InvariantCulture));
                        chkEmailAFriend.Checked = !Utility.HasValue(hostEmailFriendSetting) || Convert.ToBoolean(hostEmailFriendSetting, CultureInfo.InvariantCulture);
                    }


                    //if ratings are enabled show options
                    if (AreRatingsEnabled)
                    {
                        //get the upnlRating setting
                        ItemVersionSetting rtSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "upnlRating", "Visible", PortalId);
                        if (rtSetting != null)
                        {
                            chkRatings.Checked = Convert.ToBoolean(rtSetting.PropertyValue, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            string hostRatingSetting = HostController.Instance.GetString(Utility.PublishDefaultRatings + PortalId.ToString(CultureInfo.InvariantCulture));
                            chkRatings.Checked = !Utility.HasValue(hostRatingSetting) || Convert.ToBoolean(hostRatingSetting, CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        chkRatings.Visible = false;
                    }

                    //if comments are enabled show options.
                    if (IsCommentsEnabled)
                    {
                        //get the pnlComments setting
                        ItemVersionSetting ctSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "pnlComments", "Visible", PortalId);
                        if (ctSetting != null)
                        {
                            chkComments.Checked = Convert.ToBoolean(ctSetting.PropertyValue, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            string hostCommentSetting = HostController.Instance.GetString(Utility.PublishDefaultComments + PortalId.ToString(CultureInfo.InvariantCulture));
                            chkComments.Checked = !Utility.HasValue(hostCommentSetting) || Convert.ToBoolean(hostCommentSetting, CultureInfo.InvariantCulture);
                        }

                        if (IsPublishCommentType)
                        {
                            chkForumComments.Visible = false;
                        }
                        else
                        {
                            ItemVersionSetting forumCommentSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "chkForumComments", "Checked", PortalId);
                            chkForumComments.Checked = forumCommentSetting == null || Convert.ToBoolean(forumCommentSetting.PropertyValue, CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        chkComments.Visible = false;
                        chkForumComments.Visible = false;
                    }


                    //chkShowAuthor
                    ItemVersionSetting auSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "pnlAuthor", "Visible", PortalId);
                    if (auSetting != null)
                    {
                        chkShowAuthor.Checked = Convert.ToBoolean(auSetting.PropertyValue, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string hostAuthorSetting = HostController.Instance.GetString(Utility.PublishDefaultShowAuthor + PortalId.ToString(CultureInfo.InvariantCulture));
                        chkShowAuthor.Checked = Utility.HasValue(hostAuthorSetting) && Convert.ToBoolean(hostAuthorSetting, CultureInfo.InvariantCulture);
                    }

                    //chkShowTags
                    ItemVersionSetting tagSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "pnlTags", "Visible", PortalId);
                    if (tagSetting != null)
                    {
                        chkTags.Checked = Convert.ToBoolean(tagSetting.PropertyValue, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string hostTagsSetting = HostController.Instance.GetString(Utility.PublishDefaultShowTags + PortalId.ToString(CultureInfo.InvariantCulture));
                        chkTags.Checked = Utility.HasValue(hostTagsSetting) && Convert.ToBoolean(hostTagsSetting, CultureInfo.InvariantCulture);
                    }

                    //chkDisplayOnCurrentPage
                    ItemVersionSetting cpSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "ArticleSettings", "DisplayOnCurrentPage", PortalId);
                    if (cpSetting != null)
                    {
                        rblDisplayOnCurrentPage.SelectedValue = av.DisplayOnCurrentPage().ToString(CultureInfo.InvariantCulture);
                        if (av.DisplayOnCurrentPage())
                        {
                            chkForceDisplayTab.Checked = false;
                            chkForceDisplayTab.Visible = false;
                            lblForceDisplayTab.Visible = false;
                            ddlDisplayTabId.Enabled = false;
                        }
                        else
                        {
                            chkForceDisplayTab.Visible = true;
                            lblForceDisplayTab.Visible = true;
                            ddlDisplayTabId.Enabled = true;
                        }
                    }
                    else if (av.DisplayTabId < 0)
                    {
                        rblDisplayOnCurrentPage.SelectedValue = false.ToString(CultureInfo.InvariantCulture);
                        chkForceDisplayTab.Checked = false;
                        chkForceDisplayTab.Visible = true;
                        lblForceDisplayTab.Visible = true;
                        ddlDisplayTabId.Enabled = true;
                    }
                    else
                    {
                        rblDisplayOnCurrentPage.SelectedValue = false.ToString(CultureInfo.InvariantCulture);
                        chkForceDisplayTab.Visible = true;
                        lblForceDisplayTab.Visible = true;
                        ddlDisplayTabId.Enabled = true;
                    }

                    chkForceDisplayTab.Checked = av.ForceDisplayOnPage();

                    ItemVersionSetting rlSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "ArticleSettings", "DisplayReturnToList", PortalId);
                    if (rlSetting != null)
                    {
                        chkReturnList.Checked = Convert.ToBoolean(rlSetting.PropertyValue, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        string hostReturnToListSetting = HostController.Instance.GetString(Utility.PublishDefaultReturnToList + PortalId.ToString(CultureInfo.InvariantCulture));
                        chkReturnList.Checked = Utility.HasValue(hostReturnToListSetting) && Convert.ToBoolean(hostReturnToListSetting, CultureInfo.InvariantCulture);
                    }

                    //use approvals setting
                    ItemVersionSetting useApprovals = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "chkUseApprovals", "Checked", PortalId);
                    chkUseApprovals.Checked = useApprovals == null || Convert.ToBoolean(useApprovals.PropertyValue, CultureInfo.InvariantCulture);
                    chkUseApprovals.Visible = IsAdmin && UseApprovals;
                    phApproval.Visible = chkUseApprovals.Checked && UseApprovals;
                    lblNotUsingApprovals.Visible = !chkUseApprovals.Checked || !UseApprovals;
                    lblNotUsingApprovals.Text = Localization.GetString("ApprovalsDisabled", LocalSharedResourceFile);

                    //LoadPhotoGalleryDropDown(av);
                    LoadDisplayTabDropDown();

                    //load the article attachement settings


                }
                else
                {
                    if (ddlDisplayTabId.Enabled)
                    {
                        av.DisplayTabId = Convert.ToInt32(ddlDisplayTabId.SelectedValue, CultureInfo.InvariantCulture);
                    }

                    av.VersionNumber = txtVersionNumber.Text;
                    av.VersionDescription = txtVersionDescription.Text;
                    av.ArticleText = TeArticleText.Text;
                }

                //load the article attachement settings

                //ctlUrlSelection.Url = 

            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void Page_PreRender(object sender, EventArgs e)
        {

        }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        //protected void chkForceDisplayTab_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (chkForceDisplayTab.Checked)
        //    {
        //        rblDisplayOnCurrentPage.SelectedValue = false.ToString(CultureInfo.InvariantCulture);
        //        //populate the list of display pages with all pages configured for Publish, even if they aren't overrideable.
        //    }
        //    LoadDisplayTabDropDown();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void rblDisplayOnCurrentPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if display on current page is selected
            if (Convert.ToBoolean(rblDisplayOnCurrentPage.SelectedValue, CultureInfo.InvariantCulture))
            {
                chkForceDisplayTab.Visible = false;
                lblForceDisplayTab.Visible = false;
                chkForceDisplayTab.Checked = false;
                ddlDisplayTabId.Enabled = false;
            }
            else //if display on specific page is selected
            {
                chkForceDisplayTab.Visible = true;
                lblForceDisplayTab.Visible = true;
                ddlDisplayTabId.Enabled = true;
            }
            LoadDisplayTabDropDown();
        }

        private void CmdCancelClick(object sender, EventArgs e)
        {
            string returnUrl = Server.UrlDecode(Request.QueryString["returnUrl"]);
            if (!Utility.HasValue(returnUrl))
            {
                Response.Redirect(BuildCategoryListUrl(ItemType.Article), true);
            }
            else
            {
                Response.Redirect(returnUrl, true);
            }
        }

        private void CmdUpdateClick(object sender, EventArgs e)
        {
            try
            {
                var av = (Article)VersionInfoObject;

                txtMessage.Text = string.Empty;
                bool error = false;

                //Removed the check for the Item Description length as we no longer have a restriction on this

                //if (TextBoxMaxLengthExceeded(this.itemEditControl.DescriptionText, "Article Description", 4000))
                //{
                //    error = true;
                //}

                if (TextBoxMaxLengthExceeded(txtVersionDescription.Text, "Version Description", 8000))
                {
                    error = true;
                }

                if (!parentCategoryRelationship.IsValid)
                {
                    error = true;
                    txtMessage.Text += Localization.GetString("ErrorSelectCategory.Text", LocalResourceFile);
                }

                if (!itemApprovalStatus.IsValid && itemApprovalStatus.Visible)
                {
                    error = true;
                    txtMessage.Text += Localization.GetString("ErrorApprovalStatus.Text", LocalResourceFile);
                }

                if (!itemEditControl.IsValid)
                {
                    error = true;
                    txtMessage.Text += itemEditControl.ErrorMessage;
                }

                if (Convert.ToInt32(ddlDisplayTabId.SelectedValue, CultureInfo.InvariantCulture) > -1)
                {
                    VersionInfoObject.DisplayTabId = Convert.ToInt32(ddlDisplayTabId.SelectedValue, CultureInfo.InvariantCulture);
                }
                else
                {
                    error = true;
                    txtMessage.Text += Localization.GetString("ErrorDisplayPage.Text", LocalResourceFile);
                }

                if (error)
                {
                    txtMessage.Visible = true;
                    return;
                }

                av.ArticleText = TeArticleText.Text;
                av.VersionDescription = txtVersionDescription.Text;
                av.VersionNumber = txtVersionNumber.Text;
                av.Description = itemEditControl.DescriptionText;
                //we need to look at making moduleid be configurable at anytime, not just on item creation, this makes previewing items impossible
                //if (av.IsNew)
                int newModuleId = Utility.GetModuleIdFromDisplayTabId(VersionInfoObject.DisplayTabId, PortalId, Utility.DnnFriendlyModuleName);
                if (newModuleId > 0)
                {
                    VersionInfoObject.ModuleId = newModuleId;
                }


                //create a relationship
                var irel = new ItemRelationship { RelationshipTypeId = Util.RelationshipType.ItemToParentCategory.GetId() };
                int[] ids = parentCategoryRelationship.GetSelectedItemIds();

                //check for parent category, if none then add a relationship for Top Level Item
                if (ids.Length > 0)
                {
                    irel.ParentItemId = ids[0];
                    VersionInfoObject.Relationships.Add(irel);
                }

                foreach (int i in relatedCategoryRelationships.GetSelectedItemIds())
                {
                    var irco = new ItemRelationship
                                   {
                                       RelationshipTypeId = Util.RelationshipType.ItemToRelatedCategory.GetId(),
                                       ParentItemId = i
                                   };
                    av.Relationships.Add(irco);
                }

                if (AllowTags)
                {
                    av.Tags.Clear();
                    //Add the tags to the ItemTagCollection
                    foreach (Tag t in Tag.ParseTags(tagEntryControl.TagList, PortalId))
                    {
                        ItemTag it = ItemTag.Create();
                        it.TagId = Convert.ToInt32(t.TagId, CultureInfo.InvariantCulture);
                        av.Tags.Add(it);
                    }
                }


                if (av.Description == string.Empty)
                {
                    //trim article text to populate description

                    if (!Utility.HasValue(av.Description) || !Utility.HasValue(av.MetaDescription))
                    {
                        string description = DotNetNuke.Common.Utilities.HtmlUtils.StripTags(av.ArticleText, false);
                        if (!Utility.HasValue(av.Description))
                            av.Description = Utility.TrimDescription(3997, description) + "...";// description + "...";
                    }
                }

                //auto populate the meta description if it's not populated already
                if (!Utility.HasValue(av.MetaDescription))
                {
                    string description = DotNetNuke.Common.Utilities.HtmlUtils.StripTags(av.Description, false);
                    av.MetaDescription = Utility.TrimDescription(399, description);
                }

                //Save the ItemVersionSettings
                SaveSettings();

                //approval status
                av.ApprovalStatusId = chkUseApprovals.Checked && UseApprovals ? itemApprovalStatus.ApprovalStatusId : ApprovalStatus.Approved.GetId();

                VersionInfoObject.Save(UserId);

                string returnUrl = Server.UrlDecode(Request.QueryString["returnUrl"]);
                if (!Utility.HasValue(returnUrl))
                {
                    Response.Redirect(Globals.NavigateURL(TabId, "", "", "ctl=" + Utility.AdminContainer, "mid=" + ModuleId.ToString(CultureInfo.InvariantCulture),
                        "adminType=itemCreated", "itemId=" + VersionInfoObject.ItemId.ToString(CultureInfo.InvariantCulture)), true);
                }
                else
                {
                    Response.Redirect(returnUrl);
                }
            }

            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            txtMessage.Visible = true;
            bool itemExists = false;

            if (ItemId > -1)
            {
                //Using GetItemTypeId as substitute for IfExists
                if (Item.GetItemTypeId(ItemId, PortalId) > -1)
                {
                    itemExists = true;
                    bool inUse = false;
                    var modulesInUse = new StringBuilder();
                    var mc = new ModuleController();
                    ArrayList modules = mc.GetModulesByDefinition(PortalId, Utility.DnnFriendlyModuleName);

                    foreach (ModuleInfo module in modules)
                    {
                        Hashtable settings = module.TabModuleSettings;

                        if (settings.ContainsKey("DisplayType") && settings["DisplayType"].ToString() == "ArticleDisplay")
                        {
                            int articleId;
                            if (settings.ContainsKey("adArticleId") && int.TryParse(settings["adArticleId"].ToString(), out articleId))
                            {
                                if (articleId == ItemId)
                                {
                                    inUse = true;
                                    modulesInUse.AppendFormat("{0} ({1}){2}", module.ModuleTitle, module.TabID, Environment.NewLine);
                                    break;
                                }
                            }
                        }
                    }

                    ArrayList featuredRelationships = ItemRelationship.GetItemChildRelationships(ItemId, Util.RelationshipType.ItemToFeaturedItem.GetId());
                    bool isFeatured = featuredRelationships.Count > 0;

                    if (!inUse && !isFeatured)
                    {
                        //Item.DeleteItem(ItemId);
                        Item.DeleteItem(ItemId, PortalId);
                        txtMessage.Text = Localization.GetString("DeleteSuccess", LocalResourceFile);

                    }
                    else
                    {
                        var errorMessage = new StringBuilder();

                        if (inUse)
                        {
                            errorMessage.AppendFormat("{0}{1}", Localization.GetString("DeleteFailureInUse", LocalResourceFile), Environment.NewLine);
                            errorMessage.Append(modulesInUse.ToString());
                        }
                        if (isFeatured)
                        {
                            errorMessage.AppendFormat("{0}{1}", Localization.GetString("DeleteFailureIsFeatured", LocalResourceFile), Environment.NewLine);

                            foreach (ItemRelationship rel in featuredRelationships)
                            {
                                Category parentCategory = Category.GetCategory(rel.ChildItemId, PortalId);
                                errorMessage.AppendFormat("{0}{1}", parentCategory.Name, Environment.NewLine);
                            }
                        }
                        txtMessage.Text = errorMessage.ToString();
                    }
                }
            }
            if (!itemExists)
            {
                txtMessage.Text = Localization.GetString("DeleteFailure", LocalResourceFile);
            }
            ShowOnlyMessage();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void chkUseApprovals_CheckedChanged(object sender, EventArgs e)
        {
            phApproval.Visible = chkUseApprovals.Checked && UseApprovals;
            lblNotUsingApprovals.Visible = !chkUseApprovals.Checked || !UseApprovals;
        }
        #endregion

        private void LoadDisplayTabDropDown()
        {
            ddlDisplayTabId.Items.Clear();

            var modules = new[] { Utility.DnnFriendlyModuleName };
            //we're going to get all pages no matter if they have a Publish module on them or not. We'll only highlight Overrideable ones later
            //if (chkForceDisplayTab.Checked)
            //{
            //    //if the ForceDisplayTab is checked we need to make sure we get ALL publish modules, not just overrideable ones
            //    dt = Utility.GetDisplayTabIdsAll(modules);
            //}
            //else
            //{
            //    dt = Utility.GetDisplayTabIds(modules);
            //    if (dt.Rows.Count < 1)
            //    {
            //        //if there are no items in the list, meaning there are no modules set to be overrideable, then get the list of all Publish pages.
            //        dt = Utility.GetDisplayTabIdsAll(modules);
            //    }
            //}
            DataTable dt = Utility.GetDisplayTabIds(modules);

            //this.ddlDisplayTabId.Items.Insert(0, new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1"));

            ddlDisplayTabId.DataSource = TabController.GetPortalTabs(PortalId, 0, false, true);
            ddlDisplayTabId.DataBind();
            foreach (DataRow dr in dt.Rows)
            {
                if (ddlDisplayTabId.Items.FindByValue(dr["TabID"].ToString()) != null)
                    ddlDisplayTabId.Items.FindByValue(dr["TabID"].ToString()).Text += Localization.GetString("PublishOverrideable", LocalSharedResourceFile);
                //    ListItem li = new ListItem(dr["TabName"] + " (" + dr["TabID"] + ")", dr["TabID"].ToString());
                //    this.ddlDisplayTabId.Items.Add(li);
            }

            //check if the DisplayTabId should be set.
            var av = (Article)VersionInfoObject;
            if (!VersionInfoObject.IsNew)
            {
                ListItem li = ddlDisplayTabId.Items.FindByValue(av.DisplayTabId.ToString(CultureInfo.InvariantCulture));
                if (li != null)
                {
                    ddlDisplayTabId.ClearSelection();
                    li.Selected = true;
                }
                else
                {
                    //if we made it here we've hit an article who is pointing to a page that is no longer overrideable, set the default page.
                    if (DefaultDisplayTabId > 0)
                    {
                        li = ddlDisplayTabId.Items.FindByValue(DefaultDisplayTabId.ToString(CultureInfo.InvariantCulture));
                        if (li != null)
                        {
                            ddlDisplayTabId.ClearSelection();
                            li.Selected = true;
                        }
                    }
                }
            }
            else
            {
                Category parent = null;
                if (ParentId != -1)
                {
                    parent = Category.GetCategory(ParentId, PortalId);
                }

                //look for display tab id
                if (parent != null && parent.ChildDisplayTabId > 0)
                {
                    if (ddlDisplayTabId.Items.FindByValue(parent.ChildDisplayTabId.ToString(CultureInfo.InvariantCulture)) != null)
                    {
                        ddlDisplayTabId.SelectedIndex = -1;
                        ddlDisplayTabId.Items.FindByValue(parent.ChildDisplayTabId.ToString(CultureInfo.InvariantCulture)).Selected = true;
                    }
                }

                else
                {
                    //load the default display tab
                    ListItem li = ddlDisplayTabId.Items.FindByValue(DefaultDisplayTabId.ToString(CultureInfo.InvariantCulture));
                    if (li != null)
                    {
                        ddlDisplayTabId.ClearSelection();
                        li.Selected = true;
                    }
                }
            }
        }


        private bool TextBoxMaxLengthExceeded(string text, string controlName, int length)
        {
            bool b = (text.Length > length);
            if (b)
            {
                txtMessage.Text += String.Format(CultureInfo.CurrentCulture, Localization.GetString("MaximumLength", LocalResourceFile), controlName, length.ToString(CultureInfo.CurrentCulture), text.Length);
                txtMessage.Visible = true;
            }

            return b;
        }

        private void ShowOnlyMessage()
        {
            foreach (Control cntl in Controls)
            {
                cntl.Visible = false;
            }

            txtMessage.Visible = true;
            txtMessage.Parent.Visible = true;
        }

        private void SaveSettings()
        {
            var av = (Article)VersionInfoObject;

            av.VersionSettings.Clear();
            //Printer Friendly
            Setting setting = Setting.PrinterFriendly;
            setting.PropertyValue = chkPrinterFriendly.Checked.ToString(CultureInfo.InvariantCulture);
            var itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //AuthorName setting
            setting = Setting.AuthorName;
            setting.PropertyValue = itemEditControl.AuthorName;
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);


            //Email A Friend
            setting = Setting.EmailAFriend;
            setting.PropertyValue = chkEmailAFriend.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //ratings
            setting = Setting.Rating;
            setting.PropertyValue = chkRatings.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //comments
            setting = Setting.Comments;
            setting.PropertyValue = chkComments.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //forum comments
            setting = Setting.ForumComments;
            setting.PropertyValue = chkForumComments.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //forum comments thread ID
            //just continue forward to the next version, this doesn't get set in the edit screen
            itemVersionSetting = ItemVersionSetting.GetItemVersionSetting(av.ItemVersionId, "ArticleSetting", "CommentForumThreadId", PortalId);
            if (itemVersionSetting != null)
            {
                av.VersionSettings.Add(itemVersionSetting);
            }

            //display on current page option
            setting = Setting.ArticleSettingCurrentDisplay;
            setting.PropertyValue = rblDisplayOnCurrentPage.SelectedValue;
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //force display on specific page
            setting = Setting.ArticleSettingForceDisplay;
            setting.PropertyValue = chkForceDisplayTab.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //display return to list
            setting = Setting.ArticleSettingReturnToList;
            setting.PropertyValue = chkReturnList.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //show author
            setting = Setting.Author;
            setting.PropertyValue = chkShowAuthor.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //show tags
            setting = Setting.ShowTags;
            setting.PropertyValue = chkTags.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);


            //use approvals
            setting = Setting.UseApprovals;
            setting.PropertyValue = chkUseApprovals.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

            //article attachment
            setting = Setting.ArticleAttachment;
            setting.PropertyValue = ctlUrlSelection.Url;
            itemVersionSetting = new ItemVersionSetting(setting);
            av.VersionSettings.Add(itemVersionSetting);

        }

        private void LocalizeCollapsePanels()
        {
            //clpExtended.CollapsedText = Localization.GetString("clpExtended.CollapsedText", LocalResourceFile);
            //clpExtended.ExpandedText = Localization.GetString("clpExtended.ExpandedText", LocalResourceFile);
            //clpExtended.ExpandedImage = ApplicationUrl + Localization.GetString("ExpandedImage.Text", LocalSharedResourceFile).Replace("[L]", "");
            //clpExtended.CollapsedImage = ApplicationUrl + Localization.GetString("CollapsedImage.Text", LocalSharedResourceFile).Replace("[L]", "");
        }

    }
}

