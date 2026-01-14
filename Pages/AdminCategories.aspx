<%@ Page Language="C#" Async="true" AutoEventWireup="true"
    CodeBehind="AdminCategories.aspx.cs"
    Inherits="Library.Pages.AdminCategories"
    MasterPageFile="~/Site.Master" %>


<asp:Content ID="t" ContentPlaceHolderID="TitleContent" runat="server">
  Kategori Yönetimi
</asp:Content>

<asp:Content ID="h" ContentPlaceHolderID="HeadContent" runat="server">
  <style>
    body { background-color:#f8f9fa; }
    .card-soft { border-radius:1rem; box-shadow:0 0.25rem 1rem rgba(0,0,0,.06);}
    .list-group-item { display:flex; justify-content:space-between; align-items:center; }
  </style>
    <style>
    .alert {
        max-width: 300px;
        padding: 10px 15px;
        
    }
</style>

</asp:Content>

<asp:Content ID="m" ContentPlaceHolderID="MainContent" runat="server">

  <h3 class="mb-3">Kategori Yönetimi</h3>
 <div style="position:absolute; right:150px; top:140px; z-index:9999;">
    <asp:Label ID="lblMsg" runat="server" CssClass="alert alert-danger d-none" 
               style="padding:10px 15px; margin:0;"/>
</div>




  <!-- Kategori ekleme -->
  <div class="input-group mb-3" style="max-width:400px;">
    <asp:TextBox ID="txtNewCategory" runat="server" CssClass="form-control" placeholder="Yeni kategori adı" />
    <asp:Button ID="btnAdd" runat="server" CssClass="btn btn-dark" Text="Ekle" OnClick="btnAdd_Click" />
  </div>

  <!-- Liste -->
  <asp:Repeater ID="repCategories" runat="server" OnItemCommand="repCategories_ItemCommand">
    <HeaderTemplate>
      <ul class="list-group card-soft">
    </HeaderTemplate>
    <ItemTemplate>
      <li class="list-group-item">
        <span><asp:Literal ID="litName" runat="server" Text='<%# Eval("Name") %>' /></span>
        <span>
          <button type="button" class="btn btn-sm btn-outline-secondary me-1" onclick="openEdit(this);">Yeniden Adlandır</button>
          <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                      Text="Sil" CommandName="delete" CommandArgument='<%# Eval("Id") %>'
                      OnClientClick="return confirm('Bu kategoriyi silmek istediğinize emin misiniz?');"
                      CausesValidation="false" />
        </span>
        <asp:HiddenField ID="hidId" runat="server" Value='<%# Eval("Id") %>' />
        <asp:HiddenField ID="hidName" runat="server" Value='<%# Eval("Name") %>' />
      </li>
    </ItemTemplate>
    <FooterTemplate>
      </ul>
    </FooterTemplate>
  </asp:Repeater>

  <!-- Düzenleme Modalı -->
  <div class="modal fade" id="editModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">Kategori Düzenle</h5>
          <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
        </div>
        <div class="modal-body">
          <asp:HiddenField ID="hidEditId" runat="server" />
          <label class="form-label">Kategori Adı</label>
          <asp:TextBox ID="txtEditName" runat="server" CssClass="form-control" MaxLength="100" />
        </div>
        <div class="modal-footer">
          <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Kaydet" OnClick="btnSave_Click" />
          <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Kapat</button>
        </div>
      </div>
    </div>
  </div>

  <script>
    function openEdit(btn){
      var li = btn.closest('li');
      document.getElementById('<%= hidEditId.ClientID %>').value = li.querySelector('[id$="hidId"]').value;
      document.getElementById('<%= txtEditName.ClientID %>').value = li.querySelector('[id$="hidName"]').value;
      new bootstrap.Modal(document.getElementById('editModal')).show();
    }
  </script>

</asp:Content>
