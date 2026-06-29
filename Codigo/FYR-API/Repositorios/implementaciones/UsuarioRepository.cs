using Datos;
using DTO.Usuario.Request;
using DTO.Usuario.Response;
using Entidades.Models;
using Microsoft.EntityFrameworkCore;
using Repositorios.Interfaces;

namespace Repositorios.Implementaciones;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioResponse>> GetAllAsync()
    {
        return await _context.Usuarios
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Rol = u.Rol,
                Telefono = u.Telefono,
                IdiomaPreferido = u.IdiomaPreferido,
                FotoPerfil = u.FotoPerfil,
                Activo = u.Activo,
                FechaRegistro = u.FechaRegistro,
                EmpresaId = u.EmpresaId
            })
            .ToListAsync();
    }

    public async Task<UsuarioResponse?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new UsuarioResponse
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                Rol = u.Rol,
                Telefono = u.Telefono,
                IdiomaPreferido = u.IdiomaPreferido,
                FotoPerfil = u.FotoPerfil,
                Activo = u.Activo,
                FechaRegistro = u.FechaRegistro,
                EmpresaId = u.EmpresaId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request)
    {
        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,

            // Después lo reemplazaremos por BCrypt
            PasswordHash = request.Password,

            Rol = request.Rol,
            Telefono = request.Telefono,
            IdiomaPreferido = request.IdiomaPreferido,
            FotoPerfil = request.FotoPerfil,
            EmpresaId = request.EmpresaId,
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Telefono = usuario.Telefono,
            IdiomaPreferido = usuario.IdiomaPreferido,
            FotoPerfil = usuario.FotoPerfil,
            Activo = usuario.Activo,
            FechaRegistro = usuario.FechaRegistro,
            EmpresaId = usuario.EmpresaId
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return false;

        usuario.Nombre = request.Nombre;
        usuario.Apellido = request.Apellido;
        usuario.Email = request.Email;
        usuario.Rol = request.Rol;
        usuario.Telefono = request.Telefono;
        usuario.IdiomaPreferido = request.IdiomaPreferido;
        usuario.FotoPerfil = request.FotoPerfil;
        usuario.Activo = request.Activo;
        usuario.EmpresaId = request.EmpresaId;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
            return false;

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return true;
    }
}