namespace WARBIMPRO.Models
{
    using System.Collections.Generic;

    public class FamiliaJson
    {
        public string tipo_familia { get; set; } = string.Empty;
        public string nombre_familia { get; set; } = string.Empty;
        public string fabricante { get; set; } = string.Empty;
        public DimensionesGenerales dimensiones_generales { get; set; } = new();
        public List<Componente> componentes { get; set; } = new();
        public List<Parametro> parametros_tipo { get; set; } = new();
        public List<Parametro> parametros_instancia { get; set; } = new();
    }

    public class DimensionesGenerales
    {
        public double ancho_total { get; set; }
        public double alto_total { get; set; }
        public double profundidad_total { get; set; }
    }

    public class Componente
    {
        public string nombre { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public Punto3D origen { get; set; } = new();
        public Dim3D dimensiones { get; set; } = new();
        public string material_param { get; set; } = string.Empty;
        public bool visible { get; set; } = true;
    }

    public class Punto3D { public double x { get; set; } public double y { get; set; } public double z { get; set; } }
    public class Dim3D { public double ancho { get; set; } public double profundidad { get; set; } public double alto { get; set; } }

    public class Parametro
    {
        public string nombre { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string grupo { get; set; } = string.Empty;
        public object? valor_defecto { get; set; }
    }
}
