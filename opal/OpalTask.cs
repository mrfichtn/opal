using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Opal.Logging;

namespace Opal
{
    /// <summary>
    /// Coco build task
    /// </summary>
    public class OpalTask : Task
    {
        private bool isOk;

        #region Properties

        public string? Namespace { get; set; }

        [Required]
        public string Input { get; set; } = string.Empty;
        public string? Outputs { get; set; }
        public bool WriteTimeStamp { get; set; }

        #region ParserFrame Property
        public string? ParserFrame
        {
            get { return _parserFrame; }
            set { _parserFrame = value; }
        }
        private string? _parserFrame;
        #endregion

        #endregion

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(Input))
            {
                Log.LogError("Input file is empty");
                return false;
            }

            if (!File.Exists(Input))
            {
                Log.LogError("Input file '{0}' does not exist", Input);
                return false;
            }

            if (Outputs == null)
                Outputs = Path.ChangeExtension(Input, ".cs");

            TryGetFramePath(ref _parserFrame);

            Log.LogMessage(MessageImportance.Normal, "Opal: beginning compile {0}", DateTime.Now);
            var ns = Namespace;
            GenerateCode(ns);

            return isOk;
        }

        private void GenerateCode(string? ns)
        {
            try
            {
                var fi = new FileInfo(Input);
                if (!fi.Exists)
                    throw new FileNotFoundException("Source file not found.", Input);

                var logger = new BuildTaskLogger(Log, Input);
                var compiler = new Compiler(logger, Input)
                {
                    Namespace = ns,
                    ParserFrame = ParserFrame
                };
                if (Outputs != null)
                    compiler.OutPath = Outputs;

                isOk = compiler.Compile();
            }
            catch (Exception ex)
            {
                Log.LogError(null, null, null, Input, 0, 0, 0, 0, "Fatal error: " + ex);
            }
        }

        private bool TryGetFramePath(ref string? filePath)
        {
            bool isFound = true;
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = Path.ChangeExtension(Input, ".frame");
                if (!File.Exists(filePath))
                {
                    var dir = Path.GetDirectoryName(Input);
                    filePath = Path.Combine(dir, ".frame");
                    if (!File.Exists(filePath))
                    {
                        filePath = null;
                        isFound = false;
                    }
                }
            }
            return isFound;
        }
    }
}
