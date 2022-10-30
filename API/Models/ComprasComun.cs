namespace API.Models
{
    /// <summary>
    /// Modelo utilizado para retornar todos los productos en comun
    /// que tenga un cliente dado con otro
    /// </summary>
    public class ComprasComun
    {
        public string? nombreCliente { get; set; }
        public string? apellidoCliente { get; set; }
        public List<string> listaProductos = new List<string>();
    }
}