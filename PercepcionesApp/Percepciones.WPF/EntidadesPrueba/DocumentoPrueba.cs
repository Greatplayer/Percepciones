using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Percepciones.WPF.EntidadesPrueba
{
    public class DocumentoPrueba
    {
        public int id { get; set; }
        public string deliver { get; set; }
        public string outlet { get; set; }
        public string document_type { get; set; }
        public string document_number { get; set; }
        public string fecha { get; set; }
        public decimal? total_amount { get; set; }
        public decimal? igv { get; set; }
        public decimal? percepcion { get; set; }
        public string razon_social { get; set; }
        public string mes { get; set; }
    }
}
