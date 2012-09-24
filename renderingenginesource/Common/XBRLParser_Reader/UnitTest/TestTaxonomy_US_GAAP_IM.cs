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
	public class TestTaxonomy_US_GAAP_IM : Taxonomy
	{
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

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
		{}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
		{}
		#endregion

		string IM_FILE = TestCommon.FolderRoot + @"us-gaap-im" +System.IO.Path.DirectorySeparatorChar +"us-gaap-im-2005-02-01.xsd";
		string IM_2005_06_28_FILE = TestCommon.FolderRoot + @"us-gaap-im" + System.IO.Path.DirectorySeparatorChar + "us-gaap-im-2005-06-28.xsd";
		string OM_20060430_FILE = TestCommon.FolderRoot + @"us-gaap-im" + System.IO.Path.DirectorySeparatorChar + "omaf-20060430.xsd";

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

		/// <exclude/>
		public TestTaxonomy_US_GAAP_IM()
		{
		}

		/// <exclude/>
		[Test]
		public void IM_LoadAndParse()
		{
			TestTaxonomy_US_GAAP_IM s = new TestTaxonomy_US_GAAP_IM();
			int errors = 0;

			if ( s.Load( IM_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.ErrorList );
			}

			Assert.AreEqual( 0, errors, "parse failed" );
			// 1 duplicate element

			PresentationLink pl = s.presentationInfo[ "http://www.xbrl.org/us/fr/lr/role/PortfolioInvestments" ] as PresentationLink;
			Assert.IsNotNull( pl, "can't get http://www.xbrl.org/us/fr/lr/role/PortfolioInvestments" );

			Node parentNode = pl.CreateNode( "en", "Label" );
			Assert.AreEqual(1, parentNode.Children.Count, "wrong number of children" );

		


		}

		/// <exclude/>
		[Test]
		public void IM_2005_06_28_LoadAndParse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_US_GAAP_IM s = new TestTaxonomy_US_GAAP_IM();

			int errors = 0;

			if ( s.Load( IM_2005_06_28_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
				SendWarningsToConsole( s.errorList );
				SendInfoToConsole( s.ErrorList );
			}

			Assert.AreEqual( 0, errors, "wrong number of errors" );
			// 1 duplicate element
			Element RepurchaseAgreementsTuple = s.AllElements["usfr-ime_RepurchaseAgreementsTuple"] as Element;

			Assert.IsNotNull(RepurchaseAgreementsTuple, "Failed to find RepurchaseAgreementsTuple");

			Assert.IsTrue(RepurchaseAgreementsTuple.HasChildren, "RepurchaseAgreementsTuple does nothave any children");

		}

		/// <exclude/>
		[Test]
		public void IM_2005_06_28_TestBoolean()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_US_GAAP_IM s = new TestTaxonomy_US_GAAP_IM();
			int errors = 0;

			if ( s.Load( IM_2005_06_28_FILE, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
			s.Parse( out errors );

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if ( errors > 0 )
			{
				SendErrorsToConsole( s.errorList );
			}

			// usfr-ime_AffiliatedCompany is boolean type
			Element e = s.allElements["usfr-ime_AffiliatedCompany"] as Element;
			Assert.IsNotNull( e, "usfr-ime_AffiliatedCompany element not found" );

			Enumeration enumer = s.enumTable["xbrli:booleanItemType"] as Enumeration;
			Assert.IsNotNull( enumer );

			Assert.IsNotNull (e.EnumData, "no enumeration data" );
			Assert.AreEqual( 2, e.EnumData.Values.Count, "wrong number of enumerations" );
			Assert.AreEqual( "Yes", e.EnumData.Values[0], "wrong enumeration at index 0" );
			Assert.AreEqual( "No", e.EnumData.Values[1], "wrong enumeration at index 1" );
		}

		/// <exclude/>
		[Test]
		public void OM_20060430_FILE_LoadAndParse()
		{
			//Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );

			TestTaxonomy_US_GAAP_IM s = new TestTaxonomy_US_GAAP_IM();

			int errors = 0;

			if (s.Load(OM_20060430_FILE, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
			s.Parse(out errors);

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if (errors > 0)
			{
				SendErrorsToConsole(s.errorList);
				SendWarningsToConsole(s.errorList);
				SendInfoToConsole(s.ErrorList);
			}

			Assert.AreEqual(0, errors, "wrong number of errors");
			// 1 duplicate element
			Element RepurchaseAgreementsTuple = s.AllElements["usfr-ime_RepurchaseAgreementsTuple"] as Element;

			Assert.IsNotNull(RepurchaseAgreementsTuple, "Failed to find RepurchaseAgreementsTuple");

			Assert.IsTrue(RepurchaseAgreementsTuple.HasChildren, "RepurchaseAgreementsTuple does nothave any children");

		}



		/// <exclude/>
		[Test]
		public void Test_ExtendedTupleLabelsResolution()
		{

			TestTaxonomy_US_GAAP_IM s = new TestTaxonomy_US_GAAP_IM();

			int errors = 0;
			string fileName = TestCommon.FolderRoot + @"faif-20071031" + System.IO.Path.DirectorySeparatorChar + "faif-20071031.xsd";
			if (s.Load(fileName, out errors) != true)
			{
				Assert.Fail((string)s.ErrorList[0]);
			}

			errors = 0;

			// this loads up all dependant taxonomies, and loads the corresponding presentation, calculation, Label, and reference linkbases
			s.Parse(out errors);

			// loads the presentation linkbase for this taxonomy and merges the dependant taxonomy presentation linkbases
			if (errors > 0)
			{
				SendErrorsToConsole(s.errorList);
				SendWarningsToConsole(s.errorList);
				SendInfoToConsole(s.ErrorList);
			}
			s.CurrentLabelRole = PresentationLocator.preferredLabelRole;
			s.CurrentLanguage = "en";


			ArrayList nodes =   s.GetNodesByPresentation();

			ArrayList flatList = new ArrayList();
			GetFlatListOfPresentationNodes(nodes, ref flatList);

			Hashtable expectedValues = new Hashtable();
			expectedValues["usfr-ime_InvestmentDescription"] = "Investment Description";
			expectedValues["usfr-ime_NumberContracts"] = "Number of Contracts";
			expectedValues["usfr-ime_ContractsHeldLongShort"] = "Are Contracts Held Long or Short?";
			expectedValues["usfr-ime_ExpirationDate"] = "Expiration Date";
			expectedValues["usfr-ime_FaceValue"] = "Face Value";
			expectedValues["usfr-ime_FuturesContractUnrealizedAppreciationDepreciation"] = "Futures Contract - Unrealized Appreciation/(Depreciation)";
			expectedValues["faif_NotionalContractValue"] = "Notional Contract Value";

			foreach (Node n in flatList)
			{
				if (n.parent != null && n.parent.Id == "faif_FuturesListingTuple")
				{
					Console.WriteLine(n.Label);
					if (expectedValues[n.Id] != null)
					{
						Assert.AreEqual(expectedValues[n.Id], n.Label);

					}
					else
					{
						Assert.Fail("Failed to find child " + n.Id);
					}
				}

			}
			
		}

		internal static void GetFlatListOfPresentationNodes(ArrayList nodes, ref ArrayList flatList)
		{

			flatList.AddRange(nodes);

			foreach (Node n in nodes)
			{
				if (n.children != null)
				{
					GetFlatListOfPresentationNodes(n.children, ref flatList);
				}
			}
		}



	}
}
#endif