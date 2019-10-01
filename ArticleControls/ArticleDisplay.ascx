<%@ Control Language="c#" AutoEventWireup="True" Inherits="Engage.Dnn.Publish.ArticleControls.ArticleDisplay" CodeBehind="ArticleDisplay.ascx.cs" %>

<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div id="articleDisplay" class="Normal">

    <div id="divArticleTitle" runat="server" visible="false">
    </div>
        <h1 class="Head" id="publishTitle">
            <asp:Literal ID="lblArticleTitle" runat="server"></asp:Literal>
        </h1>
    

    <span class="publishMetaData">
                <asp:PlaceHolder ID="phPrinterFriendly" runat="server"></asp:PlaceHolder>
                <asp:PlaceHolder ID="phEmailAFriend" runat="server"></asp:PlaceHolder>
        <asp:Literal ID="lblDateCreated" runat="server"></asp:Literal>        
        <asp:Label ID="lblAuthorInfo" runat="server" resourcekey="lblAuthorInfo"></asp:Label>
                <asp:Literal ID="lblAuthor" runat="server"></asp:Literal>
                
                <asp:Literal ID="lblLastUpdated" runat="server"></asp:Literal>
    </span>
            


    <div id="divRelatedArticle" runat="server" class="divRelatedArticle dnnClear" visible="false">
        <div id="divRelatedArticleHeader"></div>
        <asp:PlaceHolder ID="phRelatedArticle" runat="server"></asp:PlaceHolder>
    </div>
    <div id="divArticleContent" class="Normal dnnClear">
        <asp:Literal ID="lblArticleText" runat="server"></asp:Literal>
    </div>

    <asp:Panel ID="pnlTags" runat="server" Visible="false" CssClass="Publish_ArticleTags">
        <strong>
            <asp:Label ID="lblTag" runat="server" resourcekey="lblTag" CssClass="control-label"></asp:Label></strong>
        <asp:PlaceHolder ID="phTags" runat="server" EnableViewState="true"></asp:PlaceHolder>
    </asp:Panel>

    <div id="divArticlePage" class="Normal">
        <asp:HyperLink rel="prev" ID="lnkPreviousPage" CssClass="Publish_lnkPrevious" runat="server" />
        <asp:HyperLink rel="next" ID="lnkNextPage" CssClass="Publish_lnkNext" runat="server" />
    </div>

    <div id="div1" class="Normal">
        <asp:HyperLink ID="lnkConfigure" runat="server" Visible="false"></asp:HyperLink>
    </div>

    <asp:Panel ID="pnlReturnToList" runat="server" CssClass="Publish_ReturnToList">
        <asp:HyperLink ID="lnkReturnToList" runat="server"></asp:HyperLink>
    </asp:Panel>

    <div id="divRelatedArticlesControl">
        <asp:PlaceHolder ID="phRelatedArticles" runat="server"></asp:PlaceHolder>
    </div>

    <asp:UpdatePanel ID="upnlRating" runat="server" UpdateMode="conditional" Visible="false" OnUnload="UpdatePanel_Unload">
        <ContentTemplate>
            <div id="divRating" class="divRatingBefore">
                <asp:Label ID="lblRatingMessage" runat="server" resourcekey="lblRatingMessage"></asp:Label>
                <%
    /*<ajaxToolkit:Rating ID="ajaxRating"
                    runat="server" MaxRating="5"
                    StarCssClass="ratingStar"
                    WaitingStarCssClass="savedRatingStar"
                    FilledStarCssClass="filledRatingStar"
                    EmptyStarCssClass="emptyRatingStar"
                    OnChanged="ajaxRating_Changed"
                    Visible="true"
                    AutoPostBack="true" />
                    */
                %>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:Panel ID="pnlComments" runat="server" Visible="false">
        <asp:MultiView ID="mvCommentDisplay" runat="server" ActiveViewIndex="0">
            <asp:View ID="vwPublishComments" runat="server">
                <div id="divCommentLink" class="dnnClear">
                    <asp:HyperLink ID="btnComment" runat="server" resourcekey="btnComment" Visible="false" />
                </div>

                <asp:Panel ID="pnlCommentDisplay" runat="server" Visible="false">
                    <div id="divCommentsDisplay">
                        <h3 class="Publish_CommentHeading">
                            <asp:Label ID="lblCommentHeading" resourcekey="lblCommentHeading" runat="server" CssClass="Head" />
                        </h3>
                        <asp:PlaceHolder ID="phCommentsDisplay" runat="server" />
                    </div>
                </asp:Panel>
            </asp:View>
            <asp:View ID="vwForumComments" runat="server">
                <div id="divForumCommentLink">
                    <asp:LinkButton ID="btnForumComment" runat="server" resourcekey="btnForumComment" Visible="false" />
                    <asp:HyperLink ID="lnkGoToForum" runat="server" resourcekey="lnkGoToForum" Visible="false" />
                </div>
            </asp:View>
        </asp:MultiView>

        <asp:UpdatePanel ID="upnlComments" runat="server" UpdateMode="Conditional" OnUnload="UpdatePanel_Unload">
            <ContentTemplate>

                <h3 class="Publish_CommentHeading">
                    <asp:Label ID="lblAddComment" resourcekey="lblAddComment" runat="server" CssClass="Head" />
                </h3>

                <asp:Panel ID="pnlCommentEntry" runat="server" CssClass="commentEntry">
                    <a id="CommentEntry"></a>
                    <div id="commentEntryWrapper">
                        <div id="commentInstructions">
                            <asp:Label ID="lblInstructions" runat="server" ResourceKey="lblInstructions"></asp:Label>
                        </div>

                        <asp:Panel ID="pnlNameComment" runat="server" CssClass="dnnClear">
                            
                                <div class="form-group">
                                    <dnn:Label ID="lblFirstNameComment" runat="server" />
                                    <asp:TextBox ID="txtFirstNameComment" runat="server" EnableViewState="false" CssClass="form-control" /><asp:RequiredFieldValidator ID="rfvFirstNameComment" resourcekey="rfvFirstNameComment" runat="server" Display="None" ControlToValidate="txtFirstNameComment" ValidationGroup="commentVal" />
                                </div>
                                <div class="form-group">
                                    <dnn:Label ID="lblLastNameComment" runat="server" />
                                    <asp:TextBox ID="txtLastNameComment" runat="server" EnableViewState="false" CssClass="form-control" /><asp:RequiredFieldValidator ID="rfvLastNameComment" resourcekey="rfvLastNameComment" runat="server" Display="None" ControlToValidate="txtLastNameComment" ValidationGroup="commentVal" />
                                </div>
                            

                        </asp:Panel>
                        <asp:Panel ID="pnlEmailAddressComment" runat="server">
                            
                                <div class="form-group">
                                    <dnn:Label ID="lblEmailAddressComment" runat="server" resourcekey="lblEmailAddressComment" />
                                    <asp:TextBox ID="txtEmailAddressComment" runat="server" EnableViewState="false" CssClass="form-control" /><asp:RequiredFieldValidator ID="rfvEmailAddressComment" resourcekey="rfvEmailAddressComment" runat="server" Display="None" ControlToValidate="txtEmailAddressComment" ValidationGroup="commentVal" />
                                </div>
                            
                        </asp:Panel>
                        <asp:Panel ID="pnlUrlComment" runat="server">
                            
                                <div class="form-group">
                                    <dnn:Label ID="lblUrlComment" runat="server" resourcekey="lblUrlComment" />
                                    <asp:TextBox ID="txtUrlComment" runat="server" EnableViewState="false" CssClass="form-control" />
                                </div>
                            
                        </asp:Panel>

                        
                            <div class="form-group">
                                <dnn:label id="lblComment" runat="server" />
                                <asp:TextBox TextMode="MultiLine" ID="txtComment" runat="server" EnableViewState="false" CssClass="form-control" /><asp:RequiredFieldValidator ID="rfvCommentText" resourcekey="rfvCommentText" runat="server" Display="None" ControlToValidate="txtComment" ValidationGroup="commentVal" />
                            </div>

                            <div class="form-group">
                                <dnn:label id="lblHumanTest" runat="server" />
                                <asp:TextBox runat="server" ID="txtHumanTest" CssClass="form-control" />
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" resourcekey="rfvHumanTest" runat="server" Display="None" ControlToValidate="txtHumanTest" ValidationGroup="commentVal" />
                            </div>
                        

                        <asp:LinkButton ID="btnSubmitComment" runat="server"
                            resourcekey="btnSubmitComment" OnClick="btnSubmitComment_Click" CssClass="dnnPrimaryAction" ValidationGroup="commentVal" />

                        <asp:LinkButton ID="btnCancelComment" runat="server" resourcekey="btnCancelComment" OnClick="btnCancelComment_Click" CssClass="dnnSecondaryAction" CausesValidation="false" ValidationGroup="commentVal" />

                        <asp:ValidationSummary ID="valCommentSummary" runat="server" ShowMessageBox="true" ShowSummary="false" DisplayMode="List" ValidationGroup="commentVal" />
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlCommentConfirmation" runat="server" Visible="false">
                    <div id="commentConfirmationWrapper">
                        <div id="commentConfirmationClose">
                            <asp:LinkButton ID="btnConfirmationClose" CausesValidation="false" runat="server" resourcekey="btnConfirmationClose" OnClick="btnConfirmationClose_Click" CssClass="dnnFormMessage"></asp:LinkButton>
                        </div>
                    </div>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>
