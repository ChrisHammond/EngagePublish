<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.ArticleControls.ArticleEdit" CodeBehind="ArticleEdit.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="sectionhead" Src="~/controls/sectionheadcontrol.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/URLControl.ascx" %>

<div id="ArticleEdit" class="Normal">
    <dnn:sectionhead ID="shPublishInstructions" CssClass="Head" runat="server" Text="Basic Options" Section="publishInstructions" ResourceKey="shPublishInstructions" IsExpanded="False" />
    <hr />
    <div id="publishInstructions" runat="server" class="instructions">
        <asp:Label ID="lblPublishInstructions" runat="server" resourcekey="lblPublishInstructions" CssClass="Normal"></asp:Label>
    </div>
    <br />
    <dnn:sectionhead ID="shArticleEdit" CssClass="Head" runat="server" Text="Basic Options" Section="tblArticleEdit" ResourceKey="shArticleEdit" IsExpanded="True" />
    <hr />
    <div id="tblArticleEdit" runat="server">
        <div class="form-group">
            <dnn:label ID="lblArticleId" ResourceKey="lblArticleId" runat="server" />

            <asp:TextBox ID="txtArticleId" runat="server" CssClass="form-control" Enabled="false" />
        </div>

        <asp:PlaceHolder ID="phControls" runat="Server" />
        <div class="form-group dnnClear">
            <dnn:label ID="lblArticleText" ResourceKey="ArticleText" runat="server" cssclass="title" />
            <asp:PlaceHolder ID="phArticleText" runat="server" />
        </div>
        <div class="form-group dnnClear">
            <dnn:label ID="lblParentCategory" runat="server" ResourceKey="ParentCategory" />
        </div>
        <div class="form-group">
                    <asp:PlaceHolder ID="phParentCategory" runat="server" />
        </div>
        <div class="form-group">
                    <div class="dnnClear">
                        <dnn:label ID="lblRelatedCategories" runat="server" ResourceKey="RelatedCategories" />
                    </div>

                    <asp:PlaceHolder ID="phRelatedCategories" runat="server" />

        </div>
        <div class="form-group dnnClear">
            <dnn:label ID="lblDisplayOptions" ResourceKey="lblDisplayOptions" runat="server" cssclass="title dnnClear" />
            <div class="dnnClear">
                <asp:CheckBox ID="chkEmailAFriend" runat="server" ResourceKey="chkEmailAFriend" CssClass="checkbox" />
                <asp:CheckBox ID="chkPrinterFriendly" runat="server" ResourceKey="chkPrinterFriendly" CssClass="checkbox" />
                <asp:CheckBox ID="chkRatings" runat="server" ResourceKey="chkRatings" CssClass="checkbox" />
                <asp:CheckBox ID="chkComments" runat="server" ResourceKey="chkComments" CssClass="checkbox" />
                <asp:CheckBox ID="chkForumComments" runat="server" ResourceKey="chkForumComments" CssClass="checkbox" />
                <asp:CheckBox ID="chkReturnList" runat="server" ResourceKey="chkReturnList" CssClass="checkbox" />
                <asp:CheckBox ID="chkShowAuthor" runat="server" ResourceKey="chkShowAuthor" CssClass="checkbox" />
                <asp:CheckBox ID="chkTags" runat="server" ResourceKey="chkTags" CssClass="checkbox" />
            </div>
        </div>
        <div class="form-group dnnClear">
            <div class="dnnClear">
                <dnn:label ID="lblArticleAttachment" ResourceKey="lblArticleAttachment" runat="server" cssclass="title" />
            </div>
            <dnn:URL ID="ctlUrlSelection" runat="server" Width="325"
                ShowFiles="true"
                ShowUrls="false"
                ShowTabs="false"
                ShowLog="false"
                ShowTrack="false"
                Required="False"
                ShowNewWindow="False" />

        </div>
        <div class="form-group">
            <dnn:label ID="lblVersionNumber" ResourceKey="lblVersionNumber" runat="server" cssclass="title" />
            <asp:TextBox ID="txtVersionNumber" runat="server" TextMode="SingleLine" CssClass="form-control" />
        </div>
        <div class="form-group">
            <dnn:label ID="lblPreviousVersionDescription" ResourceKey="lblPreviousVersionDescription" runat="server" cssclass="title" />
            <asp:TextBox ID="txtPreviousVersionDescription" runat="server" TextMode="MultiLine" Columns="50" Rows="3" ReadOnly="true" CssClass="form-control" />
        </div>
        <div class="form-group">
            <dnn:label ID="lblVersionDescription" ResourceKey="lblVersionDescription" runat="server" cssclass="title" />
            <asp:TextBox ID="txtVersionDescription" runat="server" TextMode="MultiLine" Columns="50" Rows="3" CssClass="form-control" />
        </div>
        <div class="form-group">
            <dnn:label ID="lblDisplayOnCurrentPage" ResourceKey="lblDisplayOnCurrentPage" runat="server" cssclass="title" />
                    <asp:RadioButtonList ID="rblDisplayOnCurrentPage" runat="server" CssClass="Normal" Style="display: inline" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblDisplayOnCurrentPage_SelectedIndexChanged" />
                    <asp:DropDownList ID="ddlDisplayTabId" BorderWidth="0" DataValueField="TabID" DataTextField="TabName" runat="server" />
                    <asp:Label ID="lblPublishOverrideable" runat="server" />
        </div>
        <div class="form-group">

                    <dnn:label ID="lblForceDisplayTab" runat="server" cssclass="title" Visible="false" />


                    <asp:CheckBox ID="chkForceDisplayTab" runat="server" AutoPostBack="true" Visible="false" CssClass="checkbox" /><%--OnCheckedChanged="chkForceDisplayTab_CheckedChanged"--%>
        </div>
        <div class="form-group">
            <dnn:label ID="lblApproval" runat="server" ResourceKey="ApprovalStatus" />

                    <asp:CheckBox ID="chkUseApprovals" runat="server" Text="Use Approvals" ResourceKey="chkUseApprovals" AutoPostBack="true" OnCheckedChanged="chkUseApprovals_CheckedChanged" Visible="false" Checked="true" />
                    <asp:PlaceHolder ID="phApproval" runat="Server" />
                    <asp:Label ID="lblNotUsingApprovals" runat="server" Visible="false" />
        </div>
        <div class="form-group">
            <dnn:label ID="lblTagEntry" runat="server" ResourceKey="TagEntry" />
            <asp:PlaceHolder ID="phTagEntry" runat="server"></asp:PlaceHolder>
        </div>

    </div>
    <asp:UpdateProgress ID="upPublishRelationshipsProgress" runat="server">
        <ProgressTemplate>
            <div class="progressWrap">
                <div class="progressUpdateMessage">
                    <asp:Label ID="lblProgressUpdate" runat="server" resourcekey="lblProgressUpdate" />
                    <img src="<%=ApplicationUrl%><%=DesktopModuleFolderName %>images/progressbar_green.gif" alt="Updating" id="imgProgressUpdate" />
                </div>
            </div>
            <div class="progressUpdate">
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:TextBox ID="txtMessage" runat="server" Visible="False" ReadOnly="True" ForeColor="Red" EnableViewState="False" Columns="75" TextMode="MultiLine" Rows="5" /><br />
    <asp:ValidationSummary ID="ValidationSummary1" runat="server"></asp:ValidationSummary>
    <br />
    <asp:LinkButton ID="cmdUpdate" runat="server" ResourceKey="cmdUpdate" CssClass="dnnPrimaryAction"></asp:LinkButton>&nbsp;&nbsp;
    <asp:LinkButton ID="cmdCancel" runat="server" ResourceKey="cmdCancel" CausesValidation="False" CssClass="dnnSecondaryAction"></asp:LinkButton>&nbsp;&nbsp;
    <asp:LinkButton ID="cmdDelete" runat="server" resourceKey="cmdDelete" CausesValidation="False" Text="Delete" OnClick="cmdDelete_Click" CssClass="dnnSecondaryAction"></asp:LinkButton>
</div>
