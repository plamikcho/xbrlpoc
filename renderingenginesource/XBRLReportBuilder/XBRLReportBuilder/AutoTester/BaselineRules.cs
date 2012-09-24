using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

using System.Text;
using Aucent.MAX.AXE.XBRLReportBuilder.ReportBuilder.Data;

namespace XBRLReportBuilder
{
    public class BaselineRules : IDisposable 
    {
        private InstanceReport localBaseReport;
        private InstanceReport localNewReport;

        private InstanceReportRow localBaseRows;
        private InstanceReportRow localNewRows;

        private int errorID;
        private static string connectionString = ConfigurationManager.ConnectionStrings["SaveResults"].ConnectionString;
        private const string UpdateFileBaseline = "UpdateFileBaseline";

        /// <summary>
        /// Class to evaluate specific rules on an instance report
        /// </summary>
        ///<param name="baseReport">The base report to compare</param>
        ///<param name="newReport">the new report to compare</param>
        /// <param name="errorNumber">the SQL file ID of the instance report</param>
        public BaselineRules(InstanceReport baseReport, InstanceReport newReport, int errorNumber)
        {
            if (baseReport != null && newReport != null && errorNumber != 0)
            {
                localBaseReport = baseReport;
                localNewReport = newReport;
                errorID = errorNumber;
            }
            else
            {
                throw new ArgumentNullException("report and errorNumber must be declared.");
            }
        }

        /// <summary>
        /// Evaluates two rows from an instance report
        /// </summary>
        /// <param name="baseRows">The base row to compare</param>
        /// <param name="newRows">The new row to compare</param>
        /// <param name="errorNumber">The File ID from the SQL database</param>
        public BaselineRules(InstanceReportRow baseRows, InstanceReportRow newRows, int errorNumber)
        {
            if (baseRows != null && newRows != null && errorNumber != 0)
            {
                localBaseRows = baseRows; 
                localNewRows = newRows;
                errorID = errorNumber;
            }
            else
            {
                throw new ArgumentNullException("report and errorNumber must be declared.");
            }
        }

        public void EvaluateIsAbstractGroupRule()
        {
            RowCounter baseCounter = CompareReports.BuildRowCount(localBaseReport);
            RowCounter newCounter = CompareReports.BuildRowCount(localNewReport);

            if (localBaseReport.Rows.Count == (localNewReport.Rows.Count - 1)
                && baseCounter["IsAbstractGroupTitle"] == (newCounter["IsAbstractGroupTitle"] - 1)
                && localNewReport.Rows[0].Label.ToLower().Contains("[text block]"))
            {
                //Update baseline flag: 
                UpdateDataBase();
            }

            baseCounter = null;
            newCounter = null;
        }

        public bool EvaluateRowHeaderOrder()
        {
            bool retVal = true;

            for (int i = 0; i < localBaseReport.Rows.Count; i++)
            {
                //compare the labels, look for the order: 
                if (!localBaseReport.Rows[i].Label.Equals(localNewReport.Rows[i].Label))
                {
                    //Sorting is different, update the db, return false.
                    UpdateDataBase();
                    retVal = false;
                }
            }

            return retVal;
        }

        public void EvaluateUnmatchedDataError(int rowNumber)
        {
            decimal actualResult = 0;
            List<string> comparedDates = new List<string>();

            for (int i = 0; i < localNewReport.Columns.Count; i++)
            {
                List<InstanceReportColumn> currentColumns = new List<InstanceReportColumn>();

                if (localNewReport.Columns[i].Labels.Count == 2)
                {
                    //Find the ending date of the current column:
                    string endingDate = localNewReport.Columns[i].Labels[1].Label;
                    //Find the matching amount of the current row cell: 
                    Cell currentCell = localNewReport.Rows[rowNumber - 1].Cells.Find(
                        c => c.Id == localNewReport.Columns[i].Id);
                    actualResult = currentCell.NumericAmount;
                    
                    //See if it exists anywhere else in the list: 
                    currentColumns = localNewReport.Columns.FindAll(
                        lf => 
                            lf.Labels.Count > 1 
                            && lf.Labels[1].Label.Contains(endingDate) 
                        );

                    if (currentColumns.Count > 1)
                    {
                        bool matches = false;
                        foreach (InstanceReportColumn col in currentColumns)
                        {
                            //Pull out the correct cell: 
                            Cell currentCell2 = localNewReport.Rows[rowNumber - 1].Cells.Find(c => c.Id == col.Id);
                            //Check the corresponding row.cell ID for the actual amount; can't find, update
                            if (localNewReport.Rows[rowNumber - 1].PeriodType.ToLower() == "instant"
                                && currentCell2.NumericAmount.Equals(actualResult))
                                matches = true;  
                        }

                        if (!matches)
                        {
                            UpdateDataBase();
                        }
                    }
                    else
                    {
                        UpdateDataBase();
                    }
                }
            }
           
        }

        public void EvaluateRoundingOptionError()
        {
            //If rounding options don't match...
            if (!localBaseReport.RoundingOption.Equals(localNewReport.RoundingOption))
            {
                //Evaluate base report rounding...
                if ((localBaseReport.RoundingOption.Contains(RoundingLevel.Thousands.ToString())
                    || localBaseReport.RoundingOption.Contains(RoundingLevel.Millions.ToString())
                    || localBaseReport.RoundingOption.Contains(RoundingLevel.Billions.ToString())
                    )
                    //and if new report is empty...
                    && string.IsNullOrEmpty(localNewReport.RoundingOption))
                {
                    //Update
                    UpdateDataBase();
                }

                //Else, if doesn't contain "unless otherwise specified", update.
                else if (!localNewReport.RoundingOption.Contains("unless otherwise specified"))
                {
                    {   
                        UpdateDataBase();
                    }
                }

                //Else, if DOES contain "unless..." AND does NOT contain an 'Other' unit type, update
                else if (localNewReport.RoundingOption.Contains("unless otherwise specified"))
                {
                    bool otherExists = false;
                     foreach (InstanceReportRow row in localNewReport.Rows)
                    {
                        if (!row.IsNumericDataNil())
                        {
                            if (row.Unit == UnitType.Other)
                            {
                                otherExists = true;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                     if (!otherExists)
                         UpdateDataBase();
                }

            }
        }

        private void UpdateDataBase()
        {
            string error;
            SQLLog.ModifyBaseLine(errorID, false, out error);
        }

        public void Dispose()
        {
            if (localBaseReport != null && localNewReport != null)
            {
                localBaseReport = null;
                localNewReport = null;    
            }

            if (localBaseRows != null && localNewRows != null)
            {
                localBaseRows = null;
                localNewRows = null;
            }

        }
    }
}
