using System.ComponentModel.DataAnnotations;
using System.IO;
using Cocona;

namespace Omnius.Xeus.Deamon
{
    class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        public void Compile([Option('s')][FilePathExists] string source, [Option('o')] string output, [Option('i')] string[]? include = null)
        {

        }

        private class FilePathExistsAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is string path && File.Exists(path))
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult($"The path '{value}' is not found.");
            }
        }
    }
}
