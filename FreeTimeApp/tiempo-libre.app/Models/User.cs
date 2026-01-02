using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FullName { get; set; }

    [Required]
    [Username(ErrorMessage = "El nombre de usuario debe ser un entero o un correo electrónico válido.")]
    [MaxLength(50)]
    public required string Username { get; set; }

    [Required]
    [MaxLength(100)]
    public required string PasswordHash { get; set; }

    [Required]
    public required string PasswordSalt { get; set; }

    [Required]
    public required ICollection<Rol> Roles { get; set; }

    [Required]
    [EnumDataType(typeof(UserStatus), ErrorMessage = "El estado debe ser Activo, Desactivado o Suspendido.")]
    public UserStatus Status { get; set; } = UserStatus.Activo;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public DateTime? UltimoInicioSesion { get; set; }

    public int? AreaId { get; set; }
    public int? GrupoId { get; set; }
    //public bool IsValidated { get; set; } = false;


    #region Propiedades exclusivas de los Empleados Sindicalizados

    public int? Nomina { get; set; }
    public string? Maquina { get; set; }
    public DateOnly? FechaIngreso { get; set; }
    public int? CentroCoste { get; set; }
    public int? VacacionesPorAntiguedadId { get; set; }
    public int? DiasDeVacacionesAsignados { get; set; }
    public int? AntiguedadEnAnios { get; set; }
    public int? AntiguedadEnDias { get; set; }
    public virtual VacacionesPorAntiguedad? VacacionesPorAntiguedad { get; set; }
    [MaxLength(100)]
    public string? Posicion { get; set; }

    #endregion

    // Navigation properties
    public virtual Grupo? Grupo { get; set; }
    public virtual Area? Area { get; set; }
    public virtual ICollection<AreaIngeniero> AreaIngenieros { get; set; }
    public virtual ICollection<Area> AreasAsIngeniero { get; set; }

    public User() 
    {
        AreaIngenieros = new HashSet<AreaIngeniero>();
    }

    public User(string fullName, string username, string passwordHash, string passwordSalt, List<Rol> roles)
    {
        FullName = fullName;
        Username = username;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        Roles = roles;
        AreaIngenieros = new HashSet<AreaIngeniero>();
        AreasAsIngeniero = new HashSet<Area>();
    }
}