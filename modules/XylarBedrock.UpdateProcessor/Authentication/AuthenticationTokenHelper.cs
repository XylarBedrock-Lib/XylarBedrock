using XylarBedrock.UpdateProcessor.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XylarBedrock.UpdateProcessor.Authentication
{
    public class AuthenticationTokenHelper
    {
        private const string DLLName = "XylarBedrock.TokenBroker.dll";
        private const string RuntimesDirName = "Runtimes";
        private static string GetEnv()
        {
            return Environment.Is64BitProcess ? "win-x64" : "win-x86";
        }


        static AuthenticationTokenHelper() { Init(); }
        private static void Init()
        {
            string dllImport = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RuntimesDirName, GetEnv(), DLLName);
            if (!File.Exists(dllImport)) dllImport = ExtractEmbeddedBroker() ?? dllImport;
            InteropExtensions.LoadLibrary(dllImport);
        }

        // Single-file installs carry the broker only as an embedded resource: unpack it into the launcher's data folder.
        private static string ExtractEmbeddedBroker()
        {
            Assembly assembly = typeof(AuthenticationTokenHelper).Assembly;
            string env = GetEnv();
            string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(name =>
                name.EndsWith(DLLName, StringComparison.OrdinalIgnoreCase) &&
                (name.Contains(env) || name.Contains(env.Replace('-', '_'))));
            if (resourceName == null) return null;

            string exeDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
            string targetPath = Path.Combine(exeDirectory, "data", "native", env, DLLName);

            try
            {
                using Stream stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) return null;
                if (File.Exists(targetPath) && new FileInfo(targetPath).Length == stream.Length) return targetPath;

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                using FileStream file = File.Create(targetPath);
                stream.CopyTo(file);
                return targetPath;
            }
            catch (IOException)
            {
                // Another launcher instance may already have it loaded.
                return File.Exists(targetPath) ? targetPath : null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }


        [DllImport(DLLName, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetWUToken(int userIndex, [MarshalAs(UnmanagedType.LPWStr)] out string token);

        [DllImport(DLLName, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetTotalWUAccounts();

        [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string GetWUAccountUserName(int userIndex);

        [DllImport(DLLName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string GetWUProviderName(int userIndex);
    }
}

