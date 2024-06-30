//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.



namespace Engage.Dnn.Publish.Admin
{
    using System.Web;
    using System.Web.UI;
    using System;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Data;
    using Util;


    public partial class AdminMenu : ModuleBase
    {
        #region Event Handlers
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            BindItemData();
            ConfigureMenus();
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
        }

        private void Page_Load(object sender, EventArgs e)
        {
            int itemId = ItemId;
            if (itemId != -1 && !VersionInfoObject.IsNew)
            {
            }

            try
            {
                //check VI for null then set information
                if (!Page.IsPostBack)
                {
                    //check if the user is logged in and an admin. If so let them approve items
                    if (IsAdmin && !VersionInfoObject.IsNew)
                    {
                        if (UseApprovals && Item.GetItemType(itemId,PortalId).Equals("ARTICLE", StringComparison.OrdinalIgnoreCase))
                        {
                            //ApprovalStatusDropDownList.Attributes.Clear();
                            //ApprovalStatusDropDownList.Attributes.Add("onchange", "javascript:if (!confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteConfirmation", LocalResourceFile)) + "')) resetDDLIndex(); else ");

                            //ClientAPI.AddButtonConfirm(ApprovalStatusDropDownList, Localization.GetString("DeleteConfirmation", LocalResourceFile));
                            FillDropDownList();
                        }
                        else
                        {
                            ApprovalStatusDropDownList.Visible = false;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

                private void FillDropDownList()
        {
            
            ApprovalStatusDropDownList.DataSource = DataProvider.Instance().GetApprovalStatusTypes(PortalId);
            ApprovalStatusDropDownList.DataValueField = "ApprovalStatusID";
            ApprovalStatusDropDownList.DataTextField = "ApprovalStatusName";
            ApprovalStatusDropDownList.DataBind();
            //set the current approval status
            ListItem li = ApprovalStatusDropDownList.Items.FindByValue(VersionInfoObject.ApprovalStatusId.ToString(CultureInfo.InvariantCulture));
            if (li != null)
            {
                li.Selected = true;
            }
        }

        public string BuildEditUrl(string currentItemType)
        {
            string url = string.Empty;
            try
            {
                //find the location of the ams admin module on the site.
                //DotNetNuke.Entities.Modules.ModuleController objModules = new ModuleController();

                if (ItemId > -1)
                {
                    int versionId = -1;
                    if (!VersionInfoObject.IsNew)
                    {
                        versionId = VersionInfoObject.ItemVersionId;
                    }

                    url = DotNetNuke.Common.Globals.NavigateURL(TabId, string.Empty, "ctl=" + Utility.AdminContainer,
                        "mid=" + ModuleId.ToString(CultureInfo.InvariantCulture), "adminType=" + currentItemType + "Edit",
                        "versionId=" + versionId.ToString(CultureInfo.InvariantCulture), "returnUrl=" + HttpUtility.UrlEncode(Request.RawUrl));
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }

            return url;
        }

        public string BuildAddArticleUrl()
        {
            if (ItemId <= -1)
            {
                return string.Empty;
            }
            int parentCategoryId = -1;

            if (!VersionInfoObject.IsNew)
            {
                parentCategoryId = VersionInfoObject.ItemTypeId == ItemType.Category.GetId()
                                           ? VersionInfoObject.ItemId
                                           : VersionInfoObject.GetParentCategoryId();
            }

            return DotNetNuke.Common.Globals.NavigateURL(
                    TabId,
                    string.Empty,
                    "ctl=" + Utility.AdminContainer,
                    "mid=" + ModuleId.ToString(CultureInfo.InvariantCulture),
                    "adminType=articleedit",
                    "parentId=" + parentCategoryId.ToString(CultureInfo.InvariantCulture),
                    "returnUrl=" + HttpUtility.UrlEncode(Request.RawUrl));
        }


        public string BuildCategoryListUrl()
        {
            //find the location of the ams admin module on the site.
            //DotNetNuke.Entities.Modules.ModuleController objModules = new ModuleController();
            if (ItemId > -1)
            {
                int parentCategoryId = -1;

                if (!VersionInfoObject.IsNew)
                {
                    parentCategoryId = VersionInfoObject.ItemTypeId == ItemType.Category.GetId() ? VersionInfoObject.ItemId : Category.GetParentCategory(VersionInfoObject.ItemId, PortalId);
                }

                //string currentItemType = Item.GetItemType(ItemId,PortalId);
                return DotNetNuke.Common.Globals.NavigateURL(TabId, string.Empty, "ctl=" + Utility.AdminContainer,
                    "mid=" + ModuleId.ToString(CultureInfo.InvariantCulture), "adminType=articlelist",
                    "categoryId=" + parentCategoryId.ToString(CultureInfo.InvariantCulture));
            }
            return string.Empty;
        }

        #endregion

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        //protected void ddlApprovalStatus_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    CallUpdateApprovalStatus();
        //}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", MessageId = "System.Web.UI.ITextControl.set_Text(System.String)", Justification = "Literal, '-', does not change by locale")]
        private void ConfigureMenus()
        {
            int itemId = ItemId;
            bool isAuthorOnly = IsAuthor && !IsAdmin;

            divAdminMenu.Visible = true;
            lnkUpdateStatus.Visible = !isAuthorOnly && UseApprovals;

            //// Load stats
            //// TODO: hide this if necessary
            phStats.Visible = true;
            const string pathToStatsControl = "QuickStats.ascx";
            var statsControl = (ModuleBase)LoadControl(pathToStatsControl);
            statsControl.ModuleConfiguration = ModuleConfiguration;
            statsControl.ID = System.IO.Path.GetFileNameWithoutExtension(pathToStatsControl);
            phStats.Controls.Add(statsControl);

            phLink.Visible = true;

            // TODO: IsNew is itemId != -1, do we need to check both?
            if (itemId != -1 && !VersionInfoObject.IsNew)
            {
                BuildAdminMenu(itemId, isAuthorOnly);
            }
            else
            {
                //Hide the phAdminControl placeholder for the admin controls.
                var container = (PlaceHolder)Parent;
                container.Visible = false;
            }
        }

        private void BuildAdminMenu(int itemId, bool isAuthorOnly)
        {
            string currentItemType = Item.GetItemType(itemId, PortalId);
            string localizedItemTypeName = Localization.GetString(currentItemType, LocalResourceFile);

            // the following dynamicly builds the Admin Menu for an item when viewing the item display control.
            AddMenuLink(Localization.GetString("AddNew", LocalResourceFile) + " " + Localization.GetString("Article", LocalResourceFile), BuildAddArticleUrl(),false);

            //Article List and Add New should load even if there isn't a valid item.
            AddMenuLink(Localization.GetString("ArticleList", LocalResourceFile), BuildCategoryListUrl(), false);


            if (!currentItemType.Equals("TOPLEVELCATEGORY", StringComparison.OrdinalIgnoreCase))
            {
                if (currentItemType.Equals("ARTICLE", StringComparison.OrdinalIgnoreCase) || !isAuthorOnly || AllowAuthorEditCategory(PortalId))
                {
                    AddMenuLink(Localization.GetString("Edit", LocalResourceFile) + " " + localizedItemTypeName, BuildEditUrl(currentItemType), true);
                }

                if (currentItemType.Equals("CATEGORY", StringComparison.OrdinalIgnoreCase))
                {
                    lnkUpdateStatus.Visible = false;
                }

                AddMenuLink(localizedItemTypeName + " " + Localization.GetString("Versions", LocalSharedResourceFile), BuildVersionsUrl(), false);
            }
        }

        private void AddMenuLink(string text, string navigateUrl)
        {
            AddMenuLink(text, navigateUrl, true);
        }

        private void AddMenuLink(string text, string navigateUrl, bool primary=false)
        {
            phLink.Controls.Add(new LiteralControl("<li>"));
            if (primary)
            {
                var menuLink = new HyperLink { NavigateUrl = navigateUrl, Text = text, CssClass = "btn btn-primary" };
                phLink.Controls.Add(menuLink);

                phLink.Controls.Add(new LiteralControl("</li>"));

            }
            else
            {
                var menuLink = new HyperLink { NavigateUrl = navigateUrl, Text = text, CssClass = "btn btn-secondary" };
                phLink.Controls.Add(menuLink);

                phLink.Controls.Add(new LiteralControl("</li>"));

            }
        }

        protected void lnkSaveApprovalStatus_Click(object sender, EventArgs e)
        {
            CallUpdateApprovalStatus();
        }

        protected void CallUpdateApprovalStatus()
        {
            if (!VersionInfoObject.IsNew)
            {
                VersionInfoObject.ApprovalStatusId = Convert.ToInt32(ApprovalStatusDropDownList.SelectedValue, CultureInfo.InvariantCulture);
                VersionInfoObject.ApprovalComments = txtApprovalComments.Text.Trim().Length > 0 ? txtApprovalComments.Text.Trim() : Localization.GetString("DefaultApprovalComment", LocalResourceFile);
                VersionInfoObject.UpdateApprovalStatus();

                //Utility.ClearPublishCache(PortalId);

                Response.Redirect(BuildVersionsUrl(), false);

                //redirect to the versions list for this item.
            }
        }

        protected void lnkUpdateStatus_Click(object sender, EventArgs e)
        {
            if (divApprovalStatus != null)
            {
                divApprovalStatus.Visible = true;
            }

            //check if we're editing an article, if so show version comments
            if (Item.GetItemType(ItemId, PortalId).Equals("ARTICLE", StringComparison.OrdinalIgnoreCase))
            {
                if (ItemVersionId == -1)
                {
                    Article a = Article.GetArticle(ItemId, PortalId, true, true, true);
                    lblCurrentVersionComments.Text = a.VersionDescription;
                }
                else
                {
                    Article a = Article.GetArticleVersion(ItemVersionId, PortalId);
                    lblCurrentVersionComments.Text = a.VersionDescription;
                }
                

                divVersionComments.Visible = true;
            }
            else
            {
                divVersionComments.Visible = false;
            }

            txtApprovalComments.Text = VersionInfoObject.ApprovalComments;
        }

        protected void lnkSaveApprovalStatusCancel_Click(object sender, EventArgs e)
        {
            divApprovalStatus.Visible = false;
        }
    }
}

