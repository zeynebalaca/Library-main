<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardMember.aspx.cs"
    Inherits="Library.Pages.DashboardMember" MasterPageFile="~/Site.Master" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">
    Ana Sayfa – Üye
</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06); }
        .table thead th { white-space:nowrap; }
    </style>
</asp:Content>

<asp:Content ID="c3" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-4">Merhaba, <asp:Literal ID="litMemberName" runat="server" />!</h3>

    <div class="row g-4">
        <!-- Yeni Eklenenler -->
        <div class="col-md-6 col-lg-4">
            <div class="card card-soft h-100">
                <div class="card-body">
                    <h5>Yeni Eklenen Kitaplar</h5>
                    <asp:Repeater ID="repNewBooks" runat="server">
                        <HeaderTemplate><ul class="list-group list-group-flush"></HeaderTemplate>
                        <ItemTemplate>
                            <li class="list-group-item"><%# Eval("Title") %></li>
                        </ItemTemplate>
                        <FooterTemplate></ul></FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phNoNewBooks" runat="server" Visible="false">
                        <div class="text-muted small">Henüz kitap eklenmemiş.</div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>

        <!-- En Yüksek Puanlılar -->
        <div class="col-md-6 col-lg-4">
            <div class="card card-soft h-100">
                <div class="card-body">
                    <h5>En Yüksek Puanlılar</h5>
                    <asp:Repeater ID="repTopRated" runat="server">
                        <HeaderTemplate><ol class="list-group list-group-numbered list-group-flush"></HeaderTemplate>
                        <ItemTemplate>
                            <li class="list-group-item d-flex justify-content-between">
                                <span><%# Eval("Title") %></span>
                                <span>★ <%# string.Format("{0:0.0}", Eval("AvgRating")) %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate></ol></FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phNoTopRated" runat="server" Visible="false">
                        <div class="text-muted small">Puanlanmış kitap yok.</div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>

        <!-- Benim Aktif Ödünçlerim -->
        <div class="col-md-12 col-lg-4">
            <div class="card card-soft h-100">
                <div class="card-body">
                    <h5>Aktif Ödünçlerim</h5>
                    <asp:Repeater ID="repMyLoans" runat="server">
                        <HeaderTemplate>
                            <table class="table table-sm align-middle">
                                <thead><tr><th>Kitap</th><th>Son Teslim</th></tr></thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("BookTitle") %></td>
                                <td><%# string.Format("{0:yyyy-MM-dd}", Eval("DueDate")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phNoLoans" runat="server" Visible="false">
                        <div class="text-muted small">Devam eden ödünç kaydın yok.</div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
