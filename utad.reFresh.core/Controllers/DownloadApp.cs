using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace utad.reFresh.core.Controllers;


[Route("api/[controller]")]
[ApiController]
public class DownloadApp(IConfiguration configuration) : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration Configuration = configuration;

    [HttpPost("ghw")]
    public async Task<IActionResult> Post()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();
        
        var eventType = Request.Headers["X-GitHub-Event"].FirstOrDefault();
        if (string.IsNullOrEmpty(eventType))
            return BadRequest("Missing X-GitHub-Event header.");
        
        if (eventType != "push")
            return Ok();
        

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
            return Unauthorized();

        var secret = Configuration["GH:Secret"]; // injeta IConfiguration no controller

        
        if (string.IsNullOrEmpty(secret))
            return Unauthorized("Secret not configured");
        
        if (string.IsNullOrEmpty(payload))
            return BadRequest("Payload is empty");
        
        
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var hashString = "sha256=" + BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hashString),
                Encoding.UTF8.GetBytes(signature)))
        {
            return Unauthorized();
        }

        // Rodar script de atualização
        
        // ver se estamos em linux 
        if (Environment.OSVersion.Platform != PlatformID.Unix)
            return BadRequest("This endpoint is only available on Linux systems.");
        if (!System.IO.File.Exists("Scripts/update_project.sh"))
            return BadRequest("Update script not found.");
        
        var result = await RunShellScriptAsync("Scripts/update_project.sh");

        return Ok(new { status = result });
    }

    private async Task<string> RunShellScriptAsync(string path)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = path,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _env.ContentRootPath
        };

        var process = Process.Start(startInfo);
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return $"Output:\n{output}\n\nErrors:\n{error}";
    }
}
