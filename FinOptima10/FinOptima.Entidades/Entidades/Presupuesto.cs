namespace FinOptima.Entidades.Entidades
{
    public class Presupuesto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Categoria { get; set; }
        public decimal Limite { get; set; }

        public Presupuesto()
        {
            Id = 0;
            UsuarioId = 0;
            Categoria = "";
            Limite = 0;
        }
    }
}
