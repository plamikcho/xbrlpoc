@ECHO Off

cd Dispatcher
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil Aucent.FilingServices.Dispatcher.exe

cd ..\PreviewBuilder
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\SECBuilder
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil Aucent.XBRLReportBuilder.BuilderService.exe

cd ..