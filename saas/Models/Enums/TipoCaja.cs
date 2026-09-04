namespace saas.Models.Enums
{
    public enum TipoCaja
    {
        Efectivo = 1,
        Banco = 2,
        [System.ComponentModel.DataAnnotations.Display(Name = "Billetera virtual")]
        BilleteraVirtual = 3,
        Otro = 4
    }
}
