<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DashboardAdmin.aspx.cs"
    Inherits="Library.Pages.DashboardAdmin" MasterPageFile="~/Site.Master" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">
    Ana Sayfa – Admin
</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06); }
        .table thead th { white-space:nowrap; }
    </style>
</asp:Content>

<asp:Content ID="c3" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="mb-4">Merhaba, Admin!</h3>

    <!-- Özet Kartlar -->
    <div class="row g-4 mb-4">
        <div class="col-md-3">
            <div class="card card-soft">
                <div class="card-body">
                    <h6 class="text-muted">Toplam Kitap</h6>
                    <div class="display-6"><asp:Literal ID="litTotalBooks" runat="server" /></div>
                    <small class="text-muted">Books tablosu</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card card-soft">
                <div class="card-body">
                    <h6 class="text-muted">Mevcut Kopya</h6>
                    <div class="display-6"><asp:Literal ID="litAvailableCopies" runat="server" /></div>
                    <small class="text-muted">BookCopies: Status='Available'</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card card-soft">
                <div class="card-body">
                    <h6 class="text-muted">Aktif Ödünç</h6>
                    <div class="display-6"><asp:Literal ID="litActiveLoans" runat="server" /></div>
                    <small class="text-muted">ReturnDate IS NULL</small>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card card-soft">
                <div class="card-body">
                    <h6 class="text-muted">Bugünkü İşlemler</h6>
                    <div class="display-6"><asp:Literal ID="litTodayOps" runat="server" /></div>
                    <small class="text-muted">Bugün Loan/Iade toplamı</small>
                </div>
            </div>
        </div>
    </div>

    <!-- Hızlı İşlemler -->
    <div class="row g-4 mb-4">
        <div class="col-md-12">
            <div class="card card-soft">
                <div class="card-body">
                    <h6 class="text-muted mb-2">Hızlı İşlemler</h6>
                    <a runat="server" href="~/Pages/AdminBooks.aspx" class="btn btn-dark btn-sm me-2">Kitap Ekle/Yönet</a>
                    <a runat="server" href="~/Pages/AdminBookCopies.aspx" class="btn btn-outline-dark btn-sm me-2">Kopya Ekle</a>
                    <a runat="server" href="~/Pages/AdminCategories.aspx" class="btn btn-outline-dark btn-sm me-2">Kategori Yönet</a>
                    <a runat="server" href="~/Pages/AdminLoans.aspx" class="btn btn-outline-dark btn-sm">Ödünç Yönet</a>
                </div>
            </div>
        </div>
    </div>

    <!-- Son 5 Yorum & Son 5 Ödünç -->
    <div class="row g-4">
        <div class="col-lg-6">
            <div class="card card-soft">
                <div class="card-body">
                    <h5 class="mb-3">Son 5 Yorum</h5>
                    <asp:Repeater ID="repLastReviews" runat="server">
                        <HeaderTemplate>
                            <table class="table table-sm align-middle">
                                <thead><tr><th>Kitap</th><th>Kullanıcı</th><th>Puan</th><th>Tarih</th></tr></thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("BookTitle") %></td>
                                <td><%# Eval("UserName") %></td>
                                <td>★ <%# Eval("Rating") %></td>
                                <td><%# string.Format("{0:yyyy-MM-dd HH:mm}", Eval("CreatedAt")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phNoReviews" runat="server" Visible="false">
                        <div class="text-muted">Henüz yorum yok.</div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>

        <div class="col-lg-6">
            <div class="card card-soft">
                <div class="card-body">
                    <h5 class="mb-3">Son 5 Ödünç</h5>
                    <asp:Repeater ID="repLastLoans" runat="server">
                        <HeaderTemplate>
                            <table class="table table-sm align-middle">
                                <thead><tr><th>Kitap</th><th>Üye</th><th>Ödünç</th><th>Son Teslim</th></tr></thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("BookTitle") %></td>
                                <td><%# Eval("UserName") %></td>
                                <td><%# string.Format("{0:yyyy-MM-dd}", Eval("LoanDate")) %></td>
                                <td><%# string.Format("{0:yyyy-MM-dd}", Eval("DueDate")) %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phNoLoans" runat="server" Visible="false">
                        <div class="text-muted">Henüz ödünç hareketi yok.</div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
