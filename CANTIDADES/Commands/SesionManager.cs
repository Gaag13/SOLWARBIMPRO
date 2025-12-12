using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WARBIMPRO.Models;
using FireSharp;
using FireSharp.Response;
using Newtonsoft.Json;
using Microsoft.CSharp;

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
