using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Models
{
    public class Data
    {
        private string _ID;
        private string _NIVEL;
        private string _CATEGORIA;
        private string _AREA_m2;
        private string _VOLUMEN_m3;

        public string ID { get => _ID; set => _ID = value; }
        public string NIVEL { get => _NIVEL; set => _NIVEL = value; }
        public string CATEGORIA { get => _CATEGORIA; set => _CATEGORIA = value; }
        public string AREA { get => _AREA_m2; set => _AREA_m2 = value; }
        public string VOLUMEN { get => _VOLUMEN_m3; set => _VOLUMEN_m3 = value; }
        public double Cemento { get; set; }
        public double Arena { get; set; }
        public double Grava { get; set; }
        public double Agua { get; set; }
    }
}

