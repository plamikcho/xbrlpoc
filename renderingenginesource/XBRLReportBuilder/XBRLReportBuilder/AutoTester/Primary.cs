using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Threading;
using Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Dialogs;
using Aucent.MAX.AXE.XBRLReportBuilder.AutoTester.Data;

namespace Aucent.MAX.AXE.XBRLReportBuilder.AutoTester
{
    public partial class Primary : Form
    {
		public Primary()
        {
            InitializeComponent();
        }

        private void cmdBaseZip_Click(object sender, EventArgs e)
        {
            txtBaseFilings.Text = PickFolder(txtBaseFilings);
        }


        /// <summary>
        /// Display a folder picker, and push the result, if chosen, to the specified output Text Box.
        /// </summary>
        /// <param name="txtPathBox">A <see cref="T:TextBox"/> control where the folder name is to be stored.</param>
        /// <returns>A <see cref="T:String"/> containing the new path, or an empty string if a folder was not chosen.</returns>
        private String PickFolder(TextBox txtPathBox)
        {
            FolderBrowserDialog Picker = new FolderBrowserDialog();

            // Only use given path if it's valid
            if (Directory.Exists(txtPathBox.Text))
            {
                Picker.SelectedPath = txtPathBox.Text;
            }

            if (Picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return Picker.SelectedPath;
            }
            else
            {
                return txtPathBox.Text;
            }
        }

        private void cmdNewZip_Click(object sender, EventArgs e)
        {
            txtNewFilings.Text = PickFolder(txtNewFilings);
        }

        private void cmdLogs_Click(object sender, EventArgs e)
        {
            txtLogs.Text = PickFolder(txtLogs);
        }

		private FileInfo errorFileInfo = null;
		private Progress testingProgress = null;
		private StringBuilder testSummary = null;
		private List<FilingResult> testSummaryItems = null;
		private MultiThreadedWaitState<int> threadCounter = null;
		private Semaphore throttler = null;
		private BackgroundWorker workerTester = null;
        private void cmdRun_Click(object sender, EventArgs e)
        {
			if( !this.ValidateConfiguration() )
				return;

            // Directories used to find/output files:
			Data.AutoTester.baseFilingsPath = txtBaseFilings.Text;
			Data.AutoTester.newFilingsPath = txtNewFilings.Text;
			Data.AutoTester.logPath = txtLogs.Text;


			this.txtOutput.Items.Clear();
			this.lblDifferencesCount.Text = "0";

			// For multithreading
			int numWorkers = 0;
			int.TryParse( txtMaxWorkers.Text, out numWorkers );

			if( numWorkers < 1 )
			{
				numWorkers = 1;
			}
			else if( numWorkers > 99 )
			{
				numWorkers = 99;
			}

			string errorFile = Path.Combine( Data.AutoTester.logPath, "Errors.txt" );
			this.errorFileInfo = new FileInfo( errorFile );

			//consider Environment.NewLine
			this.testSummary = new StringBuilder( "Summary of Tests\r\n===========================\r\n" );
			this.testSummaryItems = new List<FilingResult>();

			// Queueing process to execute callback after all threads have completed
			this.threadCounter = new MultiThreadedWaitState<int>();
			this.threadCounter.CompletedCallback += new ThreadStart( this.threadCounter_Completed );

			this.throttler = new Semaphore( numWorkers, numWorkers );

			// Worker thread
            this.workerTester = new BackgroundWorker(); // performs testing
			this.workerTester.WorkerSupportsCancellation = true;
			this.workerTester.DoWork += this.RunAllTests;

			// To delay execution of worker until after progressbar is visible
			// Run the testing on a background thread to keep the UI/progressbar updating
			this.testingProgress = new Progress();
			this.testingProgress.cmdCancel.Click += new EventHandler( this.cmdCancel_Click );
			this.testingProgress.HandleCreated += this.StartTesting;
            this.testingProgress.ShowDialog( this );
        }

		private void RunAllTests( object sender, EventArgs e )
		{
			string[] ZippedFilings = Directory.GetFiles( Data.AutoTester.baseFilingsPath );
			string[] FolderedFilings = Directory.GetDirectories( Data.AutoTester.baseFilingsPath );

			string[] paths = ( ZippedFilings.Length > FolderedFilings.Length ) ?
				ZippedFilings : FolderedFilings;

			//.NET 2.0 equivalent of "Action" delegate
			ThreadStart actPrepareProgress = () =>
			{
				Label lbl = this.testingProgress.Controls[ "lblStatus" ] as Label;
				lbl.Text = "Status: Clearing working folders...";

				testingProgress.pgbOverall.Minimum = 0;
				testingProgress.pgbOverall.Maximum = paths.Length;
			};

			//execute on the UI thread
			this.testingProgress.pgbOverall.Invoke( actPrepareProgress );
			this.threadCounter.CompletionTarget = paths.Length;

			Data.AutoTester.PrepareTestPaths();
			File.WriteAllText( this.errorFileInfo.FullName, string.Empty );

			foreach( string baseZipPath in paths )
			{
				string threadPath = baseZipPath;
				WaitCallback task = new WaitCallback( o => this.RunTest( threadPath, o ) );
				ThreadPool.QueueUserWorkItem( task, this.threadCounter );
			}
		}

