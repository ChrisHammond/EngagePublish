//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System.Data;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

namespace Engage.Dnn.Publish.Admin.Tools
{

    using System;
    using System.Globalization;
    using DotNetNuke.Services.Exceptions;
    using Publish;
    using Controls;
    using Data;
    using Util;


    public partial class Recategorize : ModuleBase
    {

        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            LoadControls();
            base.OnInit(e);

            // the following loads our common properties control to edit.
            // must be loaded in the OnInit, once you get to page load the properties 
            // for this control are gone from viewstate if not loaded in OnInit
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
        }

        private void LoadControls()
        {
            FillDropDown();
        }

        
        #region Event Handlers

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {


                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }


        #endregion


        protected void btnSearch_Click(object sender, EventArgs e)
        {
            //search for articles based on a string from txtTagSearch
            DataSet ds;
            ds = DataProvider.Instance().GetAdminKeywordSearch(txtTagSearch.Text, ItemType.Article.GetId(), ApprovalStatus.Approved.GetId(), PortalId);

            dgItems.DataSource = ds.Tables[0];
            dgItems.DataBind();
        }

        protected void btnAddCategories_Click(object sender, EventArgs e)
        {

            //get our list of articles based on the search
            DataSet ds;
            ds = DataProvider.Instance().GetAdminKeywordSearch(txtTagSearch.Text, ItemType.Article.GetId(), ApprovalStatus.Approved.GetId(), PortalId);

            int articleCount;
            articleCount = 0;
            DataTable allArticles = ds.Tables[0];


            foreach (DataRow dr in allArticles.Rows)
            {
                articleCount++;
                Article a = Article.GetArticle(Convert.ToInt32(dr["itemId"], CultureInfo.InvariantCulture), PortalId, true, true, true);
                if (a != null)
                {

                    //create a relationship
                    var irel = new ItemRelationship { RelationshipTypeId = RelationshipType.ItemToParentCategory.GetId() };
                    int ids = Convert.ToInt32(cboCategories.SelectedValue);

                    a.Relationships.Clear();
                    //check for parent category, if none then add a relationship for Top Level Item
                    if (ids > 0)
                    {
                        irel.ParentItemId = ids;
                        a.Relationships.Add(irel);
                    }

                    int relids = Convert.ToInt32(cboRelatedCategory.SelectedValue);


                    var irc = new ItemRelationship { RelationshipTypeId = RelationshipType.ItemToRelatedCategory.GetId() };
                    if (relids > 0)
                    {
                        irc.ParentItemId = relids;
                        a.Relationships.Add(irc);
                    }

                    
                    
                    a.Save(UserId);
                }
            }
        }

        private void FillDropDown()
        {
            ItemRelationship.DisplayCategoryHierarchy(cboCategories, -1, PortalId, false);

            var li = new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1");
            cboCategories.Items.Insert(0, li);

            ItemRelationship.DisplayCategoryHierarchy(cboRelatedCategory, -1, PortalId, false);

            var li2 = new ListItem(Localization.GetString("ChooseOne", LocalResourceFile), "-1");
            cboRelatedCategory.Items.Insert(0, li2);

        }

    }
}

