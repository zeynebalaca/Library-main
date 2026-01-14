<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BookDetails.aspx.cs"
    Inherits="Library.Pages.BookDetails" MasterPageFile="~/Site.Master" %>

<asp:Content ID="c1" ContentPlaceHolderID="TitleContent" runat="server">
  Kitap Detayı
</asp:Content>

<asp:Content ID="c2" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    .cover-box { aspect-ratio: 2/3; overflow:hidden; border-radius:.6rem; box-shadow:0 0.3rem 1rem rgba(0,0,0,.08); }
    .cover     { width:100%; height:100%; object-fit:cover; }
    .muted     { color:#6c757d; }
    .star      { color:#ffd23f; margin-right:2px; }
  </style>
</asp:Content>

<asp:Content ID="c3" ContentPlaceHolderID="MainContent" runat="server">

  <asp:Panel ID="pnlNotFound" runat="server" Visible="false" CssClass="alert alert-warning">
    Aradığınız kitap bulunamadı.
  </asp:Panel>

  <asp:Panel ID="pnlContent" runat="server" Visible="false">
    <div class="row g-4 align-items-start mb-4">
      <div class="col-md-3">
        <div class="cover-box">
          <!-- HATA YAPAN onerror satırı kaldırıldı; code-behind'da eklenecek -->
          <img id="imgCover" runat="server" class="cover" />
        </div>
      </div>
      <div class="col-md-9">
        <h3 class="mb-1"><asp:Literal ID="litTitle" runat="server" /></h3>
        <div class="muted mb-2">
          <asp:Literal ID="litAuthor" runat="server" /> • 
          <asp:Literal ID="litYear" runat="server" /> • 
          <asp:Literal ID="litCategory" runat="server" />
        </div>

        <div class="mb-3">
          <span id="stars" runat="server"></span>
          <span class="muted">( <asp:Literal ID="litAvg" runat="server" /> )</span>
        </div>

        <div class="d-flex gap-2 align-items-center">
          <span class="badge bg-success">Mevcut: <asp:Literal ID="litAvail" runat="server" /></span>
          <span class="badge bg-secondary">Toplam Kopya: <asp:Literal ID="litCopies" runat="server" /></span>

          <asp:Button ID="btnBorrow" runat="server" CssClass="btn btn-primary btn-sm"
                      Text="Ödünç Al (14 gün)" OnClick="btnBorrow_Click" />
          <asp:Label ID="lblBorrowMsg" runat="server" CssClass="ms-2"></asp:Label>
        </div>
      </div>
    </div>

    <div class="row g-4">
      <div class="col-lg-6">
        <div class="card">
          <div class="card-body">
            <h5 class="mb-3">Yorumlar</h5>
            <asp:Repeater ID="repReviews" runat="server">
              <ItemTemplate>
                <div class="mb-3 pb-3 border-bottom">
                  <div class="fw-semibold"><%# Eval("UserName") %></div>
                  <div class="small text-muted"><%# string.Format("{0:yyyy-MM-dd HH:mm}", Eval("CreatedAt")) %></div>
                  <div class="mt-1"><%# Stars(Convert.ToInt32(Eval("Rating"))) %></div>
                  <div class="mt-2"><%# Eval("Comment") %></div>
                </div>
              </ItemTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="phNoReviews" runat="server" Visible="false">
              <div class="text-muted">Bu kitap için henüz yorum yok.</div>
            </asp:PlaceHolder>
          </div>
        </div>
      </div>

      <div class="col-lg-6">
        <div class="card">
          <div class="card-body">
            <h5 class="mb-3">Yorum Yaz</h5>

            <asp:Panel ID="pnlReviewForm" runat="server">
              <div class="mb-2">
                <label class="form-label">Puan</label>
                <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select form-select-sm" />
              </div>
              <div class="mb-3">
                <label class="form-label">Yorum</label>
                <asp:TextBox ID="txtComment" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
              </div>
              <asp:Button ID="btnAddReview" runat="server" CssClass="btn btn-success btn-sm" Text="Kaydet" OnClick="btnAddReview_Click" />
              <asp:Label ID="lblReviewMsg" runat="server" CssClass="ms-2"></asp:Label>
            </asp:Panel>

            <asp:Panel ID="pnlAdminNote" runat="server" Visible="false" CssClass="alert alert-info mb-0">
              Admin kullanıcılar yorum ekleme alanını görmez.
            </asp:Panel>
          </div>
        </div>
      </div>
    </div>
  </asp:Panel>
</asp:Content>
