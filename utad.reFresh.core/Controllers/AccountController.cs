using Microsoft.IdentityModel.Tokens;

namespace utad.reFresh.core.Controllers;

using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using utad.reFresh.core.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IEmailSender emailSender,
    IConfiguration configuration)
    : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IConfiguration _configuration = configuration;
    

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }
            
            var appUrl = Request.Headers["Origin"].FirstOrDefault() ?? "https://localhost:7095";
            var uri = new Uri(appUrl);
            
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: uri.Scheme,
                host: uri.Host + (uri.IsDefaultPort ? "" : $":{uri.Port}")
            );

            await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.");

            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        
        var puser = await _userManager.FindByEmailAsync(model.Email);
        if (puser == null)
            return BadRequest(new { error = "User not found" });

        if (!await _userManager.CheckPasswordAsync(puser, model.Password))
            return BadRequest(new { error = "Wrong password" });

        
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user == null || string.IsNullOrEmpty(user.Id) || string.IsNullOrEmpty(user.Email))
            {
                return NotFound("User not found.");
            }
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience : _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        return BadRequest("Invalid login attempt.");
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");
        return Ok(new
        {
           user.DisplayName,
           user.Email,
           user.PhotoUrl
        });
    }
    
    [HttpPost("changePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            return Ok("Password changed successfully.");
        }

        return BadRequest(result.Errors);
    }
    
    [HttpPost("sendRecoveryEmail")]
    public async Task<IActionResult> SendRecoveryEmail([FromBody] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound("User not found.");

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code = code },
            protocol: Request.Scheme);

        await _emailSender.SendEmailAsync(email, "Reset Password",
            $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.");

        return Ok("Recovery email sent.");
    }
    
    [HttpPost("changeUserData")] 
    [Authorize]
    public async Task<IActionResult> ChangeUserData([FromBody] ChangeUserDataModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");

        user.DisplayName = model.DisplayName ?? user.DisplayName;
        user.PhotoUrl = model.PhotoUrl;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return Ok("User data updated successfully.");
        }

        return BadRequest(result.Errors);
    }
    
}

public class ChangeUserDataModel
{
    public required string DisplayName { get; set; }
    public string? PhotoUrl { get; set; }
}

public class ChangePasswordModel
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}


public class RegisterModel
{ 
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
