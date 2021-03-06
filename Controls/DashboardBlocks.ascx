﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DashboardBlocks.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.Misc.DashboardBlocks" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDashboard" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <div class="pull-left">
                    <h1 class="panel-title">
                        <i class="<%= GetAttributeValue( "IconCSSClass" ) %>"></i> <%= GetAttributeValue( "Title" ) %>
                    </h1>
                </div>
                <div class="pull-right">
                    <asp:LinkButton ID="btnOptions" runat="server" CssClass="btn btn-xs btn-link" OnClick="btnOptions_Click"><i class="fa fa-cog"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">
                <asp:PlaceHolder ID="phControls" runat="server"></asp:PlaceHolder>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlOptions" runat="server" Title="Options" OnSaveClick="mdlOptions_SaveClick" ValidationGroup="Options">
            <Content>
                <Rock:RockRadioButtonList ID="rblOptionsLayout" runat="server" Label="Layout" Help="The column configuration you want to use." RepeatDirection="Horizontal" />

                <Rock:RockCheckBoxList ID="cblOptionsBlocks" runat="server" Label="Widgets" Help="Which widgets are visible on your dashboard." RepeatDirection="Vertical" />

                <Rock:NotificationBox ID="nbOptionsRequiredBlocks" runat="server" NotificationBoxType="Info"></Rock:NotificationBox>
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="mdlSettings" runat="server" Title="Settings" OnSaveClick="mdlSettings_SaveClick" ValidationGroup="Settings">
            <Content>
                <Rock:RockRadioButtonList ID="rblSettingsDefaultLayout" runat="server" Label="Default Layout" Help="The column configuration you want users to use by default." RepeatDirection="Horizontal" Required="true" />

                <Rock:Grid ID="gSettingsBlocks" runat="server" Title="Blocks">
                    <Columns>
                        <asp:BoundField DataField="BlockId" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" />
                        <asp:BoundField DataField="BlockCache.Name" HeaderText="Block" />
                        <Rock:CheckBoxEditableField DataField="Required" HeaderText="Required"></Rock:CheckBoxEditableField>
                        <Rock:CheckBoxEditableField DataField="DefaultVisible" HeaderText="Visible By Default"></Rock:CheckBoxEditableField>
                    </Columns>
                </Rock:Grid>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
