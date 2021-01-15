using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Serilog;
using Serilog.Core;
using System.Reflection;

namespace Cursed_Auto_Decryptor
{
    public class Context
    {
        /// <summary>
        /// Module Path
        /// </summary>
        public string ModulePath { set; get; }
        /// <summary>
        /// Serilog Logger For Logging xd
        /// </summary>
        public Logger Log { set; get; }
        /// <summary>
        /// Module To Load / Decrypt
        /// </summary>
        public ModuleDefMD Module { set; get; }
        /// <summary>
        /// Load Module in Reflection Way To Invoke
        /// </summary>
        public Assembly Ass { set; get; }
        /// <summary>
        /// Initialise The Context
        /// </summary>
        /// <param name="Arguments">Args Contains Module Location</param>
        public Context(string[] Arguments)
        {
            System.Console.Title = "Cursed-Auto-Constants-Decryptor";
            if (Arguments.Length == 1) { ModulePath = Arguments[0]; }
            if (Arguments.Length == 0) { System.Console.Write("[+] Path : "); ModulePath = System.Console.ReadLine(); }
            Module = ModuleDefMD.Load(ModulePath);
            Ass = Assembly.UnsafeLoadFrom(ModulePath);
            Log = new LoggerConfiguration()
                .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Grayscale)
                .CreateLogger();
        }
        /// <summary>
        /// For Saving Module After Decrypting
        /// </summary>
        public void Save()
        {
            if (Module.IsILOnly)
            {
                var Options = new ModuleWriterOptions(Module)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                var NewPath = Module.Kind.Equals(ModuleKind.Dll) ? ModulePath.Replace(".dll", "-Decrypted.dll") : ModulePath.Replace(".exe", "-Decrypted.exe");
                Module.Write(NewPath, Options);
                Log.Information($"Module Saved : {NewPath}");
                System.Console.ReadKey();
            }
            else
            {
                var Options = new NativeModuleWriterOptions(Module, false)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                var NewPath = Module.Kind.Equals(ModuleKind.Dll) ? ModulePath.Replace(".dll", "-Decrypted.dll") : ModulePath.Replace(".exe", "-Decrypted.exe");
                Module.NativeWrite(NewPath, Options);
                Log.Information($"Module Saved : {NewPath}");
                System.Console.ReadKey();
            }
        }
    }
}