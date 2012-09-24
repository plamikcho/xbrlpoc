<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:decimal-format name="currency" digit="D" />
  <xsl:output method="html" omit-xml-declaration="yes" />
  <xsl:preserve-space elements="label" />

  <xsl:template match="/">
    <html>
      <head>
        <title></title>
      </head>
      <body>
        <xsl:call-template name="KMFDM">
          <xsl:with-param name="counter" select="1" />
        </xsl:call-template>
      </body>
    </html>
  </xsl:template>

  <!--<xsl:template name="header" match="factTable">
    <xsl:for-each select="*[not(child::*)]">
      <xsl:value-of select="name()"></xsl:value-of>
      : <xsl:value-of select="."/><br/>
    </xsl:for-each>       
  </xsl:template>-->
  
  <xsl:variable name="rating" select="normalize-space(rating)" />
  <xsl:param name="linkRole" select="'http://www.berkshirehathaway.com/2010-12-31/role/DocumentAndEntityInformation)'"></xsl:param>
  
  <xsl:template match="/">
    <html>
      <head>
        <title></title>
      </head>
      <body>

        <xsl:call-template name="r"/>
        <xsl:call-template name="link"></xsl:call-template>
        <xsl:call-template name="KMFDM">
          <xsl:with-param name="counter" select="1" />
        </xsl:call-template>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="r" match="@* | node()">
    
    <xsl:choose>
      <xsl:when test="*[not(child::*)]">
        <xsl:value-of select="."/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="@*='concept'">
            <xsl:value-of select="@*"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="name()"/>
          </xsl:otherwise>
        </xsl:choose>        
        
      </xsl:otherwise>
    </xsl:choose>
    
    <xsl:copy>
      <xsl:apply-templates select="@* | node()">        
      </xsl:apply-templates>
    </xsl:copy>
  </xsl:template>
  
  <xsl:template name="link">
    <xsl:for-each select="factTable/linkRole">
      test
      <xsl:call-template name="linkRole">
        <xsl:with-param name="concept" select="facts[@concept]"/>
      </xsl:call-template>    
    </xsl:for-each>
    
  </xsl:template>

  <xsl:template name="linkRole">
    <xsl:param name="concept"/>
    <xsl:if test="facts[@concept]">
      <xsl:value-of select="@concept"/>-
      
      <!--<xsl:call-template name="linkRole">
        <xsl:with-param name="concept" select="$concept"/>
      </xsl:call-template>-->
    </xsl:if>
  </xsl:template>
  
  <xsl:template name="KMFDM">
    <xsl:param name="counter" />
    <xsl:if test="$counter &lt; 11">
      <xsl:choose>
        <xsl:when test="$counter = $rating">
          <b>
            <xsl:value-of select="$counter" />
          </b>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$counter" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text> </xsl:text>
      <xsl:call-template name="KMFDM">
        <xsl:with-param name="counter" select="$counter + 1" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>


</xsl:stylesheet>
