<!-- summary.xsl   Scott Bronson   11 Oct 2002 -->

<!-- This outputs a summary of the class. -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:include href="doc.xsl"/>

<xsl:param name="l"/>
<xsl:param name="columns" select="'3'"/>  <!-- optional -->


<xsl:variable name="fulltype" select="/Type/@FullName"/>
<xsl:variable name="assembly" select="/Type/AssemblyInfo/AssemblyName"/>

<!-- this is to try to keep the doc/paramref template happy -->
<xsl:variable name="member" select="''"/>


<xsl:template match="/Type">
<html>
<head>
  <title><xsl:value-of select="@Name"/> Summary</title>
</head>
<body>

<center>
<h2>
<xsl:call-template name="print-hierarchy">
  <xsl:with-param name="ns">
    <xsl:call-template name="base-name">
      <xsl:with-param name="string" select="$fulltype"/>
    </xsl:call-template>
  </xsl:with-param>
</xsl:call-template>
<b><xsl:value-of select="@Name"/></b>
</h2>
</center>

<h4>Summary:</h4>
<xsl:apply-templates select="Docs/summary"/>
<xsl:apply-templates select="Base"/>
<p>
<xsl:if test="count(Interfaces/Interface)>0">
  <xsl:text>Implements </xsl:text>
  <xsl:apply-templates select="Interfaces"/>
</xsl:if>
</p>


<blockquote>
<table>

<xsl:call-template name="members-by-type">
  <xsl:with-param name="type" select="'Field'"/>
  <xsl:with-param name="singularReadableType" select="'Field'"/>
  <xsl:with-param name="pluralReadableType" select="'Fields'"/>
</xsl:call-template>

<xsl:call-template name="members-by-type">
  <xsl:with-param name="type" select="'Property'"/>
  <xsl:with-param name="singularReadableType" select="'Property'"/>
  <xsl:with-param name="pluralReadableType" select="'Properties'"/>
</xsl:call-template>

<xsl:call-template name="members-by-type">
  <xsl:with-param name="type" select="'Event'"/>
  <xsl:with-param name="singularReadableType" select="'Event'"/>
  <xsl:with-param name="pluralReadableType" select="'Events'"/>
</xsl:call-template>

<xsl:call-template name="members-by-type">
  <xsl:with-param name="type" select="'Constructor'"/>
  <xsl:with-param name="singularReadableType" select="'Constructor'"/>
  <xsl:with-param name="pluralReadableType" select="'Constructors'"/>
</xsl:call-template>

<xsl:call-template name="members-by-type">
  <xsl:with-param name="type" select="'Method'"/>
  <xsl:with-param name="singularReadableType" select="'Method'"/>
  <xsl:with-param name="pluralReadableType" select="'Methods'"/>
</xsl:call-template>

<tr><td colspan="2"><hr></hr></td></tr>

<xsl:call-template name="members-by-access">
  <xsl:with-param name="access" select="'public'"/>
  <xsl:with-param name="readableAccess" select="'Public'"/>
</xsl:call-template>

<xsl:call-template name="members-by-access">
  <xsl:with-param name="access" select="'protected'"/>
  <xsl:with-param name="readableAccess" select="'Protected'"/>
</xsl:call-template>

<xsl:call-template name="members-by-access">
  <xsl:with-param name="access" select="'private'"/>
  <xsl:with-param name="readableAccess" select="'Private'"/>
</xsl:call-template>

<xsl:call-template name="members-by-access">
  <xsl:with-param name="access" select="'internal'"/>
  <xsl:with-param name="readableAccess" select="'Internal'"/>
</xsl:call-template>


<tr><td colspan="2"><hr></hr></td></tr>

<xsl:call-template name="members-by-name"/>

</table>
</blockquote>


<h4>Thread Safety:</h4>
<p><xsl:apply-templates select="ThreadingSafetyStatement"/></p>


<h4>Remarks:</h4>
<xsl:apply-templates select="Docs/remarks"/>

</body>
</html>
</xsl:template>


<xsl:template match="Docs/summary">
  <xsl:apply-templates mode="doc"/>
</xsl:template>


