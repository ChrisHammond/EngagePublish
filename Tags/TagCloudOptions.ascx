<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Publish.Tags.TagCloudOptions" CodeBehind="TagCloudOptions.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>

<style type="text/css">
    @import url(<%=Engage.Dnn.Publish.ModuleBase.ApplicationUrl %><%=Engage.Dnn.Publish.ModuleBase.DesktopModuleFolderName %>Module.css);
</style>

<fieldset>
    <div class="dnnFormItem">
        <dnn:label id="lblPopularTagCount" runat="server" controlname="txtPopularTagCount" text="Limit display to 50:" ResourceKey="lblPopularTagCount" />
        <asp:CheckBox ID="chkLimitTagCount" runat="server" />
    </div>
</fieldset>
