//Engage: Publish - https://www.engagesoftware.com
//Copyright (c) 2004-2010
//by Engage Software ( https://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.


namespace Engage.Dnn.Publish.Controls
{
    using System;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;

	public partial class EmailAFriend :  ModuleBase
	{
		#region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            Load += Page_Load;
            base.OnInit(e);
        }

        void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                txtFrom.Text = UserInfo != null ? UserInfo.Email : string.Empty;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void btnCancel_Click1(object sender, EventArgs e)
        {
            ClearEmailInput();
            //mpeEmailAFriend.Hide();
        }

        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member", Justification = "Controls use lower case prefix")]
        protected void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                //split and only use the first TO email address
                var emailList = txtTo.Text.Trim().Split(',');
                var emailTo = emailList[1];

                string message = Localization.GetString("EmailAFriend", LocalResourceFile);
                message = message.Replace("[Engage:Recipient]", emailTo);
                message = message.Replace("[Engage:Url]", GetItemLinkUrlExternal(ItemId));
                message = message.Replace("[Engage:From]", txtFrom.Text.Trim().Replace(",",string.Empty));
                message = message.Replace("[Engage:Message]", txtMessage.Text.Trim());

                string subject = Localization.GetString("EmailAFriendSubject", LocalResourceFile);
                subject = subject.Replace("[Engage:Portal]", PortalSettings.PortalName);

                Mail.SendMail(PortalSettings.Email, emailTo, "", subject, message, "", "HTML", "", "", "", "");
                ClearEmailInput();
            }
            catch (Exception ex)
            {
                //the email or emails entered are invalid or mail services are not configured
                Exceptions.LogException(ex);
            }
        }
		#endregion

		private void ClearEmailInput()
        {
            txtFrom.Text = string.Empty;
            txtMessage.Text = string.Empty;
            txtTo.Text = string.Empty;
            txtFrom.Text = UserInfo != null ? UserInfo.Email : string.Empty;
        }


	}
}

