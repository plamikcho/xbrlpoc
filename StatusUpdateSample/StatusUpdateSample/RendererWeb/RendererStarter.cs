
namespace RendererWeb
{
    using System;
    using System.IO;
    using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilderRenderer;
    using XBRLReportBuilder;

    /// <summary>
    /// Wrapper for the FilingProcessor class
    /// </summary>
    public class RendererStarter
    {
        // delegate for the external messages
        UpdateStatusExecutor executor;

        // ctor
        public RendererStarter(UpdateStatusExecutor executor)
        {
            this.executor = executor;
        }

        // prevent default ctor
        private RendererStarter()
        {           
        }

        // 
        public void Run(string fileName, string reportsFolder)
        {
            if (File.Exists(fileName))
            {
                //executor.Invoke(fileName + " processing started");                
                //string binpath = System.IO.Path.GetDirectoryName(
                //    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
                try
                {
                    // simulating console args for the filing processor
                    string[] args = new string[] 
                    { 
                        @"/" + FilingProcessor.INSTANCE_COMMAND + "=" + fileName
                        ,@"/" + FilingProcessor.SAVEAS_COMMAND + "=Xml",
                        @"/" + FilingProcessor.REPORTS_FOLDER_COMMAND + "=" + reportsFolder
                        //,@"/" + FilingProcessor.XSLT_STYLESHEET_COMMAND + "=" + binpath + 
                        //    @"\" + XBRLReportBuilder.Utilities.RulesEngineUtils.RESOURCES_FOLDER + @"\" +
                        //    @"\" + XBRLReportBuilder.Utilities.RulesEngineUtils.TransformFile
                    };
                    
                    FilingProcessor proc = FilingProcessor.Load(this.executor, args);
                    proc.ProcessFilings();
                }
                catch (Exception ex)
                {
                    executor.Invoke("Renderer starter unexpected error: " + ex.Message);
                }
            }
            else
            {
                executor.Invoke("File " + fileName + " not found");
            }
        }
    }
}