<xsl:template match="Base">
  <xsl:choose>
    <xsl:when test="./ExcludedBaseTypeName">
      <p>Inherits from
        <xsl:call-template name="full-type-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="./ExcludedBaseTypeName"/>
        </xsl:call-template>
        if the <xsl:value-of select="./ExcludedLibraryName"/>
        library is installed.  Otherwise, inherits from
        <xsl:call-template name="full-type-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="./BaseTypeName"/>
        </xsl:call-template><xsl:text>.</xsl:text>
      </p>
    </xsl:when>
    <xsl:when test="./BaseTypeName">
      <p>Inherits from 
        <xsl:call-template name="full-type-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="./BaseTypeName"/>
        </xsl:call-template><xsl:text>.</xsl:text>
      </p>
    </xsl:when>
    <xsl:otherwise>
      <p>Does not inherit from any other classes.</p>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template match="Interfaces">
  <xsl:for-each select="Interface">
    <xsl:sort select="InterfaceName"/>

    <xsl:call-template name="full-summary-link">
      <xsl:with-param name="nsroot" select="$nsroot"/>
      <xsl:with-param name="fulltype" select="InterfaceName"/>
    </xsl:call-template>

    <xsl:choose>
      <xsl:when test="position()=last()">
        <xsl:text>.</xsl:text>
      </xsl:when>
      <xsl:when test="position()+1=last()">
        <xsl:text> and </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>, </xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:for-each>
</xsl:template>


<xsl:template name="members-by-type">
  <xsl:param name="type"/>
  <xsl:param name="singularReadableType"/>
  <xsl:param name="pluralReadableType"/>

  <xsl:variable name="num" select="count(Members/Member[MemberType=$type])"/>

  <tr><td>
  <xsl:value-of select="$num"/>
  </td>
  <td>
  <a name="$type">
  <xsl:call-template name="members-by-type-link">
    <xsl:with-param name="nsroot" select="$nsroot"/>
    <xsl:with-param name="fulltype" select="$fulltype"/>
    <xsl:with-param name="type" select="$type"/>
    <xsl:with-param name="content">
      <xsl:choose>
        <xsl:when test="$num=1">
	  <xsl:value-of select="$singularReadableType"/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of select="$pluralReadableType"/>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:with-param>
  </xsl:call-template>
  </a>
  </td>
  </tr>
</xsl:template>


<xsl:template name="members-by-access">
  <xsl:param name="access"/>
  <xsl:param name="readableAccess"/>

  <xsl:variable name="num" select="count(Members/Member/MemberSignature[@Language='C#' and contains(@Value,$access)])"/>

  <tr><td>
  <xsl:value-of select="$num"/>
  </td>
  <td>
  <a name="$access">
  <xsl:call-template name="members-by-access-link">
    <xsl:with-param name="nsroot" select="$nsroot"/>
    <xsl:with-param name="fulltype" select="$fulltype"/>
    <xsl:with-param name="access" select="$access"/>
    <xsl:with-param name="content">
      <xsl:value-of select="$readableAccess"/>
      <xsl:choose>
        <xsl:when test="$num=1">
	  <xsl:value-of select="' Member'"/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of select="' Members'"/>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:with-param>
  </xsl:call-template>
  </a>
  </td>
  </tr>
</xsl:template>


<xsl:template name="members-by-name">
  <tr><td>
  <xsl:value-of select="count(Members/Member)"/>
  </td>
  <td>
  <a name="all">
  <xsl:call-template name="members-by-name-link">
    <xsl:with-param name="nsroot" select="$nsroot"/>
    <xsl:with-param name="fulltype" select="$fulltype"/>
    <xsl:with-param name="content">
      <xsl:choose>
        <xsl:when test="count(Members/Member)=1">
	  <xsl:value-of select="' Member Total'"/>
	</xsl:when>
	<xsl:otherwise>
	  <xsl:value-of select="' Members Total'"/>
	</xsl:otherwise>
      </xsl:choose>
    </xsl:with-param>
  </xsl:call-template>
  </a>
  </td>
  </tr>
</xsl:template>


<xsl:template match="Docs/remarks">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

</xsl:stylesheet>

