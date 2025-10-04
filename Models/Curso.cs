using System.ComponentModel.DataAnnotations;


namespace PortalAcademico.Models;


public class Curso
{
public int Id { get; set; }


[Required]
[MaxLength(20)]
public string Codigo { get; set; } = null!; // unico


[Required]
public string Nombre { get; set; } = null!;


[Range(1, int.MaxValue, ErrorMessage = "Creditos debe ser mayor que 0")]
public int Creditos { get; set; }


public int CupoMaximo { get; set; }


[Required]
public TimeSpan HorarioInicio { get; set; }


[Required]
public TimeSpan HorarioFin { get; set; }


public bool Activo { get; set; } = true;


public List<Matricula> Matriculas { get; set; } = new();
}