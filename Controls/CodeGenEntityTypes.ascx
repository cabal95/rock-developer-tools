<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CodeGenEntityTypes.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DeveloperTools.CodeGenEntityTypes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Entity Type Code Gen</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfSettings" runat="server" OnApplyFilterClick="gfSettings_ApplyFilterClick">
                        <Rock:RockTextBox ID="tbNameFilter" runat="server" Label="Name" />
                        <Rock:RockDropDownList ID="ddlIsEntityFilter" runat="server" Label="Is Entity">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="True" Text="Yes" />
                            <asp:ListItem Value="False" Text="No" />
                        </Rock:RockDropDownList>
                        <Rock:RockDropDownList ID="ddlIsSecuredFilter" runat="server" Label="Is Secured">
                            <asp:ListItem Value="" Text="" />
                            <asp:ListItem Value="True" Text="Yes" />
                            <asp:ListItem Value="False" Text="No" />
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gEntityTypes" runat="server" AllowSorting="true" TooltipField="Id">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BoolField DataField="IsEntity" HeaderText="Is Entity" SortExpression="IsEntity" />
                            <Rock:BoolField DataField="IsSecured" HeaderText="Is Secured" SortExpression="IsSecured" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

        <pre id="pResults" runat="server" visible="false" class="margin-t-lg"><asp:Literal ID="lResults" runat="server" /></pre>
    </ContentTemplate>
</asp:UpdatePanel>