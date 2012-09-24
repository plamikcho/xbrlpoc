<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:param name="renderedHtmlFolder" select=" '__RENDERED_HTML_FOLDER__' "/>
  <xsl:decimal-format name="currency" digit="D" />
  <xsl:output method="html" omit-xml-declaration="yes" />
  <xsl:preserve-space elements="label" />

  <xsl:template match="/">
    <html>
      <head>
        <style type="text/css" media="all">
          table
          {
            width: 60%;
          }
          td
          {
            padding: 2px;
            word-break: break-all;
          }

          th
          {
            text-align: center;
            padding: 2px;
            background-color: #b6b7bc;
            max-width: 150px;
          }
          td
          {
            max-width: 250px;
          }

        </style>
        <script type="text/javascript">
        <![CDATA[
      
          function hiLiteRows() {
            var table = document.getElementById('tableReports');
            for (var i = 0; i < table.rows.length; i++) {            
                if(table.rows[i].firstChild.nodeName.toLowerCase() !== "th") {
                  table.rows[i].onmouseover = function () {
                      this.origColor = this.style.backgroundColor;
                      this.style.backgroundColor = '#BCD4EC';
                  }
                  table.rows[i].onmouseout = function () { this.style.backgroundColor = this.origColor; }
                }
            }
        }
      
        function OpenLink(reportLink) {
          window.open(reportLink,'','location=1,status=1,scrollbars=1,resizable=1,width=800,height=600');
        }
        
        ]]>
        </script>
      </head>
      <body>
        <xsl:apply-templates select="FilingSummary" >
        </xsl:apply-templates>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="header" match="FilingSummary">
    <xsl:for-each select="*[not(child::*)]">
      <xsl:value-of select="name()"></xsl:value-of>
      : <xsl:value-of select="."/><br/>
    </xsl:for-each>       
  </xsl:template>
  
  <xsl:template name="inputfiles" match="InputFiles">
    <xsl:for-each select="InputFiles/File">
      <xsl:value-of select="."/>      
      <br/>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template match="FilingSummary">
    
    <table border="0">
      <tbody>
        <tr>
          <th>Instance statistics</th>
          <th>Input files</th>
        </tr>
        <tr valign="top">
          <td>
            <xsl:call-template name="header"/>
          </td>
          <td>
            <xsl:call-template name="inputfiles"/>
          </td>
        </tr>
      </tbody>
    </table>
    <br/>
    <br/>
    
    <table border="1" id="tableReports">
      <tbody>
        <tr>
          <th>Report name / Role</th>
          <th>Html file name</th>
        </tr>
          <xsl:for-each select="MyReports/Report">
            <xsl:choose>
              <xsl:when test="HtmlFileName">                
                <tr>
                  <xsl:attribute name="onclick">
                    <xsl:text>OpenLink('</xsl:text>
                    <xsl:value-of select="$renderedHtmlFolder"/>
                    <xsl:value-of select="HtmlFileName" />
                    <xsl:text>');</xsl:text>
                  </xsl:attribute>
                  <xsl:attribute name="style">
                    <xsl:text>cursor:pointer</xsl:text>
                  </xsl:attribute>
                  <td>
                    <b><xsl:value-of select="ShortName"/></b>                    
                  </td>
                  <td>
                    <a>
                      <xsl:value-of select="HtmlFileName"/>
                    </a>
                  </td>
                </tr>
              </xsl:when>
              <xsl:otherwise>

              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>        
      </tbody>
    </table>
    <script type="text/javascript">
        <![CDATA[        
            hiLiteRows();        
        ]]>
    </script>
  </xsl:template>
</xsl:stylesheet>
