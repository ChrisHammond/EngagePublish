//Engage: Publish - http://www.engagemodules.com
//Copyright (c) 2004-2008
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using Engage.Dnn.Publish.Controls;
using Engage.Dnn.Publish.Forum;
using Engage.Dnn.Publish.Security;
using Engage.Dnn.Publish.Util;

namespace Engage.Dnn.Publish.CategoryControls
{
    public partial class CategoryEdit : ModuleBase, IActionable
    {
        #region Controls
        private ItemRelationships parentCategoryRelationships;
        //private ItemRelationships irRelated;
        private ItemRelationships featuredArticlesRelationships;
        private ItemEdit itemEditControl;
        private CategoryPermissions categoryPermissions;
        private ItemApproval itemApprovalStatus;
        #endregion

        #region Private Const
        private readonly string ItemRelationshipResourceFile = "~" + DesktopModuleFolderName + "Controls/App_LocalResources/ItemRelationships";
        private const string approvalControlToLoad = "../controls/ItemApproval.ascx";
        private const string itemControlToLoad = "../Controls/itemEdit.ascx";
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

        //[Obsolete("This is not used")]
        //private int TopLevelId
        //{
        //    get
        //    {
        //        string s = Request.QueryString["topLevelId"];
        //        return (s == null ? -1 : Convert.ToInt32(s, CultureInfo.InvariantCulture));
        //    }
        //}
        #endregion

        #region Event Handlers
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            //int tli = TopLevelId;
            LoadControlType();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
            this.cmdUpdate.Click += this.cmdUpdate_Click;
            this.cmdCancel.Click += this.cmdCancel_Click;
        }

        private void LoadControlType()
        {
            UseCache = false;
            if (ItemVersionId == -1)
            {
                BindItemData(true);
            }
            else
            {
                BindItemData();
            }

            //Item Edit
            itemEditControl = (ItemEdit)LoadControl(itemControlToLoad);
            itemEditControl.ModuleConfiguration = ModuleConfiguration;
            itemEditControl.ID = Path.GetFileNameWithoutExtension(itemControlToLoad);
            itemEditControl.VersionInfoObject = VersionInfoObject;
            this.phItemEdit.Controls.Add(itemEditControl);

            if (SecurityFilter.IsSecurityEnabled(PortalId))
            {
                trCategoryPermissions.Visible = true;

                this.categoryPermissions = (CategoryPermissions)LoadControl("../CategoryControls/CategoryPermissions.ascx");
                this.categoryPermissions.CategoryId = VersionInfoObject.ItemId;
                this.categoryPermissions.ModuleConfiguration = ModuleConfiguration;
                this.phCategoryPermissions.Controls.Add(this.categoryPermissions);
            }

            //Parent Category
            this.parentCategoryRelationships = (ItemRelationships)LoadControl("../controls/ItemRelationships.ascx");
            this.parentCategoryRelationships.ExcludeCircularRelationships = true;
            this.parentCategoryRelationships.ModuleConfiguration = ModuleConfiguration;
            this.parentCategoryRelationships.LocalResourceFile = ItemRelationshipResourceFile;
            this.parentCategoryRelationships.VersionInfoObject = VersionInfoObject;
            this.parentCategoryRelationships.ListRelationshipTypeId = RelationshipType.ItemToParentCategory.GetId();
            this.parentCategoryRelationships.CreateRelationshipTypeId = RelationshipType.ItemToParentCategory.GetId();
            this.parentCategoryRelationships.AvailableSelectionMode = ListSelectionMode.Single;
            this.parentCategoryRelationships.FlatView = true;
            this.parentCategoryRelationships.ItemTypeId = ItemType.Category.GetId();
            this.phParentCategory.Controls.Add(this.parentCategoryRelationships);


            //Related Categories
            //this.irRelated = (ItemRelationships)LoadControl("../controls/ItemRelationships.ascx");
            //this.irRelated.ModuleConfiguration = ModuleConfiguration;
            //this.irRelated.LocalResourceFile = ItemRelationshipResourceFile;
            //this.irRelated.VersionInfoObject = VersionInfoObject;
            //this.irRelated.ListRelationshipTypeId = RelationshipType.ItemToRelatedCategory.GetId();
            //this.irRelated.CreateRelationshipTypeId = RelationshipType.ItemToRelatedCategory.GetId();
            //this.irRelated.AvailableSelectionMode = ListSelectionMode.Multiple;
            //this.irRelated.FlatView = true;
            //this.irRelated.ItemTypeId = ItemType.Category.GetId();
            //this.phParentCategory.Controls.Add(this.irRelated);

            //Featured Articles
            this.featuredArticlesRelationships = (ItemRelationships)LoadControl("../controls/ItemRelationships.ascx");
            this.featuredArticlesRelationships.ModuleConfiguration = ModuleConfiguration;
            this.featuredArticlesRelationships.VersionInfoObject = VersionInfoObject;
            this.featuredArticlesRelationships.LocalResourceFile = ItemRelationshipResourceFile;
            this.featuredArticlesRelationships.ListRelationshipTypeId = RelationshipType.ItemToParentCategory.GetId();
            this.featuredArticlesRelationships.CreateRelationshipTypeId = RelationshipType.ItemToFeaturedItem.GetId();
            this.featuredArticlesRelationships.AvailableSelectionMode = ListSelectionMode.Multiple;
            this.featuredArticlesRelationships.FlatView = true;
            this.featuredArticlesRelationships.EnableDates = true;
            this.featuredArticlesRelationships.AllowSearch = true;
            this.featuredArticlesRelationships.EnableSortOrder = true;
            this.featuredArticlesRelationships.ItemTypeId = ItemType.Article.GetId();
            this.phFeaturedArticles.Controls.Add(this.featuredArticlesRelationships);

            //load approval status
            itemApprovalStatus = (ItemApproval)LoadControl(approvalControlToLoad);
            itemApprovalStatus.ModuleConfiguration = ModuleConfiguration;
            itemApprovalStatus.ID = System.IO.Path.GetFileNameWithoutExtension(approvalControlToLoad);
            itemApprovalStatus.VersionInfoObject = VersionInfoObject;
            this.phApproval.Controls.Add(itemApprovalStatus);
        }

