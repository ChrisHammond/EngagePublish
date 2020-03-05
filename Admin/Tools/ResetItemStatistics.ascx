<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Admin.Tools.ResetItemStatistics" CodeBehind="ResetItemStatistics.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div class="Normal">
    <dnn:Label Runat="server" ResourceKey="ResetDisplayPageHeader" CssClass="Head" EnableViewState="false" />
    <hr />

    <div>
        <dnn:label ID="lblResetItemViews" runat="server" />
        <asp:CheckBox ID="chkResetItemViewCount" runat="server" />
        <dnn:label ID="lblResetCommentCount" runat="server" />
        <asp:CheckBox ID="chkResetItemCommentCount" runat="server" />
    </div>
    <div>
        <asp:Label ID="SuccessMessage" runat="server" CssClass="NormalBold" EnableViewState="false" />
    </div>
    <asp:LinkButton ID="ResetButton" runat="server" ResourceKey="ResetButton" EnableViewState="false" />
</div>

