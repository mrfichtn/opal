using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.IO;
using System.Runtime.InteropServices;
using VSLangProj80;

namespace Opal
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("Opal", "Generates Opal Parser", "1.0")]
    [Guid("D3DB95F2-A082-44E2-BEEC-0C0C69F4620B")]
    [ComVisible(true)]
    [ProvideObject(typeof(OpalSingleFileGenerator))]
    [CodeGeneratorRegistration(typeof(OpalSingleFileGenerator),
        "Opal", vsContextGuids.vsContextGuidVCSProject, GeneratesDesignTimeSource = true)]
    public class OpalSingleFileGenerator: BaseCodeGeneratorWithSite
    {
        public override string GetDefaultExtension() => ".cs";

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            using (var outStream = new MemoryStream())
            {
                var textStream = new StreamWriter(outStream);
                var args = new CompileArgs(inputFileName,
                    inputFileContent,
                    FileNamespace,
                    textStream,
                    GeneratorErrorCallback
                    );

                var isOk = Generate(args);

                textStream.Flush();
                return outStream.ToArray();
            }
        }

        private bool Generate(CompileArgs args)
        {
            if (string.IsNullOrEmpty(args.InputContents))
            {
                args.Error(0, 0, "Input file is empty");
                return false;
            }

            TryGetFramePath(args.InputFilePath, out var parserFrame);

            // Log.LogMessage(MessageImportance.Normal, "Opal: beginning compile {0}", DateTime.Now);
            var isOk = GenerateCode(args, parserFrame);

            return isOk;
        }


        private bool GenerateCode(CompileArgs args, string parserFrame)
        {
            try
            {
                var compiler = new Compiler2(args)
                {
                    ParserFrame = parserFrame
                };

                return compiler.Compile();
            }
            catch (Exception ex)
            {
                args.Error(0, 0, "Fatal error: " + ex);
            }
            return false;
        }

        private bool TryGetFramePath(string input, out string filePath)
        {
            bool isFound = true;
            filePath = Path.ChangeExtension(input, ".frame");
            if (!File.Exists(filePath))
            {
                var dir = Path.GetDirectoryName(input);
                filePath = Path.Combine(dir, ".frame");
                if (!File.Exists(filePath))
                {
                    filePath = null;
                    isFound = false;
                }
            }
            return isFound;
        }
    }
}
