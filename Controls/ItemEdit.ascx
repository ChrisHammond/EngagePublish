<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Controls.ItemEdit" CodeBehind="ItemEdit.ascx.cs" %>

<%@ Register TagPrefix="dnn" TagName="SectionHead" Src="~/controls/SectionHeadControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>

<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/TextEditor.ascx" %>
<%@ Register TagPrefix="engage" TagName="Thumbnail" Src="~/DesktopModules/EngagePublish/Controls/ThumbnailSelector.ascx" %>
<%@ Register TagPrefix="dnn" TagName="URL" Src="~/controls/URLControl.ascx" %>

<div class="form-group">
    <dnn:label ID="lblName" ResourceKey="lblName" runat="server" cssclass="title" />

    <asp:TextBox ID="txtName" runat="server" CssClass="txtName form-control" />
    <asp:CheckBox ID="chkDisplayAsHyperlink" runat="server" CssClass="checkbox" />
    <dnn:label ID="lblDisplayAsHyperlink" ResourceKey="lblDisplayAsHyperlink" runat="server" cssclass="title" />
</div>
<div class="form-group">
    <dnn:label ID="lblDescription" ResourceKey="lblDescription" runat="server" cssclass="title" />
    <div class="rightAlign">
        <asp:LinkButton ID="btnChangeDescriptionEditorMode" runat="server" OnClick="btnChangeDescriptionEditorMode_Click" />
    </div>
    <dnn:TextEditor ID="teDescription" runat="server" HtmlEncode="false" ChooseRender="false" TextRenderMode="Text" Visible="false" />
    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" CssClass="publishDescriptionTxt form-control" />
</div>

<div class="form-group">
    <dnn:label ID="lblAuthor" runat="server" ResourceKey="lblAuthor" />
    <asp:DropDownList ID="ddlAuthor" runat="server"
        OnSelectedIndexChanged="ddlAuthor_SelectedIndexChanged" AutoPostBack="true" CssClass="form-control" />

</div>
<div class="form-group">
    <dnn:label ID="lblAuthorName" runat="server" ResourceKey="lblAuthorName" />
    <asp:TextBox ID="txtAuthorName" runat="server" CssClass="NormalTextBox form-control"></asp:TextBox>
</div>
<div class="form-group">
    <dnn:label ID="lblUploadFile" runat="server" ResourceKey="lblUploadFile" />
    <engage:Thumbnail ID="thumbnailSelector" runat="server" />
</div>
<div class="form-group dnnClear">
    <dnn:label ID="lblStartDate" ResourceKey="lblStartDate" runat="server" CssClass="title" />
    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" />
</div>
<div class="form-group">
    <dnn:label ID="lblEndDate" ResourceKey="lblEndDate" runat="server" cssclass="title" />
    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" />
</div>
<div id="divSearchEngine">
    <%-- runat="server"--%>
    <div class="form-group">
        <dnn:label ID="lblMetaTitle" ResourceKey="lblMetaTitle" runat="server" cssclass="title" />
        <asp:TextBox ID="txtMetaTitle" runat="server" TextMode="SingleLine" Columns="50" Rows="1" CssClass="form-control" />
    </div>
    <div class="form-group">
        <dnn:label ID="lblMetaKeywords" ResourceKey="lblMetaKeywords" runat="server" cssclass="title" />
        <asp:TextBox ID="txtMetaKeywords" runat="server" TextMode="MultiLine" Columns="50" Rows="3" CssClass="form-control" />
    </div>
    <div class="form-group">
        <dnn:label ID="lblMetaDescription" ResourceKey="lblMetaDescription" runat="server" cssclass="title" />
        <asp:TextBox ID="txtMetaDescription" runat="server" TextMode="MultiLine" Columns="50" Rows="3" CssClass="form-control" />
    </div>
</div>
<div class="form-group">
    <dnn:label ID="lblChooseUrl" runat="server" ResourceKey="lblChooseUrl" />
    
        <%-- runat="server"--%>
        <dnn:URL ID="ctlUrlSelection" runat="server" Width="325"
            ShowFiles="true"
            ShowUrls="true"
            ShowTabs="true"
            ShowLog="false"
            ShowTrack="false"
            Required="False"
            ShowNewWindow="False"  class="form-control"/>
        <asp:CheckBox ID="chkNewWindow" resourcekey="chkNewWindow" runat="server" CssClass="checbox" />
   
</div>
