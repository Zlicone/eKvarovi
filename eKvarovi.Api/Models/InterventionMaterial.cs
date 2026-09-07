namespace eKvarovi.Api.Models;

public class InterventionMaterial
{
    public int Id { get; set; }

    public int InterventionId { get; set; }
    public Intervention? Intervention { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public decimal Quantity { get; set; }
}