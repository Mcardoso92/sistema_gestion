namespace saas.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int? EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }
    }
}
