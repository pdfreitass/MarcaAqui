using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarcaAqui.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace MarcaAqui.Api.Infrastructure;

public class JwtTokenService
{
    private readonly string _secret;
    private readonly int _expirationMinutes;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Chave 'Jwt:Secret' não configurada.");
        _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "480");
        _issuer = configuration["Jwt:Issuer"] ?? "MarcaAqui.Api";
        _audience = configuration["Jwt:Audience"] ?? "MarcaAqui.Web";
    }

    public string GerarToken(Usuario usuario, int? barbeariaId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("tipo", usuario.Tipo),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (barbeariaId.HasValue)
            claims.Add(new Claim("barbearia_id", barbeariaId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidarToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

        var parametros = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, parametros, out _);
        }
        catch
        {
            return null;
        }
    }
}
