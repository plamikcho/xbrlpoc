// ===========================================================================================================
//  Common Public Attribution License Version 1.0.
//
//  The contents of this file are subject to the Common Public Attribution License Version 1.0 (the “License”); 
//  you may not use this file except in compliance with the License. You may obtain a copy of the License at
//  http://www.rivetsoftware.com/content/index.cfm?fuseaction=showContent&contentID=212&navID=180.
//
//  The License is based on the Mozilla Public License Version 1.1 but Sections 14 and 15 have been added to 
//  cover use of software over a computer network and provide for limited attribution for the Original Developer. 
//  In addition, Exhibit A has been modified to be consistent with Exhibit B.
//
//  Software distributed under the License is distributed on an “AS IS” basis, WITHOUT WARRANTY OF ANY KIND, 
//  either express or implied. See the License for the specific language governing rights and limitations 
//  under the License.
//
//  The Original Code is Rivet Dragon Tag XBRL Enabler.
//
//  The Initial Developer of the Original Code is Rivet Software, Inc.. All portions of the code written by 
//  Rivet Software, Inc. are Copyright (c) 2004-2008. All Rights Reserved.
//
//  Contributor: Rivet Software, Inc..
// ===========================================================================================================
#if UNITTEST
namespace Aucent.MAX.AXE.XBRLParser.Test_Taxonomy
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using System.Collections;
	using System.Diagnostics;
	using NUnit.Framework;
	
	using Aucent.MAX.AXE.XBRLParser.Test;
	using Aucent.MAX.AXE.XBRLParser;
	using Aucent.MAX.AXE.Common.Data;
	using Aucent.MAX.AXE.Common.Utilities;
	using Aucent.MAX.AXE.Common.Exceptions;

	/// <exclude/>
	[TestFixture] 
	public class TestTaxonomy_MSFT_FIN : Taxonomy
	{
		#region Overrides
//		public void TestParse( out int errors )
//		{
//			errors = 0;
//			ParseInternal( out errors );
//		}

		#endregion

		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
		{
			Trace.Listeners.Clear();

			//TODO: Add this line back in to see data written
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Error;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
		{
			Trace.Listeners.Clear();
		}

		///<exclude/>
		[SetUp] public void RunBeforeEachTest()
		{}

		///<exclude/>
		[TearDown] public void RunAfterEachTest() 
		{}
		#endregion

		string MSFT_FIN_FILE = TestCommon.FolderRoot + @"MSFT_FIN" +System.IO.Path.DirectorySeparatorChar +"msft-fin.xsd";

		#region helpers
		/// <exclude/>
		protected void SendErrorsToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level != TraceLevel.Error )
				{
					break;	// all the errors should be first after sort
				}

				Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
			}
		}

		/// <exclude/>
		protected int SendWarningsToConsole(ArrayList errorList)
		{
			int numwarnings = 0;
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Warning )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
					++numwarnings;
				}
			}

			return numwarnings;
		}

		/// <exclude/>
		protected void SendInfoToConsole(ArrayList errorList)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Level == TraceLevel.Info )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		/// <exclude/>
		protected void SendWarningsToConsole(ArrayList errorList, string filter)
		{
			// now display the errors 
			errorList.Sort();

			foreach ( ParserMessage pm in errorList )
			{
				if ( pm.Message.IndexOf( filter ) < 0 )
				{
					Console.WriteLine( pm.Level.ToString() + ": " + pm.Message );
				}
			}
		}

		#endregion

		/// <summary>
		/// Summary description for TestTaxonomy_MorganStanley.
		/// </summary>
		public TestTaxonomy_MSFT_FIN()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		/// <exclude/>
		[Test]
		[Ignore("MSFT_FIN is a work in progress")]
		public void MSFT_FIN_LoadAndParse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_MSFT_FIN s = new TestTaxonomy_MSFT_FIN();

			int errors = 0;

			if ( s.Load( MSFT_FIN_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			s.LoadImports( out errors );
			s.LoadPresentation( out errors );
			s.LoadElements( out errors );
			s.BindPresentationCalculationElements( true, out errors );

			// loads the elements for this taxonomy and merges the dependant taxonomy elements
			errors = 0;
			//s.Parse( out errors );

			Assert.AreEqual( 1837, s.allElements.Count, "wrong number of elements returned" );

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();
			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
//			s.Parse( out errors );

			Assert.AreEqual( 1837, s.allElements.Count, "wrong number of elements returned" );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.ErrorList );
			}

			// 37 advanced tuples
			Assert.AreEqual( 0, errors, "parse failed" );

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

			//ArrayList nodes = s.GetNodesByPresentation();
		}


		/// <exclude/>
		[Test]
		public void TestEtrade_Presentation()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"ETRADE-20060930" + System.IO.Path.DirectorySeparatorChar + "etfc-20060930.xsd";

			TestTaxonomy_MSFT_FIN s = new TestTaxonomy_MSFT_FIN();

			int errors = 0;

			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;

			s.LoadImports(out errors);
			s.LoadPresentation(out errors);
			s.LoadElements(out errors);
			s.BindPresentationCalculationElements(true, out errors);

			// loads the elements for this taxonomy and merges the dependant taxonomy elements
			errors = 0;
			//s.Parse( out errors );

			Assert.AreEqual(2525, s.allElements.Count, "wrong number of elements returned");

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();
			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			//			s.Parse( out errors );

			Assert.AreEqual(7, nodes.Count, "wrong number of top level presentation nodes");
			Assert.AreEqual(2525, s.allElements.Count, "wrong number of elements returned");

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if (errors > 0)
			{
				SendErrorsToConsole(s.errorList);
				SendWarningsToConsole(s.errorList);
				SendInfoToConsole(s.ErrorList);
			}

			// 37 advanced tuples
			Assert.AreEqual(0, errors, "parse failed");

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";

		}

		/// <exclude/>
		[Test]
		public void TestEtrade_Calculation()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"ETRADE-20060930" + System.IO.Path.DirectorySeparatorChar + "etfc-20060930.xsd";

			TestTaxonomy_MSFT_FIN s = new TestTaxonomy_MSFT_FIN();

			int errors = 0;

			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;
			s.Parse(out errors);

			s.currentLabelRole = @"preferredLabel";
			s.currentLanguage = @"en";

			ArrayList nodes = s.GetNodesByCalculation();
			Assert.AreEqual(5, nodes.Count, "wrong number of top level presentation nodes");

		}


		/// <exclude/>
		[Test, Ignore]
		public void TestGermanCI_Presentation()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			string fileName = TestCommon.FolderRoot + @"GermanCI_GCD" + System.IO.Path.DirectorySeparatorChar + "de-gaap-ci-2006-12-01.xsd";

			TestTaxonomy_MSFT_FIN s = new TestTaxonomy_MSFT_FIN();

			int errors = 0;

			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			s.Parse(out errors);

			Assert.AreEqual(0, errors, "should not have any errors");


			

			Assert.AreEqual(2677, s.allElements.Count, "wrong number of elements returned");

			s.CurrentLanguage = "en";
			s.CurrentLabelRole = "preferredLabel";

			ArrayList nodes = s.GetNodesByPresentation();
			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, label, and reference linkbases
			//			s.Parse( out errors );

			Assert.AreEqual(11, nodes.Count, "wrong number of top level presentation nodes");
			Assert.AreEqual(2677, s.allElements.Count, "wrong number of elements returned");

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if (errors > 0)
			{
				SendErrorsToConsole(s.errorList);
				SendWarningsToConsole(s.errorList);
				SendInfoToConsole(s.ErrorList);
			}

			// 37 advanced tuples
			Assert.AreEqual(0, errors, "parse failed");

			s.CurrentLabelRole = "preferredLabel";
			s.CurrentLanguage = "en";


			Element tupleChild = s.AllElements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.name.kkth.FK"] as Element;

            Assert.IsTrue(tupleChild.TupleParentList[0].Id == "de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.name", "Parent Id not set");


			Element tupleParent = s.AllElements["de-gaap-ci_kke.limitedPartners.sumEquityAccounts.kinds.name"] as Element;

			Assert.IsTrue(tupleParent.IsTuple, "Should be a tuple");
			Assert.IsTrue(tupleParent.HasChildren , "Should have child elements");

			Assert.AreEqual(14, tupleParent.TupleChildren.Count, "should have fourteen children elements");

		}


		
	}
}
#endif