namespace API.Models
{
    /// <summary>
    /// Modelo utilizado para recibir la compra de un producto
    /// junto a su cantidad, por parte de un cliente
    /// </summary>
    public class RegistroCompra
    {
        public int idProducto { get; set; }
        public int cantidad { get; set; }
    }
}
