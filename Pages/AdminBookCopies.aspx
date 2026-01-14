<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminBookCopies.aspx.cs"
    Inherits="Library.Pages.AdminBookCopies" MasterPageFile="~/Site.Master" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Kopya Yönetimi
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06);}
    .table thead th { white-space:nowrap; }
  </style>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Kopya Yönetimi</h3>
  <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

  <!-- Kitap seçimi -->
  <div class="input-group mb-3" style="max-width:420px;">
    <span class="input-group-text">Kitap</span>
    <asp:DropDownList ID="ddlBooks" runat="server" CssClass="form-select" />
    <asp:Button ID="btnList" runat="server" CssClass="btn btn-outline-secondary" Text="Listele" OnClick="btnList_Click" />
  </div>

  <!-- Kopyalar -->
  <asp:PlaceHolder ID="phTable" runat="server" Visible="false">
    <asp:Repeater ID="repCopies" runat="server" OnItemCommand="repCopies_ItemCommand">
      <HeaderTemplate>
        <table class="table table-hover align-middle bg-white card-soft">
          <thead class="table-light">
            <tr>
              <th>Kopya No (Id)</th>
              <th>Durum</th>
              <th style="width:1%"></th>
            </tr>
          </thead>
          <tbody>
      </HeaderTemplate>

      <ItemTemplate>
        <tr>
          <td><%# Eval("Id") %></td>
          <td><%# StatusBadge(Eval("Status")) %></td>
          <td class="text-end">
            <asp:Button ID="btnDelete" runat="server"
                        CssClass='<%# GetDeleteCss(Eval("Status")) %>'
                        Text="Sil"
                        CommandName="delete"
                        CommandArgument='<%# Eval("Id") %>'
                        Enabled='<%# CanDelete(Eval("Status")) %>'
                        OnClientClick="return confirm('Bu kopyayı silmek istediğinize emin misiniz?');"
                        CausesValidation="false" />
          </td>
        </tr>
      </ItemTemplate>

      <FooterTemplate>
          </tbody>
        </table>
      </FooterTemplate>
    </asp:Repeater>

    <!-- Yeni kopya ekleme (Id otomatik, Status=Available) -->
    <div class="mt-3">
      <asp:Button ID="btnAdd" runat="server" CssClass="btn btn-dark" Text="Yeni Kopya Ekle" OnClick="btnAdd_Click" />
    </div>
  </asp:PlaceHolder>

</asp:Content>
