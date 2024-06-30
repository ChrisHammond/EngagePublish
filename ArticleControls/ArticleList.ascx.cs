//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.



namespace Engage.Dnn.Publish.ArticleControls
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Data;
    using Util;


    public partial class ArticleList : ModuleBase
    {

        #region Event Handlers

        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            cboCategories.SelectedIndexChanged += CboCategoriesSelectedIndexChanged;
            cboWorkflow.SelectedIndexChanged += CboWorkflowSelectedIndexChanged;

            Load += Page_Load;

        }

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                DotNetNuke.UI.Utilities.ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteConfirm", LocalResourceFile));
                if (!Page.IsPostBack)
                {
                    Utility.LocalizeGridView(dgItems, LocalResourceFile);
                    ConfigureAddLink();
                    FillDropDown();
                    BindData();
                }
                
                if (IsAdmin)
                    cmdApprove.Visible = cmdArchive.Visible = cmdDelete.Visible = true;
                else cmdApprove.Visible = cmdArchive.Visible = cmdDelete.Visible = false;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void CboCategoriesSelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }

        private void CboWorkflowSelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdBack_Click(object sender, EventArgs e)
        {
            Response.Redirect(BuildLinkUrl(string.Empty), true);
        }

        #endregion

        #region Private Methods

        private void FillDropDown()
        {
            ItemRelationship.DisplayCategoryHierarchy(cboCategories, -1, PortalId, false);

            var li = new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1");
            cboCategories.Items.Insert(0, li);

            li = cboCategories.Items.FindByValue(CategoryId.ToString(CultureInfo.InvariantCulture));
            if (li != null) li.Selected = true;

            cboWorkflow.Visible = UseApprovals;
            lblWorkflow.Visible = UseApprovals;
            if (UseApprovals)
            {
                cboWorkflow.DataSource = DataProvider.Instance().GetApprovalStatusTypes(PortalId);
                cboWorkflow.DataValueField = "ApprovalStatusID";
                cboWorkflow.DataTextField = "ApprovalStatusName";
                cboWorkflow.DataBind();
                li = cboWorkflow.Items.FindByText(ApprovalStatus.Waiting.Name);
                if (li != null) li.Selected = true;
            }
        }

        private void ConfigureAddLink()
        {
            if (TopLevelId == -1)
            {
                string s = cboCategories.SelectedValue;
                int categoryId = (Utility.HasValue(s) ? Convert.ToInt32(s, CultureInfo.InvariantCulture) : -1);
                if (categoryId == -1)
                {
                    if (CategoryId > -1)
                    {
                        lnkAddNewArticle.NavigateUrl = BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=articleEdit&topLevelId=" + CategoryId.ToString(CultureInfo.InvariantCulture) + "&parentId=" + CategoryId.ToString(CultureInfo.InvariantCulture));
                        lnkAddNewArticle.Visible = true;
                    }
                    else
                    {
                        lnkAddNewArticle.NavigateUrl = BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=articleEdit");
                        lnkAddNewArticle.Visible = false;
                    }
                }
                else
                {
                    lnkAddNewArticle.NavigateUrl = BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=articleEdit&topLevelId=" + categoryId.ToString(CultureInfo.InvariantCulture) + "&parentId=" + categoryId.ToString(CultureInfo.InvariantCulture));
                    lnkAddNewArticle.Visible = true;
                }
            }
            else
            {
                lnkAddNewArticle.NavigateUrl = BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=articleEdit&topLevelId=" + TopLevelId.ToString(CultureInfo.InvariantCulture) + "&parentId=" + CategoryId.ToString(CultureInfo.InvariantCulture));
                lnkAddNewArticle.Visible = true;
            }
        }

        private DataTable GetGridData()
        {
            int categoryId = Convert.ToInt32(cboCategories.SelectedValue, CultureInfo.InvariantCulture);

            //set the approval status ID to approved by default, if we're using approvals look for the selected value
            int approvalStatusId = ApprovalStatus.Approved.GetId();

            if (UseApprovals)
            {
                approvalStatusId = Convert.ToInt32(cboWorkflow.SelectedValue, CultureInfo.InvariantCulture);
            }

            dgItems.DataSourceID = string.Empty;
            DataSet ds;
            if (txtArticleSearch.Text.Trim() != string.Empty)
            {
                var objSecurity = new DotNetNuke.Security.PortalSecurity();
                string searchKey = objSecurity.InputFilter(txtArticleSearch.Text.Trim(), DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL);
                //
                ds = DataProvider.Instance().GetAdminItemListingSearchKey(categoryId, ItemType.Article.GetId(), Util.RelationshipType.ItemToParentCategory.GetId(), Util.RelationshipType.ItemToRelatedCategory.GetId(), approvalStatusId, " vi.createddate desc ", searchKey, PortalId);
            }
            else
            {
                ds = DataProvider.Instance().GetAdminItemListing(categoryId, ItemType.Article.GetId(), Util.RelationshipType.ItemToParentCategory.GetId(), Util.RelationshipType.ItemToRelatedCategory.GetId(), approvalStatusId, " vi.createddate desc ", PortalId);

            }

            //

            return ds.Tables[0];
        }

        private void BindData()
        {
            dgItems.DataSource = GetGridData();
            dgItems.DataBind();

            ConfigureAddLink();

            dgItems.Visible = true;
            lblMessage.Visible = false;

            if (dgItems.Rows.Count < 1)
            {
                lblMessage.Text = UseApprovals ? String.Format(CultureInfo.CurrentCulture, Localization.GetString("NoArticlesFound", LocalResourceFile), cboCategories.SelectedItem, cboWorkflow.SelectedItem) : String.Format(CultureInfo.CurrentCulture, Localization.GetString("NoArticlesFoundNoApproval", LocalResourceFile), cboCategories.SelectedItem);

                dgItems.Visible = false;
                lblMessage.Visible = true;
            }
        }

        private int CategoryId
        {
            get
            {
                string id = Request.QueryString["categoryid"];
                return (id == null ? -1 : Convert.ToInt32(id, CultureInfo.InvariantCulture));
            }
        }


        private string GridViewSortDirection
        {
            get { return ViewState["SortDirection"] as string ?? "ASC"; }
            set { ViewState["SortDirection"] = value; }
        }

        private string GridViewSortExpression
        {
            get { return ViewState["SortExpression"] as string ?? string.Empty; }
            set { ViewState["SortExpression"] = value; }
        }


        private string GetSortDirection()
        {
            switch (GridViewSortDirection)
            {
                case "ASC":
                    GridViewSortDirection = "DESC";
                    break;

                case "DESC":
                    GridViewSortDirection = "ASC";
                    break;
            }
            return GridViewSortDirection;
        }

        //private string CategoryName
        //{
        //    get	{return (Convert.ToString(Request.QueryString["category"]));}
        //}

        private int TopLevelId
        {
            get
            {
                string s = Request.QueryString["topLevelId"];
                return (s == null ? -1 : Convert.ToInt32(s, CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region Protected Methods

        protected void dgItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgItems.DataSource = SortDataTable(GetGridData(), true);
            dgItems.PageIndex = e.NewPageIndex;
            dgItems.DataBind();
        }

        protected DataView SortDataTable(DataTable dataTable, bool isPageIndexChanging)
        {
            if (dataTable != null)
            {
                var dataView = new DataView(dataTable);
                if (!string.IsNullOrEmpty(GridViewSortExpression))
                {
                    dataView.Sort = isPageIndexChanging ? string.Format(CultureInfo.InvariantCulture, "{0} {1}", GridViewSortExpression, GridViewSortDirection) : string.Format(CultureInfo.InvariantCulture, "{0} {1}", GridViewSortExpression, GetSortDirection());
                }
                return dataView;
            }
            return new DataView();
        }


        protected void dgItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            GridViewSortExpression = e.SortExpression;
            int pageIndex = dgItems.PageIndex;
            dgItems.DataSource = SortDataTable(GetGridData(), true);
            //dgItems.DataSource = SortDataTable(dgItems.DataSource as DataTable, false);
            dgItems.DataBind();
            dgItems.PageIndex = pageIndex;
        }

        protected static string GetDescription(object description)
        {
            if (description != null)
            {
                return HtmlUtils.Shorten(HtmlUtils.Clean(description.ToString(), true), 200, string.Empty) + "&nbsp";
            }
            return string.Empty;
        }

        protected string GetVersionsUrl(object itemId)
        {
            if (itemId != null)
            {
                string categoryId = cboCategories.SelectedValue.ToString(CultureInfo.InvariantCulture);
                return BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=versionslist&itemid=" + itemId + "&categoryid=" + categoryId);
            }
            return string.Empty;
        }

        protected string GetArticleEditUrl(object itemVersionId)
        {
            if (itemVersionId != null)
            {
                return BuildLinkUrl("&ctl=" + Utility.AdminContainer + "&mid=" + ModuleId.ToString(CultureInfo.InvariantCulture) + "&adminType=articleEdit&versionid=" + itemVersionId);
            }
            return string.Empty;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Does not return class state information")]
        protected string GetLocalizedEditText()
        {
            return Localization.GetString("Edit", LocalResourceFile);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Does not return class state information")]
        protected string GetLocalizedVersionText()
        {
            return Localization.GetString("Versions", LocalSharedResourceFile);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdApprove_Click(object sender, EventArgs e)
        {
            //parse through the checked items in the list and approve them.
            try
            {
                foreach (GridViewRow gvr in dgItems.Rows)
                {
                    var lblItemVersionId = (Label)gvr.FindControl("lblItemVersionId");
                    var cb = (CheckBox)gvr.FindControl("chkSelect");
                    if (lblItemVersionId != null && cb != null && cb.Checked)
                    {
                        //approve
                        var a = Article.GetArticleVersion(Convert.ToInt32(lblItemVersionId.Text), PortalId);
                        a.ApprovalStatusId = ApprovalStatus.Approved.GetId();
                        a.UpdateApprovalStatus();
                    }
                }

                //Utility.ClearPublishCache(PortalId);
                BindData();
                lblMessage.Text = Localization.GetString("ArticlesApproved", LocalResourceFile);
                lblMessage.Visible = true;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (GridViewRow gvr in dgItems.Rows)
                {
                    var hlId = (HyperLink)gvr.FindControl("hlId");
                    var chkSelect = (CheckBox)gvr.FindControl("chkSelect");
                    if (hlId != null && chkSelect != null && chkSelect.Checked)
                    {
                        Item.DeleteItem(Convert.ToInt32(hlId.Text, CultureInfo.CurrentCulture), PortalId);
                    }
                }

                //Utility.ClearPublishCache(PortalId);
                BindData();
                lblMessage.Text = Localization.GetString("ArticlesDeleted", LocalResourceFile);
                lblMessage.Visible = true;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void cmdArchive_Click(object sender, EventArgs e)
        {
            //parse through the checked items in the list and archive them.
            try
            {
                foreach (GridViewRow gvr in dgItems.Rows)
                {
                    var hlId = (HyperLink)gvr.FindControl("hlId");
                    var cb = (CheckBox)gvr.FindControl("chkSelect");
                    if (hlId != null && cb != null && cb.Checked)
                    {
                        //approve
                        var a = (Article)Item.GetItem(Convert.ToInt32(hlId.Text), PortalId, ItemType.Article.GetId(), false);
                        a.ApprovalStatusId = ApprovalStatus.Archived.GetId();
                        a.UpdateApprovalStatus();
                    }
                }
                //Utility.ClearPublishCache(PortalId);
                BindData();
                lblMessage.Text = Localization.GetString("ArticlesArchived", LocalResourceFile);
                lblMessage.Visible = true;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


      

        #endregion

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindData();
        }


    }
}

