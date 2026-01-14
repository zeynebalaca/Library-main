<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Library.Pages.Login" %>

<!DOCTYPE html>
<html lang="tr">
<head runat="server">
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Giriş Yap</title>
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
  <style>
    body{ background:#f7f9fc; margin:0; }
    #form1{
      min-height:100vh; display:flex; align-items:center; justify-content:center; padding:16px;
    }
    .login-card{
      width:100%; max-width:420px; padding:28px; border-radius:16px; background:#fff;
      box-shadow:0 10px 30px rgba(0,0,0,.06);
    }
  </style>
</head>
<body>
  <form id="form1" runat="server">
    
    <asp:ScriptManager ID="sm" runat="server" />

    <div class="login-card">
      <h4 class="mb-3 text-center">Kütüphane — Giriş</h4>

      <asp:Label ID="lblMessage" runat="server" EnableViewState="false" />

      <div class="mb-3">
        <label class="form-label">E-posta</label>
        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
      </div>

      <div class="mb-2">
        <label class="form-label">Şifre</label>
        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" />
      </div>

      <div class="d-grid gap-2 mt-3">
        <asp:Button ID="btnLogin" runat="server" CssClass="btn btn-primary"
          Text="Giriş Yap" OnClick="btnLogin_Click" />
        <button type="button" class="btn btn-outline-secondary" data-bs-toggle="modal" data-bs-target="#registerModal">
          Kayıt Ol
        </button>
      </div>
    </div>

    
    <div class="modal fade" id="registerModal" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">

          <div class="modal-header">
            <h5 class="modal-title">Yeni Hesap Oluştur</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Kapat"></button>
          </div>

          
          <asp:UpdatePanel ID="upRegister" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
              <div class="modal-body">
                
                <asp:Label ID="lblRegisterMessage" runat="server" CssClass="text-danger small d-block mb-2" EnableViewState="false" />

                <div class="mb-3">
                  <label class="form-label">Ad Soyad</label>
                  <asp:TextBox ID="txtRegName" runat="server" CssClass="form-control" />
                </div>
                <div class="mb-3">
                  <label class="form-label">E-posta</label>
                  <asp:TextBox ID="txtRegEmail" runat="server" CssClass="form-control" TextMode="Email" />
                </div>
                <div class="mb-1">
                  <label class="form-label">Şifre</label>
                  <asp:TextBox ID="txtRegPassword" runat="server" CssClass="form-control" TextMode="Password" />
                </div>
              </div>

              <div class="modal-footer">
                <button type="button" class="btn btn-light" data-bs-dismiss="modal">Kapat</button>
                
                <asp:Button ID="btnRegister" runat="server" CssClass="btn btn-success"
                  Text="Kaydet" OnClick="btnRegister_Click" UseSubmitBehavior="false" />
              </div>
            </ContentTemplate>
          </asp:UpdatePanel>
        </div>
      </div>
    </div>
  </form>

  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
