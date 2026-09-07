namespace eKvarovi.Api.Models;

public class Material
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int UnitId { get; set; }
    public MaterialUnit? Unit { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<InterventionMaterial> InterventionMaterials { get; set; } = new List<InterventionMaterial>();
}