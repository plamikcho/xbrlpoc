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
namespace Aucent.MAX.AXE.XBRLParser.Test
{
	using System;
	using System.Xml;
	using System.Text;
	using System.Diagnostics;
	using System.Collections;
	using System.Globalization;

    using NUnit.Framework;
	using Aucent.MAX.AXE.XBRLParser;

	/// <exclude/>
	[TestFixture] 
    public class TestElement : Element
    {
		#region init

		/// <exclude/>
		[TestFixtureSetUp]
		public void RunFirst()
        {
			Trace.Listeners.Add( new TextWriterTraceListener(Console.Out) );
			Common.MyTraceSwitch = new TraceSwitch( "Common", "common trace switch" );
			Common.MyTraceSwitch.Level = TraceLevel.Verbose;
		}

		/// <exclude/>
		[TestFixtureTearDown]
		public void RunLast() 
        {}

		/// <exclude/>
		[SetUp]
		public void RunBeforeEachTest()
        {}

		/// <exclude/>
		[TearDown]
		public void RunAfterEachTest() 
        {}

		#endregion

		

		/// <exclude/>
		[Test]
		public void Test_CreateAbstractElement()
		{
			Element e = Element.CreateAbstractElement( "abs_elem" );

			Assert.IsTrue( e.IsAbstract, "element is not abstract" );
		}
		#region to xml tests
		/// <exclude/>
		[Test]
		public void ElementToXmlFragment()
		{
			Element e1 = new Element( "Test1", "Test1", "substGroup1" );
			e1.AddOptionals( "Test", true, Element.PeriodType.duration, Element.BalanceType.debit, false );

			StringBuilder sb = new StringBuilder();

			Element.WriteXmlFragment(e1, true, null, sb);
			Assert.AreEqual( "name=\"Test1\" nillable=\"True\" abstract=\"False\" tuple=\"False\" type=\"Test\" substitutionGroup=\"substGroup1\" periodType=\"duration\" balanceType=\"debit\" minOccurs=\"0\" maxOccurs=\""+Int32.MaxValue+"\"", sb.ToString() );

			sb.Remove(0, sb.Length);
			Element.WriteXmlFragment( null, true, null, sb );
			Assert.AreEqual( "name=\"\" nillable=\"False\" abstract=\"True\" tuple=\"False\" type=\"\" substitutionGroup=\"\" periodType=\"na\" balanceType=\"na\" minOccurs=\"0\" maxOccurs=\""+Int32.MaxValue+"\"", sb.ToString() );
		}

		/// <exclude/>
		[Test]
		public void ElementToXml()
		{
			Element p1 = new Element( "Parent1", "Parent1", "substGroup1" );
			p1.AddOptionals( "Test", true, Element.PeriodType.duration, Element.BalanceType.debit, false );

			Element c1 = new Element( "Child1", "Child1", "substGroup1" );
			c1.AddOptionals( "Test", true, Element.PeriodType.duration, Element.BalanceType.debit, false );
			
			p1.AddChild( c1 );
            p1.AddChildInfo(c1.Id, 1, 2);

			Element c2 = new Element( "Child2", "Child2", "substGroup1" );
			c2.AddOptionals( "Test", true, Element.PeriodType.duration, Element.BalanceType.debit, false );

			p1.AddChild( c2 );
            p1.AddChildInfo(c2.Id, 2, int.MaxValue);

			StringBuilder sb = new StringBuilder();

			p1.ToXmlString( 0, null, sb );

			string result = "<parentElement name=\"Parent1\" nillable=\"True\" abstract=\"False\" tuple=\"False\" type=\"Test\" substitutionGroup=\"substGroup1\" periodType=\"duration\" balanceType=\"debit\" minOccurs=\"0\" maxOccurs=\""+Int32.MaxValue+"\">\r\n" +
								  "\t<element name=\"Child1\" nillable=\"True\" abstract=\"False\" tuple=\"False\" type=\"Test\" substitutionGroup=\"substGroup1\" periodType=\"duration\" balanceType=\"debit\" minOccurs=\"1\" maxOccurs=\"2\"/>\r\n" +
								  "\t<element name=\"Child2\" nillable=\"True\" abstract=\"False\" tuple=\"False\" type=\"Test\" substitutionGroup=\"substGroup1\" periodType=\"duration\" balanceType=\"debit\" minOccurs=\"2\" maxOccurs=\""+Int32.MaxValue+"\"/>\r\n" +
								  "</parentElement>\r\n";
			
			Console.WriteLine( "*** Element.ToXmlString() ***" );		
			Console.WriteLine( sb.ToString() );
			Console.WriteLine( "*** Element.ToXmlString() End ***" );		

			Assert.AreEqual( result, sb.ToString() );

			XmlDocument doc = new XmlDocument();

			doc.LoadXml( result );
		}
		#endregion

		#region equals
		/// <exclude/>
		[Test]
		public void Test_ElementsEqual()
		{
			Element p1 = new Element( "p1_id", "parent_name", "subst1" );
			p1.AddOptionals( "type1", false, PeriodType.duration, BalanceType.credit, false );
			Element p2 = new Element( "p1_id", "parent_name", "subst1" );
			p2.AddOptionals( "type1", false, PeriodType.duration, BalanceType.credit, false );

			Assert.IsTrue( p1.Equals( p2 ), "p1 != P2" );

			Element p3 = new Element( "p3_id", "parent_name", "subst1" );
			p3.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );
			p3.AddChild( p1 );

