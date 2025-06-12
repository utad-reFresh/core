using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
                protocol: Request.Scheme
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    
    public class UpdateIngredientModel
    {
        public int Quantity { get; set; }
        public bool? IsFavorite { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
    
    [HttpPost("me/ingredient/{ingredientId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UpdateMyIngredient(int ingredientId, [FromBody] UpdateIngredientModel model, [FromServices] ApplicationDbContext db)
    {
        if (model.Quantity < 0)
            return BadRequest("Quantity cannot be negative.");

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("Authenticated user not found.");

        var ingredient = await db.Ingredients.FindAsync(ingredientId);
        if (ingredient == null)
            return NotFound("Ingredient not found.");

        var userIngredient = await db.UserIngredients
            .FirstOrDefaultAsync(ui => ui.UserId == user.Id && ui.IngredientId == ingredientId);

        if (userIngredient == null)
        {
            userIngredient = new UserIngredient
            {
                UserId = user.Id,
                IngredientId = ingredientId,
                Quantity = model.Quantity,
                isFavorite = model.IsFavorite ?? false,
                ExpirationDate = model.ExpirationDate
            };
            db.UserIngredients.Add(userIngredient);
        }
        else
        {
            userIngredient.Quantity = model.Quantity;
            if (model.IsFavorite.HasValue)
                userIngredient.isFavorite = model.IsFavorite.Value;
            if (model.ExpirationDate.HasValue)
                userIngredient.ExpirationDate = model.ExpirationDate;
        }

        await db.SaveChangesAsync();
        return Ok(new { ingredientId, model.Quantity, model.IsFavorite, model.ExpirationDate });
    }
    
    [HttpGet("me/ingredients")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMyIngredients([FromServices] ApplicationDbContext db)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("Authenticated user not found.");

        var ingredients = await db.UserIngredients
            .Where(ui => ui.UserId == user.Id)
            .Include(ui => ui.Ingredient)
            .Select(ui => new
            {
                ui.Ingredient.Id,
                ui.Ingredient.Name,
                ui.Ingredient.ImageUrl,
                ui.Quantity,
                ui.isFavorite,
                ui.ExpirationDate
            })
            .ToListAsync();

        return Ok(ingredients);
    }
    
    [HttpDelete("me/ingredient/{ingredientId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoveMyIngredient(int ingredientId, [FromServices] ApplicationDbContext db)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("Authenticated user not found.");

        var userIngredient = await db.UserIngredients
            .FirstOrDefaultAsync(ui => ui.UserId == user.Id && ui.IngredientId == ingredientId);

        if (userIngredient == null)
            return NotFound("Ingredient not found for user.");

        db.UserIngredients.Remove(userIngredient);
        await db.SaveChangesAsync();

        return Ok(new { message = "Ingredient removed from user." });
    }
    
    [HttpGet("me/ingredient/{ingredientId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetMyIngredient(int ingredientId, [FromServices] ApplicationDbContext db)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized("Authenticated user not found.");

        var userIngredient = await db.UserIngredients
            .Include(ui => ui.Ingredient)
            .FirstOrDefaultAsync(ui => ui.UserId == user.Id && ui.IngredientId == ingredientId);

        if (userIngredient == null)
            return NotFound("Ingredient not found for user.");

        return Ok(new
        {
            userIngredient.Ingredient.Id,
            userIngredient.Ingredient.Name,
            userIngredient.Ingredient.ImageUrl,
            userIngredient.Quantity,
            userIngredient.isFavorite,
            userIngredient.ExpirationDate
        });
    }
    
    
    [HttpPost("changePassword")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    public async Task<IActionResult> SendRecoveryEmail([FromBody] RecoveryEmailModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return NotFound("User not found.");

        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = Url.Page(
            "/Account/ResetPassword",
            pageHandler: null,
            values: new { area = "Identity", code = code },
            protocol: Request.Scheme);
        
        await _emailSender.SendEmailAsync(model.Email, "Reset Password",
            $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>clicking here</a>.");

        return Ok("Recovery email sent.");
    }
    
    [HttpPost("changeDisplayName")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangeDisplayName([FromBody] ChangeDisplayNameModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");

        user.DisplayName = model.DisplayName;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Ok("Display name updated successfully.");

        return BadRequest(result.Errors);
    }

    [HttpPost("changePhoto")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ChangePhoto([FromForm] ChangePhotoModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");

        if (model.Photo == null || model.Photo.Length == 0)
            return BadRequest("No photo uploaded.");

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Photo.FileName)}";
        var filePath = Path.Combine(uploads, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.Photo.CopyToAsync(stream);
        }

        user.PhotoUrl = $"/uploads/{fileName}";
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Ok("Photo updated successfully.");

        return BadRequest(result.Errors);
    }
    
    [HttpPost("removePhoto")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemovePhoto()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("Authenticated user not found.");

        user.PhotoUrl = null; // or set to a default image URL if needed
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
            return Ok("Photo removed successfully.");

        return BadRequest(result.Errors);
    }
    
}

public class ChangePhotoModel
{
    public IFormFile Photo { get; set; }
}

public class ChangeDisplayNameModel
{
    public required string DisplayName { get; set; }
}

public class ChangePasswordModel
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class RecoveryEmailModel
{
    public required string Email { get; set; }
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
