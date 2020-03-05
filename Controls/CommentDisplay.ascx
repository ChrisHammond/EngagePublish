<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Controls.CommentDisplay" CodeBehind="CommentDisplay.ascx.cs" %>

<div id="divComment" class="Normal">

    <asp:Label ID="lblNoComments" runat="server" resourcekey="lblNoComments" CssClass="dnnFormMessage"></asp:Label>
            <asp:Repeater ID="dlCommentText" runat="server">
                <HeaderTemplate />
                <ItemTemplate>
                    <div class="PublishComment PublishCommentWrapper dnnClear">
                        <div class="CommentAvatar">
                            <img src="<%# BuildCommentAvatar((Eval("EmailAddress") as string) ?? "empty")%>" />
                        </div>
                        <div class="CommentValue"><%# Eval("CommentText") %></div>
                        <div class="CommentNameDate"><%# BuildCommentNameDate(Eval("FirstName"), Eval("LastName"),Eval("Url"),Eval("CreatedDate")) %></div>
                    </div>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <div class="PublishComment PublishCommentAlternate dnnClear">
                        <div class="CommentAvatar">
                            <img src="<%# BuildCommentAvatar((Eval("EmailAddress") as string) ?? "empty")%>" />
                        </div>
                        <div class="CommentValue"><%# Eval("CommentText") %></div>
                        <div class="CommentNameDate"><%# BuildCommentNameDate(Eval("FirstName"), Eval("LastName"),Eval("Url"),Eval("CreatedDate")) %></div>
                    </div>
                </AlternatingItemTemplate>
            </asp:Repeater>
            <div id="divPager" runat="server" class="commentPager">
                <asp:LinkButton ID="btnPrevious" runat="server" ResourceKey="btnPrev" CssClass="commentPrev" CausesValidation="false" /><%--OnClick="btnPrevious_Click" --%>
                <asp:LinkButton ID="btnNext" runat="server" ResourceKey="btnNext" OnClick="btnNext_Click" CssClass="commentNext" CausesValidation="false" />
            </div>
</div>
