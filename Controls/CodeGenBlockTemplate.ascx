<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CodeGenBlockTemplate.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.DeveloperTools.CodeGenBlockTemplate" %>

<style>
    div.results-tabs > pre {
        border-top-left-radius: 0px;
        border-top-right-radius: 0px;
        border: 1px solid #ddd;
        border-top: 0px;
    }
</style>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Block Template Code Gen</h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="vsSummary" runat="server" CssClass="alert alert-danger" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbOrganization" runat="server" Required="true" Label="Organization" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbDomain" runat="server" Required="true" Label="Domain" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbProject" runat="server" Required="true" Label="Project" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlBlockType" runat="server" Required="true" Label="Block Type" OnSelectedIndexChanged="ddlBlockType_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false">
                            <asp:ListItem />
                            <asp:ListItem Text="Entity List" />
                        </Rock:RockDropDownList>
                    </div>
                </div>

                <asp:Panel ID="pnlEntityListBlock" runat="server" Visible="false">
                    <Rock:EntityTypePicker ID="etpEntity" runat="server" Required="true" Label="Entity Type" OnSelectedIndexChanged="etpEntity_SelectedIndexChanged" CausesValidation="false" AutoPostBack="true" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblDisplay" runat="server" Label="Display" Help="Select which properties should be displayed in the grid." Visible="false" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBoxList ID="cblFilter" runat="server" Label="Filter" Help="Select which properties should be available as filters in the grid." Visible="false" />
                        </div>
                    </div>
                </asp:Panel>

                <div class="actions margin-t-md">
                    <asp:LinkButton ID="lbGenerate" runat="server" Text="Generate" CssClass="btn btn-primary" OnClick="lbGenerate_Click" />
                </div>

            </div>
        </div>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="results-tabs">
                <ul class="nav nav-tabs">
                    <asp:Repeater ID="rptrFileHeaders" runat="server">
                        <ItemTemplate>
                            <li role="presentation"><a href='#<%# Eval( "Name" ) %>' data-filename='<%# Eval( "Name" ) %>'><%# Eval( "Name" ) %></a></li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
                <asp:Repeater ID="rptrFiles" runat="server">
                    <ItemTemplate>
                        <pre data-filename='<%# Eval( "Name" ) %>'><%# Eval( "Content" ) %></pre>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('#<%= tbOrganization.ClientID %>').on('blur', function () {
                    var organization = $('#<%= tbOrganization.ClientID %>').val();
                    if ($('#<%= tbDomain.ClientID %>').val() === '') {
                        $('#<%= tbDomain.ClientID %>').val('com.' + organization.toLowerCase().replace(/ /g, ''));
                    }
                });

                // Add click handler to activate the tab.
                var $results = $('#<%= pnlResults.ClientID %> .results-tabs');
                var $ul = $results.find('ul');
                var $pres = $results.find('pre');
                $ul.find('li a').on('click', function (e) {
                    e.preventDefault();

                    $(this).closest('li').addClass('active').siblings('li').removeClass('active');
                    $pres.hide();
                    $pres.filter('[data-filename="' + $(this).data('filename') + '"]').show();
                });

                // Select first item.
                $ul.find('li:first').addClass('active');
                $pres.hide();
                $pres.filter(':first').show();
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>