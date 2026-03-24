using OpenMcdf;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using WARBIMPRO.ShellExtension.Models;

namespace WARBIMPRO.ShellExtension.Services
{
    /// <summary>
    /// Lee el stream BasicFileInfo de un archivo .rvt SIN abrir Revit.
    ///
    /// FORMATO DEL STREAM (cambió entre versiones):
    ///   Revit 2018 y anteriores → "Revit Build: Autodesk Revit 2018 (Build: 20170106_1515(x64))"
    ///   Revit 2019 en adelante  → "Format: 2019"  (más simple y confiable)
    ///
    /// El stream está codificado en UTF-16 LE (Unicode) con bytes nulos entre caracteres.
    /// NO usar Regex para limpiar el raw antes de parsear — destruye el contenido.
    /// </summary>
    public static class RevitFileReader
    {
        public static RevitFileInfo Read(string filePath)
        {
            var info = new RevitFileInfo();

            try
            {
                var fileInfo = new FileInfo(filePath);
                info.FileName = fileInfo.Name;
                info.FilePath = filePath;
                info.LastModified = fileInfo.LastWriteTime.ToString("dd/MM/yyyy HH:mm");
                info.FileSize = FormatFileSize(fileInfo.Length);

                using (var cf = new CompoundFile(filePath))
                {
                    CFStream stream = cf.RootStorage.GetStream("BasicFileInfo");
                    byte[] data = stream.GetData();

                    // ---------------------------------------------------------
                    // El stream BasicFileInfo tiene dos partes:
                    //   1) Cabecera binaria (primeros bytes, longitud variable)
                    //   2) Texto en UTF-16 LE con los metadatos
                    //
                    // Estrategia: decodificar directamente como UTF-16.
                    // Los bytes nulos entre caracteres ASCII son normales en UTF-16.
                    // ---------------------------------------------------------
                    string raw = Encoding.Unicode.GetString(data);

                    // Limpiar SOLO los caracteres de control que rompen el split,
                    // pero CONSERVAR letras, numeros, signos de puntuacion y espacios.
                    // NO usar Regex que elimine rangos amplios: destruiria el contenido.
                    string cleaned = CleanRawString(raw);

                    ParseBasicFileInfo(cleaned, info);
                }
            }
            catch (Exception ex)
            {
                info.ErrorMessage = $"Error al leer el archivo: {ex.Message}";
            }

            return info;
        }

        /// <summary>
        /// Limpia el string raw manteniendo caracteres legibles.
        /// Solo elimina caracteres de control (excepto \r y \n que usamos para split).
        /// Los bytes nulos (\0) los convierte en saltos de linea porque en UTF-16
        /// suelen ser el separador entre campos del stream BasicFileInfo.
        /// </summary>
        private static string CleanRawString(string raw)
        {
            var sb = new StringBuilder(raw.Length);
            foreach (char c in raw)
            {
                if (c == '\r' || c == '\n' || c == '\t' || (c >= ' ' && c != '\x7F'))
                    sb.Append(c);
                else if (c == '\0')
                    sb.Append('\n'); // nulos -> newline para facilitar split
            }
            return sb.ToString();
        }

        private static void ParseBasicFileInfo(string cleaned, RevitFileInfo info)
        {
            string[] lines = cleaned.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // Buscar separador ": " (con espacio) primero, luego ":" solo
                int colonIndex = line.IndexOf(": ");
                int valueOffset = 2;

                if (colonIndex < 0)
                {
                    colonIndex = line.IndexOf(":");
                    valueOffset = 1;
                }

                if (colonIndex < 0) continue;

                string key = line.Substring(0, colonIndex).Trim();
                string value = line.Substring(colonIndex + valueOffset).Trim();

                if (string.IsNullOrEmpty(key)) continue;

                switch (key)
                {
                    // == VERSION (Revit 2019+) =================================
                    // Formato exacto: "Format: 2019"
                    // El valor es directamente el año como numero entero de 4 digitos
                    case "Format":
                        if (!string.IsNullOrEmpty(value) && Regex.IsMatch(value.Trim(), @"^\d{4}$"))
                        {
                            info.RevitVersion = $"Autodesk Revit {value.Trim()}";
                            info.BuildNumber = value.Trim();
                        }
                        break;

                    // == VERSION (Revit 2018 y anteriores) ====================
                    // Formato: "Revit Build: Autodesk Revit 2018 (Build: 20170106_1515(x64))"
                    case "Revit Build":
                    case "Autodesk Revit Build":
                        if (string.IsNullOrEmpty(info.RevitVersion))
                        {
                            info.BuildNumber = value;
                            info.RevitVersion = ExtractVersionFromBuildString(value);
                        }
                        break;

                    // == METADATA =============================================
                    case "Author":
                        info.Author = string.IsNullOrWhiteSpace(value) ? "—" : value;
                        break;

                    case "Organization Name":
                        if (string.IsNullOrWhiteSpace(info.Organization) || info.Organization == "—")
                            info.Organization = string.IsNullOrWhiteSpace(value) ? "—" : value;
                        break;

                    case "Organization Description":
                        if (string.IsNullOrWhiteSpace(info.Organization) || info.Organization == "—")
                            info.Organization = string.IsNullOrWhiteSpace(value) ? "—" : value;
                        break;

                    case "Unique Document GUID":
                    case "Document GUID":
                        info.ProjectGuid = string.IsNullOrWhiteSpace(value) ? "—" : value;
                        break;

                    case "Worksharing":
                        info.IsWorkshared = !value.Equals("Not enabled", StringComparison.OrdinalIgnoreCase);
                        break;

                    case "Central Model Path":
                        info.IsCentral = !string.IsNullOrWhiteSpace(value);
                        break;
                }
            }

            // == FALLBACK: buscar "20XX" en cualquier linea si no se encontro version ==
            if (string.IsNullOrEmpty(info.RevitVersion) || info.RevitVersion == "—")
            {
                foreach (string rawLine in cleaned.Split(new[] { '\r', '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string version = ExtractVersionFromBuildString(rawLine);
                    if (!string.IsNullOrEmpty(version))
                    {
                        info.RevitVersion = version;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(info.RevitVersion))
                info.RevitVersion = "No detectada";
        }

        /// <summary>
        /// Extrae el año de un build string.
        /// "Autodesk Revit 2024 (Build: 24.0.0.0)" → "Autodesk Revit 2024"
        /// </summary>
        private static string ExtractVersionFromBuildString(string buildString)
        {
            if (string.IsNullOrEmpty(buildString)) return null;
            var match = Regex.Match(buildString, @"\b(20[1-3]\d)\b");
            if (match.Success)
                return $"Autodesk Revit {match.Value}";
            return null;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1_073_741_824) return $"{bytes / 1_073_741_824.0:F2} GB";
            if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:F2} MB";
            if (bytes >= 1_024) return $"{bytes / 1_024.0:F2} KB";
            return $"{bytes} bytes";
        }
    }
}