			Element p4 = new Element( "p3_id", "parent_name", "subst1" );
			p4.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );
			p4.AddChild( p1 );

			Assert.IsTrue( p3.Equals( p4 ), "p3 != p4" );
		}

		/// <exclude/>
		[Test]
		public void Test_ElementsNotEqual()
		{
			Element p1 = new Element( "p1_id", "parent_name", "subst1" );
			Element p2 = new Element( "p2_id", "parent_name", "subst1" );

			Assert.IsFalse( p1.Equals( p2 ), "p1 == p2" );

			Element p3 = new Element( "p3_id", "parent_name", "subst1" );
			Element p4 = new Element( "p3_id", "parent_name2", "subst1" );

			Assert.IsFalse( p3.Equals( p4 ), "p3 == p4" );

			Element p5 = new Element( "p5_id", "parent_name", "subst1" );
			Element p6 = new Element( "p5_id", "parent_name", "subst2" );

			Assert.IsFalse( p5.Equals( p6 ), "p5 == p6" );

			Element p7 = new Element( "p7_id", "parent_name", "subst1" );
			p7.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );

			Element p8 = new Element( "p7_id", "parent_name", "subst1" );
			p8.AddOptionals( "type4", false, PeriodType.duration, BalanceType.credit, false );

			Assert.IsFalse( p7.Equals( p8 ), "p7 == p8" );

			Element p9 = new Element( "p9_id", "parent_name", "subst1" );
			p9.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );

			Element p10 = new Element( "p9_id", "parent_name", "subst1" );
			p10.AddOptionals( "type3", true, PeriodType.duration, BalanceType.credit, false );

			Assert.IsFalse( p9.Equals( p10 ), "p9 == p10" );

			Element p11 = new Element( "p11_id", "parent_name", "subst1" );
			p11.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );

			Element p12 = new Element( "p11_id", "parent_name", "subst1" );
			p12.AddOptionals( "type3", false, PeriodType.instant, BalanceType.credit, false );

			Assert.IsFalse( p11.Equals( p12 ), "p11 == p12" );

			Element p13 = new Element( "p13_id", "parent_name", "subst1" );
			p13.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );

			Element p14 = new Element( "p13_id", "parent_name", "subst1" );
			p14.AddOptionals( "type3", false, PeriodType.duration, BalanceType.debit, false );

			Assert.IsFalse( p13.Equals( p14 ), "p13 == p14" );

			Element p15 = new Element( "p15_id", "parent_name", "subst1" );
			p15.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, false );

			Element p16 = new Element( "p15_id", "parent_name", "subst1" );
			p16.AddOptionals( "type3", false, PeriodType.duration, BalanceType.credit, true );

			Assert.IsFalse( p15.Equals( p16 ), "p15 == p16" );

			Element p17 = new Element( "p17_id", "parent_name", "subst1" );
			p17.AddChild( p1 );

			Element p18 = new Element( "p17_id", "parent_name", "subst1" );

			Assert.IsFalse( p17.Equals( p18 ), "p17 == p18" );

			Element p19 = new Element( "p19_id", "parent_name", "subst1" );
			p19.AddChild( p1 );

			Element p20 = new Element( "p19_id", "parent_name", "subst1" );
			p20.AddChild( p2 );

			Assert.IsFalse( p19.Equals( p20 ), "p19 == p20" );

			// c1 is considered the same element even though it has different parents
			Element c1 = new Element( "c1" );
			Element p21 = new Element( "p21_id", "parent_name", "subst1" );
			p21.AddChild( c1 );

			Element c2 = new Element( "c1" );
			Element p22 = new Element( "p21_id", "parent_name", "subst1" );
			p22.AddChild( c2 );

			Assert.IsTrue( c1.Equals( c2 ), "c1 != c2" );
		}

		#endregion

		#region validation

		/// <exclude/>
		public void DoTryValidateInvalidElement(string markupData, ArrayList errors)
		{
			string error = null;
			object parsedObj = null;
			if( TryValidateElement(markupData, ref parsedObj, out error) )
			{
				errors.Add(ElementType);
			}
		}

		/// <exclude/>
		[Test]
		public void TryValidateElementNegative()
		{
			ArrayList errors = new ArrayList();

			#region markup values

			string theString = "theString";
			string theReturnString = "the\rString";
			string theLineFeedString = "the\nString";
			string theTabString = "the\tString";

			string theZero = "0";

			string thePositiveDecimal = decimal.MaxValue.ToString();
			string thePositiveDecimalPlusOne = thePositiveDecimal + "1";
			string theNegativeDecimal = decimal.MinValue.ToString();
			string theNegativeDecimalPlusOne = theNegativeDecimal + "1";

			string thePositiveDouble = "1.79769313486231E+308";
			string theNegativeDouble = "-1.79769313486231E+308";

			string thePositiveLong = long.MaxValue.ToString();
			string thePositiveLongPlusOne = thePositiveLong + "1";
			string theNegativeLong = long.MinValue.ToString();
			string theNegativeLongPlusOne = theNegativeLong + "1";

			string thePositiveULong = ulong.MaxValue.ToString();
			string thePositiveULongPlusOne = thePositiveULong + "1";

			string thePositiveInt = int.MaxValue.ToString();
			string theNegativeInt = int.MinValue.ToString();

			string thePositiveUInt = uint.MaxValue.ToString();
			string thePositiveUIntPlusOne = thePositiveUInt + "1";

			string thePositiveShort = short.MaxValue.ToString();
			string theNegativeShort = short.MinValue.ToString();

			string thePositiveUShort = ushort.MaxValue.ToString();
			string thePositiveUShortPlusOne = thePositiveUShort + "1";

			string theNegativeSByte = sbyte.MinValue.ToString();

			string thePositiveByte = byte.MaxValue.ToString();
			string thePositiveBytePlusOne = thePositiveByte + "1";

			#endregion

			#region misc

			type = string.Empty;
			DoTryValidateInvalidElement("invalid", errors);

			type = Element.INTEGER_ITEM_TYPE;
			DoTryValidateInvalidElement(null, errors);
			DoTryValidateInvalidElement(string.Empty, errors);

			#endregion

			#region numeric

			string [] decimalElements = new string[]
				{	Element.MONETARY,
					Element.SHARES,
					Element.PURE,
					Element.NONZERODECIMAL,
					Element.NONZERONONINFINITEFLOAT,
					Element.PRECISION_TYPE,
					Element.DECIMALS_TYPE,
					Element.DECIMAL_ITEM_TYPE,
					Element.MONETARY_ITEM_TYPE,
					Element.SHARES_ITEM_TYPE,
					Element.PURE_ITEM_TYPE,
					Element.FRACTION_ITEM_TYPE,
					Element.INTEGER_ITEM_TYPE,
					Element.NONPOSITIVEINTEGER_ITEM_TYPE,
					Element.NEGATIVEINTEGER_ITEM_TYPE,
					Element.NONNEGATIVEINTEGER_ITEM_TYPE,
					Element.POSITIVEINTEGER_ITEM_TYPE };

			foreach(string elementType in decimalElements)
			{
				type = elementType;
				DoTryValidateInvalidElement(theString, errors);
				DoTryValidateInvalidElement(thePositiveDecimalPlusOne, errors);
				DoTryValidateInvalidElement(theNegativeDecimalPlusOne, errors);
			}

			type = Element.NONZERODECIMAL;
			DoTryValidateInvalidElement(theZero, errors);

			type = Element.NONZERONONINFINITEFLOAT;
			DoTryValidateInvalidElement(theZero, errors);

			type = Element.PRECISION_TYPE;
			DoTryValidateInvalidElement(theNegativeDecimal, errors);

			type = Element.NONPOSITIVEINTEGER_ITEM_TYPE;
			DoTryValidateInvalidElement(thePositiveDecimal, errors);

			type = Element.NEGATIVEINTEGER_ITEM_TYPE;
			DoTryValidateInvalidElement(thePositiveDecimal, errors);
			DoTryValidateInvalidElement(theZero, errors);

			type = Element.NONNEGATIVEINTEGER_ITEM_TYPE;
			DoTryValidateInvalidElement(theNegativeDecimal, errors);

			type = Element.POSITIVEINTEGER_ITEM_TYPE;
			DoTryValidateInvalidElement(theNegativeDecimal, errors);
			DoTryValidateInvalidElement(theZero, errors);

			type = Element.FLOAT_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveDouble, errors);
			DoTryValidateInvalidElement(theNegativeDouble, errors);

			type = Element.DOUBLE_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			// Note: double.MaxValue.ToString() and double.MinValue.ToString()
			//  will not parse back to a double (b/c they're rounded string
			//  representations of double) so these should fail
			DoTryValidateInvalidElement(double.MaxValue.ToString(), errors);
			DoTryValidateInvalidElement(double.MinValue.ToString(), errors);

			type = Element.LONG_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveLongPlusOne, errors);
			DoTryValidateInvalidElement(theNegativeLongPlusOne, errors);

			type = Element.INT_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveLong, errors);
			DoTryValidateInvalidElement(theNegativeLong, errors);

			type = Element.SHORT_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			type = Element.BYTE_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveShort, errors);
			DoTryValidateInvalidElement(theNegativeShort, errors);

			type = Element.UNSIGNEDLONG_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveULongPlusOne, errors);
			DoTryValidateInvalidElement(theNegativeLong, errors);

			type = Element.UNSIGNEDINT_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveUIntPlusOne, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			type = Element.UNSIGNEDSHORT_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveUShortPlusOne, errors);
			DoTryValidateInvalidElement(theNegativeShort, errors);

			type = Element.UNSIGNEDBYTE_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveBytePlusOne, errors);
			DoTryValidateInvalidElement(theNegativeSByte, errors);

			#endregion

			#region nonNumeric

			// for string types - "almost anything is accepatable"
			//type = Element.STRING_ITEM_TYPE;
			//type = Element.HEXBINARY_ITEM_TYPE;
			//type = Element.BASE64BINARY_ITEM_TYPE;
			//type = Element.TOKEN_ITEM_TYPE;
			//type = Element.LANGUAGE_ITEM_TYPE;
			//type = Element.NAME_ITEM_TYPE;
			//type = Element.NCNAME_ITEM_TYPE;

			type = Element.ANYURI_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);

			type = Element.NORMALIZEDSTRING_ITEM_TYPE;
			DoTryValidateInvalidElement(theReturnString, errors);
			DoTryValidateInvalidElement(theLineFeedString, errors);
			DoTryValidateInvalidElement(theTabString, errors);

