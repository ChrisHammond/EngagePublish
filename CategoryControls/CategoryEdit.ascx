<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.CategoryControls.CategoryEdit" CodeBehind="CategoryEdit.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="sectionhead" Src="~/controls/sectionheadcontrol.ascx" %>


<div id="CategoryEdit" class="Normal">
    <dnn:sectionhead id="shPublishInstructions" cssclass="Head" runat="server" text="Basic Options" section="publishInstructions" resourcekey="shPublishInstructions" isexpanded="False" />
    <hr />
    <div id="publishInstructions" runat="server" class="instructions">
        <asp:Label ID="lblPublishInstructions" runat="server" resourcekey="lblPublishInstructions" CssClass="Normal"></asp:Label>
    </div>
    <br />
    <dnn:sectionhead id="shCategoryEdit" cssclass="Head" runat="server" text="Basic Options" section="CategoryEdit" resourcekey="shCategoryEdit" isexpanded="True" />
    <hr />
    <div id="categoryEdit" class="Normal" runat="server">
        <div class="form-group" id="trCategoryId" runat="server">
            <dnn:label id="lblCategoryId" resourcekey="lblCategoryId" Runat="server" />
            <asp:Label ID="txtCategoryId" runat="server" />
        </div>
        <div class="form-group">
            <dnn:label id="lblSortOrder" resourcekey="lblSortOrder" Runat="server" />
            <asp:TextBox ID="txtSortOrder" resourcekey="txtSortOrder" runat="server" />
        </div>
        
        <asp:PlaceHolder ID="phItemEdit" runat="Server" />
            <div class="form-group">
                <dnn:label ID="lblChooseRoles" runat="server" ResourceKey="ChooseRoles" />
            </div>
            <div class="form-group" id="trCategoryPermissions" runat="server">
                <asp:PlaceHolder ID="phCategoryPermissions" runat="server" />
            </div>
        
            <div class="form-group">
                <dnn:label ID="lblParentCategory" runat="server" ResourceKey="ParentCategory" />
                <asp:PlaceHolder ID="phParentCategory" runat="server" />
            </div>

            <asp:Panel ID="TitlePanel" runat="server" CssClass="collapsePanelHeader">

            <div class="form-group">
                <asp:Label ID="lblCategoryEditExtendedHeader" CssClass="SubHead" resourcekey="lblCategoryEditExtendedHeader" runat="server" />
                <asp:Image ID="imgCategoryEditExtendedHeader" runat="server" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlCategoryEditExtended" runat="server">

        <div class="form-group">
                        <dnn:label ID="lblFeaturedArticles" runat="server" ResourceKey="ChooseFeatured" />
                        <asp:PlaceHolder ID="phFeaturedArticles" runat="server" />
        </div>

        <div class="form-group" id="rowCommentForum" runat="server">
            <dnn:Label ID="lblCommentForum" Runat="server" class="title" />
             <asp:DropDownList ID="ddlCommentForum" runat="server" />
        </div>
        

            <table class="PublishEditTable Normal">
                <tr id="Tr1" runat="server">
                    <td class="editTableLabelColumn nowrap">
                        <dnn:Label ID="lblRssUrl" Runat="server" class="title" />
                    </td>
                    <td class="fullWidth">
                        <asp:TextBox ID="txtRssUrl" runat="server"></asp:TextBox>
                        <hr />
                    </td>
                </tr>
            </table>

        </asp:Panel>

        <div class="form-group">
                    <dnn:Label ID="lblDisplayOnCurrentPage" ResourceKey="lblDisplayOnCurrentPage" Runat="server" class="title" />
                    <asp:RadioButtonList ID="rblDisplayOnCurrentPage" runat="server" CssClass="Normal" Style="display: inline" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblDisplayOnCurrentPage_SelectedIndexChanged" />
                    <asp:DropDownList ID="ddlDisplayTabId" BorderWidth="0" DataValueField="TabID" DataTextField="TabName" runat="server" />
                    <br />
                    <asp:Label ID="lblPublishOverrideable" runat="server" resourcekey="lblPublishOverrideable" />

        </div>
        <div class="form-group">
                    <dnn:label ID="lblForceDisplayTab" runat="server" class="title" Visible="false" />
                    <asp:CheckBox ID="chkForceDisplayTab" runat="server" OnCheckedChanged="chkForceDisplayTab_CheckedChanged" AutoPostBack="true" Visible="false" />
                    <hr />
        </div>
        <div class="form-group">
            <dnn:Label ID="lblChildDisplayTabId" ResourceKey="lblChildDisplayTabId" Runat="server" class="title" />
                    <asp:DropDownList ID="ddlChildDisplayTabId" BorderWidth="0" DataValueField='TabID' DataTextField="TabName" runat="server" />
                    <asp:CheckBox ID="chkResetChildDisplayTabs" runat="server" ResourceKey="chkResetChildDisplayTabs" />
                    <br />
                    <asp:Label ID="lblPublishOverrideableChild" runat="server" resourcekey="lblPublishOverrideable" />
        </div>
        <div class="form-group">
                    <dnn:label ID="lblApproval" runat="server" ResourceKey="ApprovalStatus" />
                    <asp:CheckBox ID="chkUseApprovals" runat="server" Text="Use Approvals" ResourceKey="chkUseApprovals" AutoPostBack="true" OnCheckedChanged="chkUseApprovals_CheckedChanged" Visible="false" Checked="true" />
                    <asp:PlaceHolder ID="phApproval" runat="Server" />
                    <asp:Label ID="lblNotUsingApprovals" runat="server" Text="Approvals are turned off.  Any changes you make will appear immediately on your website." ResourceKey="lblNotUsingApprovals" Visible="false" />
        </div>
        
        <asp:UpdateProgress ID="upPublishRelationshipsProgress" runat="server">
            <ProgressTemplate>
                <div class="progressWrap">
                    <div class="progressUpdateMessage">
                        <asp:Label ID="lblProgressUpdate" runat="server" resourcekey="lblProgressUpdate"></asp:Label>
                        <img src="<%=ApplicationUrl%><%=DesktopModuleFolderName %>images/progressbar_green.gif" alt="Updating" id="imgProgressUpdate" />
                    </div>
                </div>
                <div class="progressUpdate">
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <asp:TextBox ID="txtMessage" runat="server" Visible="False" ReadOnly="True" ForeColor="Red" EnableViewState="False" Columns="75" TextMode="MultiLine" Rows="5" />
        <br />
        <asp:LinkButton class="CommandButton" ID="cmdUpdate" resourcekey="cmdUpdate" runat="server" Text="Update" />&nbsp;&nbsp;
        <asp:LinkButton class="CommandButton" ID="cmdCancel" resourcekey="cmdCancel" runat="server" Text="Cancel" CausesValidation="False" />&nbsp;&nbsp;
        <asp:LinkButton class="CommandButton" ID="cmdDelete" resourcekey="cmdDelete" runat="server" Text="Delete" CausesValidation="false" OnClick="cmdDelete_Click" />&nbsp;&nbsp;
    </div>
</div>