</div>


<script type="text/javascript">
    <%
    /*
    <1% if (ajaxRating.Visible)
       { %1>
    // Method called when the Rating is changed
    function changeCssClassMethod(eventElement) {
        Sys.UI.DomElement.removeCssClass($get('divRating'), 'divRatingBefore');
        Sys.UI.DomElement.addCssClass($get('divRating'), 'divRatingAfter');

        //Sys.UI.DomElement.toggleCssClass($get('divRating'), "divRatingAfter");
    }
    // Add handler using the getElementById methdo
    $addHandler(Sys.UI.DomElement.getElementById('<%= ajaxRating.ClientID %1>'), 'click', changeCssClassMethod);
    <1% } %1>
    */
    %>

    $("#<%=btnSubmitComment.ClientID %>").click(function (e) {
        var txt = $("#<%=txtHumanTest.ClientID %>");
        var txtval = txt.val().toLowerCase();
        if (txtval == 'human2') {
            $("#<%=btnSubmitComment.ClientID %>").removeAttr('disabled');

            _gaq.push(['_trackEvent', 'Human2 Comment Validated', 'Submitted', '<%=GetItemName()%>']);
        } else {
            e.preventDefault();
            alert('<%=Localization.GetString("HumanTestResult.Text",LocalResourceFile)%>');
            if (typeof (_gaq) !== 'undefined') {
                _gaq.push(['_trackEvent', 'Failed Comment', 'Submitted', '<%=GetItemName()%>']);
            }
        }
    });
</script>