//			type = Element.BOOLEAN_ITEM_TYPE;
//			DoTryValidateInvalidElement(theString, errors);
//			DoTryValidateInvalidElement(thePositiveInt, errors);
//			DoTryValidateInvalidElement(theNegativeInt, errors);

			// valid: PnYnMnDTnHnMnS
			type = Element.DURATION_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("1Y", errors);// missing 'P' designator
			DoTryValidateInvalidElement("P1AY", errors);// invalid token
			DoTryValidateInvalidElement("P1S", errors); // missing 'T' delimiter
			DoTryValidateInvalidElement("P1Y2M3M", errors);// missing 'T' delimiter (minutes is special case)
			DoTryValidateInvalidElement("P1Y2MT", errors);// missing time elements when 'T' is present

			// valid: CCYY-MM-DDThh:mm:ss
			type = Element.DATETIME_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("20044-12-31T12:33:33+03:00", errors);// bad year
			DoTryValidateInvalidElement("20044-12-31T", errors);// missing time elements when 'T' is present
			DoTryValidateInvalidElement("2004-12-31T25:33:33", errors);// bad time

			type = Element.TIME_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("T", errors);// missing time elements when 'T' is present
			DoTryValidateInvalidElement("T25:33:33", errors);// bad time

			type = Element.DATE_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("20044-12-31", errors);// bad year

			type = Element.GYEARMONTH_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("20044-12", errors);// bad year
			DoTryValidateInvalidElement("2004-13", errors);// bad month

			type = Element.GYEAR_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement("-20044", errors);

			type = Element.GMONTHDAY_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("--13-25", errors);// bad month
			DoTryValidateInvalidElement("--12-32", errors);// bad day

			type = Element.GDAY_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);

			DoTryValidateInvalidElement("---32", errors); // invalid day
			DoTryValidateInvalidElement("+03:00", errors); // missing day but includes timezone

			type = Element.GMONTH_ITEM_TYPE;
			DoTryValidateInvalidElement(theString, errors);

			DoTryValidateInvalidElement("--13--", errors); // invalid month
			DoTryValidateInvalidElement("+03:00", errors); // missing month but includes timezone

			type = Element.DATEUNION;
			DoTryValidateInvalidElement(theString, errors);
			DoTryValidateInvalidElement(thePositiveInt, errors);
			DoTryValidateInvalidElement(theZero, errors);
			DoTryValidateInvalidElement(theNegativeInt, errors);

			DoTryValidateInvalidElement("20044-12-31T12:33:33+03:00", errors);// bad year
			DoTryValidateInvalidElement("20044-12-31T", errors);// missing time elements when 'T' is present
			DoTryValidateInvalidElement("2004-12-31T25:33:33", errors);// bad time

			#endregion

			if(errors.Count > 0)
			{
				StringBuilder errorMessage = new StringBuilder();
				errorMessage.Append(Environment.NewLine);
				errorMessage.Append("Failed to correctly validate the following element type(s):");
				errorMessage.Append(Environment.NewLine);
				foreach(string error in errors)
					errorMessage.Append("\t\t").Append(error).Append(Environment.NewLine);

				Assert.Fail( errorMessage.ToString() );
			}
		}

		/// <exclude/>
		public void DoTryValidateValidElement(string markupData, ArrayList errors)
		{
			string error = null;
			object parsedObj = null;
			if( !TryValidateElement(markupData, ref parsedObj, out error) )
			{
				errors.Add(string.Format("Element Type: {0}, Error: {1} Failed value: {2}", ElementType, error, markupData) );
			}
		}

		/// <exclude/>
		[Test]
		public void TryValidateElementPositive()
		{
			ArrayList errors = new ArrayList();

			#region markup values

			string theString = "theString";
			string theHttpString = "http:\\www.yahoo.com";
			string theWwwString = "www.yahoo.com";

			string theZero = "0";

			string thePositiveDecimal = decimal.MaxValue.ToString();
			string theNegativeDecimal = decimal.MinValue.ToString();

			string thePositiveFloat = float.MaxValue.ToString();
			string theNegativeFloat = float.MinValue.ToString();

			// Note: double.MaxValue.ToString() and double.MinValue.ToString()
			//  will not parse back to a double (b/c they're rounded string
			//  representations of double)
			string thePositiveDouble = "1.79769313486231E+308";
			string theNegativeDouble = "-1.79769313486231E+308";

			string thePositiveLong = long.MaxValue.ToString();
			string theNegativeLong = long.MinValue.ToString();

			string thePositiveInt = int.MaxValue.ToString();
			string theNegativeInt = int.MinValue.ToString();

			string thePositiveShort = short.MaxValue.ToString();
			string theNegativeShort = short.MinValue.ToString();

			string thePositiveSByte = sbyte.MaxValue.ToString();
			string theNegativeSByte = sbyte.MinValue.ToString();

			string thePositiveULong = ulong.MaxValue.ToString();
			string theNegativeULong = ulong.MinValue.ToString();

			string thePositiveUInt = uint.MaxValue.ToString();
			string theNegativeUInt = uint.MinValue.ToString();

			string thePositiveUShort = ushort.MaxValue.ToString();
			string theNegativeUShort = ushort.MinValue.ToString();

			string thePositiveByte = byte.MaxValue.ToString();
			string theNegativeByte = byte.MinValue.ToString();

			#endregion

			#region misc

			type = "some_type";
			DoTryValidateValidElement("some_value", errors);

			type = Element.STRING_ITEM_TYPE;
			DoTryValidateValidElement(string.Empty, errors);

			#endregion

			#region numeric

		string [] decimalElements = new string[]
				{	Element.MONETARY,
					Element.SHARES,
					Element.PURE,
					Element.DECIMALS_TYPE,
					Element.DECIMAL_ITEM_TYPE,
					Element.MONETARY_ITEM_TYPE,
					Element.SHARES_ITEM_TYPE,
					Element.PURE_ITEM_TYPE,
					Element.FRACTION_ITEM_TYPE
					};

			foreach(string elementType in decimalElements)
			{
				type = elementType;
				DoTryValidateValidElement(theZero, errors);
				DoTryValidateValidElement(thePositiveDecimal, errors);
				DoTryValidateValidElement(theNegativeDecimal, errors);
			}

			type = Element.NONZERODECIMAL;
			DoTryValidateValidElement(thePositiveDecimal, errors);
			DoTryValidateValidElement(theNegativeDecimal, errors);

			type = Element.NONZERONONINFINITEFLOAT;
			DoTryValidateValidElement(thePositiveDecimal, errors);
			DoTryValidateValidElement(theNegativeDecimal, errors);

			type = Element.PRECISION_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveDecimal, errors);

			type = Element.NONPOSITIVEINTEGER_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(theNegativeInt, errors);

			type = Element.NEGATIVEINTEGER_ITEM_TYPE;
			DoTryValidateValidElement(theNegativeInt, errors);

			type = Element.NONNEGATIVEINTEGER_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveInt, errors);

			type = Element.POSITIVEINTEGER_ITEM_TYPE;
			DoTryValidateValidElement(thePositiveInt, errors);

			type = Element.FLOAT_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveFloat, errors);
			DoTryValidateValidElement(theNegativeFloat, errors);

			type = Element.DOUBLE_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveDouble, errors);
			DoTryValidateValidElement(theNegativeDouble, errors);

			type = Element.LONG_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveLong, errors);
			DoTryValidateValidElement(theNegativeLong, errors);

			type = Element.INT_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveInt, errors);
			DoTryValidateValidElement(theNegativeInt, errors);

			type = Element.SHORT_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveShort, errors);
			DoTryValidateValidElement(theNegativeShort, errors);

			type = Element.BYTE_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveSByte, errors);
			DoTryValidateValidElement(theNegativeSByte, errors);

 			type = Element.UNSIGNEDLONG_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveULong, errors);
			DoTryValidateValidElement(theNegativeULong, errors);

			type = Element.UNSIGNEDINT_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveUInt, errors);
			DoTryValidateValidElement(theNegativeUInt, errors);

			type = Element.UNSIGNEDSHORT_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveUShort, errors);
			DoTryValidateValidElement(theNegativeUShort, errors);

			type = Element.UNSIGNEDBYTE_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement(thePositiveByte, errors);
			DoTryValidateValidElement(theNegativeByte, errors);

			#endregion

			#region nonNumeric

			string [] stringElements = new string[]
				{	Element.STRING_ITEM_TYPE,
					Element.HEXBINARY_ITEM_TYPE,
					Element.BASE64BINARY_ITEM_TYPE,
					Element.NORMALIZEDSTRING_ITEM_TYPE,
					Element.TOKEN_ITEM_TYPE,
					Element.LANGUAGE_ITEM_TYPE,
					Element.NAME_ITEM_TYPE,
					Element.NCNAME_ITEM_TYPE };

			foreach(string elementType in stringElements)
			{
				type = elementType;
				DoTryValidateValidElement(theString, errors);
			}

			type = Element.ANYURI_ITEM_TYPE;
			DoTryValidateValidElement(theHttpString, errors);
			DoTryValidateValidElement(theWwwString, errors);

			type = Element.BOOLEAN_ITEM_TYPE;
			DoTryValidateValidElement(theZero, errors);
			DoTryValidateValidElement("1", errors);
			DoTryValidateValidElement("true", errors);
			DoTryValidateValidElement("false", errors);

			// PnYnMnDTnHnMnS

			type = Element.DURATION_ITEM_TYPE;

			DoTryValidateValidElement("P1Y", errors);
			DoTryValidateValidElement("P1M", errors);
			DoTryValidateValidElement("P1D", errors);
			DoTryValidateValidElement("P1Y1M1D", errors);
			DoTryValidateValidElement("-P1Y1M1D", errors);

			DoTryValidateValidElement("PT1H", errors);
			DoTryValidateValidElement("PT1M", errors);
			DoTryValidateValidElement("PT1S", errors);
			DoTryValidateValidElement("PT2.222S", errors);
			DoTryValidateValidElement("-PT1H1M2.222S", errors);

			DoTryValidateValidElement("P1Y1M1DT1H1M2.222S", errors);
			DoTryValidateValidElement("-P1Y1M1DT1H1M2.222S", errors);

			// CCYY-MM-DDThh:mm:ss

			type = Element.DATETIME_ITEM_TYPE;
			DoTryValidateValidElement("0001-01-01T12:33:33", errors);
			DoTryValidateValidElement("9999-12-31T12:33:33", errors);
			DoTryValidateValidElement("2004-12-31T12:33:33+03:00", errors);
			DoTryValidateValidElement("2004-12-31T12:33:33-03:00", errors);

			type = Element.TIME_ITEM_TYPE;
			DoTryValidateValidElement("12:33:33", errors);
			DoTryValidateValidElement("12:33:33+03:00", errors);
			DoTryValidateValidElement("12:33:33-03:00", errors);

			type = Element.DATE_ITEM_TYPE;
			DoTryValidateValidElement("0001-01-01", errors);
			DoTryValidateValidElement("9999-12-31", errors);

			type = Element.GYEARMONTH_ITEM_TYPE;
			DoTryValidateValidElement("0001-01", errors);
			DoTryValidateValidElement("9999-12", errors);
			DoTryValidateValidElement("9999-12+03:00", errors);
			DoTryValidateValidElement("9999-12-03:00", errors);

			type = Element.GYEAR_ITEM_TYPE;
			DoTryValidateValidElement("0001", errors);
			DoTryValidateValidElement("9999", errors);

			type = Element.GMONTHDAY_ITEM_TYPE;
			DoTryValidateValidElement("--01-01", errors);
			DoTryValidateValidElement("--12-31", errors);
			DoTryValidateValidElement("--12-31+03:00", errors);
			DoTryValidateValidElement("--12-31-03:00", errors);

			type = Element.GDAY_ITEM_TYPE;
			DoTryValidateValidElement("01", errors);
			DoTryValidateValidElement("31", errors);

			type = Element.GMONTH_ITEM_TYPE;
			DoTryValidateValidElement("01", errors);
			DoTryValidateValidElement("12", errors);

			type = Element.DATEUNION;
			DoTryValidateValidElement("0001-01-01T12:33:33", errors);
			DoTryValidateValidElement("9999-12-31T12:33:33", errors);
			DoTryValidateValidElement("2004-12-31T12:33:33+03:00", errors);
			DoTryValidateValidElement("2004-12-31T12:33:33-03:00", errors);

			#endregion

			if(errors.Count > 0)
			{
				StringBuilder errorMessage = new StringBuilder();
				errorMessage.Append(Environment.NewLine);
				errorMessage.Append("Failed to correctly validate the following element type(s):");
				errorMessage.Append(Environment.NewLine);
				foreach(string error in errors)
					errorMessage.Append("\t\t").Append(error).Append(Environment.NewLine);

				Assert.Fail( errorMessage.ToString() );
			}
		}

		#endregion

		#region tuple support
		/// <exclude/>
		[Test]
		public void CreateTupleParent()
		{
			Element e = Element.CreateTupleParent( "tupleParent" );
			Assert.IsTrue( e.IsTuple, "tuple not created correctly" );
			//Assert.AreEqual( 1, e.TupleParent, "tuple parent not created correctly" );
		}

		/// <exclude/>
		[Test]
		public void CreateTupleChild()
		{
			Element e = Element.CreateTupleParent( "tupleParent" );
			Assert.IsTrue( e.IsTuple, "tuple not created correctly" );
			//Assert.AreEqual( 1, e.TupleParent, "tuple parent not created correctly" );

			Element c = Element.CreateMonetaryElement( "child", false, BalanceType.credit, PeriodType.duration );

			e.AddChild( c );

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement tupParent = doc.CreateElement( "element" );
			e.WriteXml( "prefix", tupParent, doc );
			root.AppendChild( tupParent );

			c.WriteTupleChildXml(e , "prefix", tupParent, doc, null, false );
			

			System.IO.StringWriter xml = new System.IO.StringWriter();

			doc.Save( xml );

			Console.WriteLine( xml.ToString() );

			string expectedXml = 
@"<?xml version=""1.0"" encoding=""utf-16""?>
<root>
  <element id=""prefix_tupleParent"" name=""tupleParent"" substitutionGroup=""xbrli:tuple"" nillable=""true"">
    <complexType>
      <complexContent>
        <restriction base=""anyType"">
          <sequence>
            <element ref=""prefix:child"" minOccurs=""0"" maxOccurs=""unbounded"" />
          </sequence>
          <attribute name=""id"" type=""ID"" use=""optional"" />
        </restriction>
      </complexContent>
    </complexType>
  </element>
</root>";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString() );
		}

		/// <exclude/>
		[Test]
		public void CreateTupleChildren()
		{
			Element e = Element.CreateTupleParent( "tupleParent" );
			Assert.IsTrue( e.IsTuple, "tuple not created correctly" );
			//Assert.AreEqual( 1, e.TupleParent, "tuple parent not created correctly" );

			Element c = Element.CreateMonetaryElement( "child", false, BalanceType.credit, PeriodType.duration );
			Element d = Element.CreateMonetaryElement( "child2", false, BalanceType.credit, PeriodType.duration );

			e.AddChild( c );
			e.AddChild( d );

			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );

			XmlElement tupParent = doc.CreateElement( "element" );
			e.WriteXml( "prefix", tupParent, doc );
			root.AppendChild( tupParent );

			c.WriteTupleChildXml( e, "prefix", tupParent, doc, null, false );
			d.WriteTupleChildXml( e,"prefix", tupParent, doc, null, false );
			

			System.IO.StringWriter xml = new System.IO.StringWriter();

			doc.Save( xml );

			Console.WriteLine( xml.ToString() );

			string expectedXml = 
				@"<?xml version=""1.0"" encoding=""utf-16""?>
