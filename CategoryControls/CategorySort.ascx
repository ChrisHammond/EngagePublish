<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.CategoryControls.CategorySort" CodeBehind="CategorySort.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="sectionhead" Src="~/controls/sectionheadcontrol.ascx" %>


<asp:Label ID="lblCategory" runat="server" CssClass="Head"></asp:Label>
<hr />
<dnn:sectionhead ID="shPublishInstructions" CssClass="Head" runat="server" Section="publishInstructions" ResourceKey="shPublishInstructions" IsExpanded="False" />
<hr />
<div id="publishInstructions" runat="server" class="instructions">
    <asp:Label ID="lblPublishInstructions" runat="server" resourcekey="lblPublishInstructions" CssClass="Normal"></asp:Label>
</div>
<asp:UpdatePanel ID="upnlPublish" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <asp:Panel ID="pnlSortList" runat="server">
            <div id="Publish_CategorySortInitialListDiv">
                <dnn:label ID="lblCategoryItems" runat="server" />
                <asp:ListBox ID="lbCategoryItems" CssClass="Publish_CategorySortUnsortedList" runat="server" DataTextField="ChildName" DataValueField="ItemRelationshipId" SelectionMode="single"></asp:ListBox>
                <br />
                <asp:LinkButton ID="lbMoveToSort" runat="server" resourcekey="lbMoveToSort" OnClick="lbMoveToSort_Click"></asp:LinkButton>
            </div>
            <div id="Publish_CategorySortSortedListDiv">
                <dnn:label ID="lblSortedItems" runat="server" />
                REMOVED - CJH 1/4/2014
            </div>
        </asp:Panel>
        <asp:Label ID="lblMessage" runat="server" Visible="false"></asp:Label>
    </ContentTemplate>
</asp:UpdatePanel>
<div id="publishSortSave">
    <asp:LinkButton ID="lbSaveSort" runat="server" resourcekey="lbSaveSort" OnClick="lbSaveSort_Click"></asp:LinkButton>
    <asp:LinkButton ID="lbCancel" runat="server" resourcekey="lbCancel" OnClick="lbCancel_Click"></asp:LinkButton>
</div>
