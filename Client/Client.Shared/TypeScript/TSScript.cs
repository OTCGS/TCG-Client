using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Client.TypeScript
{
    public class TSScript
    {
        private const string CHACHED_FOLDER = "cachedJS";
        private TaskCompletionSource<string> taskCompletionSource;

        public TSScript(string tsScript)
        {
            this.TS = tsScript;
        }

        public TSScript(string tsScript, string jsScript) : this(tsScript)
        {
            this.taskCompletionSource = new TaskCompletionSource<string>();
            this.taskCompletionSource.SetResult(jsScript);
        }

        public string TS { get; }
        public Task<string> JS
        {
            get
            {
                return GenerateScript();
            }
        }

        private async Task<string> GenerateScript()
        {
            if (this.taskCompletionSource == null)
            {
                Logger.Information("Before Compilation");

                this.taskCompletionSource = new TaskCompletionSource<string>();
                Logger.Information("Check for Chached File");
                var js = await LoadCachedScript();
                if (js == null)
                {
                    Logger.Information("Start Compiling");
                    js = await TypeScriptCompiler.CompileTypeScript(this.TS);
                    Logger.Information("Compiling Done");
                    await CachScript(js);
                }
                Logger.Information("Returning Result");
                taskCompletionSource.SetResult(js);
            }
            return await taskCompletionSource.Task;
        }

        private async Task CachScript(string js)
        {
            var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("cachedJS", Windows.Storage.CreationCollisionOption.OpenIfExists);

            string name = GenerteHashedName();

            var file = await folder.CreateFileAsync(name, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            using (var str = await file.OpenStreamForWriteAsync())
            {
                var writer = new System.IO.StreamWriter(str);
                await writer.WriteAsync(js);
                await writer.FlushAsync();
            }
        }

        private string GenerteHashedName()
        {
            var hash = Security.SecurityFactory.HashMd5(UTF8Encoding.UTF8.GetBytes(this.TS));
            var name = Convert.ToBase64String(hash);
            return name;
        }

        private async Task<string> LoadCachedScript()
        {
            try
            {
                var folder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync(CHACHED_FOLDER, Windows.Storage.CreationCollisionOption.OpenIfExists);
                var file = await folder.GetFileAsync(GenerteHashedName());
                using (var str = await file.OpenReadAsync())
                {
                    var reader = new System.IO.StreamReader(str.AsStream());
                    var erg = await reader.ReadToEndAsync();
                    return erg;
                }
            }
            catch (Exception e)
            {

                return null;
            }
        }
    }
}
