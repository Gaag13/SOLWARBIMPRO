using System;

namespace WARBIMPRO.ShellExtension.Models
{
    /// <summary>
    /// Modelo que representa la información extraída de un archivo .rvt
    /// </summary>
    public class RevitFileInfo
    {
        public string FileName { get; set; } = "—";
        public string FilePath { get; set; } = "—";
        public string RevitVersion { get; set; } = "—";
        public string BuildNumber { get; set; } = "—";
        public string Author { get; set; } = "—";
        public string Organization { get; set; } = "—";
        public string ProjectGuid { get; set; } = "—";
        public string FileSize { get; set; } = "—";
        public string LastModified { get; set; } = "—";
        public bool IsCentral { get; set; } = false;
        public bool IsWorkshared { get; set; } = false;
        public string ErrorMessage { get; set; } = null;
    }
}
