<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CodeGenBlockTypes.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DeveloperTools.CodeGenBlockTypes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Block Type Code Gen</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server" OnApplyFilterClick="gfSettings_ApplyFilterClick">
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:RockTextBox ID="tbPathFilter" runat="server" Label="Path" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBlockTypes" runat="server" AllowSorting="true" TooltipField="Description">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField HeaderText="Category" DataField="Category" SortExpression="Category" />
                            <Rock:RockBoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <pre id="pResults" runat="server" visible="false" class="margin-t-lg"><asp:Literal ID="lResults" runat="server" /></pre>
    </ContentTemplate>
</asp:UpdatePanel>