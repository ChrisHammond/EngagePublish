//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System.Data;
using System.IO;
using DotNetNuke.Common.Utilities;
using Engage.Dnn.Publish.Tags;

namespace Engage.Dnn.Publish.Admin.Tools
{

    using System;
    using System.Globalization;
    using System.Web.UI;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;
    using Publish;
    using Controls;
    using Data;
    using Util;


    public partial class TagSearch : ModuleBase
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

            tagEntryControl = (TagEntry)LoadControl("../../Tags/TagEntry.ascx");
            tagEntryControl.ModuleConfiguration = ModuleConfiguration;
            tagEntryControl.ID = Path.GetFileNameWithoutExtension("../../Tags/TagEntry.ascx");


            phTagEntry.Controls.Add(tagEntryControl);
        }

        private TagEntry tagEntryControl; //tag entry control

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


        protected void btnTagSearch_Click(object sender, EventArgs e)
        {
            //search for articles based on a string from txtTagSearch
            var ds = new DataSet();
            ds = DataProvider.Instance().GetAdminKeywordSearch(txtTagSearch.Text, ItemType.Article.GetId(), ApprovalStatus.Approved.GetId(), PortalId);

            dgItems.DataSource = ds.Tables[0];
            dgItems.DataBind();
            //GetAdminItemListingSearchKey(-1, ItemType.Article.GetId(), Util.RelationshipType.ItemToParentCategory.GetId(), Util.RelationshipType.ItemToRelatedCategory.GetId(), approvalStatusId, " vi.createddate desc ", searchKey, PortalId);

        }

        protected void btnAddTags_Click(object sender, EventArgs e)
        {

            //get our list of articles based on the search
            var ds = new DataSet();
            ds = DataProvider.Instance().GetAdminKeywordSearch(txtTagSearch.Text, ItemType.Article.GetId(), ApprovalStatus.Approved.GetId(), PortalId);

            int articleCount = 0;
            //int articleUpdate = 0; // CJH 5/30/2018 removed for non-use
            DataTable allArticles = ds.Tables[0];


            foreach (DataRow dr in allArticles.Rows)
            {
                articleCount++;
                Article a = Article.GetArticle(Convert.ToInt32(dr["itemId"], CultureInfo.InvariantCulture), PortalId, true, true, true);
                if (a != null)
                {
                    //get our list of tags
                    foreach (Tag t in Tag.ParseTags(tagEntryControl.TagList, PortalId))
                    {
                        
                        ItemTag it = ItemTag.Create();
                        it.TagId = Convert.ToInt32(t.TagId, CultureInfo.InvariantCulture);

                        bool tagFound = false;
                        foreach (ItemTag tt in a.Tags)
                        {
                            if (it.TagId == tt.TagId)
                                tagFound = true;
                        }
                        if (!tagFound)
                            a.Tags.Add(it);
                    }
                    
                    a.Save(UserId);
                }
            }
        }

        /*
         * 
         * 
         * protected void lbReplace_Click(object sender, EventArgs e)
        {
            
            int articleCount = 0;
            int articleUpdate = 0;
            DataTable allArticles = Article.GetArticles(PortalId);
            foreach (DataRow dr in allArticles.Rows)
            {
                articleCount++;
                Article a = Article.GetArticle(Convert.ToInt32(dr["itemId"], CultureInfo.InvariantCulture), PortalId, true, true, true);
                if (a != null)
                {
                    //if our article is over 8k characters be sure to trim it
                    if (!Utility.HasValue(a.Description) || !Utility.HasValue(a.MetaDescription))
                    {
                        string description = DotNetNuke.Common.Utilities.HtmlUtils.StripTags(a.ArticleText, false);

                        if (!Utility.HasValue(a.MetaDescription))
                        a.MetaDescription = Utility.TrimDescription(399, description);

                        if (!Utility.HasValue(a.Description))
                            //TODO: localize the end of the description 
                            a.Description = Utility.TrimDescription(3997, description) + "...";// description + "...";
                        
                        a.UpdateDescription();
                        articleUpdate++;
                    }
                }
            }

            //Utility.ClearPublishCache(PortalId);
            //X articles updated out of Y
            lblOutput.Text = String.Format(CultureInfo.CurrentCulture, Localization.GetString("ArticleUpdate", LocalResourceFile).ToString(), articleUpdate, articleCount);

        }
         * 
         * 
         */

    }
}

