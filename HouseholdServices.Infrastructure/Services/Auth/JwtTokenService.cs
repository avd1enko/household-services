using HouseholdServices.Domain.Entities;
using HouseholdServices.Application.Services.Auth;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HouseholdServices.Infrastructure.Services.Auth;

public class JwtTokenService : IJwtTokenService {
    private readonly IConfiguration _configuration; //IConfiguration — это объект, через который можно читать настройки из appsettings and env
    
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(User user, string role)
    {
        string key = _configuration["Jwt:Key"]!;
        string issuer = _configuration["Jwt:Issuer"]!;
        string audience = _configuration["Jwt:Audience"]!;
        int expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]!);

        // Claims нужны, чтобы собрать информацию о пользователе, которую мы хотим включить внутрь JWT
        // мы не можем использовать просто значения, потому что JWT библиотека ожидает именно клеймы с указанными типами (например Name → firstlogin)
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // используем стандартные названия клеймов
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, role)
        };
        
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)); // берем ключ из аппсеттингс и превращаем в байты для подписи. создаем объект ключа для подписи
        
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); // указываем алгоритм подписи и сам ключ, которым подписываем

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims, // данные пользователя, собранные при помощи claim
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token); // превращает наш объект токена в готовую строку


    }
    
}