using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using BookRenting.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;

namespace BookRenting.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly PasswordHasher<RegisterUser> _passwordHasher;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = new PasswordHasher<RegisterUser>();
        }

        // ---------------- GET Register ----------------
        [HttpGet]
        public IActionResult Register() => View();

        // ---------------- POST Register ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUser user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                // Hash password
                user.Password = _passwordHasher.HashPassword(user, user.Password);
                user.IsEmailVerified = true;

                _context.RegisterUsers.Add(user);
                await _context.SaveChangesAsync();

                // Add login credentials to Logins table
                var login = new Login
                {
                    registered_id = user.registered_id, 
                    Email = user.Email,
                    UserName = user.UserName,
                    Password = user.Password // hashed
                };
                _context.Logins.Add(login);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving new user");
                ModelState.AddModelError(string.Empty, "An error occurred. Please try again.");
                return View(user);
            }
        }

        // ---------------- Send OTP ----------------
        [HttpPost]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email is required." });

            if (_context.RegisterUsers.Any(u => u.Email == email))
                return Json(new { success = false, message = "Email already exists." });

            var otp = new Random().Next(100000, 999999).ToString();

            TempData["OTP"] = otp;
            TempData["OtpTime"] = DateTime.UtcNow;

            try
            {
                await SendOtpEmail(email, otp);
                return Json(new { success = true, message = "OTP sent successfully. It will expire in 3 minutes." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email.");
                return Json(new { success = false, message = "Failed to send OTP. Please try again later." });
            }
        }

        private async Task SendOtpEmail(string toEmail, string otp)
        {
            var emailUser = Environment.GetEnvironmentVariable("EMAIL_USER") ?? "llamedo.kylamarie@gmail.com";
            var emailPass = Environment.GetEnvironmentVariable("EMAIL_PASS") ?? "zjgu qroi agfb ahzj";

            using var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new System.Net.NetworkCredential(emailUser, emailPass),
                EnableSsl = true
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(emailUser),
                Subject = "Your OTP Code",
                Body = $"Your OTP code is: {otp}",
                IsBodyHtml = false
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }

        // ---------------- GET Login ----------------
        [HttpGet]
        public IActionResult Login() => View();

   // ---------------- POST Login ----------------
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(string email, string password)
{
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        ModelState.AddModelError(string.Empty, "Email and password are required.");
        return View();
    }

    // 1️⃣ Check normal registered users first
    var loginUser = await _context.RegisterUsers.FirstOrDefaultAsync(u => u.Email == email);

    if (loginUser != null)
    {
        if (string.IsNullOrEmpty(loginUser.Password))
        {
            ModelState.AddModelError(string.Empty, "This account uses Google login. Please sign in with Google.");
            return View();
        }

        var result = _passwordHasher.VerifyHashedPassword(loginUser, loginUser.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View();
        }

        // Sign in as normal user
        await SignInUser(loginUser.Email, "User");
        return RedirectToAction("Dashboard", "RentingStore");
    }

    // 2️⃣ Check Admins table
    var adminUser = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
    if (adminUser != null)
    {
        if (string.IsNullOrEmpty(adminUser.Password))
        {
            ModelState.AddModelError(string.Empty, "Admin password is not set. Contact developer.");
            return View();
        }

        var adminHasher = new PasswordHasher<Admin>();
        var result = adminHasher.VerifyHashedPassword(adminUser, adminUser.Password, password);

        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
            return View();
        }

        // Sign in as admin
        await SignInUser(adminUser.Email, "Admin");
        return RedirectToAction("AdminDashboard", "Admin");
    }

    // 3️⃣ Not found in either table
    ModelState.AddModelError(string.Empty, "Invalid credentials.");
    return View();
}


// ---------------- Helper: SignInUser ----------------
private async Task SignInUser(string email, string role)
{
    var claimsIdentity = new System.Security.Claims.ClaimsIdentity(
        new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, email),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
        },
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new System.Security.Claims.ClaimsPrincipal(claimsIdentity)
    );
}

        // ---------------- Google Login ----------------
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // ---------------- Google OAuth callback ----------------
[HttpGet]
public async Task<IActionResult> GoogleResponse()
{
    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
    var claims = result?.Principal?.Identities.FirstOrDefault()?.Claims;

    var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    var fullName = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "User";

    if (string.IsNullOrEmpty(email))
        return RedirectToAction("Login");

    // Check if user already exists
    var user = await _context.RegisterUsers.FirstOrDefaultAsync(u => u.Email == email);
    if (user != null)
    {
        // Make sure there's also a record in Logins table
        var existingLogin = await _context.Logins.FirstOrDefaultAsync(l => l.Email == email);
        if (existingLogin == null)
        {
            var login = new Login
            {
                registered_id = user.registered_id,
                Email = user.Email,
                UserName = user.UserName,
                Password = user.Password // whatever password hash is stored
            };
            _context.Logins.Add(login);
            await _context.SaveChangesAsync();
        }

        await SignInUser(user.Email);
        return RedirectToAction("Dashboard", "RentingStore");
    }

    // Safe handling of fullName
    var names = fullName.Split(' ', 2); 
    var firstName = names.Length > 0 ? names[0] : "User";
    var lastName = names.Length > 1 ? names[1] : "";

    // Create new Google user
    var newUser = new RegisterUser
    {
        Email = email,
        UserName = fullName.Replace(" ", ""),
        FirstName = firstName,
        LastName = lastName
    };

    // Assign dummy password since Google users don't have one
    newUser.Password = _passwordHasher.HashPassword(newUser, Guid.NewGuid().ToString());

    _context.RegisterUsers.Add(newUser);
    await _context.SaveChangesAsync();

    // Add to Logins table too
    var googleLogin = new Login
    {
        registered_id = newUser.registered_id,
        Email = newUser.Email,
        UserName = newUser.UserName,
        Password = newUser.Password
    };
    _context.Logins.Add(googleLogin);
    await _context.SaveChangesAsync();

    await SignInUser(newUser.Email);
    return RedirectToAction("Dashboard", "RentingStore");
}


        // ---------------- Helper: Sign In ----------------
        private async Task SignInUser(string email)
        {
            var claimsIdentity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, email) },
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }

        // ---------------- Logout ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult RentBook(int id)
{
    var book = _context.Books.FirstOrDefault(b => b.Id == id);
    if (book == null) return NotFound();

    return View(book);
}

    }
}
