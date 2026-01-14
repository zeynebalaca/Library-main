<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminBooks.aspx.cs"
    Inherits="Library.Pages.AdminBooks" MasterPageFile="~/Site.Master" %>

<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Kitap Yönetimi (Admin)
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06); }
    .cover-thumb { width:42px; height:60px; object-fit:cover; border-radius:.25rem; border:1px solid #e5e7eb; }
  </style>
</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Kitap Yönetimi</h3>
  <asp:Label ID="lblMsg" runat="server" EnableViewState="false" />

  <div class="d-flex justify-content-between align-items-center mb-3">
    <div></div>
    <button type="button" class="btn btn-dark" onclick="openNew();">Yeni Kitap Ekle</button>
  </div>

  <asp:Repeater ID="repBooks" runat="server" OnItemCommand="repBooks_ItemCommand">
    <HeaderTemplate>
      <table class="table table-striped align-middle bg-white card-soft">
        <thead class="table-light">
          <tr>
            <th>Kapak</th>
            <th>Başlık</th>
            <th>Yazar</th>
            <th>ISBN</th>
            <th>Kategori</th>
            <th style="width:1%"></th>
          </tr>
        </thead>
        <tbody>
    </HeaderTemplate>

    <ItemTemplate>
      <tr>
        <td>
          <img src='<%# CoverSrc(Eval("CoverPath")) %>' class="cover-thumb" alt="kapak" />
        </td>
        <td><%# Eval("Title") %></td>
        <td><%# Eval("Author") %></td>
        <td><%# Eval("ISBN") %></td>
        <td><%# Eval("CategoryName") %></td>
        <td class="text-end">
          <button type="button" class="btn btn-sm btn-outline-secondary me-1" onclick="openEdit(this);">Düzenle</button>
          <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                      Text="Sil" CommandName="delete" CommandArgument='<%# Eval("Id") %>'
                      OnClientClick="return confirm('Bu kitabı silmek istediğinize emin misiniz?');"
                      CausesValidation="false" />
        </td>

        <!-- JS için -->
        <asp:HiddenField ID="hidId" runat="server" Value='<%# Eval("Id") %>' />
        <asp:HiddenField ID="hidTitle" runat="server" Value='<%# Eval("Title") %>' />
        <asp:HiddenField ID="hidAuthor" runat="server" Value='<%# Eval("Author") %>' />
        <asp:HiddenField ID="hidIsbn" runat="server" Value='<%# Eval("ISBN") %>' />
        <asp:HiddenField ID="hidCategoryId" runat="server" Value='<%# Eval("CategoryId") %>' />
      </tr>
    </ItemTemplate>

    <FooterTemplate>
        </tbody>
      </table>
    </FooterTemplate>
  </asp:Repeater>

  <!-- Modal -->
  <div class="modal fade" id="editModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-lg">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title" id="mdlTitle">Kitap</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <asp:HiddenField ID="hidEditId" runat="server" />
          <div class="row g-3">
            <div class="col-md-6">
              <label class="form-label">Başlık</label>
              <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-6">
              <label class="form-label">Yazar</label>
              <asp:TextBox ID="txtAuthor" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-6">
              <label class="form-label">ISBN</label>
              <asp:TextBox ID="txtIsbn" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-6">
              <label class="form-label">Kategori</label>
              <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
            </div>
            <div class="col-md-12">
              <label class="form-label">Kapak (jpg/png)</label>
              <asp:FileUpload ID="fuCover" runat="server" CssClass="form-control" />
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Kaydet" OnClick="btnSave_Click" />
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
        </div>
      </div>
    </div>
  </div>

  <script>
    function openNew(){
      document.getElementById('mdlTitle').innerText='Yeni Kitap';
      document.getElementById('<%= hidEditId.ClientID %>').value='';
      document.getElementById('<%= txtTitle.ClientID %>').value='';
      document.getElementById('<%= txtAuthor.ClientID %>').value='';
      document.getElementById('<%= txtIsbn.ClientID %>').value='';
      document.getElementById('<%= ddlCategory.ClientID %>').value='';
      new bootstrap.Modal(document.getElementById('editModal')).show();
    }
    function openEdit(btn){
      var r=btn.closest('tr');
      document.getElementById('mdlTitle').innerText='Kitabı Düzenle';
      document.getElementById('<%= hidEditId.ClientID %>').value=r.querySelector('[id$="hidId"]').value;
      document.getElementById('<%= txtTitle.ClientID %>').value=r.querySelector('[id$="hidTitle"]').value;
      document.getElementById('<%= txtAuthor.ClientID %>').value=r.querySelector('[id$="hidAuthor"]').value;
      document.getElementById('<%= txtIsbn.ClientID %>').value=r.querySelector('[id$="hidIsbn"]').value;
      document.getElementById('<%= ddlCategory.ClientID %>').value = r.querySelector('[id$="hidCategoryId"]').value || '';
          new bootstrap.Modal(document.getElementById('editModal')).show();
      }
  </script>

</asp:Content>
