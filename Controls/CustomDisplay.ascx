<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Controls.CustomDisplay" CodeBehind="CustomDisplay.ascx.cs" %>
<asp:Label ID="lblMessage" Font-Bold="True" Font-Size="Larger" runat="server" Visible="False"></asp:Label>
<div class="Normal">
    <%--<div class="<%= DataDisplayFormat %>">--%>
    <div class="CustomList_ParentCategoryName" id="divParentCategoryName" runat="server">
        <h1>
            <asp:Literal runat="server" ID="lblCategory" /></h1>
    </div>
    <div class="CustomList_ParentCategoryDescription" id="divParentCategoryDescription" runat="server">
        <asp:Label runat="server" ID="lblCategoryDescription" />
    </div>

    <div class="divItemsListing">
        <asp:Repeater ID="lstItems" runat="server" OnItemDataBound="lstItems_ItemDataBound">
            <HeaderTemplate />
            <AlternatingItemTemplate>
                <article class='blog-post categoryItemList altCategoryItemList <%# GetItemTypeCssClass(Container.DataItem) %>'>
                    <div class="post-title">
                        <asp:Panel ID="pnlTitle" runat="server" CssClass="itemTitle customlistTitle">
                            <asp:HyperLink runat="server" ID="lnkEdit" NavigateUrl='<%# BuildEditUrl(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "ChildItemId")), TabId, ModuleId, PortalId) %>' Text='<%# this.EditText.ToString() %>' Visible='<%# this.Visibility %>' />
                            <h2>
                                <asp:HyperLink runat="server" ID="lnkTitle" NavigateUrl='<%# GetItemLinkUrl(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' Text='<%# DataBinder.Eval(Container.DataItem, "ChildName") %>' Target='<%# GetItemLinkTarget(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' />
                                <asp:Label runat="server" ID="lblTitle" Text='<%# DataBinder.Eval(Container.DataItem, "ChildName") %>' />
                            </h2>

                        </asp:Panel>

                    </div>
                    <div class="post-info">
                        <asp:Panel ID="pnlDate" runat="server" CssClass="itemDate">
                            <time runat="server" id="time" datetime='<%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>' title='<%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>'>
                                <%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>
                            </time>
                        </asp:Panel>
                        <asp:Panel ID="pnlAuthor" runat="server" CssClass="itemAuthor">
                            <asp:Literal ID="lblAuthor" runat="server" Text='<%# GetAuthor(DataBinder.Eval(Container.DataItem, "Author"), DataBinder.Eval(Container.DataItem, "AuthorUserId"), PortalId) %>'></asp:Literal>
                        </asp:Panel>
                    </div>

                    <asp:Panel ID="pnlDescription" runat="server" CssClass="itemDescription">
                        <asp:Literal runat="server" ID="lblDescription" Text='<%# FormatText(DataBinder.Eval(Container.DataItem, "ChildDescription")) %>' />
                        <asp:Panel ID="pnlReadMore" runat="server" CssClass="itemReadmore">
                            <asp:HyperLink runat="server" ID="lnkReadMore" NavigateUrl='<%# GetItemLinkUrl(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' Text="Read More..." ResourceKey="lnkReadMore" Target='<%# GetItemLinkTarget(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' CssClass="btn btn-primary" />
                        </asp:Panel>

                    </asp:Panel>
                    <asp:Panel ID="pnlStats" runat="server" CssClass="itemStats">
                        <asp:Label ID="lblViews" runat="server" Text='<%#DisplayItemViewCount(DataBinder.Eval(Container.DataItem, "ViewCount")) %>' />
                        <asp:Label ID="lblComments" runat="server" Text='<%#DisplayItemCommentCount(DataBinder.Eval(Container.DataItem, "CommentCount")) %>' />
                    </asp:Panel>
                </article>
            </AlternatingItemTemplate>
            <ItemTemplate>
                <article class='blog-post categoryItemList altCategoryItemList <%# GetItemTypeCssClass(Container.DataItem) %>'>
                    <div class="post-title">
                        <asp:Panel ID="pnlTitle" runat="server" CssClass="itemTitle customlistTitle">
                            <asp:HyperLink runat="server" ID="lnkEdit" NavigateUrl='<%# BuildEditUrl(Convert.ToInt32(DataBinder.Eval(Container.DataItem, "ChildItemId")), TabId, ModuleId, PortalId) %>' Text='<%# this.EditText.ToString() %>' Visible='<%# this.Visibility %>' />
                            <h2>
                                <asp:HyperLink runat="server" ID="lnkTitle" NavigateUrl='<%# GetItemLinkUrl(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' Text='<%# DataBinder.Eval(Container.DataItem, "ChildName") %>' Target='<%# GetItemLinkTarget(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' />
                                <asp:Label runat="server" ID="lblTitle" Text='<%# DataBinder.Eval(Container.DataItem, "ChildName") %>' />
                            </h2>

                        </asp:Panel>

                    </div>
                    <div class="post-info">
                        <asp:Panel ID="pnlDate" runat="server" CssClass="itemDate">
                            <time runat="server" id="time" datetime='<%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>' title='<%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>'>
                                <%# FormatDate(DataBinder.Eval(Container.DataItem, "StartDate")) %>
                            </time>
                        </asp:Panel>
                        <asp:Panel ID="pnlAuthor" runat="server" CssClass="itemAuthor">
                            <asp:Literal ID="lblAuthor" runat="server" Text='<%# GetAuthor(DataBinder.Eval(Container.DataItem, "Author"), DataBinder.Eval(Container.DataItem, "AuthorUserId"), PortalId) %>'></asp:Literal>
                        </asp:Panel>
                    </div>

                    <asp:Panel ID="pnlDescription" runat="server" CssClass="itemDescription">
                        <asp:Literal runat="server" ID="lblDescription" Text='<%# FormatText(DataBinder.Eval(Container.DataItem, "ChildDescription")) %>' />
                        <asp:Panel ID="pnlReadMore" runat="server" CssClass="itemReadmore">
                            <asp:HyperLink runat="server" ID="lnkReadMore" NavigateUrl='<%# GetItemLinkUrl(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' Text="Read More..." ResourceKey="lnkReadMore" Target='<%# GetItemLinkTarget(DataBinder.Eval(Container.DataItem, "ChildItemId")) %>' CssClass="btn btn-primary" />
                        </asp:Panel>

                    </asp:Panel>
                    <asp:Panel ID="pnlStats" runat="server" CssClass="itemStats">
                        <asp:Label ID="lblViews" runat="server" Text='<%#DisplayItemViewCount(DataBinder.Eval(Container.DataItem, "ViewCount")) %>' />
                        <asp:Label ID="lblComments" runat="server" Text='<%#DisplayItemCommentCount(DataBinder.Eval(Container.DataItem, "CommentCount")) %>' />
                    </asp:Panel>
                </article>

            </ItemTemplate>
            <FooterTemplate />
        </asp:Repeater>
    </div>
    <asp:Panel ID="pnlPaging" runat="server" CssClass="Publish_CustomDisplayPaging">
        <asp:HyperLink ID="lnkPrevious" runat="server" resourcekey="lnkPrevious" Visible="false" CssClass="Publish_lnkPrevious"></asp:HyperLink>
        <asp:HyperLink ID="lnkNext" runat="server" resourcekey="lnkNext" Visible="false" CssClass="Publish_lnkNext"></asp:HyperLink>

    </asp:Panel>

    <asp:HyperLink runat="server" ID="lnkRss" Visible="False">
        <img runat="server" id="imgRss" />
    </asp:HyperLink>
</div>
