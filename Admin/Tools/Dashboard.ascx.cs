// <copyright file="Dashboard.ascx.cs" company="Engage Software">
// Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
// by Engage Software ( https://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Publish.Admin.Tools
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// A control which contains a list of links to administrative tools for this module.
    /// </summary>
    public partial class Dashboard : ModuleBase
    {
        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        override protected void OnInit(EventArgs e)
        {
            Load += Page_Load;
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                //TODO: uncomment below when reporting works
                ////this.AddDashboardLink("ItemViewReport", "ItemViewReport");
                AddDashboardLink("DescriptionReplace", "DescriptionReplace");
                AddDashboardLink("ResetDisplayPage", "ResetDisplayPage");
                //CJH - January 2013
                AddDashboardLink("TagSearch", "TagSearch");
                AddDashboardLink("Recategorize", "Recategorize");

                //TODO: build a way to clear Publish cache
                //TODO: create a report for items that are approved but not longer have a valid parent category id
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Adds a link pointing to a tool in the dashboard.
        /// </summary>
        /// <param name="resourceKey">The resource key for the link's text.</param>
        /// <param name="toolValue">The value of the tool <c>QueryString</c> key for this tool.</param>
        private void AddDashboardLink(string resourceKey, string toolValue)
        {
            phAdminTools.Controls.Add(new LiteralControl(" <p><strong> "));
            phAdminTools.Controls.Add(GetDashboardLink(Localization.GetString(resourceKey, LocalResourceFile), toolValue));
            phAdminTools.Controls.Add(new LiteralControl(" </strong><br /> "));
            phAdminTools.Controls.Add(GetDashboardLinkDescription(Localization.GetString(resourceKey + ".Help", LocalResourceFile), toolValue));
            phAdminTools.Controls.Add(new LiteralControl(" </p> "));
        }

        /// <summary>
        /// Creates a link pointing to a tool visible in the dashboard.
        /// </summary>
        /// <param name="linkText">The text of the link.</param>
        /// <param name="toolValue">The value of the tool <c>QueryString</c> key for this tool.</param>
        /// <returns></returns>
        private HyperLink GetDashboardLink(string linkText, string toolValue)
        {
            var hl = new HyperLink
                         {
                             NavigateUrl =
                                 BuildLinkUrl("&amp;mid=" + ModuleId +
                                                   "&amp;ctl=admincontainer&amp;adminType=admintools&amp;tool=" +
                                                   toolValue),
                             Text = linkText
                         };
            return hl;
        }
        /// <summary>
        /// Creates a link pointing to a tool visible in the dashboard.
        /// </summary>
        /// <param name="descriptionText">The text of the link.</param>
        /// <param name="toolValue">The value of the tool <c>QueryString</c> key for this tool.</param>
        /// <returns></returns>
        private Literal GetDashboardLinkDescription(string descriptionText, string toolValue)
        {
            Literal lit = new Literal();
            lit.Text = descriptionText;
            return lit;
        }
    }
}

