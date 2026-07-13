using System.ComponentModel.DataAnnotations;

namespace Indotel.Core.DTOs;

public class RegisterCiudadanoDto
{
    private string _cedula = string.Empty;
    private string _nombres = string.Empty;
    private string _apellidos = string.Empty;
    private string _telefono = string.Empty;
    private string _correo = string.Empty;
    private string _direccion = string.Empty;
    private string _password = string.Empty;

    [Required]
    [StringLength(20)]
    public string Cedula
    {
        get => _cedula;
        set => _cedula = value ?? string.Empty;
    }

    [Required]
    [StringLength(100)]
    public string Nombres
    {
        get => _nombres;
        set => _nombres = value ?? string.Empty;
    }

    [Required]
    [StringLength(100)]
    public string Apellidos
    {
        get => _apellidos;
        set => _apellidos = value ?? string.Empty;
    }

    [StringLength(30)]
    public string Telefono
    {
        get => _telefono;
        set => _telefono = value ?? string.Empty;
    }

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Correo
    {
        get => _correo;
        set => _correo = value ?? string.Empty;
    }

    [StringLength(300)]
    public string Direccion
    {
        get => _direccion;
        set => _direccion = value ?? string.Empty;
    }

    [Required]
    [StringLength(200)]
    public string Password
    {
        get => _password;
        set => _password = value ?? string.Empty;
    }
}
