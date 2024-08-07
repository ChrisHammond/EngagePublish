//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


namespace Engage.Dnn.Publish.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Xml;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security.Roles;
    using Security;
    using Util;
    using Microsoft.ApplicationBlocks.Data;
    using System.Configuration;

    public class SqlDataProvider : DataProvider
    {
        private const string ProviderType = "data";

        #region Private Members

        private readonly ProviderConfiguration _providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
        private readonly string _connectionString;
        private readonly string _providerPath;
        private readonly string _objectQualifier;
        private readonly string _databaseOwner;

        #endregion

        #region Constructors
        public SqlDataProvider()
        {
            var provider = ((Provider)_providerConfiguration.Providers[_providerConfiguration.DefaultProvider]);


            //_connectionString = Config.GetConnectionString();
            _connectionString = ConfigurationManager.ConnectionStrings["SiteSqlServer"].ConnectionString;


            if (String.IsNullOrEmpty(_connectionString))
            {
                _connectionString = provider.Attributes["connectionString"];
            }

            //if (provider.Attributes["connectionStringName"] != "" && ConfigurationManager.AppSettings[provider.Attributes["connectionStringName"]] != "") 
            //{
            //    this._connectionString = ConfigurationManager.AppSettings[provider.Attributes["connectionStringName"]];
            //} 
            //else 
            //{
            //    this._connectionString = provider.Attributes["_connectionString"];
            //}

            _providerPath = provider.Attributes["providerPath"];

            _objectQualifier = provider.Attributes["objectQualifier"];
            if (!String.IsNullOrEmpty(_objectQualifier) && _objectQualifier.EndsWith("_", StringComparison.Ordinal) == false)
            {
                _objectQualifier += "_";
            }

            //this._objectQualifier = "Publish_";

            _databaseOwner = provider.Attributes["databaseOwner"];
            if (!String.IsNullOrEmpty(_databaseOwner) && _databaseOwner.EndsWith(".", StringComparison.Ordinal) == false)
            {
                _databaseOwner += ".";
            }
        }

        #endregion

        #region Properties
        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string ProviderPath
        {
            get { return _providerPath; }
        }

        public string ObjectQualifier
        {
            get { return _objectQualifier; }
        }

        public string DatabaseOwner
        {
            get { return _databaseOwner; }
        }

        public string NamePrefix
        {
            get { return _databaseOwner + _objectQualifier + ModuleQualifier; }
        }


        #endregion

        //#region Private Methods

        //private static object GetNull(object Field)
        //{
        //    return DotNetNuke.Common.Utilities.Null.GetNull(Field, DBNull.Value);
        //}

        //#endregion

        //get item types
        #region "Methods"
        //TODO: Parameterize all dynamic SQL
        public override DataTable GetItemTypes()
        {
            var sql = new StringBuilder(128);

            sql.Append("select name, itemTypeId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemTypes ");
            sql.Append("where IsActive = 1 ");
            sql.Append("and IsTopLevel  = 0 ");
            sql.Append("order by ");
            sql.Append("Name");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataSet GetApprovalStatusTypes(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ApprovalStatusID, ApprovalStatusName, ResourceKey ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ApprovalStatusType ");
            sql.Append("order by ");
            sql.Append(" ApprovalStatusID");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetApprovalStatusTypesForAuthors(int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select ApprovalStatusID, ApprovalStatusName, ResourceKey ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ApprovalStatusType where approvalstatusname not like 'Approved' ");
            sql.Append("order by ");
            sql.Append(" ApprovalStatusID");
            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }



        public override string GetApprovalStatusTypeName(int approvalStatusId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select ApprovalStatusName ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ApprovalStatusType where ApprovalStatusId = ");
            sql.Append(approvalStatusId);


            return SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()).ToString();
        }

        public override string GetItemTypeName(int itemTypeId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select [Name] ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ItemType where ItemTypeId = ");
            sql.Append(itemTypeId);
            return SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()).ToString();
        }

        public override string GetItemTypeName(int itemTypeId, bool useCache, int portalId, int cacheTime)
        {
            string typeName;
            string cacheKey = Utility.CacheKeyPublishItemTypeName + itemTypeId.ToString(CultureInfo.InvariantCulture); // +"PageId";
            if (useCache)
            {
                object o = DataCache.GetCache(cacheKey);
                typeName = o != null ? o.ToString() : Instance().GetItemTypeName(itemTypeId);
                if (typeName != null)
                {
                    DataCache.SetCache(cacheKey, typeName, DateTime.Now.AddMinutes(cacheTime));
                    Utility.AddCacheKey(cacheKey, portalId);
                }
            }
            else
            {
                typeName = Instance().GetItemTypeName(itemTypeId);
            }

            return typeName;
        }

        public override IDataReader GetItems(int itemTypeId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select [name], itemId, CreatedDate, AuthorUserId, u.DisplayName, Author, RevisingUserId, LastUpdated, Ltrim(str(itemId)) + '-' + [name] as 'listName' from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems vi join ");
            sql.Append(_databaseOwner);
            sql.Append(_objectQualifier);
            sql.Append("users u on (u.UserId = vi.AuthorUserId) ");
            sql.Append(" where iscurrentversion = 1 and portalId=");
            sql.Append(portalId);
            sql.Append(" and itemTypeID = ");
            sql.Append(itemTypeId);
            sql.Append(" order by [name]");
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetItems(int parentItemId, int portalId, int RelationshipTypeId)
        {
            return GetItems(parentItemId, portalId, RelationshipTypeId, -1);
        }

        //public override DataSet GetItems(int parentItemId, int portalId, int RelationshipTypeId, int otherRelationshipTypeId)
        //{
        //    StringBuilder sql = new StringBuilder(256);

        //    sql.Append("select ");
        //    sql.Append(" c.[Name], c.[Description], c.ItemId, c.ItemVersionId, c.CreatedDate, c.LastUpdated ");
        //    sql.Append(" from ");
        //    sql.Append(NamePrefix);
        //    sql.Append("vwChildItems c ");
        //    sql.Append(" where ");
        //    sql.Append(" c.StartDate <= getdate() ");
        //    sql.Append(" and (c.EndDate > getdate() or c.EndDate is null) ");
        //    sql.Append(" and c.IsCurrentVersion = 1 ");			
        //    sql.Append(" and (c.RelationshipTypeId  = ");
        //    sql.Append(RelationshipTypeId);
        //    if (otherRelationshipTypeId != -1)
        //    {
        //        sql.Append("or c.RelationshipTypeId = ");
        //        sql.Append(otherRelationshipTypeId);
        //    }
        //    sql.Append(")");
        //    sql.Append(" and c.PortalID = ");
        //    sql.Append(portalId);
        //    sql.Append(" and c.ParentItemID = ");
        //    sql.Append(parentItemId);
        //    sql.Append(" order by ");
        //    sql.Append("c.[Name]");

        //    return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        //}

        public override DataSet GetItems(int parentItemId, int portalId, int RelationshipTypeId, int itemTypeId)
        {
            return GetItems(parentItemId, portalId, RelationshipTypeId, -1, itemTypeId);
        }

        public override DataSet GetItems(int parentItemId, int portalId, int RelationshipTypeId, int otherRelationshipTypeId, int itemTypeId)
        {
            var sql = new StringBuilder(256);

            sql.Append(" select ");
            sql.Append(" c.[Name], c.[Description], c.ItemId, c.ItemVersionId, c.ItemTypeId, c.CreatedDate, c.LastUpdated, c.Thumbnail, c.StartDate, c.AuthorUserId, u.DisplayName, c.Author, c.RevisingUserId, c.ApprovedItemVersionID ");
            sql.Append(" from ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems c  join ");
            sql.Append(_databaseOwner);
            sql.Append(_objectQualifier);
            sql.Append("users u on (u.UserId = c.AuthorUserId) ");
            sql.Append(" where ");
            sql.Append(" c.StartDate <= getdate() ");
            sql.Append(" and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append(" and c.IsCurrentVersion = 1 ");
            if (itemTypeId != -1)
            {
                sql.Append(" and c.ItemTypeId = ");
                sql.Append(itemTypeId);
            }
            sql.Append(" and (c.RelationshipTypeId  = ");
            sql.Append(RelationshipTypeId);

            if (otherRelationshipTypeId != -1)
            {
                sql.Append(" or c.RelationshipTypeId = ");
                sql.Append(otherRelationshipTypeId);
            }
            sql.Append(")");
            sql.Append(" and c.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and c.ParentItemID = ");
            sql.Append(parentItemId);
            sql.Append(" order by ");
            sql.Append(" c.[Name] ");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetParentItems(int itemId, int portalId, int RelationshipTypeId)
        {
            var sql = new StringBuilder(256);

            sql.Append(" select i.Name, i.Description, i.itemId, i.LastUpdated, i.Thumbnail, i.itemVersionId, i.StartDate, i.ItemVersionIdentifier, i.authoruserid, i.author ");
            sql.Append("from  ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("Itemrelationship r on (r.ParentItemId = i.ItemId) ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems ic on (r.ChildItemId = ic.ItemId and r.childitemversionid = ic.itemversionid) ");
            sql.Append("where ");
            sql.Append(" getdate() >= r.StartDate ");
            sql.Append(" and (r.EndDate > getdate() or r.EndDate is null) ");
            sql.Append(" and getdate() >= i.StartDate ");
            sql.Append(" and (i.EndDate > getdate() or i.EndDate is null) ");
            sql.Append(" and i.IsCurrentVersion = 1 ");
            sql.Append(" and ic.IsCurrentVersion = 1 ");
            sql.Append("and r.RelationshipTypeId  = ");
            sql.Append(RelationshipTypeId);
            sql.Append(" and i.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and r.ChildItemId = ");
            sql.Append(itemId);
            sql.Append(" order by ");
            sql.Append("r.SortOrder, i.[Name]");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }


        public override IDataReader GetItem(int itemId, int portalId, bool isCurrent)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovalDate, ApprovalUserId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ApprovedItemVersionID, Author ");
            sql.Append("from  ");
            sql.Append(NamePrefix);
            sql.Append("vwItems ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ItemID = ");
            sql.Append(itemId);
            if (isCurrent)
            {
                sql.Append(" and iscurrentversion = 1");
            }

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataTable GetCategories(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select c.itemId ");
            sql.Append(", (select count(ParentItemId) from ");
            sql.Append(NamePrefix);
            sql.Append("vwChilditems vi where ParentItemId = c.ItemId and RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId().ToString(CultureInfo.InvariantCulture));
            sql.Append(" and ItemTypeId = ");
            sql.Append(ItemType.Category.GetId());

            sql.Append(") 'ChildCount' from ");

            sql.Append(NamePrefix);
            sql.Append("vwCategories c ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and IsCurrentVersion = 1 ");
            sql.Append(" and ItemType != 'TopLevelCategory' ");
            sql.Append(" order by ");
            sql.Append("[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            return dt;
        }

        public override DataTable GetCategoriesByPortalId(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories c ");
            sql.Append("where PortalId = ");
            sql.Append(portalId);
            sql.Append(" and ItemType != 'TopLevelCategory' ");
            sql.Append("and IsCurrentVersion = 1 ");
            sql.Append(" order by ");
            sql.Append("[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            //do we need this?
            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            return dt;
        }

        public override DataTable GetCategoriesByModuleId(int moduleId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories c ");
            sql.Append("where ModuleId = ");
            sql.Append(moduleId);
            sql.Append(" and ItemType != 'TopLevelCategory' ");
            sql.Append(" order by ");
            sql.Append("[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            //do we need this?
            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            return dt;
        }

        public override DataTable GetCategoriesHierarchy(int portalId)
        {
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAllCategoriesHierachy", portalId);
            return ds.Tables[0];
        }


        public override DataTable GetAllChildrenNLevels(int parentCategoryId, int nLevels, int mItems, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select gcil.ChildItemId 'ItemId', gcil.ParentItemId,  gcil.[level], vi.[Name] 'Name', vpar.[Name] 'ParentName' from ");
            sql.Append(NamePrefix);
            sql.Append("fnGetChildItemsLevel(");
            sql.Append(parentCategoryId);   
            sql.Append(",");
            sql.Append(nLevels);
            sql.Append(",");
            sql.Append(mItems);
            sql.Append(") gcil join ");
            sql.Append(NamePrefix);
            sql.Append("vwitems vi on (vi.ItemId = gcil.ChildItemId and vi.ItemVersionId = gcil.ChildItemVersionId) ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwitems vpar on (vpar.ItemId = gcil.ParentItemId) ");

            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwrelationships vr on (vi.itemVersionId = vr.ChildItemVersionId and vr.ParentItemId = vpar.ItemId  and vr.RelationshipTypeId=gcil.RelationshipTypeId)");

            sql.Append(" where vpar.IsCurrentVersion = 1 AND vi.PortalId = ");  
            sql.Append(portalId);
            sql.Append(" AND vi.StartDate < GetDate() AND (vi.EndDate > GetDate() OR vi.EndDate is null) ");

            //TODO: mItems isn't being used in fnGetChildItemsLevel 
            sql.Append(" order by  gcil.[Level], vr.SortOrder asc, vi.ItemTypeId desc, vpar.[Name], vi.[Name] ");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            var keys = new DataColumn[2];
            keys[0] = dt.Columns[0];
            dt.PrimaryKey = keys;

            return (dt);
        }



        [Obsolete("This method is not used")]
        public override IDataReader GetCategories(int itemTypeId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select c.itemId ");
            sql.Append(", (select count(ParentItemId) from ");
            sql.Append(NamePrefix);
            sql.Append("vwChilditems vi where ParentItemId = c.ItemId and RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId().ToString(CultureInfo.InvariantCulture));
            sql.Append("and ItemTypeId = ");
            sql.Append(ItemType.Category.GetId());
            sql.Append(") 'ChildCount' from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories c ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwItemTypes it on (c.ItemTypeID = it.ItemTypeID) ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and it.Name = 'Category' ");
            sql.Append("and c.ItemTypeID = ");
            sql.Append(itemTypeId);
            sql.Append(" and PortalID = ");
            sql.Append(portalId);
            sql.Append(" order by ");
            sql.Append("c.[Name]");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetTopLevelCategories(int portalId)
        {
            //this needs parentitemid to fix the heirechy of the itemrelationship control
            var sql = new StringBuilder(256);
            sql.Append("select c.Name, c.Description, c.ItemId, c.ItemVersionId, c.DisplayTabId, c.SortOrder, c.ChildDisplayTabId ");
            sql.Append(", (select count(ParentItemId) from ");
            sql.Append(NamePrefix);
            sql.Append("vwChilditems vi where ParentItemId = c.ItemId and RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId().ToString(CultureInfo.InvariantCulture));
            sql.Append("and ItemTypeId = ");
            sql.Append(ItemType.Category.GetId());

            sql.Append(") 'ChildCount' from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories c ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and c.ItemType = 'Category' ");
            sql.Append(" and PortalID = ");
            sql.Append(portalId);
            sql.Append(" order by ");
            sql.Append("[Name]");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override int GetParentCategory(int childItemId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select top 1 vp.itemid from ");
            sql.Append(NamePrefix);
            sql.Append("vwParentItems vp join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems vichild  on (vp.childitemversionid = vichild.itemversionid)");
            sql.Append(" where childItemId = ");
            sql.Append(childItemId);
            sql.Append(" and vichild.iscurrentversion=1 ");
            sql.Append(" and vp.IsCurrentVersion = 1  and vp.itemTypeId = "); //3");
            sql.Append(ItemType.Category.GetId());
            sql.Append(" and vp.RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId());
            //category
            object parentCategory = SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
            if (parentCategory != null)
            {
                return (int)parentCategory;
            }
            return -1;
        }


        public override DataTable GetChildCategories(int parentItemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ");
            sql.Append("c.Name, c.Description, c.ItemId, c.ItemVersionId, c.DisplayTabId, c.CreatedDate, c.LastUpdated, cv.SortOrder, cv.ChildDisplayTabId, c.ParentItemId ");
            sql.Append(", (select count(ParentItemId) from ");
            sql.Append(NamePrefix);
            sql.Append("vwChilditems vi where ParentItemId = c.ItemId and RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId().ToString(CultureInfo.InvariantCulture));
            sql.Append("and ItemTypeId = ");
            sql.Append(ItemType.Category.GetId());

            sql.Append(") 'ChildCount' from ");

            sql.Append(NamePrefix);
            sql.Append("vwChildItems c ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("CategoryVersion cv on (c.ItemVersionId = cv.ItemVersionId) ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and c.RelationshipName = 'Item to Parent Category' ");
            sql.Append(" and c.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and c.ParentItemID = ");
            sql.Append(parentItemId);
            sql.Append(" order by ");
            sql.Append("[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            return dt;
        }

        public override DataTable GetChildCategories(int parentItemId, int portalId, int itemTypeId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ");
            sql.Append("c.Name, c.Description, c.ItemId, c.ItemVersionId, c.DisplayTabId, c.CreatedDate, c.LastUpdated, cv.SortOrder, cv.ChildDisplayTabId, c.ParentItemId ");

            sql.Append(", (select count(ParentItemId) from ");
            sql.Append(NamePrefix);
            sql.Append("vwChilditems vi where ParentItemId = c.ItemId and RelationshipTypeId = ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId().ToString(CultureInfo.InvariantCulture));
            sql.Append(" and ItemTypeId = ");
            sql.Append(ItemType.Category.GetId());

            sql.Append(") 'ChildCount' from ");

            sql.Append(NamePrefix);
            sql.Append("vwChildItems c ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("CategoryVersion cv on (c.ItemVersionId = cv.ItemVersionId) ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and c.RelationshipName = 'Item to Parent Category' ");
            sql.Append(" and c.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and c.ParentItemID = ");
            sql.Append(parentItemId);
            sql.Append(" and c.ItemTypeId = ");
            sql.Append(itemTypeId);
            sql.Append(" order by ");
            sql.Append("[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            return dt;
        }

        //TODO: does this need to return only categories?
        public override DataTable GetAllChildCategories(int parentItemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ");
            sql.Append("iv.[Name], iv.[Description], gc.ChildItemId as ItemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("fnGetChildItems(");
            sql.Append(parentItemId);
            sql.Append(", ");
            sql.Append(Util.RelationshipType.ItemToParentCategory.GetId());
            sql.Append(") gc ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems iv on (gc.childItemId = iv.ItemId) ");
            sql.Append("where ");
            sql.Append("iv.IsCurrentVersion = 1");
            sql.Append("and iv.itemTypeId = ");
            sql.Append(ItemType.Category.GetId());
            sql.Append(" order by ");
            sql.Append("iv.[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            DataTable dt = ds.Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            return dt;
        }

        public override IDataReader GetCategoryVersion(int itemVersionId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select SortOrder, ChildDisplayTabId, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovalDate, ApprovalUserId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow, ModuleId   ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            //sql.Append(" and ItemID = ");
            //sql.Append(itemId);
            sql.Append(" and itemVersionId = ");
            sql.Append(itemVersionId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }



        public override int GetCategoryItemId(string categoryName, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select itemId from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and Name = '");
            sql.Append(categoryName);
            sql.Append("'");
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }


        public override IDataReader GetCategory(int itemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select SortOrder, ChildDisplayTabId, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovalDate, ApprovalUserId, ApprovedItemVersionId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow, ModuleId  ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ItemID = ");
            sql.Append(itemId);
            sql.Append(" and IsCurrentVersion =1");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override IDataReader GetCategory(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select  SortOrder, ChildDisplayTabId, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovalDate, ApprovalUserId, ApprovedItemVersionId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ModuleId, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories ");
            sql.Append("where ItemID = ");
            sql.Append(itemId);
            sql.Append(" and IsCurrentVersion =1");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }


        public override DataTable GetParentCategories(int articleId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append("select ");
            sql.Append("c.ItemId as ArticleId, cat.[Name] as RoleName ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems c ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwCategories cat on (c.ParentItemId = cat.ItemId) ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and c.RelationshipName = 'Item To Parent Category' ");
            sql.Append(" and c.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and c.ItemID = ");
            sql.Append(articleId);
            sql.Append(" order by ");
            sql.Append("cat.[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override int GetOldCategoryId(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select newItemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("CategoryMapping ");
            sql.Append(" where OldCategoryID = ");
            sql.Append(itemId);

            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }


        public override IDataReader GetCategoryListing(int parentItemId, int portalId)
        {
            return SqlHelper.ExecuteReader(ConnectionString, NamePrefix + "spGetCategoryListing", parentItemId, portalId);
        }

        public override DataSet GetAdminItemListing(int parentItemId, int itemTypeId, int RelationshipTypeId, int approvalStatusId, int portalId)
        {
            return GetAdminItemListing(parentItemId, itemTypeId, RelationshipTypeId, -1, approvalStatusId, portalId);
        }

        public override DataSet GetAdminItemListing(int parentItemId, int itemTypeId, int RelationshipTypeId, int approvalStatusId, string orderBy, int portalId)
        {
            return GetAdminItemListing(parentItemId, itemTypeId, RelationshipTypeId, -1, approvalStatusId, orderBy, portalId);
        }

        public override DataSet GetAdminItemListing(int parentItemId, int itemTypeId, int RelationshipTypeId, int otherRelationshipTypeId, int approvalStatusId, int portalId)
        {
            return GetAdminItemListing(parentItemId, itemTypeId, RelationshipTypeId, otherRelationshipTypeId, approvalStatusId, " vi.[Name] asc ", portalId);
        }

        public override DataSet GetAdminItemListing(int parentItemId, int itemTypeId, int RelationshipTypeId, int otherRelationshipTypeId, int approvalStatusId, string orderBy, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAdminItemListing", Utility.CreateIntegerParam("@ParentItemId", parentItemId), Utility.CreateIntegerParam("@ItemTypeId", itemTypeId), Utility.CreateIntegerParam("@RelationshipTypeId", RelationshipTypeId), Utility.CreateIntegerParam("@OtherRelationshipTypeId", otherRelationshipTypeId), Utility.CreateIntegerParam("@ApprovalStatusId", approvalStatusId), Utility.CreateIntegerParam("@PortalId", portalId), Utility.CreateNvarcharParam("@OrderBy", orderBy, 100));
        }

        public override DataSet GetAdminItemListingSearchKey(int parentItemId, int itemTypeId, int RelationshipTypeId, int otherRelationshipTypeId, int approvalStatusId, string orderBy, string searchKey, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAdminItemListingSearchKey", Utility.CreateIntegerParam("@ParentItemId", parentItemId), Utility.CreateIntegerParam("@ItemTypeId", itemTypeId), Utility.CreateIntegerParam("@RelationshipTypeId", RelationshipTypeId), Utility.CreateIntegerParam("@OtherRelationshipTypeId", otherRelationshipTypeId), Utility.CreateIntegerParam("@ApprovalStatusId", approvalStatusId), Utility.CreateIntegerParam("@PortalId", portalId), Utility.CreateNvarcharParam("@OrderBy", orderBy, 100), Utility.CreateNvarcharParam("@SearchKey", searchKey, 250));
        }


        public override DataSet GetAdminCommentListing(int categoryId, int approvalStatusId, int portalId, int authorUserId, string articleSearch)
        {
            var sql = new StringBuilder(723);
            sql.Append("select comment.commentId,  ");
            sql.Append(" comment.commentText,  ");
            sql.Append(" comment.approvalStatusId,  ");
            sql.Append(" comment.userId,  ");
            sql.Append(" comment.itemId,  ");
            sql.Append(" comment.itemVersionId, ");
            sql.Append(" comment.createdDate, ");
            sql.Append(" comment.lastUpdated, ");
            sql.Append(" comment.firstName, ");
            sql.Append(" comment.lastName, ");
            sql.Append(" comment.emailAddress, ");
            sql.Append(" comment.Url, ");
            sql.Append(" comment.ratingId, ");
            sql.Append(" vi.name ");
            sql.AppendFormat(" from {0}vwComments comment ", NamePrefix);
            
            sql.AppendFormat(" join {0}vwItems vi on comment.itemId = vi.ItemId  ", NamePrefix);
            if (categoryId !=-1)  sql.AppendFormat(" join {0}vwChildItems article on comment.itemId = article.ItemId ", NamePrefix);

            sql.Append(" where vi.IsCurrentVersion=1 ");
            if (categoryId != -1)
            {
                sql.Append(" and article.IsCurrentVersion = 1 ");
            sql.AppendFormat(" and article.RelationshipTypeId in (select RelationshipTypeId from {0}RelationshipType where ", NamePrefix);
            sql.Append(" (RelationshipName = 'Item To Parent Category' or RelationshipName = 'Item to Related Category'))  ");
                sql.Append(" and article.parentItemId = @categoryId ");
            }

            //search article titles

            if(articleSearch != string.Empty)
                sql.AppendFormat(" and vi.Name like '%{0}%' ", articleSearch);

            if (authorUserId != -1)
            {
                sql.Append(" and vi.authorUserId = ");
                sql.Append(authorUserId.ToString());
            }
            sql.Append(" and comment.approvalStatusId = @approvalStatusId ");
            sql.Append(" and vi.PortalId = @portalId ");
            sql.Append(" order by vi.lastUpdated, vi.Name, vi.itemId, comment.lastUpdated ");

            var sqlCategoryId = new SqlParameter("@categoryId", categoryId);
            var sqlApprovalStatusId = new SqlParameter("@approvalStatusId", approvalStatusId);
            var sqlPortalId = new SqlParameter("@portalId", portalId);

            return SqlHelper.ExecuteDataset(_connectionString, CommandType.Text, sql.ToString(), sqlCategoryId, sqlApprovalStatusId, sqlPortalId);
        }

        public override DataSet GetItemVersions(int itemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ItemVersionId, DisplayTabId, Name, Description, AuthorUserId, RevisingUserId, approvalStatusId, StartDate, EndDate, adminType, VersionNumber, ItemVersionDate, ModuleId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions v ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ItemID = ");
            sql.Append(itemId);
            sql.Append(" order by itemversionId desc ");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override int GetItemIdFromVersion(int itemVersionId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select itemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions v ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ItemVersionID = ");
            sql.Append(itemVersionId);
            sql.Append(" order by itemversionId desc ");

            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }

        public override int GetItemIdFromVersion(int itemVersionId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select itemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions v ");
            sql.Append("where ItemVersionID = ");
            sql.Append(itemVersionId);
            sql.Append(" order by itemversionId desc ");

            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }

        public override IDataReader GetItemRelationships(int childItemId, int childItemVersionId,  bool isActive)
        {
            return GetItemRelationships(childItemId, childItemVersionId, -1, isActive);
        }

        public override IDataReader GetItemRelationships(int childItemId, int childItemVersionId, int RelationshipTypeId, bool isActive)
        {

            //TODO: we're not checking to see if the child items are using the proper start dates.

            var sql = new StringBuilder(521);

            sql.Append("select ");
            sql.Append(" r.ChildItemId, r.ChildItemVersionId, r.ParentItemId, r.StartDate, r.EndDate, r.RelationshipTypeId, r.SortOrder, rt.RelationshipName ");
            sql.Append(" from ");
            sql.Append(NamePrefix);
            sql.Append("ItemRelationship r ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("RelationshipType rt on (r.RelationshipTypeId = rt.RelationshipTypeId) ");
            
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems Parent on (r.ParentItemId = parent.ItemId) ");

            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems child on (r.ChildItemVersionID = child.ItemVersionID) ");
            sql.Append(" where ");
            sql.Append(" child.ItemID = ");
            sql.Append(childItemId);
            if (isActive)
            {
                sql.Append(" and r.StartDate <= getdate() ");
                sql.Append(" and (r.EndDate > getdate() or r.EndDate is null) ");
                sql.Append(" and parent.StartDate <= getdate() ");
                sql.Append(" and (parent.EndDate > getdate() or parent.EndDate is null) ");

            }
            //sql.Append("and child.IsCurrentVersion = 1 ");

            sql.Append(" and child.ItemVersionID = ");
            sql.Append(childItemVersionId);
            if (RelationshipTypeId > 0)
            {
                sql.Append(" and r.RelationshipTypeId =  ");
                sql.Append(RelationshipTypeId);
            }

            sql.Append(" and Parent.IsCurrentVersion=1 ");
            sql.Append(" order by r.SortOrder ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetItemRelationshipByItemRelationshipId(int itemRelationshipId)
        {
            var sql = new StringBuilder(521);

            sql.Append("select r.itemrelationshipid, r.parentitemid, r.childitemid as 'itemid', r.RelationshipTypeId, r.startdate, r.enddate, r.sortorder, child.Name  ");
            sql.Append(" from ");
            sql.Append(NamePrefix);
            sql.Append("ItemRelationship r ");

            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions child on (r.ChildItemId = child.ItemId) "); 
            sql.Append(" where ");
            sql.Append(" ItemRelationshipId = ");
            sql.Append(itemRelationshipId);
            sql.Append(" and child.isCurrentVersion=1");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override IDataReader GetItemChildRelationships(int parentItemId, int RelationshipTypeId)
        {
            return SqlHelper.ExecuteReader(ConnectionString, NamePrefix + "spGetItemChildRelationships", parentItemId, RelationshipTypeId);
        }

        public override DataSet GetAllChildren(int parentId, int RelationshipTypeId, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAllChildren", parentId, RelationshipTypeId, portalId);
        }

        public override DataSet GetAllChildren(int itemTypeId, int parentId, int RelationshipTypeId, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAllChildrenByType", itemTypeId, parentId, RelationshipTypeId, portalId);
        }

        public override DataSet GetAllChildren(int itemTypeId, int parentId, int RelationshipTypeId, int otherRelationshipTypeId, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAllChildrenByTypeWithTwoRelationshipTypes", itemTypeId, parentId, RelationshipTypeId, otherRelationshipTypeId, portalId);
        }

        public override IDataReader GetAllChildrenAsDataReader(int itemTypeId, int parentId, int RelationshipTypeId, int otherRelationshipTypeId, int portalId)
        {
            return SqlHelper.ExecuteReader(ConnectionString, NamePrefix + "spGetAllChildrenByTypeWithTwoRelationshipTypes", itemTypeId, parentId, RelationshipTypeId, otherRelationshipTypeId, portalId);
        }

        [Obsolete("This method is not used.")]
        public override DataSet GetAllChildrenFromTwoParents(int itemTypeId, int parentId, int RelationshipTypeId, int otherParentId, int otherRelationshipTypeId, int portalId)
        {
            return SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetAllChildrenFromTwoParents", itemTypeId, parentId, RelationshipTypeId, otherParentId, otherRelationshipTypeId, portalId);
        }

        [Obsolete("This method is not used.")]
        public override DataSet GetChildren(int parentId, int RelationshipTypeId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select itemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems ");
            sql.Append("where ");
            sql.Append("PortalID = ");
            sql.Append(portalId);
            sql.Append(" and RelationshipTypeId = ");
            sql.Append(RelationshipTypeId);
            sql.Append(" and IsCurrentVersion = 1 ");
            sql.Append(" and ParentItemId = ");
            sql.Append(parentId);
            sql.Append(" order by ");
            sql.Append("[Name]");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override string GetItemName(int itemId)
        {
            var sql = new StringBuilder(100);
            sql.Append("select [Name] from ");
            sql.Append(NamePrefix);
            sql.Append("Item i join ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersion t on i.ItemId = t.ItemId and i.ApprovedItemVersionId = t.ItemVersionId where i.itemId = ");
            sql.Append(itemId);

            return (string)SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
        }



        #region "article functions"
        public override IDataReader GetArticleVersion(int itemVersionId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ArticleText, VersionNumber, VersionDescription, ReferenceNumber, AverageRating, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovalDate, ApprovalUserId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow, ModuleId  ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and itemVersionId = ");
            sql.Append(itemVersionId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataTable GetArticles(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select name, itemId, ArticleText, DisplayTabID, IsCurrentVersion, Disabled, Description, MetaKeywords, MetaDescription, MetaTitle, AuthorUserId, RevisingUserId, LastUpdated, ModuleId, ItemIdentifier, ItemIdentifier ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" AND IsCurrentVersion = 1 AND StartDate < GetDate() AND (EndDate > GetDate() OR EndDate is null) ");
            sql.Append("order by [Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override IDataReader GetAllArticles(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select name, itemId, ArticleText, DisplayTabID, IsCurrentVersion, Disabled, Description, MetaKeywords, MetaDescription, MetaTitle, AuthorUserId, RevisingUserId, LastUpdated, ModuleId, ItemIdentifier, ItemIdentifier ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" AND IsCurrentVersion = 1 AND StartDate < GetDate() AND (EndDate > GetDate() OR EndDate is null) ");
            sql.Append("order by [Name]");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
            
        }


        public override DataTable GetArticlesByPortalId(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select name, itemId, ArticleText, DisplayTabID, IsCurrentVersion, Disabled, Description, ItemIdentifier, ItemIdentifier, ItemVersionId, PortalId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" AND IsCurrentVersion = 1 AND StartDate < GetDate() AND (EndDate > GetDate() OR EndDate is null) ");
            sql.Append("order by [Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataTable GetArticlesByModuleId(int moduleId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select name, itemId, ArticleText, DisplayTabID, PortalId, IsCurrentVersion, Disabled, Description, MetaKeywords, MetaDescription, MetaTitle, AuthorUserId, RevisingUserId, LastUpdated, ModuleId, ItemIdentifier, ItemIdentifier, ItemVersionId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where ModuleId = ");
            sql.Append(moduleId);
            sql.Append(" order by [Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataTable GetArticlesByModuleIdCurrent(int moduleId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select name, itemId, ArticleText, DisplayTabID, PortalId, IsCurrentVersion, Disabled, Description, MetaKeywords, MetaDescription, MetaTitle, AuthorUserId, RevisingUserId, LastUpdated, ModuleId, ItemIdentifier, ItemIdentifier, ItemVersionId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where ModuleId = ");
            sql.Append(moduleId);
            sql.Append(" and IsCurrentVersion=1 order by [Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataTable GetArticlesSearchIndexingUpdated(int portalId, int moduleDefId, int displayTabId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select * from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticleSearchIndexingUpdated ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ModuleDefId = ");
            sql.Append(moduleDefId);
            sql.Append(" and DisplayTabId = ");
            sql.Append(displayTabId);
            sql.Append(" order by [Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }
        //TODO: this won't work in DNN 9 due to searchitem being removed
        public override DataTable GetArticlesSearchIndexingNew(int portalId, int displayTabId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select 	name, va.itemId, ArticleText, DisplayTabID, IsCurrentVersion, Disabled, va.Description, MetaKeywords, MetaDescription, MetaTitle, AuthorUserId, RevisingUserId, LastUpdated, va.PortalId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles va ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and IsCurrentVersion = 1 ");
            sql.Append(" and (va.ItemId not in (select Cast(SUBSTRING(guid, 8,8000) as int) from ");
            sql.Append(DatabaseOwner);
            sql.Append(ObjectQualifier);
            sql.Append("searchitem where guid like 'ItemId=%') ) ");
            sql.Append(" and DisplayTabId = ");
            sql.Append(displayTabId);
            sql.Append(" order by [Name]");


            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }



        public override DataTable GetArticles(int parentItemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ");
            sql.Append("c.[Name], c.[Description], c.ItemId, c.ItemVersionId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems c ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("ArticleVersion av on (c.ItemVersionId = av.ItemVersionId) ");
            sql.Append("where ");
            sql.Append("c.StartDate <= getdate() ");
            sql.Append("and (c.EndDate > getdate() or c.EndDate is null) ");
            sql.Append("and c.IsCurrentVersion = 1 ");
            sql.Append("and c.RelationshipName = 'Item to Parent Category' ");
            sql.Append(" and c.PortalID = ");
            sql.Append(portalId);
            sql.Append(" and c.ParentItemID = ");
            sql.Append(parentItemId);
            sql.Append(" order by ");
            sql.Append("c.[Name]");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }


        public override IDataReader GetArticle(int itemId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ArticleText, VersionNumber, VersionDescription, ReferenceNumber, AverageRating, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovedItemVersionId, ApprovalDate, ApprovalUserId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ModuleId, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where PortalID = ");
            sql.Append(portalId);
            sql.Append(" and ItemID = ");
            sql.Append(itemId);
            sql.Append(" and iscurrentversion = 1");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override IDataReader GetArticle(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ArticleText, VersionNumber, VersionDescription, ReferenceNumber, AverageRating, ItemId, ItemVersionId, OriginalItemVersionId, Name, Description, ItemVersionDate, CreatedDate, StartDate, EndDate, LanguageId, AuthorUserId, RevisingUserId, ApprovalStatusId,  ApprovedItemVersionId, ApprovalDate, ApprovalUserId, ApprovalComments, MetaKeywords, MetaDescription, MetaTitle, DisplayTabId, LastUpdated, ItemTypeId, PortalId, Disabled, Thumbnail, ModuleId, ItemIdentifier, ItemVersionIdentifier, Url, NewWindow  ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles ");
            sql.Append("where ItemID = ");
            sql.Append(itemId);
            sql.Append(" and IsCurrentVersion =1");
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override int GetOldArticleId(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select newItemId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ArticleMapping ");
            sql.Append(" where OldArticleID = ");
            sql.Append(itemId);

            //return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }


        #endregion

        public override IDataReader GetRelationshipType(string relationshipName)
        {
            var sql = new StringBuilder(256);

            sql.Append("select RelationshipTypeId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("RelationshipType ");
            sql.Append("where ");
            sql.Append("RelationshipName = @relationshipName");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateVarcharParam("@relationshipName", relationshipName, 50));
        }

        public override IDataReader GetPermissionType(string permissionName)
        {
            var sql = new StringBuilder(256);

            sql.Append("select PermissionId ");
            sql.Append("from ");
            sql.Append(DatabaseOwner);
            sql.Append(ObjectQualifier);
            sql.Append("Permission ");
            sql.Append("where ");
            sql.Append("PermissionName = @permissionName");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateVarcharParam("@permissionName", permissionName, 50));
        }

        public override IDataReader GetItemType(string itemName)
        {
            var sql = new StringBuilder(256);

            sql.Append("select ItemTypeID, Name, Description ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ItemType ");
            sql.Append("where ");
            sql.Append("Name = @itemName");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateVarcharParam("@itemName", itemName, 50));
        }

        public override IDataReader GetTopLevelCategoryItem(string itemName)
        {
            var sql = new StringBuilder(256);
            sql.Append("select i.ItemId, iv.[Name], iv.[Description]  ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersion iv , ");
            sql.Append(NamePrefix);
            sql.Append("Item i where iv.[Name] =  @itemName");
            sql.Append("  and iv.itemid = i.itemid and i.ItemTypeId = (select ItemTypeId from ");
            sql.Append(NamePrefix);
            sql.Append("itemtype where [name] = 'TopLevelCategory') ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateVarcharParam("@itemName", itemName, 510));
        }

        public override IDataReader GetApprovalStatusId(string itemName)
        {
            var sql = new StringBuilder(256);
            sql.Append("select a.ApprovalStatusId, a.ApprovalStatusName, a.ResourceKey  ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ApprovalStatusType a where ApprovalStatusName = '");
            sql.Append(itemName);
            sql.Append("'");
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override string GetItemType(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select [Name] ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("Item i join ");
            sql.Append(NamePrefix);
            sql.Append("ItemType it on (i.ItemTypeId = it.ItemTypeId) ");
            sql.Append("where itemId =  ");
            sql.Append(itemId);

            object o = SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
            return o != null ? o.ToString() : string.Empty;
        }

        public override int GetItemTypeId(int itemId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select [ItemTypeId]");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("Item ");
            sql.Append("where itemId =  ");
            sql.Append(itemId);

            object o = SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
            if (o != null)
            {
                return Convert.ToInt32(o, CultureInfo.InvariantCulture);
            }

            return -1;
        }

        public override string GetItemTypeFromVersion(int itemVersionId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select it.[Name]");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("Item i join ");
            sql.Append(NamePrefix);
            sql.Append("ItemType it on (i.ItemTypeId = it.ItemTypeId) ");

            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersion iv on (i.ItemId = iv.ItemId) ");


            sql.Append("where iv.itemVersionId =  ");
            sql.Append(itemVersionId);

            return SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString()).ToString();
        }


        public override IDataReader GetItemVersionSettings(int itemVersionId)
        {
            var sql = new StringBuilder(256);
            sql.Append("select [settingsid], [controlName], [propertyName], [propertyValue] from ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionSettings where itemVersionId = ");
            sql.Append(itemVersionId);
            sql.Append(" order by [controlName], [propertyName]");
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());


        }

        public override IDataReader GetItemVersionSettings(int itemVersionId, string controlName)
        {
            if (!Utility.HasValue(controlName))
            {
                controlName = string.Empty;
            }

            var sql = new StringBuilder(256);
            sql.Append("select [settingsid], [controlName], [propertyName], [propertyValue] from ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionSettings where itemVersionId = ");
            sql.Append(itemVersionId);
            sql.Append(" and controlName = '");
            sql.Append(controlName.Trim());
            sql.Append("' order by [propertyName]");
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Code Analysis doesn't see HasValue as validation")]
        public override IDataReader GetItemVersionSetting(int itemVersionId, string controlName, string propertyName)
        {
            if (!Utility.HasValue(controlName))
            {
                controlName = string.Empty;
            }
            if (!Utility.HasValue(propertyName))
            {
                propertyName = string.Empty;
            }

            var sql = new StringBuilder(256);
            sql.Append("select [settingsid], [controlName], [propertyName], [propertyValue] from ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionSettings where itemVersionId = ");
            sql.Append(itemVersionId);
            sql.Append(" and controlName = '");
            sql.Append(controlName.Trim());
            sql.Append("' and propertyName = '");
            sql.Append(propertyName.Trim());
            sql.Append("' order by [propertyName]");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }


        #endregion

        #region "Add Methods"

        public override int AddItem(IDbTransaction trans, int itemTypeId, int portalId, int moduleId, Guid itemIdentifier)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar((SqlTransaction)trans, NamePrefix + "spInsertItem", itemTypeId, portalId, moduleId, itemIdentifier), CultureInfo.InvariantCulture);
        }

        public override int AddItemVersion(int itemId, int originalItemVersionId, string name, string description, string startDate, string endDate, int languageId, int authorUserId, string metaKeywords, string metaDescription, string metaTitle, int displayTabId, bool disabled, string thumbnail, Guid itemVersionIdentifier, string url, bool newWindow, int revisingUserId)
        {
            //return the new versionId
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, NamePrefix + "spInsertItemVersion", Utility.CreateIntegerParam("@ItemId", itemId), Utility.CreateIntegerParam("@OriginalitemVersionId", originalItemVersionId), Utility.CreateNvarcharParam("@Name", name, 255), description, Utility.CreateDateTimeParam("@ItemVersionDate", DateTime.Now.ToString(CultureInfo.InvariantCulture)), Utility.CreateDateTimeParam("@StartDate", startDate), Utility.CreateDateTimeParam("@EndDate", endDate), Utility.CreateIntegerParam("@LanguageId", languageId), Utility.CreateIntegerParam("@AuthorUserId", authorUserId), Utility.CreateNvarcharParam("@MetaKeywords", metaKeywords, 255), Utility.CreateNvarcharParam("@MetaDescription", metaDescription, 400), Utility.CreateNvarcharParam("@MetaTitle", metaTitle, 255), Utility.CreateIntegerParam("@DisplayTabId", displayTabId), Utility.CreateBitParam("@Disabled", disabled), Utility.CreateVarcharParam("@Thumbnail", thumbnail, 300), Utility.CreateGuidParam("@itemVersionIdentifier", itemVersionIdentifier), Utility.CreateNvarcharParam("@url", url, 255), Utility.CreateBitParam("@newWindow", newWindow), Utility.CreateIntegerParam("@RevisingUserId", revisingUserId)), CultureInfo.InvariantCulture);
        }

        public override int AddItemVersion(IDbTransaction trans, int itemId, int originalItemVersionId, string name, string description, string startDate, string endDate, int languageId, int authorUserId, string metaKeywords, string metaDescription, string metaTitle, int displayTabId, bool disabled, string thumbnail, Guid itemVersionIdentifier, string url, bool newWindow, int revisingUserId)
        {
            //return the new versionId			
            return Convert.ToInt32(SqlHelper.ExecuteScalar((SqlTransaction)trans, NamePrefix + "spInsertItemVersion", Utility.CreateIntegerParam("@ItemId", itemId), Utility.CreateIntegerParam("@OriginalitemVersionId", originalItemVersionId), Utility.CreateNvarcharParam("@Name", name, 255), Utility.CreateNtextParam("@Description", description), Utility.CreateDateTimeParam("@ItemVersionDate", DateTime.Now.ToString(CultureInfo.InvariantCulture)), Utility.CreateDateTimeParam("@StartDate", startDate), Utility.CreateDateTimeParam("@EndDate", endDate), Utility.CreateIntegerParam("@LanguageId", languageId), Utility.CreateIntegerParam("@AuthorUserId", authorUserId), Utility.CreateNvarcharParam("@MetaKeywords", metaKeywords, 255), Utility.CreateNvarcharParam("@MetaDescription", metaDescription, 400), Utility.CreateNvarcharParam("@MetaTitle", metaTitle, 255), Utility.CreateIntegerParam("@DisplayTabId", displayTabId), Utility.CreateBitParam("@Disabled", disabled), Utility.CreateVarcharParam("@Thumbnail", thumbnail, 300), Utility.CreateGuidParam("@itemVersionIdentifier", itemVersionIdentifier), Utility.CreateNvarcharParam("@url", url, 255), Utility.CreateBitParam("@newWindow", newWindow), Utility.CreateIntegerParam("@RevisingUserId", revisingUserId)), CultureInfo.InvariantCulture);
        }

        public override void AddItemVersionSetting(int itemVersionId, string controlName, string propertyName, string propertyValue)
        {

            SqlHelper.ExecuteScalar(ConnectionString, NamePrefix + "spInsertItemVersionSettings", Utility.CreateIntegerParam("@ItemVersionId", itemVersionId), Utility.CreateNvarcharParam("@ControlName", controlName, 200), Utility.CreateNvarcharParam("@PropertyName", propertyName, 200), Utility.CreateNvarcharParam("@PropertyValue", propertyValue, 200));
        }


        public override void AddItemVersionSetting(IDbTransaction trans, int itemVersionId, string controlName, string propertyName, string propertyValue)
        {
            //return the new versionId			
            SqlHelper.ExecuteScalar((SqlTransaction)trans, NamePrefix + "spInsertItemVersionSettings", Utility.CreateIntegerParam("@ItemVersionId", itemVersionId), Utility.CreateNvarcharParam("@ControlName", controlName, 200), Utility.CreateNvarcharParam("@PropertyName", propertyName, 200), Utility.CreateNvarcharParam("@PropertyValue", propertyValue, 200));
        }

        public override void AddCategoryVersion(int itemVersionId, int itemId, int sortOrder, int childDisplayTabId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spInsertCategoryVersion", itemVersionId, itemId, sortOrder, childDisplayTabId);
        }

        public override void AddCategoryVersion(IDbTransaction trans, int itemVersionId, int itemId, int sortOrder, int childDisplayTabId)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spInsertCategoryVersion", itemVersionId, itemId, sortOrder, childDisplayTabId);
        }

        public override void AddItemRelationship(int childItemId, int childItemVersionId, int parentItemId, int RelationshipTypeId, string startDate, string endDate, int sortOrder)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spInsertItemRelationship", Utility.CreateIntegerParam("@ChildItemId", childItemId), Utility.CreateIntegerParam("@ChildItemVersionId", childItemVersionId), Utility.CreateIntegerParam("@ParentItemId", parentItemId), Utility.CreateIntegerParam("@RelationshipTypeId", RelationshipTypeId), Utility.CreateDateTimeParam("@StartDate", startDate), Utility.CreateDateTimeParam("@EndDate", endDate), Utility.CreateIntegerParam("@SortOrder", sortOrder));
        }

        public override void AddItemRelationshipWithOriginalSortOrder(IDbTransaction trans, int childItemId, int childItemVersionId, int parentItemId, int RelationshipTypeId, string startDate, string endDate, int originalItemVersionId)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spInsertItemRelationshipWithPreviousSortOrder", Utility.CreateIntegerParam("@ChildItemId", childItemId), Utility.CreateIntegerParam("@ChildItemVersionId", childItemVersionId), Utility.CreateIntegerParam("@ParentItemId", parentItemId), Utility.CreateIntegerParam("@RelationshipTypeId", RelationshipTypeId), Utility.CreateDateTimeParam("@StartDate", startDate), Utility.CreateDateTimeParam("@EndDate", endDate), Utility.CreateIntegerParam("@OriginalItemVersionId", originalItemVersionId));
        }

        public override void AddItemRelationship(IDbTransaction trans, int childItemId, int childItemVersionId, int parentItemId, int RelationshipTypeId, string startDate, string endDate, int sortOrder)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spInsertItemRelationship", Utility.CreateIntegerParam("@ChildItemId", childItemId), Utility.CreateIntegerParam("@ChildItemVersionId", childItemVersionId), Utility.CreateIntegerParam("@ParentItemId", parentItemId), Utility.CreateIntegerParam("@RelationshipTypeId", RelationshipTypeId), Utility.CreateDateTimeParam("@StartDate", startDate), Utility.CreateDateTimeParam("@EndDate", endDate), Utility.CreateIntegerParam("@SortOrder", sortOrder));
        }

        public override void AddArticleVersion(int itemVersionId, int itemId, string versionNumber, string versionDescription, string articleText, string referenceNumber)
        {
            //TODO: covert these to parameters
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spInsertArticleVersion", itemVersionId, itemId, versionNumber, versionDescription, articleText, referenceNumber);
        }

        public override void AddArticleVersion(IDbTransaction trans, int itemVersionId, int itemId, string versionNumber, string versionDescription, string articleText, string referenceNumber)
        {
            //TODO: covert these to parameters
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spInsertArticleVersion", itemVersionId, itemId, versionNumber, versionDescription, articleText, referenceNumber);
        }

        public override void AddItemView(int itemId, int itemVersionId, int userId, int tabId, string ipAddress)
        {
            //TODO: covert these to parameters
            SqlHelper.ExecuteScalar(ConnectionString, NamePrefix + "spInsertItemView", itemId, itemVersionId, userId, tabId, ipAddress);
        }

        #endregion


        #region "Update Methods"

        public override void UpdateItemVersion(IDbTransaction trans, int itemId, int itemVersionId, int approvalStatusId, int userId, string approvalComments)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spUpdateItemVersion", itemId, itemVersionId, approvalStatusId, userId, approvalComments);
        }

        public override void UpdateItem(IDbTransaction trans, int itemId, int moduleId)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spUpdateItem", itemId, moduleId);
        }



        public override void UpdateDescription(int itemVersionId, string description, string metaDescription)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spUpdateItemDescription", itemVersionId, description, metaDescription);
        }

        public override void UpdateItemRelationship(int itemRelationshipId, int sortOrder)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spUpdateItemRelationship", Utility.CreateIntegerParam("@ItemRelationshipId", itemRelationshipId), Utility.CreateIntegerParam("@SortOrder", sortOrder));
        }



#endregion


        #region "Delete Method"

        public override void DeleteItem(int itemId)
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spDeleteItem", itemId);
        }

        public override void DeleteItems()
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spDeleteAllItems");
        }

        #endregion

        public override DataSet GetCategoryItems(int categoryId, int itemTypeId)
        {
            return GetCategoryItems(categoryId, itemTypeId, -1);
        }

        public override DataSet GetCategoryItems(int categoryId, int itemTypeId, int approvalStatusId)
        {
            return GetCategoryItems(categoryId, itemTypeId, approvalStatusId, SearchSortOption.ItemId);
        }

        public override DataSet GetCategoryItems(int categoryId, int itemTypeId, int approvalStatusId, SearchSortOption sort)
        {
            var sql = new StringBuilder(512);

            sql.Append("select ");
            sql.Append("childItemId ItemId, ChildItemVersionId ItemVersionId, iv.DisplayTabId, iv.[Name] 'Name', ItemVersionDate, it.[Name] 'ItemType' ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("fnGetChildItemsAdminSearch(");
            sql.Append(categoryId);
            sql.Append(") fp ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersion iv on  (fp.ChildItemId = iv.ItemId and fp.ChildItemVersionId = iv.ItemVersionId) ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("ItemType it on (fp.ItemTypeId = it.ItemTypeId) ");
            sql.Append("where fp.ItemTypeId = ");
            sql.Append(itemTypeId);

            if (approvalStatusId != -1)
            {
                sql.Append(" and ApprovalStatusId = ");//3 ");
                sql.Append(approvalStatusId);
            }

            sql.Append(" order by ");
            switch (sort)
            {
                case SearchSortOption.ItemId:
                    sql.Append("ChildItemId");
                    break;
                case SearchSortOption.Name:
                    sql.Append("iv.Name");
                    break;
                case SearchSortOption.UpdatedDate:
                    sql.Append("ItemVersionDate");
                    break;
                default:
                    sql.Append("ChildItemId");
                    break;
            }

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataSet GetAdminKeywordSearch(string searchString, int itemTypeId, int approvalStatusId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append(" select i.itemId, i.itemVersionId, i.DisplayTabId,  i.ItemVersionDate, it.[Name] 'itemType', Ltrim(str(i.itemId)) + '-' + i.[name] as 'Name', Ltrim(str(i.itemId)) + '-' + i.[name] as 'ListName' ");
            sql.Append(" from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("ItemType it on (i.ItemTypeId = it.ItemTypeId) ");
            sql.Append(" where  ");
            sql.Append(" i.portalId = '");
            sql.Append(portalId.ToString(CultureInfo.InvariantCulture));
            sql.Append("' ");
            sql.Append(" and (i.[name] like @partialSearchString ");
            sql.Append(" or i.[description] like @partialSearchString ");
            sql.Append(" or ltrim(str(i.[itemId])) = @exactSearchString) ");
            sql.Append(" and i.itemtypeid = ");
            sql.Append(itemTypeId);
            sql.Append(" and approvalStatusId = ");
            sql.Append(approvalStatusId);
            sql.Append(" and isCurrentVersion =1 order by i.[name]");

            var partialSearchString = new SqlParameter("@partialSearchString", "%" + searchString + "%");
            var exactSearchString = new SqlParameter("@exactSearchString", searchString);

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString(), partialSearchString, exactSearchString);
        }

        public override DataSet GetAdminKeywordSearch(string searchString, int itemTypeId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append(" select i.itemId, i.itemVersionId, i.ItemVersionDate, it.[Name] 'itemType', Ltrim(str(i.itemId)) + '-' + i.[name] as 'Name', Ltrim(str(i.itemId)) + '-' + i.[name] as 'ListName' ");
            sql.Append(" from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("ItemType it on (i.ItemTypeId = it.ItemTypeId) ");
            sql.Append(" where  ");
            sql.Append(" i.portalId = '");
            sql.Append(portalId.ToString(CultureInfo.InvariantCulture));
            sql.Append("' ");
            sql.Append(" and (i.[name] like '%");
            sql.Append(searchString);
            sql.Append("%' ");
            sql.Append(" or i.[description] like '%");
            sql.Append(searchString);
            sql.Append("%') ");
            sql.Append(" and i.itemtypeid = ");
            sql.Append(itemTypeId);
            sql.Append(" and isCurrentVersion =1 ");

            return SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataTable GetDataTable(string sql, int portalId)
        {
            if (String.IsNullOrEmpty(sql))
            {
                return null;
            }
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql);
            return ds.Tables[0];
        }

        public override IDictionary GetViewableCategoryIds(int permissionId, int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("SELECT ");
            sql.Append("rp.CategoryId ");
            sql.Append("FROM ");
            sql.Append(NamePrefix);
            sql.Append("CategoryRolePermission rp ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("Item i on (rp.CategoryId = i.ItemId) ");
            sql.Append("join ");
            sql.Append(DatabaseOwner);
            sql.Append(ObjectQualifier);
            sql.Append("Roles r on (rp.RoleId = r.RoleId and i.PortalId = r.PortalId) ");
            sql.Append("WHERE ");
            sql.Append("rp.PermissionId = ");
            sql.Append(permissionId);
            sql.Append(" and r.RoleName in (");
            sql.Append(GetUserRoles(portalId));
            sql.Append(")");

            IDataReader dr = null;
            IDictionary d = new Hashtable();

            try
            {
                dr = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
                while (dr.Read())
                {
                    d.Add(dr["CategoryId"], null);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
            }

            return d;
        }

        public override IDictionary GetViewableArticleIds(int permissionId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append("SELECT ");
            sql.Append("distinct ci.ItemId ArticleId ");
            sql.Append("FROM ");
            sql.Append(NamePrefix);
            sql.Append("vwArticles a ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems ci on (a.ItemId = ci.ItemId) ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("CategoryRolePermission rp on (ci.ParentItemId = rp.CategoryId) ");
            sql.Append("join ");
            sql.Append(DatabaseOwner);
            sql.Append(ObjectQualifier);
            sql.Append("Roles r on (rp.RoleId = r.RoleId) ");
            sql.Append("WHERE ");
            sql.Append("a.IsCurrentVersion = 1 ");
            sql.Append("and ci.RelationshipName = 'Item to Parent Category' ");
            sql.Append("and a.PortalId = r.PortalId ");
            sql.Append(" and r.RoleName in (");
            sql.Append(GetUserRoles(portalId));
            sql.Append(") ");
            sql.Append("and rp.PermissionId = ");
            sql.Append(permissionId);

            IDataReader dr = null;
            IDictionary d = new Hashtable();

            try
            {
                dr = SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
                while (dr.Read())
                {
                    d.Add(dr["ArticleId"], null);
                }
            }
            finally
            {
                if (dr != null) dr.Close();
            }

            return d;
        }

        private static string GetUserRoles(int portalId)
        {
            if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var sb = new StringBuilder(128);

                UserInfo ui = UserController.Instance.GetCurrentUserInfo();
                var rc = new RoleController();
                //Not sure why DNN methods that return roles don't consistently return RoleInfo objects. hk
                if (ui.IsSuperUser)
                {
                    foreach (RoleInfo role in rc.GetRoles(portalId))
                    {
                        sb.Append("'");
                        sb.Append(role.RoleName);
                        sb.Append("',");
                    }
                }
                else
                {
                    var roles = RoleController.Instance.GetUserRoles(ui, false);
                    foreach (RoleInfo s in roles)
                    {
                        sb.Append("'");
                        sb.Append(s.RoleName);
                        sb.Append("',");
                    }

                }

                //trim the last ,
                if (sb.Length > 0) sb.Length -= 1;

                return sb.ToString();
            }
            return "'Everyone'"; //is this always 'Everyone'?
        }

        public override DataTable GetAssignedRoles(int categoryId)
        {
            var sql = new StringBuilder(256);

            sql.Append("SELECT ");
            sql.Append("r.RoleId, RoleName ");
            sql.Append("FROM ");
            sql.Append(NamePrefix);
            sql.Append("CategoryRolePermission cp ");
            sql.Append("join ");
            sql.Append(DatabaseOwner);
            sql.Append(ObjectQualifier);
            sql.Append("Roles r on (cp.RoleId = r.RoleId) ");
            sql.Append("WHERE ");
            sql.Append("CategoryId = ");
            sql.Append(categoryId);
            sql.Append(" ORDER BY ");
            sql.Append("RoleName ");

            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override void InsertPermission(int categoryId, int roleId, int permissionId, int userId)
        {
            var sql = new StringBuilder(256);

            sql.Append("insert ");
            sql.Append(NamePrefix);
            sql.Append("CategoryRolePermission ");
            sql.Append("(CategoryId, RoleId, PermissionId, RevisingUser, RevisionDate) values (");
            sql.Append(categoryId);
            sql.Append(", ");
            sql.Append(roleId);
            sql.Append(", ");
            sql.Append(permissionId);
            sql.Append(", ");
            sql.Append(userId);
            sql.Append(", ");
            sql.Append("getdate())");

            SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override void DeletePermissions(int categoryId)
        {
            var sql = new StringBuilder(256);

            sql.Append("delete ");
            sql.Append(NamePrefix);
            sql.Append("CategoryRolePermission ");
            sql.Append("where ");
            sql.Append("CategoryId = ");
            sql.Append(categoryId);

            SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override DataTable GetChildrenInCategory(int categoryId, int childTypeId, int maxItems, int portalId)
        {
            return GetChildrenInCategory(categoryId, childTypeId, maxItems, portalId, "childname");
        }

        public override DataTable GetChildrenInCategory(int categoryId, int childTypeId, int maxItems, int portalId, string sortOrder)
        {
            var sql = new StringBuilder();

            sql.Append("select ");
            if (maxItems > -1)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, "top {0} ", maxItems);
            }
            sql.Append("il.ChildItemId, il.Thumbnail, il.CategoryName, il.ChildName, il.ChildDescription, il.ChildItemTypeId, il.StartDate, il.LastUpdated, il.CreatedDate, il.AuthorUserId, u.DisplayName, il.Author, il.RevisingUserId ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemListing il ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i on (il.ChildItemId = i.ItemId) join ");
            sql.Append(_databaseOwner);
            sql.Append(_objectQualifier);
            sql.Append("users u on (u.UserId = il.AuthorUserId) ");
            sql.Append("where ");
            sql.Append("il.PortalId = ");
            sql.Append(portalId);
            if (childTypeId > 0)
            {
                sql.Append(" and il.ChildItemTypeId = ");
                sql.Append(childTypeId);
            }
            sql.Append(" and i.StartDate < GetDate() ");
            sql.Append(" and (i.EndDate > GetDate() or i.EndDate is null) ");
            sql.Append(" and i.IsCurrentVersion=1 ");
            if (categoryId > -1)
            {
                sql.Append(" and il.ItemId =");
                sql.Append(categoryId);
            }

            sql.Append(" order by ");
            if (Utility.HasValue(sortOrder))
            {
                sql.Append("il.");
                sql.Append(sortOrder);
                sql.Append(", ");
            }
            sql.Append("CategoryName");

            DataTable dt = Instance().GetDataTable(sql.ToString(), portalId);

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            LimitRows(maxItems, dt);

            return dt;
        }

        public override DataTable GetChildrenInCategoryPaging(int categoryId, int childTypeId, int maxItems, int portalId, bool customSort, bool customSortDirection, string sortOrder, int index, int pageSize )
        {
            DataTable dt = SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetChildrenInCategoryPaging", childTypeId, categoryId, index, pageSize, customSort, customSortDirection, sortOrder, portalId).Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            //LimitRows(maxItems, dt);

            return dt;
        }

        private static void LimitRows(int maxItems, DataTable dt)
        {
            //remove rows over the limit
            if (maxItems != -1)
            {
                var al = new ArrayList();
                for (int i = maxItems; i < dt.Rows.Count; i++)
                {
                    al.Add(dt.Rows[i]);
                }

                foreach (DataRow r in al)
                {
                    dt.Rows.Remove(r);
                }
            }
        }

        public override DataTable GetMostPopular(int categoryId, int childTypeId, int maxItems, int portalId)
        {
            var sql = new StringBuilder();

            sql.Append("select ");
            if (maxItems > -1)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, "top {0} ", maxItems);
            }
            sql.Append("(select count(*) TimesViewed from ");
            sql.Append(NamePrefix);
            sql.Append("itemview where itemid = il.childitemid ) TimesViewed, ");
            sql.Append("(select count(*) TimesViewed from ");
            sql.Append(NamePrefix);
            sql.Append("itemview where itemid = il.childitemid ) TotalRows, ");
            sql.Append(" il.ItemId, il.CategoryName, il.ChildName, ");
            sql.Append(" il.ChildDescription, il.ChildItemId, il.ChildItemTypeId, il.Thumbnail, il.StartDate, il.AuthorUserId, il.RevisingUserId, u.DisplayName, il.Author, i.ViewCount, i.CommentCount ");
			sql.Append("from ");
			sql.Append(NamePrefix);
            sql.Append("vwItemListing il");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i on (il.ChildItemId = i.ItemId) ");
            sql.Append(" join ");
            sql.Append(_objectQualifier);
            sql.Append("users u on (u.UserId = il.AuthorUserId) ");
            //sql.Append(" left join ");
            //sql.Append(NamePrefix);
            //sql.Append("ItemView iv on (il.ChildItemId = iv.ItemId) ");
            sql.Append(" where il.PortalId = ");
            sql.Append(portalId);
            sql.Append(" and il.ChildItemTypeId = ");
            sql.Append(childTypeId);
            sql.Append(" and i.StartDate < GetDate() ");
            sql.Append(" and (i.EndDate > GetDate() OR i.EndDate is null) ");
            sql.Append(" and i.IsCurrentVersion=1 ");
            if (categoryId > -1)
            {
                sql.Append(" and il.ItemId = ");
                sql.Append(categoryId);
            }
            //sql.Append(" group by ");
            //sql.Append("il.ItemId, il.CategoryName, il.ChildName, il.ChildDescription,  il.ChildItemId, il.Thumbnail, il.StartDate ");
            sql.Append(" order by ");
            sql.Append(" TimesViewed desc");

            DataTable dt = Instance().GetDataTable(sql.ToString(), portalId);
            SecurityFilter sf = SecurityFilter.Instance;

            sf.FilterCategories(dt, portalId);
            LimitRows(maxItems, dt);
            return dt;
        }

        public override DataTable GetMostRecent(int childTypeId, int maxItems, int portalId)
        {
            var sql = new StringBuilder();

            sql.Append("select ");
            if (maxItems > -1)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, "top {0}", maxItems);
            }
            sql.Append("ci.Name as ChildName,");
            sql.Append("ci.Thumbnail,");
            sql.Append("ci.itemId as ChildItemId,");
            sql.Append("ci.Description as ChildDescription, ");
            sql.Append("ci.ItemVersionID, ");
            sql.Append("ci.LastUpdated, ");
            sql.Append("ci.CreatedDate, ");
            sql.Append("ci.StartDate, ");
            sql.Append("i.name as CategoryName, ");
            sql.Append("ci.AuthorUserId, ");
            sql.Append("ci.Author, ");
            sql.Append("ci.ItemVersionIdentifier ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwChildItems ci ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i ");
            sql.Append("on (ci.parentItemId = i.itemId) ");
            sql.Append("where ");
            sql.Append("ci.PortalId = ");
            sql.Append(portalId);
            sql.Append(" and ci.IsCurrentVersion = 1 ");
            sql.Append(" and i.IsCurrentVersion = 1 ");
            sql.Append(" and ci.StartDate < GetDate() ");
            sql.Append(" and (ci.EndDate > GetDate() OR ci.EndDate is null) ");
            sql.Append(" and (ci.RelationshipTypeId = ");
            sql.Append(Engage.Dnn.Publish.Util.RelationshipType.ItemToParentCategory.GetId());
            if (childTypeId == ItemType.Category.GetId())
            {
                sql.Append(" or ci.RelationshipTypeId = ");
                sql.Append(Util.RelationshipType.CategoryToTopLevelCategory.GetId());
            }
            sql.Append(")");
            sql.Append(" and ci.itemTypeID = ");
            sql.Append(childTypeId);
            sql.Append(" order by ");
            sql.Append("ci.StartDate desc");

            DataTable dt = Instance().GetDataTable(sql.ToString(), portalId);

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            LimitRows(maxItems, dt);

            return dt;
        }

        public override DataTable GetMostRecentByCategoryId(int categoryId, int childTypeId, int maxItems, int portalId)
        {
            var sql = new StringBuilder(741);
            sql.Append("select ");
            if (maxItems > -1)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, "top {0} ", maxItems);
            }
            sql.Append(" il.ChildName, il.ChildDescription, il.itemId, il.ChilditemId, il.LastUpdated, child.StartDate, il.Thumbnail, il.CategoryName, child.itemVersionId, child.ItemVersionIdentifier, child.AuthorUserId, child.Author ");
            sql.AppendFormat(CultureInfo.InvariantCulture, "from {0}vwItemListing il ", NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}vwItems child on (il.ChilditemId = child.itemId) ", NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}vwItems parent on (il.itemId = parent.itemId) ", NamePrefix);
            sql.Append("where il.PortalId = @portalId ");
            sql.Append(" and il.ChildItemTypeId = @itemTypeId ");
            sql.Append(" and il.itemId = @categoryId ");
            sql.Append(" and child.iscurrentversion = 1 ");
            sql.Append(" and child.StartDate < GetDate() ");
            sql.Append(" and (child.EndDate > GetDate() OR child.EndDate is null) ");
            sql.Append(" and parent.iscurrentversion = 1 ");
            sql.Append(" and parent.StartDate < GetDate() ");
            sql.Append(" and (parent.EndDate > GetDate() OR parent.EndDate is null) ");
            sql.Append(" and il.StartDate < GetDate() ");
            sql.Append("order by child.StartDate desc ");

            DataTable dt = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@portalId", portalId), Utility.CreateIntegerParam("@categoryId", categoryId), Utility.CreateIntegerParam("@itemTypeId", childTypeId)).Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            LimitRows(maxItems, dt);

            return dt;
        }

        public override IDataReader GetComments(int itemId, int approvalStatusId)
        {
            return SqlHelper.ExecuteReader(ConnectionString, NamePrefix + "spGetComments", itemId, approvalStatusId);
        }

        private static void EnforceMaxCount(int? maxCount, DataTable photos)
        {
            if (maxCount.HasValue)
            {
                while (photos.Rows.Count > maxCount.Value)
                {
                    //Random random = new Random(Utility.GetRandomSeed());
                    photos.Rows.RemoveAt(photos.Rows.Count - 1);//random.Next(photos.Rows.Count));
                }
            }
        }

        internal override IDataReader GetModulesByModuleId(int moduleId)
        {
            string sql = String.Format(CultureInfo.InvariantCulture, "select * from {0}{1}Modules m join {0}{1}TabModules tm on (m.ModuleId = tm.ModuleId) where m.ModuleId = @moduleId", DatabaseOwner, ObjectQualifier);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql, Utility.CreateIntegerParam("@moduleId", moduleId));
        }

        #region "find items"
        public override int FindItemId(string name, int authorUserId)
        {
            var sql = new StringBuilder(741);
            sql.Append("select itemId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwItems il ", NamePrefix);
            sql.Append(" where name like @Name");
            sql.Append(" and authoruserid = @AuthorUserId");
            //TODO: this replace function below should be put into a larger utility of wildcard escapes
            name = name.Trim().Replace("[", "[[]");
            name = name.Trim().Replace("'", "''");
            var sqlName = new SqlParameter("@Name", '%' + name.Trim() + '%');

            var sqlAuthorUserId = new SqlParameter("@AuthorUserId", authorUserId);
            
            object o = SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString(), sqlName, sqlAuthorUserId);
            if (o != null)
            {
                return Convert.ToInt32(o.ToString());
            }
            return -1;
            
        }
        public override int FindItemId(string name, int authorUserId, int categoryId)
        {
            var sql = new StringBuilder(741);
            sql.Append("select itemId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwChildItems il ", NamePrefix);
            
            sql.Append(" where name like @Name");
            sql.Append(" and authoruserid = @AuthorUserId");
            sql.Append(" and parentItemId = @CategoryId");

            //TODO: this replace function below should be put into a larger utility of wildcard escapes
            //TODO: we need to escape '
            name = name.Trim().Replace("[", "[[]");
            name = name.Trim().Replace("'", "%");
            var sqlName = new SqlParameter("@Name", '%' + name.Trim() + '%');

            var sqlAuthorUserId = new SqlParameter("@AuthorUserId", authorUserId);
            var sqlCategoryId = new SqlParameter("@CategoryId", categoryId);

            object o = SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql.ToString(), sqlName, sqlAuthorUserId, sqlCategoryId);
            if (o != null)
            {
                return Convert.ToInt32(o.ToString());
            }
            return -1;

        }

        #endregion


        #region stats
        //statistics
        //Get number of items waiting for approval
        public override int WaitingForApprovalCount(int portalId)
        {
            string sql = String.Format(CultureInfo.InvariantCulture, "select count(itemversionId) from {0}vwItems vi where portalId = @portalId and vi.ApprovalStatusId = {1}", ObjectQualifier + ModuleQualifier, ApprovalStatus.Waiting.GetId());
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql, Utility.CreateIntegerParam("@portalId", portalId)));
        }

        public override int CommentsWaitingForApprovalCount(int portalId, int authorUserId)
        {
            string sql = String.Format(CultureInfo.InvariantCulture, "select count(commentId) from {0}comment pc join {0}vwItems vi on (vi.itemversionId = pc.itemversionid)  where vi.portalId = @portalId and vi.authorUserId = @AuthorUserId and pc.ApprovalStatusId = {1}", ObjectQualifier + ModuleQualifier, ApprovalStatus.Waiting.GetId());
            
            //select count(*) from dnn_publish_comment pc join dnn_publish_vwitems vi on (vi.itemversionId = pc.itemversionid) where pc.approvalstatusid != 3 and portalId =0 and vi.authoruserid = 1
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, CommandType.Text, sql, Utility.CreateIntegerParam("@portalId", portalId), Utility.CreateIntegerParam("@AuthorUserId", authorUserId)));
            
        }

        public override void ClearItemsViewCount(int portalId)
        {
            string sql = String.Format(CultureInfo.InvariantCulture, "update {0}item set viewcount=0 where portalId = @portalId", ObjectQualifier + ModuleQualifier);
            SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql, Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override void ClearItemsCommentCount(int portalId)
        {
            string sql = String.Format(CultureInfo.InvariantCulture, "update {0}item set commentcount=0 where portalId = @portalId", ObjectQualifier + ModuleQualifier);
            SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql, Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override void RunPublishStats()
        {
            SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, NamePrefix + "spRunPublishStats");
        }


        #endregion

        #region tags

        public override DataTable GetTags(int portalId)
        {
            var sql = new StringBuilder(128);
            sql.Append("select tagId, name, description, totalItems, mostRecentDate, languageid, datecreated ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("tags ");
            sql.Append("where PortalId = ");
            sql.Append(portalId);
            sql.Append(" order by ");
            sql.Append(" Name ");
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataTable GetTagsByString(string partialTag, int portalId)
        {
            var sql = new StringBuilder(128);
            sql.Append("select  ");
            sql.Append(" tagId, name, description, totalItems, mostRecentDate, languageid, datecreated ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("tags ");
            sql.Append("where PortalId = ");
            sql.Append(portalId);
            sql.Append(" and name like '");
            sql.Append(partialTag);
            sql.Append("%' ");
            sql.Append(" order by ");
            sql.Append(" Name ASC ");
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString());
            return ds.Tables[0];
        }

        public override DataTable GetTag(string tag, int portalId)
        {
            DataSet ds = SqlHelper.ExecuteDataset(_connectionString, NamePrefix + "spGetTag", portalId, Utility.CreateNvarcharParam("@TagName", tag, 256));
            return ds.Tables[0];
        }


        public override DataTable GetTag(int tagId)
        {
            var sql = new StringBuilder(128);
            sql.Append("select  ");
            sql.Append(" tagId, name, description, totalItems, mostRecentDate, languageid, datecreated ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("tags ");
            sql.Append("where TagId = @tagId");
            DataSet ds = SqlHelper.ExecuteDataset(ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@tagId", tagId));
            return ds.Tables[0];
        }

        public override DataTable GetPopularTags(int portalId, ArrayList tagList, bool selectTop)
        {
            DataSet ds = SqlHelper.ExecuteDataset(_connectionString, NamePrefix + "spGetpopulartags", portalId, (tagList == null ? null : Utility.CreateNvarcharParam("@TagList", ConvertTagsToXml(tagList).ToString(), 4000)), selectTop);
            return ds.Tables[0];
        }

        public override int GetPopularTagsCount(int portalId, ArrayList tagList, bool selectTop)
        {
            object count = SqlHelper.ExecuteScalar(_connectionString, NamePrefix + "spgetpopulartagscount", portalId, (tagList == null ? null : Utility.CreateNvarcharParam("@TagList", ConvertTagsToXml(tagList).ToString(), 4000)), selectTop);
            return !string.IsNullOrEmpty(count.ToString()) ? Convert.ToInt32(count, CultureInfo.InvariantCulture) : 0;
        }

        
        public override DataTable GetItemsFromTags(int portalId, ArrayList tagList)
        {
            DataSet ds = SqlHelper.ExecuteDataset(_connectionString, NamePrefix + "spGetItemsForTags", portalId, (tagList == null ? null : Utility.CreateNvarcharParam("@TagList", ConvertTagsToXml(tagList).ToString(), 4000)));
            return ds.Tables[0];
        }
        //implement paging for GetItemsFromTags
        public override DataTable GetItemsFromTagsPaging(int portalId, ArrayList tagList, int maxItems, int pageId, string sortOrder)
        {
            DataSet ds = SqlHelper.ExecuteDataset(_connectionString, NamePrefix + "spGetItemsForTagsPaging", portalId, (tagList == null ? null : Utility.CreateNvarcharParam("@TagList", ConvertTagsToXml(tagList).ToString(), 4000)), Utility.CreateIntegerParam("@PageIndex", pageId), Utility.CreateIntegerParam("@PageSize", maxItems), Utility.CreateNvarcharParam("@sortParameters", sortOrder, 400));
            return ds.Tables[0];
        }


        //private string GetMultiTagItems(int portalId, ArrayList tagId, int itemTypeId)
        //{
        //    StringBuilder sql = new StringBuilder(500);
        //    sql.Append(" from ");
        //    sql.Append(NamePrefix);
        //    sql.Append("vwItems vi join ");
        //    sql.Append(_databaseOwner);
        //    sql.Append(_objectQualifier);
        //    sql.Append("users u on (u.UserId = vi.AuthorUserId) ");

        //    sql.Append("join ");
        //    sql.Append(NamePrefix);
        //    sql.Append("itemversiontags ivt");

        //    sql.Append(" on (ivt.itemversionid = vi.itemversionid)");


        //    int count = 0;
        //    StringBuilder tagWhere = new StringBuilder(300);
        //    foreach (int i in tagId)
        //    {
        //        if (count == 0) tagWhere.Append(" and ivt.tagId = ");
        //        else tagWhere.Append(" or ivt.tagId = ");


        //        tagWhere.Append(i);
        //        count++;
        //    }

        //    sql.Append(" where iscurrentversion = 1 and portalId = ");
        //    sql.Append(portalId);
        //    if (itemTypeId > 0)
        //    {
        //        sql.Append(" and itemTypeID = ");
        //        sql.Append(itemTypeId);
        //    }
        //    sql.Append(tagWhere.ToString());
        //    //sql.Append(" order by [name]");
        //    return sql.ToString();

        //}

        public override IDataReader GetItemTags(int itemVersionId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select ");
            sql.Append(" t.tagId, t.name, t.description, t.totalItems, t.mostRecentDate, t.languageid, t.datecreated ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionTags ivt ");
            sql.Append(" join ");
            sql.Append(NamePrefix);
            sql.Append("Tags t on (t.tagId = ivt.tagid)");
            sql.Append("where ivt.itemVersionId = ");
            sql.Append(itemVersionId);
            sql.Append(" order by ");
            sql.Append(" t.Name ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override void AddItemTag(int itemVersionId, int tagId)
        {
            var sql = new StringBuilder(128);
            sql.Append("insert into ");
            sql.Append(NamePrefix);
            sql.Append("itemversiontags (ItemVersionId, TagId) Values(");
            sql.Append(itemVersionId);
            sql.Append(", ");
            sql.Append(tagId);
            sql.Append(")");
            SqlHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql.ToString());

        }

        public override void AddItemTag(IDbTransaction trans, int itemVersionId, int tagId)
        {
            var sql = new StringBuilder(128);
            sql.Append("insert into ");
            sql.Append(NamePrefix);
            sql.Append("itemversiontags (ItemVersionId, TagId) Values (");
            sql.Append(itemVersionId);
            sql.Append(", ");
            sql.Append(tagId);
            sql.Append(")");
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, CommandType.Text, sql.ToString());
        }

        public override int AddTag(Tag tag)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, NamePrefix + "spInsertTag", tag.Name, tag.Description, tag.TotalItems, tag.LanguageId, tag.PortalId), CultureInfo.InvariantCulture);
        }

        public override void UpdateTag(Tag tag)
        {

            SqlHelper.ExecuteNonQuery(ConnectionString, NamePrefix + "spUpdateTag", tag.TagId, tag.Description, tag.TotalItems);
        }
        public override int AddTag(IDbTransaction trans, Tag tag)
        {
            
            return Convert.ToInt32(SqlHelper.ExecuteScalar((SqlTransaction)trans, NamePrefix + "spInsertTag", tag.Name, tag.Description, tag.TotalItems, tag.LanguageId, tag.PortalId), CultureInfo.InvariantCulture);
            //return Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionString, NamePrefix + "spInsertTag", tag.Name, tag.Description, tag.TotalItems, tag.LanguageId, tag.PortalId), CultureInfo.InvariantCulture);
        }

        public override void UpdateTag(IDbTransaction trans, Tag tag)
        {
            SqlHelper.ExecuteNonQuery((SqlTransaction)trans, NamePrefix + "spUpdateTag", tag.TagId, tag.Description, tag.TotalItems);
        }


        public override int CheckItemTag(IDbTransaction trans, int itemId, int tagId)
        {
            var sql = new StringBuilder(250);
            sql.Append("select top 1 itemversionid from ");
            sql.Append(NamePrefix);
            sql.Append("itemversiontags ivt ");
            sql.Append(" where ivt.itemversionId in (select itemversionId from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions viv where itemId=");
            sql.Append(itemId);
            sql.Append(")and tagId = ");
            sql.Append(tagId);
            return Convert.ToInt32(SqlHelper.ExecuteScalar((SqlTransaction)trans, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }

        public override int CheckItemTag(int itemId, int tagId)
        {
            var sql = new StringBuilder(250);
            sql.Append("select top 1 itemversionid from ");
            sql.Append(NamePrefix);
            sql.Append("itemversiontags ivt ");
            sql.Append(" where ivt.itemversionId in (select itemversionId from ");
            sql.Append(NamePrefix);
            sql.Append("vwItemVersions viv where itemId=");
            sql.Append(itemId);
            sql.Append(")and tagId = ");
            sql.Append(tagId);
            return Convert.ToInt32(SqlHelper.ExecuteScalar(_connectionString, CommandType.Text, sql.ToString()), CultureInfo.InvariantCulture);
        }



        public static object ConvertTagsToXml(ArrayList tagIds)
        {
            if (tagIds == null || tagIds.Count == 0)
            {
                return DBNull.Value;
            }
            var sw = new StringWriter(CultureInfo.InvariantCulture);
            var xtw = new XmlTextWriter(sw);
            xtw.WriteStartElement("Tags");

            foreach (int tagId in tagIds)
            {
                xtw.WriteElementString("Tag", tagId.ToString(CultureInfo.InvariantCulture));
            }

            xtw.Close();
            return sw.ToString();
        }


        #endregion

        #region Publish Reports

        public override DataTable GetItemViewPaging(int itemTypeId, int categoryId, int pageIndex, int pageSize, string sortOrder, string startDate, string endDate, int portalId)
        {
            DataTable dt = SqlHelper.ExecuteDataset(ConnectionString, NamePrefix + "spGetItemViewReport", itemTypeId, categoryId, pageIndex, pageSize, startDate, endDate, sortOrder, portalId).Tables[0];

            SecurityFilter sf = SecurityFilter.Instance;
            sf.FilterCategories(dt, portalId);

            //LimitRows(maxItems, dt);

            return dt;
        }



        #endregion

        #region IPortable Methods

        public override IDataReader GetModuleInfo(int moduleId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwModuleInfo ");
            sql.Append("where ModuleId = ");
            sql.Append(moduleId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override IDataReader GetPublishTabName(int tabId, int portalId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwModuleInfo ");
            sql.Append("where tabid = ");
            sql.Append(tabId);
            sql.Append(" and FriendlyName = @FriendlyName ");
            sql.Append(" and PortalId = @PortalId ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), 
                Utility.CreateNvarcharParam("@FriendlyName", Utility.DnnFriendlyModuleName, 100),
                Utility.CreateIntegerParam("@PortalId", portalId));
           
        }

        public override IDataReader GetPublishTabId(string tabName, int portalId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwModuleInfo ");
            sql.Append("where TabName = @tabName");
            sql.Append(" and PortalId = @PortalId ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(), 
                Utility.CreateNvarcharParam("@tabName", tabName, 100),
                Utility.CreateIntegerParam("@PortalId", portalId));
        }

        public override IDataReader GetPublishModuleId(string moduleTitle, int portalId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwModuleInfo ");
            sql.Append("where ModuleTitle = @ModuleTitle");
            sql.Append(" and PortalId = @PortalId ");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(),
                Utility.CreateNvarcharParam("@ModuleTitle", moduleTitle, 100),
                Utility.CreateIntegerParam("@PortalId", portalId));
        }

        public override IDataReader GetItemVersionInfo(int itemVersionId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select ");
            sql.Append(" * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems ");
            sql.Append("where ItemVersionId = ");
            sql.Append(itemVersionId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());            
        }

        /// <summary>
        /// This currently returns all rows in the item relationship table. We may/could filter to only active at some point.
        /// </summary>
        /// <returns></returns>
        public override IDataReader GetAllRelationships(int moduleId)
        {
            var sql = new StringBuilder(128);
            
            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwRelationships ");
            sql.Append("Where ModuleId = ");
            sql.Append(moduleId);
            sql.Append("Order by ItemRelationshipId");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
        }

        public override IDataReader GetAllRelationshipsByPortalId(int portalId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwRelationships ");
            sql.Append("Where PortalId = ");
            sql.Append(portalId);
            sql.Append("Order by ItemRelationshipId");

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());
           
        }

        public override IDataReader GetItemVersion(Guid guid, int portalId)
        {
            var sql = new StringBuilder(128);

            sql.Append("select * ");
            sql.Append("from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems ");
            sql.Append("Where ItemVersionIDentifier = @ItemVersionIDentifier ");
            sql.Append("And PortalId = @PortalId");
            
            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString(),
                Utility.CreateGuidParam("@ItemVersionIdentifier", guid),
                Utility.CreateIntegerParam("@PortalId", portalId));
        }


        public override IDataReader GetItemRelationshipByIdentifiers(Guid parentItemIdentifier, Guid childItemVersionIdentifier, int currentPortalId)
        {
           return SqlHelper.ExecuteReader(ConnectionString, NamePrefix + "spGetItemRelationshipDataByIdentifiers", parentItemIdentifier, childItemVersionIdentifier, currentPortalId);
        }

        public override IDataReader GetItemVersionSettingsByPortalId(int portalId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select vs.*, ModuleId, ItemVersionIdentifier from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i  ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionSettings vs on (vs.ItemVersionId = i.ItemVersionId) ");
            sql.Append("where PortalId = ");
            sql.Append(portalId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());            
        }

        public override IDataReader GetItemVersionSettingsByModuleId(int moduleId)
        {
            var sql = new StringBuilder(256);

            sql.Append("select vs.*, ModuleId, ItemVersionIdentifier from ");
            sql.Append(NamePrefix);
            sql.Append("vwItems i  ");
            sql.Append("join ");
            sql.Append(NamePrefix);
            sql.Append("ItemVersionSettings vs on (vs.ItemVersionId = i.ItemVersionId) ");
            sql.Append("where ModuleId = ");
            sql.Append(moduleId);

            return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql.ToString());            
        }


        #endregion

    }
}


