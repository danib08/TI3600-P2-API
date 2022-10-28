namespace API.Models
{
    /// <summary>
    /// Modelo para los nodos Productos
    /// </summary>
    public class Productos
    {
        public int id { get; set; }
        public string? nombre { get; set; }
        public string? marca { get; set; }
        public int precio { get; set; }
    }
}
