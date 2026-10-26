using DTO.IA;

namespace Logica.Interfaces;

public interface IGeminiCatalogoService
{
    Task<CatalogoIaResponse> BuscarAsync(CatalogoIaRequest request, CancellationToken cancellationToken);

    Task<OutfitIaResponse> RecomendarOutfitAsync(OutfitIaRequest request, CancellationToken cancellationToken);
}