		private void RunTest( string threadPath, object o )
		{
			this.throttler.WaitOne();

			try
			{
				// Discontinue loop if cancelled
				if( !this.workerTester.CancellationPending )
				{
					FilingResult result = new FilingResult();
					StringBuilder testErrors = new StringBuilder();
					Data.AutoTester.RunTest( result, testErrors, threadPath );

					lock( this.errorFileInfo )
					{
						using( StreamWriter writer = new StreamWriter( this.errorFileInfo.FullName, true ) )
						{
							writer.Write( testErrors.ToString() );
						}
					}

					//.NET 2.0 equivalent of "Action" delegate
					ThreadStart actUpdateProgress = new ThreadStart( () => this.UpdateProgress( result ) );

					//execute on the UI thread
					this.testingProgress.Invoke( actUpdateProgress );
				}
			}
			catch( Exception ex )
			{
				if( this.workerTester.CancellationPending )
				{
					//suppress exceptions caused by cancelation
				}
				else
				{
					//otherwise, throw the exception
					throw ex;
				}
			}
			// Release thread in any case
			finally
			{
				this.threadCounter.CompletionState++;
				this.throttler.Release();
			}
		}

		private void StartTesting( object sender, EventArgs e)
		{
			this.workerTester.RunWorkerAsync();
		}

		void cmdCancel_Click( object sender, EventArgs e )
		{
			// Cancel operation
			this.threadCounter.Cancel();
			this.workerTester.CancelAsync();
		}

		private void threadCounter_Completed()
		{
			// Accomodate cancellation
			if( workerTester.CancellationPending )
				this.testSummary.AppendFormat( "\r\nPROCESSING CANCELLED at {0}/{1}", threadCounter.CompletionState, threadCounter.CompletionTarget );

			//.NET 2.0 equivalent of "Action" delegate
			ThreadStart actCloseProgress = () =>
			{
				this.testingProgress.Close();
			};

			this.testingProgress.Invoke( actCloseProgress );
		}

		private bool ValidateConfiguration()
		{
			// Ensure directories chosen exist
			if( !Directory.Exists( txtBaseFilings.Text ) )
			{
				MessageBox.Show( "The directory specified for the Base Zip Files does not exist.", "Directory Invalid", MessageBoxButtons.OK );
				return false;
			}

			if( !Directory.Exists( txtNewFilings.Text ) )
			{
				MessageBox.Show( "The directory specified for the New Zip Files does not exist.", "Directory Invalid", MessageBoxButtons.OK );
				return false;
			}

			if( !Directory.Exists( txtLogs.Text ) )
			{
				MessageBox.Show( "The directory specified for the Log Files does not exist.", "Directory Invalid", MessageBoxButtons.OK );
				return false;
			}

			return true;
		}

        private void Primary_Load(object sender, EventArgs e)
        {
            // Initialize TextBox paths with defaults
			txtBaseFilings.Text = Data.AutoTester.baseFilingsPath;
			txtNewFilings.Text = Data.AutoTester.newFilingsPath;
			txtLogs.Text = Data.AutoTester.logPath;

			if( this.DesignMode )
			{
				this.splitContainer1.Panel2Collapsed = false;
			}
			else
			{
				this.splitContainer1.Panel2Collapsed = true;
			}
        }

		private void txtMaxWorkers_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Only allow numbers and control (backspace,etc)
			if ((!char.IsNumber(e.KeyChar)) && (!char.IsControl(e.KeyChar)))
			{
				e.Handled = true;
			}
		}

		private void UpdateProgress( FilingResult result )
		{
			this.testingProgress.pgbOverall.Value++;

			Label lbl = this.testingProgress.Controls[ "lblStatus" ] as Label;
			lbl.Text = string.Format( "Status: {0}/{1}",
				this.threadCounter.CompletionState, this.threadCounter.CompletionTarget );

			ListViewItem item = new ListViewItem( result.Name );
			item.SubItems.Add( new ListViewItem.ListViewSubItem( item, result.Success.ToString() ) );
			item.SubItems.Add( new ListViewItem.ListViewSubItem( item, result.Errors.ToString() ) );
			item.SubItems.Add( new ListViewItem.ListViewSubItem( item, result.Reason ) );
			item.Tag = result;
		
			this.txtOutput.Items.Add( item );
			this.txtOutput.AutoResizeColumns( ColumnHeaderAutoResizeStyle.ColumnContent );

			if( result.Success == false || result.Errors > 0 )
				this.lblDifferencesCount.Text = ( int.Parse( this.lblDifferencesCount.Text ) + 1 ).ToString();
		}

		private void txtOutput_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			if( this.txtOutput.SelectedItems == null || this.txtOutput.SelectedItems.Count == 0 )
				return;

			FilingResult fr = this.txtOutput.SelectedItems[ 0 ].Tag as FilingResult;
			if( fr == null )
				return;

			if( this.splitContainer1.Panel2Collapsed )
			{
				int tmp = this.splitContainer1.Panel1.Width;
				this.splitContainer1.Panel2Collapsed = false;
				if( tmp != this.splitContainer1.Panel1.Width )
				{
					this.splitContainer1.SplitterDistance = tmp;
				}
			}

			string path = Path.Combine( Data.AutoTester.logPath, fr.Name );
			if( Directory.Exists( path ) )
				this.webBrowser1.Navigate( path );
			else if( Directory.Exists( Data.AutoTester.logPath ) )
				this.webBrowser1.Navigate( path );
		}
	}
}
