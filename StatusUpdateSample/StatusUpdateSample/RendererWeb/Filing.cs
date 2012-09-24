using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using XBRLReportBuilder;


namespace Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilderRenderer
{
	public class Filing
	{
		internal enum SaveAs
		{
			Auto = 0,
			Xml = 1,
			Zip = 2
		}

		public string InstancePath { get; set; }
		public string TaxonomyPath { get; set; }

		public Filing(UpdateStatusExecutor executor, string instancePath )
		{
			if( !Path.IsPathRooted( instancePath ) )
			{
				string tmp = Path.Combine( FilingProcessor.cd, instancePath );
                executor.Invoke("Information: Instance document has a relative path " + instancePath + ".");
                executor.Invoke("\tAdjusting path to current locatin " + tmp + "");
				instancePath = tmp;
			}

			this.InstancePath = instancePath;
		}
	}
}
