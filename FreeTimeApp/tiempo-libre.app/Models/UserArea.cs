using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class UserArea
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int AreaId { get; set; }
    public virtual User User { get; set; }
    public virtual Area Area { get; set; }
}
