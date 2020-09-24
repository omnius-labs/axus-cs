using System.ComponentModel.DataAnnotations;
using System.IO;
using Cocona;
using Omnius.Xeus.Deamon.Internal;
using Omnius.Xeus.Deamon.Models;

namespace Omnius.Xeus.Deamon
{
    class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        public void Init([Option('d')] string? directory = null)
        {
            directory ??= Path.Combine(Directory.GetCurrentDirectory(), ".config");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var config = new XeusConfig()
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
            };

            YamlHelper.WriteFile(Path.Combine(directory, "config.yaml"), config);
        }

        private class FilePathExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
            {
                if (value is string path && File.Exists(path))
                {
                    return ValidationResult.Success!;
                }
                return new ValidationResult($"The path '{value}' is not found.");
            }
        }
    }
}
