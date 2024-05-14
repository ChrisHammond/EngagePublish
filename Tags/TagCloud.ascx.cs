//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


namespace Engage.Dnn.Publish.Tags
{

    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Util;
    using Microsoft.Security.Application; // Line 35


    public partial class TagCloud : ModuleBase
    {
        private ArrayList _tagQuery;
        private string _qsTags = string.Empty;
        private int _popularTagsTotal;
        private int _mostPopularTagCount;
        private int _leastPopularTagCount;

        private bool UsePopularTags
        {
            get
            {
                object o = Settings["tcPopularTagBool"];
                return (o == null ? true : Convert.ToBoolean(o, CultureInfo.InvariantCulture));
            }
        }

        override protected void OnInit(EventArgs e)
        {
            Load += Page_Load;
            base.OnInit(e);
            LoadTagInfo();
        }

        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //check VI for null then set information
                //if (!Page.IsPostBack)
                //{
                lnkTagFilters.NavigateUrl = DotNetNuke.Common.Globals.NavigateURL(DefaultTagDisplayTabIdForPortal(PortalId));
                SetTagPageTitle();
                LoadTagList();
                //}
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void SetTagPageTitle()
        {
            if (AllowTitleUpdate)
            {
                var tp = (DotNetNuke.Framework.CDefault)Page;
                tp.Title += " " + _qsTags;
            }
        }

        private void LoadTagList()
        {
            if (_popularTagsTotal > 0)
            {
                DataTable dt = Tag.GetPopularTags(PortalId, _tagQuery, UsePopularTags);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dt.DefaultView.Sort = "TotalItems";

                    var totalItemsSort = dt.Select(null, "TotalItems asc");
                    DataRow leastPop = totalItemsSort[0];
                    DataRow mostPop = totalItemsSort[totalItemsSort.Length - 1];

                    _mostPopularTagCount = Convert.ToInt32(mostPop["TotalItems"].ToString());
                    _leastPopularTagCount = Convert.ToInt32(leastPop["TotalItems"].ToString());

                    var drs = dt.Select(null, "name asc");
                    phTagCloud.Controls.Clear();
                    string itemsWithTag = Localization.GetString("ItemsWithTag", LocalResourceFile);
                    foreach (DataRow dr in drs)
                    {
                        var totalItems = (int)dr["TotalItems"];
                        var tagName = (string)dr["Name"];
                        var lnkTag = new Literal();
                        var sb = new StringBuilder(255);
                        sb.Append("<li class=\"");
                        sb.Append(GetTagSizeClass(totalItems));
                        sb.Append("\">");
                        sb.Append("<a href=\"");
                        sb.Append(BuildTagLink(tagName, false, string.Empty));
                        sb.Append("\" class=\"tag\">");
                        sb.Append(HttpUtility.HtmlEncode(tagName)); // Modified line 106
                        sb.Append("</a> </li>");
                        lnkTag.Text = sb.ToString();
                        phTagCloud.Controls.Add(lnkTag);
                    }
                }
                else
                {
                    phTagCloud.Controls.Add(new LiteralControl(Localization.GetString("NoTags.Text", LocalResourceFile)));
                }
            }
            else
            {
                phTagCloud.Controls.Add(new LiteralControl(Localization.GetString("NoTags.Text", LocalResourceFile)));
            }
        }


        private string GetTagSizeClass(int itemCount)
        {
            int tagCountSpread = _mostPopularTagCount - _leastPopularTagCount;
            double result = Convert.ToDouble(itemCount) / Convert.ToDouble(tagCountSpread);

            string resultString;
            if (result <= .1666)
            {
                resultString = "size1";
            }
            else if (result <= .3333)
            {
                resultString = "size2";
            }
            else if (result <= .4999)
            {
                resultString = "size3";
            }
            else if (result <= .6666)
            {
                resultString = "size4";
            }
            else if (result <= .8333)
            {
                resultString = "size5";
            }
            else
            {
                resultString = "size6";
            }
            return resultString;
        }

        private string BuildTagLink(string name, bool useExisting, string useOthers)
        {
            object o = Request.QueryString["tags"];
            string existingTags;
            if (o != null && useExisting)
            {
                existingTags = o + "-";
            }
            else
            {
                existingTags = useOthers;
            }   
            return DotNetNuke.Common.Globals.NavigateURL(DefaultTagDisplayTabId, string.Empty, "tags=" + existingTags + HttpUtility.UrlEncode(name));
        }

        private void LoadTagInfo()
        {
            if (AllowTags)
            {
                string tags = Request.QueryString["Tags"];
                if (tags != null)
                {
                    _qsTags = Sanitizer.GetSafeHtmlFragment(tags); // Modified line 145
                    char[] separator = { '-' };
                    ArrayList tagList = Tag.ParseTags(_qsTags, PortalId, separator, false);
                    _tagQuery = new ArrayList(tagList.Count);
                    string useOthers = string.Empty;

                    // Create a list of tagids to query the database
                    foreach (Tag tg in tagList)
                    {
                        _tagQuery.Add(tg.TagId);

                        // Add the separator in first
                        phTagFilters.Controls.Add(new LiteralControl(Localization.GetString("TagSeperator.Text", LocalResourceFile)));

                        var sb = new StringBuilder(255);
                        sb.Append("<li class=\"PublishFilterList");
                        sb.Append("\">");
                        sb.Append("<a href=\"");
                        sb.Append(BuildTagLink(tg.Name, false, useOthers));
                        sb.Append("\" class=\"tag\">");
                        sb.Append(HttpUtility.HtmlEncode(tg.Name)); // Modified line 162
                        sb.Append("</a> ");

                        phTagFilters.Controls.Add(new LiteralControl(sb.ToString()));

                        useOthers += tg.Name + "-";
                    }
                }
                _popularTagsTotal = Tag.GetPopularTagsCount(PortalId, _tagQuery, true);
            }
        }

    }
}

