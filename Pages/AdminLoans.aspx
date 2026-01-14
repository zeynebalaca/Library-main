<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminLoans.aspx.cs"
    Inherits="Library.Pages.AdminLoans" MasterPageFile="~/Site.Master" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Ödünç – İade Listesi
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06); }
    .table thead th { white-space: nowrap; }
    .status-badge { font-size:.75rem; }
  </style>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Ödünç – İade Listesi</h3>
  <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

  <!-- Filtreler -->
  <div class="card card-soft mb-3">
    <div class="card-body">
      <div class="row g-2 align-items-end">
        <div class="col-12 col-md-3">
          <label class="form-label mb-1">Kullanıcı (ad/e-posta)</label>
          <asp:TextBox ID="txtUser" runat="server" CssClass="form-control" />
        </div>
        <div class="col-12 col-md-3">
          <label class="form-label mb-1">Kitap (başlık)</label>
          <asp:TextBox ID="txtBook" runat="server" CssClass="form-control" />
        </div>
        <div class="col-6 col-md-2">
          <label class="form-label mb-1">Durum</label>
          <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
            <asp:ListItem Value="">Hepsi</asp:ListItem>
            <asp:ListItem Value="ongoing">Devam ediyor</asp:ListItem>
            <asp:ListItem Value="overdue">Gecikmiş</asp:ListItem>
            <asp:ListItem Value="returned">İade edildi</asp:ListItem>
          </asp:DropDownList>
        </div>
        <div class="col-6 col-md-2">
          <label class="form-label mb-1">Tarih</label>
          <asp:DropDownList ID="ddlDate" runat="server" CssClass="form-select">
            <asp:ListItem Value="">Hepsi</asp:ListItem>
            <asp:ListItem Value="last7">Son 7 gün</asp:ListItem>
            <asp:ListItem Value="thismonth">Bu ay</asp:ListItem>
            <asp:ListItem Value="prevmonth">Geçen ay</asp:ListItem>
          </asp:DropDownList>
        </div>
        <div class="col-12 col-md-2 d-grid">
          <asp:Button ID="btnFilter" runat="server" CssClass="btn btn-dark" Text="Filtrele" OnClick="btnFilter_Click" />
        </div>
      </div>
    </div>
  </div>

  <!-- Tablo -->
  <div class="table-responsive card card-soft">
    <asp:Repeater ID="repLoans" runat="server" OnItemCommand="repLoans_ItemCommand">
      <HeaderTemplate>
        <table class="table table-hover align-middle mb-0">
          <thead class="table-light">
            <tr>
              <th>Kullanıcı</th>
              <th>Kitap</th>
              <th>Kopya Id</th>
              <th>Alış</th>
              <th>Son Teslim</th>
              <th>Durum</th>
              <th class="text-end">İşlem</th>
            </tr>
          </thead>
          <tbody>
      </HeaderTemplate>

      <ItemTemplate>
        <tr>
          <td><%# Eval("UserName") %> (<%# Eval("UserEmail") %>)</td>
          <td><%# Eval("BookTitle") %></td>
          <td><%# Eval("CopyId") %></td>
          <td><%# Eval("LoanDate","{0:dd.MM.yyyy}") %></td>
          <td><%# Eval("DueDate","{0:dd.MM.yyyy}") %></td>
          <td><%# StatusBadge(Eval("ReturnDate"), Eval("DueDate")) %></td>
          <td class="text-end">
            <asp:Button ID="btnReturn" runat="server"
                        CssClass='<%# GetReturnCss(Eval("ReturnDate")) %>'
                        Text="İade Al"
                        CommandName="return"
                        CommandArgument='<%# Eval("Id") %>'
                        Enabled='<%# !IsReturned(Eval("ReturnDate")) %>'
                        OnClientClick="return confirm('Bu ödüncü iade almak istiyor musunuz?');"
                        CausesValidation="false" />
            <asp:Button ID="btnExtend" runat="server"
                        CssClass='<%# GetExtendCss(Eval("ReturnDate")) %>'
                        Text="Süre Uzat (+7 gün)"
                        CommandName="extend"
                        CommandArgument='<%# Eval("Id") %>'
                        Enabled='<%# !IsReturned(Eval("ReturnDate")) %>'
                        CausesValidation="false" />
          </td>
          <asp:HiddenField ID="hidLoanId" runat="server" Value='<%# Eval("Id") %>' />
        </tr>
      </ItemTemplate>

      <FooterTemplate>
          </tbody>
        </table>
      </FooterTemplate>
    </asp:Repeater>
  </div>

</asp:Content>
