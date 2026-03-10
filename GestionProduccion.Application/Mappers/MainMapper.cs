using GestionProduccion.Domain.Entities;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Application.Mapping;

namespace GestionProduccion.Application.Mappers;

/// <summary>
/// Injectable wrapper for ManualMapper extension methods.
/// </summary>
public class MainMapper
{
    public ProductDto ToDto(Product entity) => entity.ToDto();
    public List<ProductDto> ToDtoList(IEnumerable<Product> entities) => entities.ToDtoList();
    public UserDto ToDto(User entity) => entity.ToDto();
    public List<UserDto> ToDtoList(IEnumerable<User> entities) => entities.ToDtoList();
    public ProductionOrderDto ToDto(ProductionOrder entity) => entity.ToDto();
    public List<ProductionOrderDto> ToDtoList(IEnumerable<ProductionOrder> entities) => entities.ToDtoList();
    public TaskDto ToDto(OperationalTask entity) => entity.ToDto();
    public List<TaskDto> ToDtoList(IEnumerable<OperationalTask> entities) => entities.ToDtoList();
    public SewingTeamDto ToDto(SewingTeam entity) => entity.ToDto();
    public List<SewingTeamDto> ToDtoList(IEnumerable<SewingTeam> entities) => entities.ToDtoList();
    public QADefectDto ToDto(QADefect entity) => entity.ToDto();
    public List<QADefectDto> ToDtoList(IEnumerable<QADefect> entities) => entities.ToDtoList();
}
