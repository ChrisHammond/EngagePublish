//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


using DotNetNuke.Entities.Controllers;

namespace Engage.Dnn.Publish.Security
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Web;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Search;
    using Util;

	/// <summary>
	/// Summary description for SecurityFilter.
	/// </summary>
	public abstract class SecurityFilter
	{
		//private static SecurityFilter instance;
		private const string SecuritySettingName = "EnablePermissionsForPortal";


		public abstract void FilterCategories(DataTable data, int portalId);
		public abstract void FilterArticles(SearchResultsInfoCollection data, int portalId);

        //TODO: should this still exist?
		private static bool IsAdmin
		{
			get
			{
			    if (HttpContext.Current.Request.IsAuthenticated)
				{
					return (UserController.Instance.GetCurrentUserInfo().IsSuperUser || IsUserInRole("Administrators"));
				}
			    return false;
			}
		}

		private static bool IsUserInRole(string roleName)
		{
			UserInfo ui = UserController.Instance.GetCurrentUserInfo();
            //var rc = new RoleController();
            //string[] roles = rc.GetRolesByUser(ui.UserID, ui.PortalID);

            foreach (string role in ui.Roles)
			{
				if (roleName == role) return true;
			}
	
			return false;
		}

		private static bool IsSecurityEnabled()
		{
			//this is an issue with the dnn searching, may need to look at this again
			if (HttpContext.Current == null) return false;

			//check the portal setting
			int portalId = PortalController.Instance.GetCurrentPortalSettings().PortalId;
            string s = HostController.Instance.GetString(SecuritySettingName + portalId);
            if (Utility.HasValue(s))
            {
                //if security is off.....it's off
                bool enable = Convert.ToBoolean(s, CultureInfo.InvariantCulture);
                if (enable == false) return false;
            }
            else
            {
                //no setting exists. TURN OFF!!!!
                return false;
            }

			//check if user is Admin, if so, no security
			if (IsAdmin) return false;

			//otherwise it's on
			return true;
		}

		private static SecurityFilter CreateFilter()
		{
			return (IsSecurityEnabled() ? RoleBasedSecurityFilter.Instance : NullSecurityFilter.Instance);
		}

		public static SecurityFilter Instance
		{
			get
			{
				return CreateFilter();

//				if (instance == null)
//				{
//					lock (typeof(SecurityFilter))
//					{
//						if (instance == null)
//						{
//							instance = CreateFilter();
//						}
//					}
//				}
//
//				return instance;
			}
		}

		public static bool IsSecurityEnabled(int portalId)
		{
            string s = HostController.Instance.GetString(SecuritySettingName + portalId);
			//Hashtable ht = PortalSettings.GetSiteSettings(portalId);
			//string s = Convert.ToString(ht[SecuritySettingName]);

			return (Utility.HasValue(s) ? Convert.ToBoolean(s, CultureInfo.InvariantCulture) : false);
		}

		public static void EnableSecurity(bool enabled, int portalId)
		{
            HostController.Instance.Update(SecuritySettingName + portalId, enabled.ToString());
		}
	}
}

