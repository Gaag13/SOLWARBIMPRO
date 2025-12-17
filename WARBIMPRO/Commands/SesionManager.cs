using System.IO;
using Newtonsoft.Json;

namespace WARBIMPRO.Commands
{
    public class SesionManager
    {
        public void GuardarEstadoDeSesion(string usuarioId)
        {
            var estadoDeSesion = new
            {
                UsuarioId = usuarioId,
                IsloggedIn = true,
            };
            string json = JsonConvert.SerializeObject(estadoDeSesion);
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WARBIMPRO");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(Path.Combine(path,"estadoSesion.json"),json);
        }

        public bool EstaLogueado()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WARBIMPRO", "estadoSesion.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var estadoDeSesion = JsonConvert.DeserializeObject<dynamic>(json);

                return estadoDeSesion.IsloggedIn;
            }

            return false;
        }
        public string ObtenerUsuarioActual()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WARBIMPRO", "estadoSesion.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var estadoDeSesion = JsonConvert.DeserializeObject<dynamic>(json);

                return estadoDeSesion.UsuarioId;
            }

            return null;
        }


    }
}
