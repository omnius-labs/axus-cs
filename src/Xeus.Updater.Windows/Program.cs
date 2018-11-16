using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Amoeba.Update
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string sessionId = args[0];
                string sourceDirectoryPath = args[1];
                string destinationDirectoryPath = args[2];
                string runExeFilePath = args[3];

                var mutex = new Mutex(false, sessionId);
                if (!mutex.WaitOne(1000 * 30)) return;

                {
                    string tempDirectoryPath = GetUniqueDirectoryPath(destinationDirectoryPath);

                    for (int i = 0; i < 128; i++)
                    {
                        try
                        {
                            CopyDirectory(destinationDirectoryPath, tempDirectoryPath);
                            DeleteDirectory(destinationDirectoryPath);

                            break;
                        }
                        catch (Exception)
                        {

                        }

                        Thread.Sleep(1000);
                    }

                    for (int i = 0; i < 128; i++)
                    {
                        try
                        {
                            CopyDirectory(sourceDirectoryPath, destinationDirectoryPath);
                            DeleteDirectory(sourceDirectoryPath);

                            break;
                        }
                        catch (Exception)
                        {

                        }

                        Thread.Sleep(1000);
                    }

                    for (int i = 0; i < 128; i++)
                    {
                        try
                        {
                            DeleteDirectory(tempDirectoryPath);

                            break;
                        }
                        catch (Exception)
                        {

                        }

                        Thread.Sleep(1000);
                    }
                }

                {
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = Path.GetFullPath(runExeFilePath);
                    startInfo.WorkingDirectory = Path.GetFullPath(Path.GetDirectoryName(runExeFilePath));

                    Process.Start(startInfo);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Amoeba.Update Error", MessageBoxButtons.OK);
            }
        }

        public static void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath)
        {
            if (!Directory.Exists(destDirectoryPath))
            {
                Directory.CreateDirectory(destDirectoryPath);
                File.SetAttributes(destDirectoryPath, File.GetAttributes(sourceDirectoryPath));
            }

            foreach (string file in Directory.GetFiles(sourceDirectoryPath))
            {
                File.Copy(file, Path.Combine(destDirectoryPath, Path.GetFileName(file)), true);
            }

            foreach (string dir in Directory.GetDirectories(sourceDirectoryPath))
            {
                CopyDirectory(dir, Path.Combine(destDirectoryPath, Path.GetFileName(dir)));
            }
        }

        public static void DeleteDirectory(string target_dir)
        {
            foreach (string file in Directory.GetFiles(target_dir))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in Directory.GetDirectories(target_dir))
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        private static string GetUniqueDirectoryPath(string path)
        {
            if (!Directory.Exists(path))
            {
                return path;
            }

            for (int index = 1; ; index++)
            {
                string text = string.Format(
                    @"{0}\{1} ({2})",
                    Path.GetDirectoryName(path),
                    Path.GetFileName(path),
                    index);

                if (!Directory.Exists(text))
                {
                    return text;
                }
            }
        }
    }
}
