//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.



namespace Engage.Dnn.Publish.Util
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Xml.XPath;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search;
    using DotNetNuke.Services.Search.Entities;
    using Portability;

    /// <summary>
    /// Features Controller Class supports IPortable currently.
    /// </summary>
    public class FeaturesController : ModuleSearchBase, IPortable //, ISearchable
    {
        #region IPortable Members

        /// <summary>
        /// Method is invoked when portal template is imported or user selects Import content from menu.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="content"></param>
        /// <param name="version"></param>
        /// <param name="userId"></param>
        public void ImportModule(int moduleId, string content, string version, int userId)
        {

            var validator = new TransportableXmlValidator();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            if (validator.Validate(stream) == false)
            {
                var invalidXml = new Exception("Unable to import publish content due to incompatible XML file. Error: " + validator.Errors[0]);
                Exceptions.LogException(invalidXml);
                throw invalidXml;
            }

            //The DNN ValidatorBase closes the stream? Must re-create. hk
            stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var doc = new XPathDocument(stream);
            var builder = new XmlTransporter(moduleId);

            try
            {
                XmlDirector.Deconstruct(builder, doc);
            }
            catch (Exception e)
            {
                Exceptions.LogException(new Exception(e.ToString()));
                throw;
            }
        }


        /// <summary>
        /// Method is invoked when portal template is created or user selects Export Content from menu.
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public string ExportModule(int moduleId)
        {
            bool exportAll = false;

            //check query string for a "All" param to signal all rows, not just for a moduleId
            if (HttpContext.Current != null && HttpContext.Current.Request.QueryString["all"] != null)
            {
                exportAll = true;
            }
            XmlTransporter builder;
            try
            {
                builder = new XmlTransporter(moduleId);
                var mi = ModuleController.Instance.GetModule(moduleId, -1, true);
                XmlDirector.Construct(builder, exportAll, mi.PortalID);
            }
            catch (Exception e)
            {
                Exceptions.LogException(new Exception(e.ToString()));
                throw;
            }

            return builder.Document.OuterXml;
        }

        #endregion

        //#region ISearchable Members

        public SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo)
        {
            var items = new SearchItemInfoCollection();
            AddArticleSearchItems(items, modInfo);
            return items;
        }

        //#endregion

        private static void AddArticleSearchItems(SearchItemInfoCollection items, ModuleInfo modInfo)
        {
            //get all the updated items
            //DataTable dt = Article.GetArticlesSearchIndexingUpdated(modInfo.PortalID, modInfo.ModuleDefID, modInfo.TabID);

            //TODO: we should get articles by ModuleID and only perform indexing by ModuleID 
            DataTable dt = Article.GetArticlesByModuleId(modInfo.ModuleID, true);
            SearchArticleIndex(dt, items, modInfo);

        }

        private static void SearchArticleIndex(DataTable dt, SearchItemInfoCollection items, ModuleInfo modInfo)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];


                var searchedContent = new StringBuilder(8192);
                //article name
                string name = HtmlUtils.Clean(row["Name"].ToString().Trim(), false);
                if (Utility.HasValue(name))
                {
                    searchedContent.AppendFormat("{0}{1}", name, " ");
                }
                else
                {
                    //do we bother with the rest?
                    continue;
                }

                //article text
                string articleText = row["ArticleText"].ToString().Trim();
                if (Utility.HasValue(articleText))
                {
                    searchedContent.AppendFormat("{0}{1}", articleText, " ");
                }

                //article description
                string description = row["Description"].ToString().Trim();
                if (Utility.HasValue(description))
                {
                    searchedContent.AppendFormat("{0}{1}", description, " ");
                }

                //article metakeyword
                string keyword = row["MetaKeywords"].ToString().Trim();
                if (Utility.HasValue(keyword))
                {
                    searchedContent.AppendFormat("{0}{1}", keyword, " ");
                }

                //article metadescription
                string metaDescription = row["MetaDescription"].ToString().Trim();
                if (Utility.HasValue(metaDescription))
                {
                    searchedContent.AppendFormat("{0}{1}", metaDescription, " ");
                }

                //article metatitle
                string metaTitle = row["MetaTitle"].ToString().Trim();
                if (Utility.HasValue(metaTitle))
                {
                    searchedContent.AppendFormat("{0}{1}", metaTitle, " ");
                }

                string itemId = row["ItemId"].ToString();
                var item = new SearchItemInfo
                {
                    Title = name,
                    Description = HtmlUtils.Clean(description, false),
                    Author = Convert.ToInt32(row["AuthorUserId"], CultureInfo.InvariantCulture),
                    PubDate = Convert.ToDateTime(row["LastUpdated"], CultureInfo.InvariantCulture),
                    ModuleId = modInfo.ModuleID,
                    SearchKey = "Article-" + itemId,
                    Content =
                                           HtmlUtils.StripWhiteSpace(
                                           HtmlUtils.Clean(searchedContent.ToString(), false), true),
                    GUID = "itemid=" + itemId
                };

                items.Add(item);

                //Check if the Portal is setup to enable venexus indexing

                //}
            }
        }

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDate)
        {
            var searchDocuments = new List<SearchDocument>();
            DataTable dt = Article.GetArticlesByModuleId(moduleInfo.ModuleID, true);

            foreach (DataRow row in dt.Rows)
            {
                if (Convert.ToDateTime(row["LastUpdated"]).ToUniversalTime() <= beginDate.ToUniversalTime() ||
                    Convert.ToDateTime(row["LastUpdated"]).ToUniversalTime() >= DateTime.UtcNow)
                    continue;

                var content = string.Format("{0}<br />{1}", HtmlUtils.Clean(row["Name"].ToString().Trim(), false), row["Description"].ToString().Trim());

                var searchDocumnet = new SearchDocument
                {
                    UniqueKey = string.Format("Articles:{0}:{1}", moduleInfo.ModuleID, row["itemId"].ToString()),  // any unique identifier to be able to query for your individual record
                    PortalId = moduleInfo.PortalID,  // the PortalID
                    TabId = moduleInfo.TabID, // the TabID
                    AuthorUserId = (int)row["AuthorUserId"], // the person who created the content
                    Title = row["Name"].ToString().Trim(),  // the title of the content, but should be the module title
                    Description = row["Description"].ToString().Trim(),  // the description or summary of the content
                    Body = content,  // the long form of your content
                    ModifiedTimeUtc = Convert.ToDateTime(row["LastUpdated"]).ToUniversalTime(),  // a time stamp for the search results page
                    CultureCode = moduleInfo.CultureCode, // the current culture code
                    IsActive = true, // allows you to remove the item from the search index (great for soft deletes)
                    Url = Utility.GetItemLinkUrl(Convert.ToInt32(row["itemId"], CultureInfo.InvariantCulture), moduleInfo.PortalID)
            };

                searchDocuments.Add(searchDocumnet);
            }

            return searchDocuments;
        }

       
    }
}
