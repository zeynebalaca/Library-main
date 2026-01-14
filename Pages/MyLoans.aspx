<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyLoans.aspx.cs"
    Inherits="Library.Pages.MyLoans" MasterPageFile="~/Site.Master" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Benim Ödünçlerim
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06);}
    .table thead th { white-space:nowrap; }
  </style>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Benim Ödünçlerim</h3>
  <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

  <asp:PlaceHolder ID="phNoLoans" runat="server" Visible="false">
    <div class="alert alert-info">Henüz ödünç kaydınız yok.</div>
  </asp:PlaceHolder>

  <asp:Repeater ID="repLoans" runat="server"
                OnItemDataBound="repLoans_ItemDataBound"
                OnItemCommand="repLoans_ItemCommand">
    <HeaderTemplate>
      <table class="table table-hover align-middle bg-white card-soft">
        <thead class="table-light">
          <tr>
            <th>Kitap</th>
            <th>Alış</th>
            <th>Son Teslim</th>
            <th>Durum</th>
            <th style="width:1%"></th>
          </tr>
        </thead>
        <tbody>
    </HeaderTemplate>

    <ItemTemplate>
      <tr>
        <td>
          <!-- DÜZELTME: asp:HyperLink kullanıyoruz -->
          <asp:HyperLink ID="lnkBook" runat="server" />
        </td>
        <td><asp:Literal ID="litLoan" runat="server" /></td>
        <td><asp:Literal ID="litDue" runat="server" /></td>
        <td><asp:Literal ID="litStatus" runat="server" /></td>
        <td class="text-end">
          <asp:Button ID="btnReturn" runat="server"
                      CommandName="return"
                      CommandArgument='<%# Eval("LoanId") %>'
                      CssClass="btn btn-outline-secondary btn-sm"
                      Text="İade Et" />
        </td>

        <!-- ItemDataBound için değerler -->
        <asp:HiddenField ID="hidBookId"   runat="server" Value='<%# Eval("BookId") %>' />
        <asp:HiddenField ID="hidLoanDate" runat="server" Value='<%# Eval("LoanDate","{0:O}") %>' />
        <asp:HiddenField ID="hidDueDate"  runat="server" Value='<%# Eval("DueDate","{0:O}") %>' />
        <asp:HiddenField ID="hidReturnDate" runat="server" Value='<%# Eval("ReturnDate") %>' />
      </tr>
    </ItemTemplate>

    <FooterTemplate>
        </tbody>
      </table>
    </FooterTemplate>
  </asp:Repeater>

</asp:Content>
