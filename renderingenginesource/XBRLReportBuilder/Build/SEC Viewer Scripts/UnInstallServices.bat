@ECHO Off

cd PreviewBuilder
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil /u Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\SECBuilder
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil /u Aucent.XBRLReportBuilder.BuilderService.exe

cd ..\Dispatcher
C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil /u Aucent.FilingServices.Dispatcher.exe

cd ..