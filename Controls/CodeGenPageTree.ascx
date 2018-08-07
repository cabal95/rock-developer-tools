<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CodeGenPageTree.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DeveloperTools.CodeGenPageTree" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Page Tree Code Gen</h1>
            </div>
            <div class="panel-body">

                <Rock:PagePicker ID="ppPage" runat="server" Required="true" Label="Page" />

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbGenerate" runat="server" Text="Generate" CssClass="btn btn-primary" OnClick="lbGenerate_Click" />
                </div>

            </div>
        </div>

        <pre id="pResults" runat="server" visible="false" class="margin-t-lg"><asp:Literal ID="lResults" runat="server" /></pre>
    </ContentTemplate>
</asp:UpdatePanel>