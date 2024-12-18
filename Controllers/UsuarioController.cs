using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SiteAgendamento.Models;
using SiteAgendamento.Repositorio;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace SiteAgendamento.Controllers
{
    
    public class UsuarioController : Controller
    {
        private readonly UsuarioRepositorio _usuarioRepositorio;
        private readonly ILogger<UsuarioController> _logger;
        public UsuarioController(UsuarioRepositorio usuarioRepositorio, ILogger<UsuarioController> logger)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _logger = logger;
        }
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }
        public IActionResult Cadastro()
        {
            return View();

        }
        [Authorize]
        public IActionResult Index()
        {
           

            List<SelectListItem> tipoUsuario = new List<SelectListItem>
             {
                 new SelectListItem { Value = "1", Text = "Administrador" },
                 new SelectListItem { Value = "0", Text = "Cliente" }            
             };

            ViewBag.lstTipoUsuario = new SelectList(tipoUsuario, "Value", "Text");

            var Usuarios = _usuarioRepositorio.ListarUsuarios();
            return View(Usuarios);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> VerificarLogin(LoginViewModel model)
        {
            try
            {
                // Verifica se o modelo é válido
                if (!ModelState.IsValid)
                {
                    // Retorna à view de login com o modelo, incluindo os erros de validação, se houver
                    return View("Login", model); // Especifica a view "Login"
                }

                // Verifica se o usuário existe com as credenciais fornecidas
                var usuario = _usuarioRepositorio.VerificarLogin(model.Email, model.Senha);

                if (usuario != null)
                {
                    // Criação das claims do usuário para o cookie de autenticação
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                // Você pode adicionar outras claims, como roles ou permissões, se necessário
            };

                    // Criação da identidade do usuário para o cookie de autenticação
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    // Criação do principal (usuário autenticado)
                    var principal = new ClaimsPrincipal(identity);

                    // Autenticação do usuário e criação do cookie
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Redireciona para a página inicial ou outra página que você preferir
                    return RedirectToAction("Index", "Home");  // Redirecionamento para a página inicial
                }
                else
                {
                    // Caso o usuário não seja encontrado, exibe uma mensagem de erro
                    ViewData["ErrorMessage"] = "E-mail ou senha inválidos.";
                    Debug.WriteLine("Erro: E-mail ou senha inválidos.");

                    // Redireciona para a view "Login", onde a mensagem de erro será exibida
                    return View("Login", model); // Retorna para a view Login com o modelo
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro inesperado, captura o erro e exibe a mensagem
                ViewData["ErrorMessage"] = "Erro ao processar a solicitação. Detalhes: " + ex.Message;
                return View("Login", model); // Retorna à mesma view "Login" com a mensagem de erro
            }
        }
            [HttpPost]
            [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
            {
                try
                {
                    var csrfToken = Request.Form["__RequestVerificationToken"];
                    if (string.IsNullOrEmpty(csrfToken))
                    {
                        throw new Exception("Token CSRF ausente");
                    }

                    // Limpa o cookie de autenticação
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    // Limpar a sessão
                    HttpContext.Session.Clear();  // Limpa todos os dados de sessão armazenados

                    // Limpar os cookies de sessão
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);  // Deleta todos os cookies
                    }

                    // Evitar cache de páginas protegidas
                    Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                    Response.Headers["Pragma"] = "no-cache";
                    Response.Headers["Expires"] = "0";

                // Após limpar tudo, redirecionar para a página de login
                // if (!User.Identity.IsAuthenticated)
                //// {
                //    return RedirectToAction("Login", "Usuario");
                // }

                // Verificar se é uma requisição AJAX
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    else
                    {
                        return RedirectToAction("Login", "Usuario");  // Redireciona para a página de login
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
        public IActionResult InserirUsuario(string Nome, string Senha, string Email, string Telefone, int TipoUsuario)
        {
            try
            {
                // Chama o método do repositório que realiza a inserção no banco de dados
                var resultado = _usuarioRepositorio.InserirUsuario(Nome, Email, Telefone, Senha, TipoUsuario);

                // Verifica o resultado da inserção
                if (resultado)
                {
                    // Se o resultado for verdadeiro, significa que o usuário foi inserido com sucesso
                    return Json(new { success = true, message = "Cliente inserido com sucesso!" });
                }
                else
                {
                    // Se o resultado for falso, significa que houve um erro ao tentar inserir o usuário
                    return Json(new { success = false, message = "Erro ao inserir o cliente. Tente novamente." });
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro inesperado, captura e exibe o erro
                return Json(new { success = false, message = "Erro ao processar a solicitação. Detalhes: " + ex.Message });
            }
        }       
        public IActionResult AtualizarUsuario(int id, string Nome, string Senha, string Email, string Telefone, int TipoUsuario)
        {
            try
            {
                // Chama o repositório para atualizar o usuário
                var resultado = _usuarioRepositorio.AtualizarUsuario(id, Nome, Email, Telefone, Senha, TipoUsuario);

                if (resultado)
                {
                    return Json(new { success = true, message = "Usuário atualizado com sucesso!" });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao atualizar o usuário. Verifique se o usuário existe." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao processar a solicitação. Detalhes: " + ex.Message });
            }
        }
        public IActionResult ExcluirUsuario(int id)
        {
            try
            {
                // Chama o repositório para excluir o usuário
                var resultado = _usuarioRepositorio.ExcluirUsuario(id);

                if (resultado)
                {
                    return Json(new { success = true, message = "Usuário excluído com sucesso!" });
                }
                else
                {
                    // Se o resultado for falso, você pode fornecer uma mensagem mais específica.
                    return Json(new { success = false, message = "Não foi possível excluir o usuário. Verifique se ele está vinculado a outros registros no sistema." });
                }
            }
            catch (Exception ex)
            {
                // Captura qualquer erro e inclui a mensagem detalhada da exceção
                return Json(new { success = false, message = "Erro ao processar a solicitação. Detalhes: " + ex.Message });
            }
        }       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    
 }



  