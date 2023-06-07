using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BANREGIO.Models
{
    public class Prestamo
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; } = "";
        public string RFC { get; set; } = "";
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public bool Aprobado { get; set; }

        //En caso de no ser aprobado aqui se mostrara por que
        public string Razon { get; set; } = "";

    }
}
