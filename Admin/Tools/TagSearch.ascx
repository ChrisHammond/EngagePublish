<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Admin.Tools.TagSearch" CodeBehind="TagSearch.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>

<asp:TextBox runat="server" ID="txtTagSearch"></asp:TextBox>
<asp:LinkButton runat="server" CssClass="dnnPrimaryAction" resourcekey="btnTagSearch" ID="btnTagSearch" OnClick="btnTagSearch_Click"></asp:LinkButton>

<asp:GridView ID="dgItems"
    runat="server"
    EnableViewState="true"
    AlternatingRowStyle-CssClass="DataGrid_AlternatingItem Normal"
    HeaderStyle-CssClass="DataGrid_Header"
    RowStyle-CssClass="DataGrid_Item Normal"
    PagerStyle-CssClass="Normal"
    CssClass="Normal"
    AutoGenerateColumns="false"
    Width="100%"
    AllowPaging="false"
    AllowSorting="true">
    <Columns>

        <asp:TemplateField ShowHeader="true" HeaderText="SelectText" ItemStyle-CssClass="Publish_CheckBoxColumn">
            <ItemTemplate>
                <asp:CheckBox ID="chkSelect" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField ShowHeader="true" HeaderText="ID" SortExpression="ItemId">
            <ItemTemplate>
                <asp:HyperLink ID="hlId" runat="server" CssClass="Normal" NavigateUrl='<%# GetItemVersionLinkUrl(DataBinder.Eval(Container.DataItem,"ItemVersionId")) %>' Text='<%# DataBinder.Eval(Container.DataItem,"ItemId") %>' />
                <asp:Label ID="lblItemVersionId" runat="server" Visible="false" Text='<%# DataBinder.Eval(Container.DataItem,"ItemVersionId") %>'></asp:Label>
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField ShowHeader="true" HeaderText="Name" SortExpression="Name">
            <ItemTemplate>
                <asp:HyperLink ID="hlPreview" runat="server" CssClass="Normal" NavigateUrl='<%# GetItemVersionLinkUrl(DataBinder.Eval(Container.DataItem,"ItemVersionId")) %>'
                    Text='<%# DataBinder.Eval(Container.DataItem,"Name") %>'></asp:HyperLink>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
<dnn:label id="lblTag" runat="server" /> <br />
<asp:PlaceHolder ID="phTagEntry" runat="server"></asp:PlaceHolder>



<asp:LinkButton runat="server" CssClass="dnnPrimaryAction" resourcekey="btnAddTags" ID="btnAddTags" OnClick="btnAddTags_Click"></asp:LinkButton>