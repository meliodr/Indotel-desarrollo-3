using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using INDOTEL.WEB.Models;

namespace INDOTEL.WEB.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Inyectamos el HttpClientFactory configurado en Program.cs
        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Ciudadano");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");

                
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

               
                var response = await client.PostAsync("/api/auth/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    
                    var loginResult = JsonSerializer.Deserialize<LoginResponse>(responseString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResult != null && !string.IsNullOrEmpty(loginResult.Token))
                    {
                        
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, loginResult.Usuario.Id.ToString()),
                            new Claim(ClaimTypes.Name, $"{loginResult.Usuario.Nombres} {loginResult.Usuario.Apellidos}"),
                            new Claim(ClaimTypes.Email, loginResult.Usuario.Correo),
                            new Claim("JWToken", loginResult.Token), 
                            new Claim("RefreshToken", loginResult.RefreshToken)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true, // Mantiene la cookie activa
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20)
                        };

                        
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                        
                        return RedirectToAction("Index", "Ciudadano");
                    }
                }
                else
                {
                    // Manejo de errores según la tabla del documento (ej. 401 Credenciales incorrectas, 423 Bloqueado)
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                    }
                    else if ((int)response.StatusCode == 423)
                    {
                        ModelState.AddModelError(string.Empty, "Usuario bloqueado temporalmente por intentos fallidos.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Error al intentar iniciar sesión. Inténtelo más tarde.");
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Hubo un problema de conexión con el servidor central.");
            }

            return View(model);
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Ciudadano");
            }
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterCiudadanoRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("IndotelCore");
                var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

                
                var response = await client.PostAsync("/api/auth/register-ciudadano", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    
                    TempData["SuccessMessage"] = "Registro completado con éxito. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                else
                {
                    // Manejo de errores específicos según el documento (ej: 409 Conflicto si el correo o cédula ya existen)
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        ModelState.AddModelError(string.Empty, "La cédula o el correo electrónico ya se encuentran registrados.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        ModelState.AddModelError(string.Empty, "Los datos enviados son incorrectos. Verifique el formato.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Ocurrió un error en el servidor al procesar el registro.");
                    }
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "No se pudo establecer conexión con el servicio central.");
            }

            return View(model);
        }


        // POST: /Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
          
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}