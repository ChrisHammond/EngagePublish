//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


namespace Engage.Dnn.Publish.Portability
{
    using System;
    using System.Xml.XPath;
    public static class XmlDirector
    {
        public static void Construct(XmlTransporter transporter, bool exportAll, int portalId)
        {
            if (transporter == null) throw new ArgumentNullException("transporter");

            transporter.BuildRootNode();
            //transporter.BuildCategories(exportAll);
            transporter.BuildCategories(true, portalId);
            transporter.BuildArticles(exportAll);
            transporter.BuildRelationships(exportAll);
            transporter.BuildItemVersionSettings(exportAll);
            transporter.BuildModuleSettings();

            //TODO: we need to export module settings
        }

        public static void Deconstruct(XmlTransporter transporter, IXPathNavigable doc)
        {
            //builder.ParseItemTypes(doc);
            //builder.ParseUtil.RelationshipTypes(doc);
            //builder.ParseApprovalStatusTypes(doc);
            transporter.ImportCategories(doc);
            transporter.ImportArticles(doc);
            transporter.ImportRelationships(doc);
            transporter.ImportItemVersionSettings(doc);
            transporter.ImportModuleSettings(doc);

            //TODO: we need to import module settings
           
        }
    }
}
