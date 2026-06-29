using Entidades.Models;
using Logica.DTO.Empresa.Request;
using Logica.DTO.Empresa.Response;
using Logica.Interfaces;
using Repositorios.Interfaces;


namespace Logica.Services;

public class EmpresaService : IEmpresaService
{
    private readonly IEmpresaRepository _repository;

    public EmpresaService(IEmpresaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<EmpresaResponse>> GetAllAsync()
    {
        var empresas = await _repository.GetAllAsync();

        return empresas.Select(e => new EmpresaResponse
        {
            Id = e.Id,
            NombreComercial = e.NombreComercial,
            LogoUrl = e.LogoUrl,
            ColorPrimario = e.ColorPrimario,
            ColorSecundario = e.ColorSecundario,
            Tipografia = e.Tipografia,
            Cuit = e.Cuit,
            RazonSocial = e.RazonSocial,
            CondicionIVA = e.CondicionIVA,
            Direccion = e.Direccion,
            Activa = e.Activa
        }).ToList();
    }

    public async Task<EmpresaResponse?> GetByIdAsync(int id)
    {
        var empresa = await _repository.GetByIdAsync(id);

        if (empresa == null)
            return null;

        return new EmpresaResponse
        {
            Id = empresa.Id,
            NombreComercial = empresa.NombreComercial,
            LogoUrl = empresa.LogoUrl,
            ColorPrimario = empresa.ColorPrimario,
            ColorSecundario = empresa.ColorSecundario,
            Tipografia = empresa.Tipografia,
            Cuit = empresa.Cuit,
            RazonSocial = empresa.RazonSocial,
            CondicionIVA = empresa.CondicionIVA,
            Direccion = empresa.Direccion,
            Activa = empresa.Activa
        };
    }

    public async Task<EmpresaResponse> GetByCuitAsync(string cuit)
    {
        var empresa = await _repository.GetByCuitAsync(cuit);

        if (empresa == null)
            throw new Exception("No existe una empresa con ese CUIT.");

        return new EmpresaResponse
        {
            Id = empresa.Id,
            NombreComercial = empresa.NombreComercial,
            LogoUrl = empresa.LogoUrl,
            ColorPrimario = empresa.ColorPrimario,
            ColorSecundario = empresa.ColorSecundario,
            Tipografia = empresa.Tipografia,
            Cuit = empresa.Cuit,
            RazonSocial = empresa.RazonSocial,
            CondicionIVA = empresa.CondicionIVA,
            Direccion = empresa.Direccion,
            Activa = empresa.Activa
        };
    }

    public async Task<EmpresaResponse> CrearAsync(CrearEmpresaRequest request)
    {
        var existe = await _repository.GetByCuitAsync(request.Cuit);

        if (existe != null)
            throw new Exception("Ya existe una empresa con ese CUIT.");

        var empresa = new Empresa
        {
            NombreComercial = request.NombreComercial,
            LogoUrl = request.LogoUrl,
            ColorPrimario = request.ColorPrimario,
            ColorSecundario = request.ColorSecundario,
            Tipografia = request.Tipografia,
            Cuit = request.Cuit,
            RazonSocial = request.RazonSocial,
            CondicionIVA = request.CondicionIVA,
            Direccion = request.Direccion,
            Activa = true
        };

        await _repository.AddAsync(empresa);
        await _repository.SaveChangesAsync();

        return new EmpresaResponse
        {
            Id = empresa.Id,
            NombreComercial = empresa.NombreComercial,
            LogoUrl = empresa.LogoUrl,
            ColorPrimario = empresa.ColorPrimario,
            ColorSecundario = empresa.ColorSecundario,
            Tipografia = empresa.Tipografia,
            Cuit = empresa.Cuit,
            RazonSocial = empresa.RazonSocial,
            CondicionIVA = empresa.CondicionIVA,
            Direccion = empresa.Direccion,
            Activa = empresa.Activa
        };
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarEmpresaRequest request)
    {
        var empresa = await _repository.GetByIdAsync(id);

        if (empresa == null)
            return false;

        empresa.NombreComercial = request.NombreComercial;
        empresa.LogoUrl = request.LogoUrl;
        empresa.ColorPrimario = request.ColorPrimario;
        empresa.ColorSecundario = request.ColorSecundario;
        empresa.Tipografia = request.Tipografia;
        empresa.Cuit = request.Cuit;
        empresa.RazonSocial = request.RazonSocial;
        empresa.CondicionIVA = request.CondicionIVA;
        empresa.Direccion = request.Direccion;
        empresa.Activa = request.Activa;

        await _repository.UpdateAsync(empresa);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var empresa = await _repository.GetByIdAsync(id);

        if (empresa == null)
            return false;

        await _repository.DeleteAsync(empresa);
        await _repository.SaveChangesAsync();

        return true;
    }
}