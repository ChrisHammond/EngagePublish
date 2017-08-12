<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Controls.EmailAFriend" CodeBehind="EmailAFriend.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div id="divEmailAFriend" >
    <asp:LinkButton ID="btnEmailAFriend" runat="server" CssClass="btnEmailAFriend" CausesValidation="false">
        <span class="glyphicon glyphicon-envelope"></span>
    </asp:LinkButton>
</div>

<div id="emailAFriendDialog" style="display: none;" class="modalStyle">
    <asp:Label runat="server" ID="lblConfirmation" CssClass="dnnFormMessage" resourcekey="lblConfirmation" Visible="false"></asp:Label>

    <div id="divEmailAFriendForm" class="divEmailAFriendForm" runat="server">
            <div class="form-group">
                <dnn:Label ID="lblTo" ResourceKey="lblTo" runat="server"></dnn:Label>
                <asp:TextBox ID="txtTo" runat="server" TextMode="SingleLine" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <dnn:Label ID="lblFrom" ResourceKey="lblFrom" runat="server"></dnn:Label>
                <asp:TextBox ID="txtFrom" runat="server" TextMode="SingleLine" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <dnn:Label ID="lblMessage" ResourceKey="lblMessage" runat="server"></dnn:Label>
                <asp:TextBox runat="server" ID="txtMessage" TextMode="MultiLine" Columns="30" Rows="5" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-group">
                <asp:Label ID="lblPrivacy" runat="server" ResourceKey="lblPrivacy"></asp:Label>
            </div>
        <asp:LinkButton ID="btnSend"
            ResourceKey="btnSend"
            runat="server"
            OnClick="btnSend_Click"
            CssClass="dnnPrimaryAction" CausesValidation="False" />
    </div>
</div>

<script>

    $("#<%=btnEmailAFriend.ClientID %>").click(function () {
        var dlg = $("#emailAFriendDialog").dialog({
            title: "Email a Friend",
            autoOpen: true,
            width: 600,
            modal: true,
            dialogClass: "dnnFormPopup",
            buttons: {
                //Ok: function () {
                //$('#<%=btnSend.ClientID %>').click();
                //},
                Close: function () {
                    $(this).dialog("close");
                }
            }
        });


        dlg.parent().appendTo($("form:first"));
        dlg.dialog('open');
        trackEvent('Email A Friend', 'Open');
    });

    $("#<%=btnSend.ClientID %>").click(function (e) {
            trackEvent('Email A Friend', 'Send');
            $(this).parent().parent().dialog('close');
    });


    function trackEvent(eventType, eventName) {
        if (typeof (_gaq) !== 'undefined') {
            _gaq.push(['_trackEvent', eventType, eventName, '<%=GetItemName()%>']);
        }
    }
</script>


