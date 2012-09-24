using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XBRLReportBuilder
{
    public delegate void UpdateProgressExecutor(int percentageComplete, string message);

    public delegate void UpdateStatusExecutor(string message);
}