<root>
  <element id=""prefix_tupleParent"" name=""tupleParent"" substitutionGroup=""xbrli:tuple"" nillable=""true"">
    <complexType>
      <complexContent>
        <restriction base=""anyType"">
          <sequence>
            <element ref=""prefix:child"" minOccurs=""0"" maxOccurs=""unbounded"" />
            <element ref=""prefix:child2"" minOccurs=""0"" maxOccurs=""unbounded"" />
          </sequence>
          <attribute name=""id"" type=""ID"" use=""optional"" />
        </restriction>
      </complexContent>
    </complexType>
  </element>
</root>";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString() );
		}
		#endregion

		#region WriteXml for element types
		/// <exclude/>
		[Test]
		public void Test_WriteXmlNormalizedString()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.NormalizedString, "elem_normalizedString", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Normalized String element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_normalizedString"" name=""elem_normalizedString"" type=""xbrli:normalizedStringItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlDateTime()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.DateTime, "elem_DateTime", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"DateTime element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_DateTime"" name=""elem_DateTime"" type=""xbrli:dateTimeItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlDate()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.Date, "elem_Date", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Date element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_Date"" name=""elem_Date"" type=""xbrli:dateItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlDecimal()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.Decimal, "elem_Decimal", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Decimal element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_Decimal"" name=""elem_Decimal"" type=""xbrli:decimalItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlPure()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.Pure, "elem_Pure", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Pure element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_Pure"" name=""elem_Pure"" type=""xbrli:pureItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlShares()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.Shares, "elem_Shares", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Shares element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_Shares"" name=""elem_Shares"" type=""xbrli:sharesItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlPositiveInteger()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.PositiveInteger, "elem_PositiveInteger", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"Positive Integer element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_PositiveInteger"" name=""elem_PositiveInteger"" type=""xbrli:positiveIntegerItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		/// <exclude/>
		[Test]
		public void Test_WriteXmlanyURI()
		{
			Element e = Element.CreateElement( Element.DataTypeCode.anyURI, "elem_anyURI", true, Element.PeriodType.duration );

			Assert.IsNotNull(e,"anyURI element not created");

			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement( "root" );
			doc.AppendChild( root );
            
			e.WriteXml("prefix",root, doc);

			System.IO.StringWriter xml = new System.IO.StringWriter();
			doc.Save( xml );
			Console.WriteLine( xml.ToString() );

			string expectedXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<root id=""prefix_elem_anyURI"" name=""elem_anyURI"" type=""xbrli:anyURIItemType"" substitutionGroup=""xbrli:item"" nillable=""true"" xbrli:periodType=""duration"" xmlns:xbrli=""http://www.xbrl.org/2003/instance"" />";

			Assert.AreEqual( string.Format( expectedXml ), xml.ToString(),"Invalid Normalized String created." );
		}

		#endregion

		#region tuple order
		/// <exclude/>
		[Test]
		public void Test_GetNewFirstTupleOrder_NoElements()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			
			Assert.AreEqual( 1.0, parent.GetNewFirstTupleOrder(), "wrong order returned" );
		}

		/// <exclude/>
		[Test]
		public void Test_GetNewFirstTupleOrder_WithElements()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			parent.AddChild( child );
			
			Assert.AreEqual( 0.5, parent.GetNewFirstTupleOrder(), "wrong order returned" );
		}

		/// <exclude/>
		[Test]
		public void Test_GetTupleOrderBeforeFirst()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			parent.AddChild( child );

			Assert.AreEqual( 0.5, parent.GetNewTupleOrderForBefore( child ) );
		}

		/// <exclude/>
		[Test]
		public void Test_GetTupleOrderBeforeThird()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child1 = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			Element child2 = Element.CreateElement( DataTypeCode.String, "child2", true, PeriodType.duration );
			Element child3 = Element.CreateElement( DataTypeCode.String, "child3", true, PeriodType.duration );
			parent.AddChild( child1 );
			parent.AddChild( child2 );
			parent.AddChild( child3 );

			Assert.AreEqual( 2.5, parent.GetNewTupleOrderForBefore( child3 ) );
		}

		/// <exclude/>
		[Test]
		public void Test_GetTupleOrderAfterFirst()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			parent.AddChild( child );

			Assert.AreEqual( 2.0, parent.GetNewTupleOrderForAfter( child ) );
		}

		/// <exclude/>
		[Test]
		public void Test_GetTupleOrderAfterSecond()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child1 = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			Element child2 = Element.CreateElement( DataTypeCode.String, "child2", true, PeriodType.duration );
			Element child3 = Element.CreateElement( DataTypeCode.String, "child3", true, PeriodType.duration );
			parent.AddChild( child1 );
			parent.AddChild( child2 );
			parent.AddChild( child3 );

			Assert.AreEqual( 2.5, parent.GetNewTupleOrderForAfter( child2 ) );
		}

		/// <exclude/>
		[Test]
		public void Test_GetTupleOrderAfterThird()
		{
			Element parent = Element.CreateTupleParent( "parent" );
			Element child1 = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			Element child2 = Element.CreateElement( DataTypeCode.String, "child2", true, PeriodType.duration );
			Element child3 = Element.CreateElement( DataTypeCode.String, "child3", true, PeriodType.duration );
			parent.AddChild( child1 );
			parent.AddChild( child2 );
			parent.AddChild( child3 );

			Assert.AreEqual( 4, parent.GetNewTupleOrderForAfter( child3 ) );
		}
		#endregion

		/// <exclude/>
		[Test]
		public void Test_GermanNumbers()
		{
			CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentUICulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
			
			object parsedObj = decimal.Parse( "12.345,67", NumberStyles.Currency, CultureInfo.CurrentUICulture);

			System.Threading.Thread.CurrentThread.CurrentUICulture = ci;
		}

		

		

		/// <exclude/>
		[Test]
		public void TestCheckIsNumeric()
		{
			Assert.IsTrue( Element.CheckIsNumeric( DataTypeCode.Monetary ), "Monetary should be a numeric" );
			Assert.IsTrue( Element.CheckIsNumeric( DataTypeCode.Integer ), "Integer should be a numeric" );

			Assert.IsFalse( Element.CheckIsNumeric( DataTypeCode.String ), "String is not numeric" );
			Assert.IsFalse( Element.CheckIsNumeric( DataTypeCode.YearMonth ), "Year Month is not numeric" );
		}


		/// <summary>
		/// Test that our "IsTextBlock" method correctly identifies a "Native" textblock, or any of the mistyped ones
		/// </summary>
		[Test]
		public void TestCheckIsTextBlock()
		{
			//The original type is wrong, but the element name is right
			Element child1 = Element.CreateElement( DataTypeCode.String, "us-gaap_MortgageLoansOnRealEstateWriteDownOrReserveDisclosureTextBlock", true, PeriodType.duration );
			child1.OrigElementType = "xbrli:stringItemType";

			bool test1 = child1.IsTextBlock();
			Assert.IsTrue( test1 );

			//The original type is right
			child1 = Element.CreateElement( DataTypeCode.String, "child1", true, PeriodType.duration );
			child1.OrigElementType = TEXT_BLOCK_ITEM_TYPE;

			bool test2 = child1.IsTextBlock();
			Assert.IsTrue( test2 );

			//Close, but not the same - should be false
			child1 = Element.CreateElement( DataTypeCode.String, "us-gaap_MortgageLoansOnRealEstateWriteDownOrReserveDisclosureTextBloc", true, PeriodType.duration );
			child1.OrigElementType = "xbrli:stringItemType";
			bool test3 = child1.IsTextBlock();
			Assert.IsFalse( test3 );

			//Not even close...
			child1 = Element.CreateElement( DataTypeCode.String, "us-gaap", true, PeriodType.duration );
			child1.OrigElementType = "xbrli:positiveIntegerItemType";
			bool test4 = child1.IsTextBlock();
			Assert.IsFalse( test4 );


			//Check a real element
			string fileName = "http://xbrl.us/us-gaap/1.0/ind/ci/us-gaap-ci-stm-2008-03-31.xsd";
			int errors = 0;
			Taxonomy s = new Taxonomy();
			if( s.Load( fileName, out errors ) != true )
			{
				Assert.Fail( (string)s.ErrorList[ 0 ] );
			}

			s.Parse( out errors );

			Element child2 = s.allElements[ "us-gaap_MortgageLoansOnRealEstateWriteDownOrReserveDisclosureTextBlock" ] as Element;
			bool test5 = child2.IsTextBlock();
			Assert.IsTrue( test5 );

			child2 = s.allElements[ "us-gaap_AccountingChangesAndErrorCorrectionsTextBlock" ] as Element;
			bool test6 = child2.IsTextBlock();
			Assert.IsTrue( test6 );


			child2 = s.allElements["us-gaap_BankruptcyClaimsNumberClaimsFiled"] as Element;

			object parsed = new object();
			string err;
			Assert.IsTrue(child2.TryValidateElement("1,000", ref parsed, out err),
				"Should be valid");
		}
	}
}
#endif