//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Publish.Util
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Reflection;
    using Data;

	/// <summary>
	/// Summary description for Util.RelationshipType.
	/// </summary>
	public class RelationshipType
	{
		private readonly string _name = string.Empty;
		private int _id = -1;

		public static readonly Util.RelationshipType ItemToParentCategory = new Util.RelationshipType("Item To Parent Category");
		public static readonly Util.RelationshipType CategoryToTopLevelCategory = new Util.RelationshipType("Category To Top Level Category");
		public static readonly Util.RelationshipType ItemToSpecialContentArticle = new Util.RelationshipType("Article to Special Content Article");
		public static readonly Util.RelationshipType ItemToRelatedCategory = new Util.RelationshipType("Item To Related Category");
		public static readonly Util.RelationshipType ItemToRelatedArticle = new Util.RelationshipType("Item To Related Article");
		public static readonly Util.RelationshipType ItemToRelatedDocument = new Util.RelationshipType("Item To Related Document");
		public static readonly Util.RelationshipType ItemToRelatedProduct = new Util.RelationshipType("Item To Related Product");
		public static readonly Util.RelationshipType ItemToRelatedMedia = new Util.RelationshipType("Item To Related Media");
		public static readonly Util.RelationshipType ItemToVideo = new Util.RelationshipType("Item To Video");
		public static readonly Util.RelationshipType ItemToArticleLinks = new Util.RelationshipType("Item To Article Links");
		public static readonly Util.RelationshipType ItemToFeaturedItem = new Util.RelationshipType("Item To Featured Item");
    	public static readonly Util.RelationshipType MediaToMediaLargeImage = new Util.RelationshipType("Media to Media Large Image");
        
		//special content displayed in top right corner of an article, links, etc
		public static readonly Util.RelationshipType ItemToSlideshowImage = new Util.RelationshipType("Item To Slideshow Image");

		private RelationshipType(string name)
		{
			_name = name;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Not a simple/cheap operation")]
        public int GetId()
		{
			if (_id == -1)
			{
				IDataReader dr = null;

				try
				{
					dr = DataProvider.Instance().GetRelationshipType(_name);
					if (dr.Read())
					{
						_id = Convert.ToInt32(dr["RelationshipTypeId"], CultureInfo.InvariantCulture);
					}
				}
				finally
				{
					if (dr != null) dr.Close();
				}
			}

			return _id;
		}

        public static Util.RelationshipType GetFromId(int id, Type ct)
        {
            if (ct == null) throw new ArgumentNullException("ct");
            if (id < 1) throw new ArgumentOutOfRangeException("id");

            Type type = ct;
            while (type.BaseType != null)
            {
                FieldInfo[] fi = type.GetFields();

                foreach (FieldInfo f in fi)
                {
                    var cot = f.GetValue(type) as Util.RelationshipType;
                    if (cot != null)
                    {
                        //this prevents old, bogus classes defined in the code from killing the app
                        //client needs to check the return value
                        try
                        {
                            if (id == cot.GetId())
                            {
                                return cot;
                            }
                        }

                        catch
                        {
                            //drive on
                        }
                    }
                }

                type = type.BaseType; //check the super type 
            }

            return null;
        }

        public static Util.RelationshipType GetFromName(string name, Type ct)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (ct == null) throw new ArgumentNullException("ct");

            Type type = ct;
            while (type.BaseType != null)
            {
                FieldInfo[] fi = type.GetFields();

                foreach (FieldInfo f in fi)
                {
                    var cot = f.GetValue(type) as Util.RelationshipType;
                    if (cot != null)
                    {
                        //this prevents old, bogus classes defined in the code from killing the app
                        //client needs to check the return value
                        try
                        {
                            if (name.Equals(cot._name, StringComparison.OrdinalIgnoreCase))
                            {
                                return cot;
                            }
                        }
                        catch
                        {
                            //drive on
                        }
                    }
                }

                type = type.BaseType; //check the super type 
            }

            return null;
        }
	}
}

