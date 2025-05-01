using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly AplicationDbContext context;

        public UsuariosController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, AplicationDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = new IdentityUser()
            {
                UserName = modelo.Email,
                Email = modelo.Email,
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(usuario, modelo.Password);

            if (resultado.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(modelo);
            }

        }

        [AllowAnonymous]
        public IActionResult Login(string mensaje = null)
        {
            if(mensaje is not null)
            {
                ViewData["mensaje"] = mensaje;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = context.Users.FirstOrDefault(u => u.Email == modelo.Email);
            var resultado = await signInManager.PasswordSignInAsync(modelo.Email, modelo.Password, modelo.Recuerdame, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Nombre de usuario o password incorrecto");
                return View(modelo);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult LoginExterno(string proveedor, string urlRetorno = null)
        {
            var urlRedireccion = Url.Action("RegistrarUsuarioExterno", values: new { urlRetorno });
            var proppiedades = signInManager.ConfigureExternalAuthenticationProperties(proveedor, urlRedireccion);
            return new ChallengeResult(proveedor, proppiedades);
        }

        [AllowAnonymous]
        public async Task<IActionResult> RegistrarUsuarioExterno(string urlRetorno = null, string remoteError = null)
        {
            urlRetorno = urlRetorno ?? Url.Content("~/");

            var mensaje = "";

            if (remoteError is not null)
            {
                mensaje = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info is null)
            {
                mensaje = "Error cargando la data de login externo";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoLoginExterno = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            //Ya la cuenta existe
            if (resultadoLoginExterno.Succeeded)
            {
                return LocalRedirect(urlRetorno);
            }

            //No existe la cuenta, se crea una nueva
            string email = "";

            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                email = info.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                mensaje = "Error leyendo el email del usuario del proveedor";
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var usuario = new IdentityUser()
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var resultadoCrearUsuario = await userManager.CreateAsync(usuario);

            if (!resultadoCrearUsuario.Succeeded)
            {
                mensaje = resultadoCrearUsuario.Errors.First().Description;
                return RedirectToAction("Login", routeValues: new { mensaje });
            }

            var resultadoAgregarLogin = await userManager.AddLoginAsync(usuario, info);

            if (resultadoAgregarLogin.Succeeded)
            {
                await signInManager.SignInAsync(usuario, isPersistent: false, info.LoginProvider);
                return LocalRedirect(urlRetorno);
            }

            mensaje = "Ha ocurrido un error agregando el login";
            return RedirectToAction("Login", routeValues: new { mensaje });

        }

        [HttpGet]
        [Authorize(Roles =Constantes.RolAdmin)]
        public async Task<IActionResult> Listado(string mensaje = null)
        {
            var usuarios = await context.Users.Select(x=>new UsuarioViewModel
            {
                Email = x.Email
            }).ToListAsync();

            var modelo = new UsuariosListadoViewModel();
            modelo.Usuarios = usuarios;
            modelo.Mensaje = mensaje;

            return View(modelo);
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> HacerAdmin(string email)
        {
            var usuario = await context.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (usuario is null)
            {
                return NotFound();
            }
            
            await userManager.AddToRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado", routeValues: new {mensaje="Rol asignado correctamente a " + email} );
        }

        [HttpPost]
        [Authorize(Roles = Constantes.RolAdmin)]
        public async Task<IActionResult> RemoverAdmin(string email)
        {
            var usuario = await context.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
            if (usuario is null)
            {
                return NotFound();
            }

            await userManager.RemoveFromRoleAsync(usuario, Constantes.RolAdmin);

            return RedirectToAction("Listado", routeValues: new { mensaje = "Rol removido correctamente a " + email });
        }


    }
}
