<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Admin.Tools.Recategorize" CodeBehind="Recategorize.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>

<fieldset>

    <div class="dnnFormItem">
        <asp:TextBox runat="server" ID="txtTagSearch"></asp:TextBox>
        <asp:LinkButton runat="server" CssClass="dnnPrimaryAction" resourcekey="btnSearch" ID="btnSearch" OnClick="btnSearch_Click"></asp:LinkButton>
    </div>
    <div class="dnnFormItem">
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
    </div>

    <div class="dnnFormItem">
        <dnn:label ID="lblParentCategory" runat="server" ResourceKey="ParentCategory" />
        <asp:DropDownList ID="cboCategories" Runat="server" AutoPostBack="false" CssClass="Normal"></asp:DropDownList>
    </div>
    <div class="dnnFormItem">

        <dnn:label ID="lblRelatedCategory" runat="server" ResourceKey="RelatedCategory" />

        <asp:DropDownList ID="cboRelatedCategory" Runat="server" AutoPostBack="false" CssClass="Normal"></asp:DropDownList>
    </div>

    <div class="dnnFormItem">
        <asp:LinkButton runat="server" CssClass="dnnPrimaryAction" resourcekey="btnAddCategories" ID="btnAddCategories" OnClick="btnAddCategories_Click"></asp:LinkButton>
    </div>
</fieldset>


