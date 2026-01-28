using FoodDeliveryBackend.API.DTOs.ChainOwner;
using FoodDeliveryBackend.API.Services;
using FoodDeliveryBackend.Core.Entities.ChainOwner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodDeliveryBackend.API.Controllers.ChainOwner;

[ApiController]
[Route("api/auth")]
public class ChainOwnerAuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly string _jwtSecret;

    public ChainOwnerAuthController(IAccountService accountService)
    {
        _accountService = accountService;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? "super_secret_key_change_this_in_production_1234567890";
    }

    /// <summary>
    /// Register a new Chain Owner account.
    /// Account will be created with Pending status and auto-submitted for approval.
    /// </summary>
    [HttpPost("register/chain-owner")]
    public async Task<ActionResult<ChainOwnerProfileDto>> RegisterChainOwner([FromBody] RegisterChainOwnerDto dto)
    {
        try
        {
            var chainOwner = await _accountService.RegisterChainOwnerAsync(
                dto.Email,
                dto.Password,
                dto.FullName,
                dto.PhoneNumber,
                dto.BusinessName,
                dto.BusinessRegistrationNumber,
                dto.RegionCode);

            var response = MapToProfileDto(chainOwner);
            return CreatedAtAction(nameof(RegisterChainOwner), response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Chain Owner login.
    /// </summary>
    [HttpPost("login/chain-owner")]
    public async Task<ActionResult<ChainOwnerLoginResponseDto>> LoginChainOwner([FromBody] ChainOwnerLoginDto dto)
    {
        var chainOwner = await _accountService.AuthenticateChainOwnerAsync(dto.Email, dto.Password);
        if (chainOwner == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateJwtToken(chainOwner);

        return Ok(new ChainOwnerLoginResponseDto(
            chainOwner.Id,
            chainOwner.Email,
            chainOwner.FullName,
            chainOwner.BusinessName,
            chainOwner.Status.ToString(),
            chainOwner.RegionCode,
            token));
    }

    /// <summary>
    /// Store Manager login.
    /// </summary>
    [HttpPost("login/store-manager")]
    public async Task<ActionResult<ManagerLoginResponseDto>> LoginManager([FromBody] ManagerLoginDto dto)
    {
        var manager = await _accountService.AuthenticateManagerAsync(dto.Email, dto.Password);
        if (manager == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var token = GenerateManagerJwtToken(manager);

        return Ok(new ManagerLoginResponseDto(
            manager.Id,
            manager.Email,
            manager.FullName,
            manager.StoreAccountId,
            manager.Store.StoreName,
            manager.Status.ToString(),
            token));
    }

    private string GenerateJwtToken(ChainOwnerAccount chainOwner)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, chainOwner.Id.ToString()),
            new(ClaimTypes.Email, chainOwner.Email),
            new(ClaimTypes.Name, chainOwner.FullName),
            new(ClaimTypes.Role, "ChainOwner"),
            new("AccountType", "ChainOwner"),
            new("RegionCode", chainOwner.RegionCode),
            new("Status", chainOwner.Status.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateManagerJwtToken(StoreManager manager)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, manager.Id.ToString()),
            new(ClaimTypes.Email, manager.Email),
            new(ClaimTypes.Name, manager.FullName),
            new(ClaimTypes.Role, "StoreManager"),
            new("AccountType", "StoreManager"),
            new("StoreAccountId", manager.StoreAccountId.ToString()),
            new("Status", manager.Status.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static ChainOwnerProfileDto MapToProfileDto(ChainOwnerAccount chainOwner)
    {
        return new ChainOwnerProfileDto(
            chainOwner.Id,
            chainOwner.Email,
            chainOwner.FullName,
            chainOwner.PhoneNumber,
            chainOwner.BusinessName,
            chainOwner.BusinessRegistrationNumber,
            chainOwner.RegionCode,
            chainOwner.Status.ToString(),
            chainOwner.CreatedAt,
            chainOwner.Stores?.Select(s => new StoreDto(
                s.Id,
                s.StoreName,
                s.Description,
                s.Address,
                s.Latitude,
                s.Longitude,
                s.PhoneNumber,
                s.RegionCode,
                s.Status.ToString(),
                s.IsOpen,
                s.CreatedAt,
                s.Managers?.Count ?? 0,
                s.Foods?.Count ?? 0)).ToList() ?? new List<StoreDto>());
    }
}