        private void Page_Load(object sender, System.EventArgs e)
        {
            try
            {
                //because we're in the Edit options turn off caching
                DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteConfirm", LocalResourceFile));
                Category cv = (Category)VersionInfoObject;

                if (!Page.IsPostBack)
                {
                    this.rblDisplayOnCurrentPage.Items.Add(new ListItem(Localization.GetString("CurrentPage", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
                    this.rblDisplayOnCurrentPage.Items.Add(new ListItem(Localization.GetString("SpecificPage", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));

                    this.txtSortOrder.Text = cv.SortOrder.ToString(CultureInfo.CurrentCulture);
                    this.txtCategoryId.Text = cv.ItemId.ToString(CultureInfo.CurrentCulture);
                    this.cmdDelete.Visible = false;

                    //check if new or edit
                    if (VersionInfoObject.IsNew)
                    {
                        this.txtCategoryId.Visible = false;
                        this.lblCategoryId.Visible = false;

                        if (ParentId != -1)
                        {
                            Category parent = Category.GetCategory(ParentId, PortalId);
                            this.parentCategoryRelationships.AddToSelectedItems(parent);
                        }
                    }

                    trCategoryId.Visible = ShowItemIds;

                    //check if the DisplayTabId should be set.


                    //chkDisplayOnCurrentPage
                    ItemVersionSetting cpSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "CategorySettings", "DisplayOnCurrentPage");
                    if (cpSetting != null)
                    {
                        //TODO: cache display on currentpage setting somewhere?
                        rblDisplayOnCurrentPage.SelectedValue = VersionInfoObject.DisplayOnCurrentPage().ToString(CultureInfo.InvariantCulture);
                        if (VersionInfoObject.DisplayOnCurrentPage())
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
                    else if (VersionInfoObject.DisplayTabId < 0)
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

                    chkForceDisplayTab.Checked = VersionInfoObject.ForceDisplayOnPage();

                    LoadCommentForumsDropDown();
                    LoadCategoryDisplayTabDropDown();
                    LoadChildDisplayTabDropDown();

                    ItemVersionSetting useApprovals = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "chkUseApprovals", "Checked");
                    chkUseApprovals.Checked = useApprovals == null || Convert.ToBoolean(useApprovals.PropertyValue, CultureInfo.InvariantCulture);
                    chkUseApprovals.Visible = IsAdmin && UseApprovals;
                    phApproval.Visible = UseApprovals && chkUseApprovals.Checked;
                    lblNotUsingApprovals.Visible = !UseApprovals || !chkUseApprovals.Checked;
                }
                else
                {
                    cv.SortOrder = Convert.ToInt32(this.txtSortOrder.Text, CultureInfo.InvariantCulture);
                    if (ddlDisplayTabId.SelectedIndex > 0)
                    {
                        VersionInfoObject.DisplayTabId = Convert.ToInt32(ddlDisplayTabId.SelectedValue, CultureInfo.InvariantCulture);
                    }

                    if (ddlChildDisplayTabId.SelectedIndex > 0)
                    {
                        cv.ChildDisplayTabId = Convert.ToInt32(ddlChildDisplayTabId.SelectedValue, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        private void cmdUpdate_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.txtMessage.Text = string.Empty;
                bool error = false;

                //create a relationship
                ItemRelationship irel = new ItemRelationship();
                irel.RelationshipTypeId = RelationshipType.ItemToParentCategory.GetId();
                int[] ids = this.parentCategoryRelationships.GetSelectedItemIds();

                //check for parent category, if none then add a relationship for Top Level Item
                if (ids.Length == 0)
                {
                    //add relationship to TLC
                    irel.RelationshipTypeId = RelationshipType.CategoryToTopLevelCategory.GetId();
                    irel.ParentItemId = TopLevelCategoryItemType.Category.GetId();
                    VersionInfoObject.Relationships.Add(irel);
                }
                else
                {
                    irel.ParentItemId = ids[0];
                    VersionInfoObject.Relationships.Add(irel);
                }

                //check for parent category, if none then add a relationship for Top Level Item
                //foreach (int i in this.irRelated.GetSelectedItemIds())
                //{
                //    ItemRelationship irco = ItemRelationship.Create();
                //    irco.RelationshipTypeId = RelationshipType.ItemToRelatedCategory.GetId();
                //    irco.ParentItemId = i;
                //    VersionInfoObject.Relationships.Add(irco);
                //}

                if (this.itemEditControl.IsValid == false)
                {
                    error = true;
                    this.txtMessage.Text += this.itemEditControl.ErrorMessage;
                }

                if (Convert.ToInt32(ddlDisplayTabId.SelectedValue, CultureInfo.InvariantCulture) < -1)
                {
                    error = true;

                    this.txtMessage.Text += Localization.GetString("ChooseAPage", LocalResourceFile);
                }

                if (Convert.ToInt32(ddlChildDisplayTabId.SelectedValue, CultureInfo.InvariantCulture) == -1)
                {
                    error = true;
                    this.txtMessage.Text += Localization.GetString("ChooseChildPage", LocalResourceFile);
                }

                if (!this.itemApprovalStatus.IsValid)
                {
                    this.txtMessage.Text += Localization.GetString("ChooseApprovalStatus", LocalResourceFile);
                }

                if (error)
                {
                    this.txtMessage.Visible = true;
                    return;
                }

                VersionInfoObject.Description = itemEditControl.DescriptionText;
                if (VersionInfoObject.IsNew) VersionInfoObject.ModuleId = ModuleId;

                int sortCount = 0;

                foreach (int i in this.featuredArticlesRelationships.GetSelectedItemIds())
                {
                    ItemRelationship irArticleso = new ItemRelationship();
                    irArticleso.RelationshipTypeId = RelationshipType.ItemToFeaturedItem.GetId();
                    irArticleso.ParentItemId = i;
                    
                    if (Utility.HasValue(this.featuredArticlesRelationships.GetAdditionalSetting("startDate", i.ToString(CultureInfo.InvariantCulture))))
                    {
                        irArticleso.StartDate = this.featuredArticlesRelationships.GetAdditionalSetting("startDate", i.ToString(CultureInfo.InvariantCulture));
                    }
                    if (Utility.HasValue(this.featuredArticlesRelationships.GetAdditionalSetting("endDate", i.ToString(CultureInfo.InvariantCulture))))
                    {
                        irArticleso.EndDate = this.featuredArticlesRelationships.GetAdditionalSetting("endDate", i.ToString(CultureInfo.InvariantCulture));
                    }
                    irArticleso.SortOrder = sortCount;

                    sortCount++;
                    VersionInfoObject.Relationships.Add(irArticleso);
                }

                SaveSettings();

                //approval status
                if (chkUseApprovals.Checked && UseApprovals)
                {
                    VersionInfoObject.ApprovalStatusId = itemApprovalStatus.ApprovalStatusId;
                }
                else
                {
                    VersionInfoObject.ApprovalStatusId = ApprovalStatus.Approved.GetId();
                }

                VersionInfoObject.Save(UserId);

                //Clear the cache for all Publish items
                Utility.ClearPublishCache(PortalId);

                if (SecurityFilter.IsSecurityEnabled(PortalId))
                {
                    this.categoryPermissions.CategoryId = VersionInfoObject.ItemId;
                    this.categoryPermissions.Save();
                }


                string returnUrl = Server.UrlDecode(Request.QueryString["returnUrl"]);
                if (!Utility.HasValue(returnUrl))
                {
                    Response.Redirect(Globals.NavigateURL(TabId, "", "", "ctl=" + Utility.AdminContainer, "mid=" + ModuleId, "adminType=itemCreated",
                        "itemId=" + VersionInfoObject.ItemId), true);
                }
                else
                {
                    Response.Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private void cmdCancel_Click(object sender, System.EventArgs e)
        {
            string returnUrl = Server.UrlDecode(Request.QueryString["returnUrl"]);
            if (!Utility.HasValue(returnUrl))
            {
                Response.Redirect(BuildCategoryListUrl(ItemType.Category), true);
            }
            else
            {
                Response.Redirect(returnUrl, true);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            bool itemExists = false;
            txtMessage.Visible = true;

            if (ItemId > -1)
            {
                //Using GetItemTypeId as substitute for IfExists
                if (Item.GetItemTypeId(ItemId) > -1)
                {
                    itemExists = true;
                    DataSet children = ItemRelationship.GetAllChildren(ItemId, RelationshipType.ItemToParentCategory.GetId(), PortalId);
                    bool hasChildren = children.Tables.Count > 0 && children.Tables[0].Rows.Count > 0;

                    if (!hasChildren)
                    {
                        Item.DeleteItem(ItemId);
                        txtMessage.Text = Localization.GetString("DeleteSuccess", LocalResourceFile);
                        Util.Utility.ClearPublishCache(PortalId);
                    }
                    else
                    {
                        StringBuilder errorMessage = new StringBuilder();
                        errorMessage.AppendFormat("{0}{1}", Localization.GetString("DeleteFailureHasChildren", LocalResourceFile), Environment.NewLine);

                        foreach (DataRow row in children.Tables[0].Rows)
                        {
                            int itemId;// = 0;
                            if (int.TryParse(row["itemId"].ToString(), out itemId))
                            {
                                errorMessage.AppendFormat(CultureInfo.CurrentCulture, "{0} ({1}, id: {2}){3}", row["name"], Item.GetItemType(itemId), itemId, Environment.NewLine);
                            }
                            else
                            {
                                errorMessage.AppendFormat(CultureInfo.CurrentCulture, "{0} (id: {1}){2}", row["name"], row["itemId"], Environment.NewLine);
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
            LoadCategoryDisplayTabDropDown();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void chkForceDisplayTab_CheckedChanged(object sender, EventArgs e)
        {
            if (chkForceDisplayTab.Checked)
            {
                rblDisplayOnCurrentPage.SelectedValue = false.ToString(CultureInfo.InvariantCulture);
                //populate the list of display pages with all pages configured for Publish, even if they aren't overrideable.
            }
            LoadCategoryDisplayTabDropDown();
        }

        #endregion

        /// <summary>
        /// Hides all controls except for the message box.
        /// </summary>
        private void ShowOnlyMessage()
        {
            foreach (Control cntl in this.Controls)
            {
                cntl.Visible = false;
            }
            this.txtMessage.Visible = true;
            this.txtMessage.Parent.Visible = true;

        }

        private void LoadCommentForumsDropDown()
        {
            if (base.IsCommentsEnabled && !base.IsPublishCommentType)
            {
                ddlCommentForum.Items.Clear();
                foreach (KeyValuePair<int, string> pair in ForumProvider.GetInstance(PortalId).GetForums())
                {
                    ddlCommentForum.Items.Add(new ListItem(pair.Value, pair.Key.ToString(CultureInfo.InvariantCulture)));
                }
                ddlCommentForum.Items.Insert(0,(new ListItem(Localization.GetString("NoForum",LocalResourceFile),"-1")));

                ItemVersionSetting commentForumSetting = ItemVersionSetting.GetItemVersionSetting(VersionInfoObject.ItemVersionId, "CategorySettings", "CommentForumId");
                if (commentForumSetting != null)
                {
                    ddlCommentForum.SelectedValue = commentForumSetting.PropertyValue;
                }
            }
            else
            {
                rowCommentForum.Visible = false;
            }
        }

        private void LoadCategoryDisplayTabDropDown()
        {
            ddlDisplayTabId.Items.Clear();

            string[] modules = new string[] { "Engage: Publish" };
            DataTable dt = Utility.GetDisplayTabIds(modules);

            ListItem l = new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1");
            this.ddlDisplayTabId.Items.Insert(0, l);

            foreach (DataRow dr in dt.Rows)
            {
                ListItem li = new ListItem(dr["TabName"] + " (" + dr["TabID"] + ")", dr["TabID"].ToString());
                this.ddlDisplayTabId.Items.Add(li);
            }

            if (!VersionInfoObject.IsNew)
            {
                ListItem li = ddlDisplayTabId.Items.FindByValue(VersionInfoObject.DisplayTabId.ToString(CultureInfo.InvariantCulture));
                if (li != null)
                {
                    ddlDisplayTabId.ClearSelection();
                    li.Selected = true;
                }
            }
            else
            {
                Category parent = null;
                if (ParentId != -1)
                {
                    parent = Category.GetCategory(ParentId, PortalId);
                    this.parentCategoryRelationships.AddToSelectedItems(parent);
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

        private void LoadChildDisplayTabDropDown()
        {

            ddlChildDisplayTabId.Items.Clear();
            Category cv = (Category)VersionInfoObject;
            string[] modules = new string[] { "Engage: Publish" };
            DataTable dt = Utility.GetDisplayTabIds(modules);

            ListItem l = new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1");
            this.ddlChildDisplayTabId.Items.Insert(0, l);

            foreach (DataRow dr in dt.Rows)
            {
                ListItem li = new ListItem(dr["TabName"] + " (" + dr["TabID"] + ")", dr["TabID"].ToString());
                this.ddlChildDisplayTabId.Items.Add(li);
            }

            ListItem child = ddlChildDisplayTabId.Items.FindByValue(cv.ChildDisplayTabId.ToString(CultureInfo.InvariantCulture));
            if (child != null && child.Value != "-1")
            {
                child.Selected = true;
            }
            else
            {
                ddlChildDisplayTabId.SelectedIndex = 0;
            }

        }

        private void SaveSettings()
        {
            //use approvals
            Setting setting = Setting.UseApprovals;
            setting.PropertyValue = chkUseApprovals.Checked.ToString(CultureInfo.InvariantCulture);
            ItemVersionSetting itemVersionSetting = new ItemVersionSetting(setting);
            //useApprovalSetting.ControlName = "chkUseApprovals";
            //useApprovalSetting.PropertyName = "Checked";
            //useApprovalSetting.PropertyValue = chkUseApprovals.Checked.ToString(CultureInfo.InvariantCulture);
            VersionInfoObject.VersionSettings.Add(itemVersionSetting);


            //display on current page option
            setting = Setting.CategorySettingsCurrentDisplay;
            setting.PropertyValue = rblDisplayOnCurrentPage.SelectedValue;
            itemVersionSetting = new ItemVersionSetting(setting);
            //cpSetting.ControlName = "CategorySettings";
            //cpSetting.PropertyName = "DisplayOnCurrentPage";
            //cpSetting.PropertyValue = rblDisplayOnCurrentPage.SelectedValue;
            VersionInfoObject.VersionSettings.Add(itemVersionSetting);

            //force display on specific page
            setting = Setting.CategorySettingsForceDisplay;
            setting.PropertyValue = chkForceDisplayTab.Checked.ToString(CultureInfo.InvariantCulture);
            itemVersionSetting = new ItemVersionSetting(setting);
            //fpSetting.ControlName = "CategorySettings";
            //fpSetting.PropertyName = "ForceDisplayOnPage";
            //fpSetting.PropertyValue = chkForceDisplayTab.Checked.ToString(CultureInfo.InvariantCulture);
            VersionInfoObject.VersionSettings.Add(itemVersionSetting);

            if (rowCommentForum.Visible)
            {
                setting = Setting.CategorySettingsCommentForumId;
                setting.PropertyValue = ddlCommentForum.SelectedValue;
                itemVersionSetting = new ItemVersionSetting(setting);
                VersionInfoObject.VersionSettings.Add(itemVersionSetting);
            }
        }

        #region Optional Interfaces

        public DotNetNuke.Entities.Modules.Actions.ModuleActionCollection ModuleActions
        {
            get
            {
                DotNetNuke.Entities.Modules.Actions.ModuleActionCollection actions = new DotNetNuke.Entities.Modules.Actions.ModuleActionCollection();
                actions.Add(GetNextActionID(), Localization.GetString(DotNetNuke.Entities.Modules.Actions.ModuleActionType.AddContent, LocalResourceFile), DotNetNuke.Entities.Modules.Actions.ModuleActionType.AddContent, "", "", "", false, DotNetNuke.Security.SecurityAccessLevel.Edit, true, false);
                return actions;
            }
        }

        

        #endregion
    }
}

    
