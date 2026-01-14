<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Books.aspx.cs"
    Inherits="Library.Pages.Books" MasterPageFile="~/Site.Master" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">
  Kitaplar
</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    .card-soft { border-radius:.75rem; box-shadow:0 0.25rem .75rem rgba(0,0,0,.05);}
    .cover-box { aspect-ratio: 2/3; overflow:hidden; }
    .cover     { width:100%; height:100%; object-fit:cover; }
    .card-body { padding:.75rem; text-align:center; }
    .card-title{ font-size:.95rem; margin-bottom:.25rem; font-weight:600; }
    .card-text { font-size:.8rem;  margin-bottom:.25rem; color:#6c757d; }
  </style>
</asp:Content>

<asp:Content ID="c3" ContentPlaceHolderID="MainContent" runat="server">

  <!-- Başlık + Arama/Filtresi -->
  <div class="d-flex flex-column flex-md-row justify-content-between align-items-md-center gap-2 mb-3">
    <h3 class="mb-0">Kütüphanedeki Kitaplar</h3>

    <div class="d-flex gap-2">
      <asp:TextBox ID="txtQ" runat="server" CssClass="form-control form-control-sm" placeholder="Başlık / Yazar" />
      <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm" />
      <asp:Button ID="btnFilter" runat="server" CssClass="btn btn-primary btn-sm" Text="Listele" OnClick="btnFilter_Click" />
    </div>
  </div>

  <!-- Kart ızgarası (6 sütun) -->
  <asp:Repeater ID="repBooks" runat="server">
    <HeaderTemplate>
      <div class="row row-cols-2 row-cols-sm-3 row-cols-md-4 row-cols-lg-6 g-3">
    </HeaderTemplate>

    <ItemTemplate>
      <div class="col">
        <a href='<%# ResolveUrl("~/Pages/BookDetails.aspx?id=") + Eval("Id") %>' class="text-decoration-none text-reset">
          <div class="card card-soft h-100">
            <div class="cover-box">
              <img class="cover"
                   src="<%# CoverSrc(Eval("CoverPath")) %>"
                   alt="<%# Eval("Title") %>"
                   onerror="this.onerror=null;this.src='<%# ResolveUrl("~/Content/placeholder-book.png") %>';">
            </div>
            <div class="card-body">
              <div class="card-title"><%# Eval("Title") %></div>
              <div class="card-text"><%# Eval("Author") %></div>
            </div>
          </div>
        </a>
      </div>
    </ItemTemplate>

    <FooterTemplate>
      </div>
    </FooterTemplate>
  </asp:Repeater>

  <asp:PlaceHolder ID="phNoBooks" runat="server" Visible="false">
    <div class="alert alert-warning mt-3 mb-0">Filtrenize uygun kitap bulunamadı.</div>
  </asp:PlaceHolder>

</asp:Content>
