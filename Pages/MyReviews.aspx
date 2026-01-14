<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MyReviews.aspx.cs"
    Inherits="Library.Pages.MyReviews" MasterPageFile="~/Site.Master" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Benim Yorumlarım
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .review-title { font-weight:600; }
    .muted { color:#6c757d; }
  </style>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Benim Yorumlarım</h3>
  <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

  <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
    <div class="alert alert-info">Henüz bir yorumunuz yok.</div>
  </asp:PlaceHolder>

  <asp:Repeater ID="repReviews" runat="server"
                OnItemCommand="repReviews_ItemCommand"
                OnItemDataBound="repReviews_ItemDataBound">
    <HeaderTemplate>
      <div class="list-group">
    </HeaderTemplate>

    <ItemTemplate>
      <div class="list-group-item d-flex justify-content-between align-items-start">
        <div>
          <div class="review-title">
            <asp:Literal ID="litBookTitle" runat="server" /> – 
            <asp:Literal ID="litStars" runat="server" />
          </div>
          <small class="text-muted">“<asp:Literal ID="litComment" runat="server" />”</small>
          <div class="muted small">
            <asp:Literal ID="litDates" runat="server" />
          </div>
        </div>

        <div class="ms-3">
          <!-- Düzenle: postback YAPMA, sadece modal aç (JS) -->
          <asp:Button ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                      Text="Düzenle" UseSubmitBehavior="false" CausesValidation="false"
                      OnClientClick="return openEditModal(this);" />
          <!-- Sil: server tarafında çalışsın -->
          <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                      Text="Sil" CommandName="delete" CommandArgument='<%# Eval("ReviewId") %>'
                      OnClientClick="return confirm('Yorumu silmek istediğinize emin misiniz?');"
                      CausesValidation="false" />
        </div>

        <!-- satır verileri (JS buradan okuyacak) -->
        <asp:HiddenField ID="hidReviewId" runat="server" Value='<%# Eval("ReviewId") %>' />
        <asp:HiddenField ID="hidBookId"   runat="server" Value='<%# Eval("BookId") %>' />
        <asp:HiddenField ID="hidRating"   runat="server" Value='<%# Eval("Rating") %>' />
        <asp:HiddenField ID="hidComment"  runat="server" Value='<%# Eval("Comment") %>' />
      </div>
    </ItemTemplate>

    <FooterTemplate>
      </div>
    </FooterTemplate>
  </asp:Repeater>

  <!-- Düzenleme Modalı -->
  <div class="modal fade" id="editModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Yorumu Düzenle</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
        </div>
        <div class="modal-body">
          <asp:HiddenField ID="hidEditReviewId" runat="server" />
          <div class="mb-3">
            <label class="form-label">Puan</label>
            <asp:DropDownList ID="ddlRating" runat="server" CssClass="form-select">
              <asp:ListItem Value="1">1</asp:ListItem>
              <asp:ListItem Value="2">2</asp:ListItem>
              <asp:ListItem Value="3">3</asp:ListItem>
              <asp:ListItem Value="4">4</asp:ListItem>
              <asp:ListItem Value="5">5</asp:ListItem>
            </asp:DropDownList>
          </div>
          <div class="mb-3">
            <label class="form-label">Yorum</label>
            <asp:TextBox ID="txtComment" runat="server" CssClass="form-control"
                         TextMode="MultiLine" Rows="4" MaxLength="1000"></asp:TextBox>
          </div>
        </div>
        <div class="modal-footer">
          <!-- Bu buton SERVER'A postback yapar ve DB'yi günceller -->
          <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary"
                      Text="Kaydet" OnClick="btnSave_Click" />
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
        </div>
      </div>
    </div>
  </div>

  <script>
    // Düzenle butonu JS: satırdaki hidden'lardan veriyi al, modal inputlarını doldur, modalı aç
    function openEditModal(btn) {
      var row = btn.closest('.list-group-item');
      if (!row) return false;

      var id       = row.querySelector('[id$="hidReviewId"]').value;
      var rating   = row.querySelector('[id$="hidRating"]').value;
      var comment  = row.querySelector('[id$="hidComment"]').value;

      document.getElementById('<%= hidEditReviewId.ClientID %>').value = id;
      document.getElementById('<%= ddlRating.ClientID %>').value = rating || "5";
      document.getElementById('<%= txtComment.ClientID %>').value = comment || "";

          var m = new bootstrap.Modal(document.getElementById('editModal'));
          m.show();

          // false döndürürsek buton postback yapmaz (sadece modal açılır)
          return false;
      }
  </script>

</asp:Content>
