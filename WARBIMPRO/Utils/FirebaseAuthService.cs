using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace WARBIMPRO.Utils
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _client = new HttpClient();

        private readonly string registerUrl = "https://us-central1-warbimpro.cloudfunctions.net/registrarUsuario";
        private readonly string loginUrl = "https://us-central1-warbimpro.cloudfunctions.net/iniciarSesion";

        public async Task<string> RegistrarUsuarioAsync(object usuario)
        {
            string json = JsonConvert.SerializeObject(usuario);
            var response = await _client.PostAsync(registerUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<DataFirebase> IniciarSesionUsuarioAsync(string email, string password)
        {
            var payload = new { Email = email, Password = password }; // ✅ usar la real
            string json = JsonConvert.SerializeObject(payload);

            var response = await _client.PostAsync(loginUrl, new StringContent(json, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            var parsed = JObject.Parse(content);

            var usuarioJson = parsed["usuario"]?.ToString();
            return usuarioJson != null ? JsonConvert.DeserializeObject<DataFirebase>(usuarioJson) : null;
        }


    }
}
