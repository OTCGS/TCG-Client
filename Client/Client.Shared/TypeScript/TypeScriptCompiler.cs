using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native.Object;
using Jint.Native;

namespace Client.TypeScript
{
    static class TypeScriptCompiler
    {

        private static class EngineGenerator
        {
            #region ScriptData



            #endregion

            private static TaskCompletionSource<Jint.Engine> engineWaiter;

            public static Task<Jint.Engine> Compiler
            {
                get
                {
                    return GetCompiler();
                }
            }

            private static async Task<Engine> GetCompiler()
            {
                if (engineWaiter != null)
                    return await engineWaiter.Task;

                try
                {
                    Logger.Information("Generating Compiler");
                    engineWaiter = new TaskCompletionSource<Jint.Engine>();
                    var compilerFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"TypeScript\typescript.js");
                    var dtsFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"TypeScript\lib.d.ts");
                    var functionFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"TypeScript\Compile.js");
                    var compilerScript = await Windows.Storage.FileIO.ReadTextAsync(compilerFile);
                    var dtsScript = await Windows.Storage.FileIO.ReadTextAsync(dtsFile);
                    var functionScript = await Windows.Storage.FileIO.ReadTextAsync(functionFile);
                    var engine = new Jint.Engine();
                    Logger.Information("Data for Compiler loaded.");
                    engine.Execute(compilerScript);
                    engine.SetValue("libsource", dtsScript);
                    engine.Execute(functionScript);
                    Logger.Information("Compiler Ready.");
                    engineWaiter.SetResult(engine);
                }
                catch (Exception e)
                {
                    engineWaiter.SetException(e);
                }
                return await engineWaiter.Task;
            }
        }

        public static async Task<String> CompileTypeScript(string source)
        {
            using (await semaphor.Enter())
            {
                Logger.Information("Compile Typescript");

                var engine = await EngineGenerator.Compiler;
                var jsValue = await Task.Run(() => engine.Invoke("Compile", source));
                dynamic erg = await Task.Run(() => ToObject(jsValue));
                string output = erg.output;
                dynamic[] diagnostics = erg.diagnostics;
                var bla = await Task.Run(() => diagnostics.Select(x => new Diagnostic(x)).ToArray());

                Logger.Information("Finished compileing Typescript");
                return output;
            }

        }

        private static readonly System.Threading.DisposingUsingSemaphore semaphor = new System.Threading.DisposingUsingSemaphore();


        private static readonly Dictionary<ObjectInstance, System.Dynamic.ExpandoObject> objectCach = new Dictionary<ObjectInstance, System.Dynamic.ExpandoObject>();
        private static dynamic ToObject(JsValue? value2)
        {
            var erg = ToObjectIntern(value2);
            objectCach.Clear();
            return erg;
        }
        private static dynamic ToObjectIntern(JsValue? value2)
        {
            if (!value2.HasValue)
                return null;

            var value = value2.Value;

            if (value.IsArray())
            {
                var array = value.AsArray();
                var length = (int)array.Properties["length"].Value.Value.AsNumber();
                var objArray = new Object[length];
                for (int i = 0; i < length; i++)
                    if (array.Properties.ContainsKey(i.ToString()))
                        objArray[i] = ToObjectIntern(array.Properties[i.ToString()].Value);

                return objArray;
            }

            if (value.IsObject())
            {
                var obj = value.AsObject();

                if (objectCach.ContainsKey(obj))
                    return objectCach[obj];

                dynamic erg = new System.Dynamic.ExpandoObject();
                objectCach.Add(obj, erg);

                foreach (var p in obj.Properties)
                {
                    ((IDictionary<String, Object>)erg).Add(p.Key, ToObjectIntern(p.Value.Value));

                }
                return erg;

            }
            if (value == Undefined.Instance)
            {
                return null;
            }

            if (value == Null.Instance)
            {
                return null;
            }

            if (value.IsBoolean())
            {
                return value.AsBoolean();
            }

            if (value.IsNumber())
            {
                return value.AsNumber();
            }

            if (value.IsString())
            {
                return value.AsString();
            }
            throw new NotImplementedException();
        }

        public class Diagnostic
        {

            public Diagnostic(dynamic jsObj)
            {
                this.Start = (int)jsObj.start;
                this.Length = (int)jsObj.length;
                this.Code = (int)jsObj.code;
                this.Category = (DiagnosticCategory)jsObj.category;
                if (jsObj.messageText is String)
                    Message = System.Linq.Enumerable.Repeat(new DiagnosticMessage(jsObj.messageText as String), 1);
                else
                {
                    TranslateMessageChain(jsObj.messageText);
                }


            }

            private IEnumerable<DiagnosticMessage> TranslateMessageChain(dynamic messageChain)
            {
                dynamic current = messageChain;
                while (current != null)
                {
                    yield return new DiagnosticMessage(current);

                    current = current.next;
                }
                throw new NotImplementedException();
            }

            public int Start { get; }
            public int Length { get; }
            public int Code { get; }
            public DiagnosticCategory Category { get; }

            public IEnumerable<DiagnosticMessage> Message { get; }
        }

        public class DiagnosticMessage
        {

            public DiagnosticMessage(dynamic x)
            {
                this.Text = x.messageText;
                this.Code = (int)x.number;
                this.Category = (DiagnosticCategory)x.category;
            }

            public DiagnosticMessage(String message)
            {
                this.Text = message;
            }

            public string Text { get; }
            public int? Code { get; }
            public DiagnosticCategory? Category { get; }
        }

        public enum DiagnosticCategory : byte
        {
            Warning = 0,
            Error = 1,
            Message = 2,
        }

    }
}
