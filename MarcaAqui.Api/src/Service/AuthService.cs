using MarcaAqui.Api.DTOs;
using MarcaAqui.Api.Infrastructure;
using MarcaAqui.Api.Models;

namespace MarcaAqui.Api.Service;

public class AuthService
{
    private readonly UsuarioRepository _usuarioRepo;
    private readonly ClienteRepository _clienteRepo;
    private readonly ProfissionalRepository _profissionalRepo;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtService;

    public AuthService(
        UsuarioRepository usuarioRepo,
        ClienteRepository clienteRepo,
        ProfissionalRepository profissionalRepo,
        PasswordHasher passwordHasher,
        JwtTokenService jwtService)
    {
        _usuarioRepo = usuarioRepo;
        _clienteRepo = clienteRepo;
        _profissionalRepo = profissionalRepo;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<TokenDto> RegistrarAsync(RegistroDto dto)
    {
        // Verifica se o email já está em uso
        if (await _usuarioRepo.EmailExisteAsync(dto.Email))
            throw new InvalidOperationException("Email já está em uso.");

        // Cria o utilizador
        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email,
            SenhaHash = _passwordHasher.Hash(dto.Senha),
            Tipo = dto.Tipo,
            CriadoEm = DateTime.UtcNow
        };

        usuario = await _usuarioRepo.CriarAsync(usuario);

        // Cria perfil específico (cliente ou profissional) — profissional sem barbearia
        int? barbeariaId = null;

        if (dto.Tipo == "cliente")
        {
            await _clienteRepo.CriarAsync(new Cliente { UsuarioId = usuario.Id });
        }
        else if (dto.Tipo == "barbeiro")
        {
            // Profissional fica sem barbearia_id por enquanto (será definido na Spec 0020)
            await _profissionalRepo.CriarAsync(new Profissional { UsuarioId = usuario.Id, BarbeariaId = 0 });
        }

        // Gera token JWT
        var token = _jwtService.GerarToken(usuario, barbeariaId);

        return new TokenDto
        {
            Token = token,
            ExpiraEm = DateTime.UtcNow.AddMinutes(int.Parse(
                Environment.GetEnvironmentVariable("ASPNETCORE_Jwt__ExpirationMinutes") ?? "480")),
            Tipo = usuario.Tipo
        };
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto)
    {
        // Busca utilizador pelo email
        var usuario = await _usuarioRepo.ObterPorEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Email ou senha inválidos.");

        // Verifica senha
        if (!_passwordHasher.Verificar(dto.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        // Obtém barbearia_id para barbeiros
        int? barbeariaId = null;

        if (usuario.Tipo == "barbeiro")
        {
            var profissional = await _profissionalRepo.ObterPorUsuarioIdAsync(usuario.Id);
            if (profissional != null && profissional.BarbeariaId > 0)
                barbeariaId = profissional.BarbeariaId;
        }

        // Gera token JWT
        var token = _jwtService.GerarToken(usuario, barbeariaId);

        return new TokenDto
        {
            Token = token,
            ExpiraEm = DateTime.UtcNow.AddMinutes(int.Parse(
                Environment.GetEnvironmentVariable("ASPNETCORE_Jwt__ExpirationMinutes") ?? "480")),
            Tipo = usuario.Tipo
        };
    }
}
