<!-- item.xsl   Scott Bronson   26 Oct 2002 -->

<!-- displays information about a single member.  called item.xsl because
I didn't want two scripts: member.xsl and members.xsl -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" encoding="ISO-8859-1" indent="no"/>

<xsl:include href="doc.xsl"/>

<xsl:param name="l"/>
<xsl:param name="m"/>   <!-- member name -->

<xsl:variable name="fulltype" select="/Type/@FullName"/>
<xsl:variable name="assembly" select="/Type/AssemblyInfo/AssemblyName"/>
<xsl:variable name="member" select="$m"/>

<xsl:variable name="readableMember">
  <xsl:choose>
    <xsl:when test="$m='.ctor'">
      <xsl:value-of select="/Type/@Name"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$m"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:variable>


<xsl:template match="/Type">
<!-- sanity check, make sure the member even exists -->
<xsl:if test="count(./Members/Member[@MemberName=$member])&lt;1">
  <!-- ideally, the following should be set to "yes".  Problem is,
  System/Type.xml refers to System.Reflection/BindingFlags.FlattenHierarchy,
  which doesn't exist.  See bug-flattenhierarchy. -->
  <xsl:message terminate="no">
    Could not find <xsl:value-of select="$m"/> in this xml file!
  </xsl:message>
</xsl:if>

<html>
<xsl:text>
</xsl:text>
<head>
<xsl:text>
</xsl:text>
  <title><xsl:value-of select="@Name"/> Members</title>
</head>
<xsl:text>
</xsl:text>
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

<xsl:call-template name="full-summary-link">
  <xsl:with-param name="nsroot" select="$nsroot"/>
  <xsl:with-param name="fulltype" select="@FullName"/>
  <xsl:with-param name="content">
    <xsl:value-of select="@Name"/>
  </xsl:with-param>
</xsl:call-template>

<xsl:text> . </xsl:text>
<xsl:value-of select="$readableMember"/>
</h2>
</center>

<xsl:if test="count(./Members/Member[@MemberName=$member])>1">
  <b>Overloaded:</b><xsl:text>
  </xsl:text><blockquote>
  <xsl:for-each select="./Members/Member[@MemberName=$member]">
    <xsl:if test="ReturnValue/ReturnType">
      <xsl:call-template name="full-type-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fulltype" select="ReturnValue/ReturnType"/>
      </xsl:call-template>
      <xsl:text> </xsl:text>
    </xsl:if>
    <b>
    <!-- I assume that only methods will ever be overloaded. -->
    <xsl:call-template name="full-method-link">
      <xsl:with-param name="nsroot" select="$nsroot"/>
      <xsl:with-param name="fullmethod" select="concat($fulltype,'.',$member)"/>
      <xsl:with-param name="content" select="$readableMember"/>
      <xsl:with-param name="overload">
        <xsl:call-template name="calculate-anchor"/>
      </xsl:with-param>
    </xsl:call-template>
    </b>
    <xsl:if test="Parameters/Parameter">
      <!-- only insert a space if we have params, else it looks weird. -->
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:text>(</xsl:text>
    <xsl:apply-templates select="Parameters/Parameter"/>
    <xsl:text>)</xsl:text>
    <xsl:if test="not(position()=last())">
      <xsl:text>
      </xsl:text>
      <br></br>
    </xsl:if>
  </xsl:for-each>
  <xsl:text>
  </xsl:text>
  </blockquote>
  <hr></hr>
</xsl:if>

<xsl:apply-templates select="./Members/Member[@MemberName=$member]"/>

</body>
</html>
</xsl:template>


<xsl:template match="Member">
  <xsl:variable name="anchor">
    <xsl:call-template name="calculate-anchor"/>
  </xsl:variable>
 
  <!-- create the fully linked prototype -->
  <h3><a name="{$anchor}">
  <xsl:if test="ReturnValue/ReturnType">
    <xsl:call-template name="full-type-link">
      <xsl:with-param name="nsroot" select="$nsroot"/>
      <xsl:with-param name="fulltype" select="ReturnValue/ReturnType"/>
    </xsl:call-template>
    <xsl:text> </xsl:text>
  </xsl:if>
  <xsl:value-of select="$readableMember"/>
  <xsl:if test="MemberType='Constructor' or MemberType='Method'">
    <xsl:if test="Parameters/Parameter">
      <!-- only insert a space if we have params, else it looks weird. -->
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:text>(</xsl:text>
    <xsl:apply-templates select="Parameters/Parameter"/>
    <xsl:text>)</xsl:text>
  </xsl:if>
  <xsl:if test="MemberValue">
    <xsl:text> = </xsl:text>
    <xsl:value-of select="MemberValue"/>
  </xsl:if>
  </a></h3>

  <xsl:text>
  </xsl:text>
    
  <xsl:apply-templates select="Docs/summary"/>

  <xsl:text>
  </xsl:text>

  <xsl:if test="Docs/param">
    <b><xsl:text>Parameters:</xsl:text></b>
    <table border="0" cellspacing="16">
    <xsl:apply-templates select="Docs/param"/>
    </table>
  </xsl:if>

  <xsl:if test="Docs/returns">
    <b><xsl:text>Returns:</xsl:text></b>
    <xsl:apply-templates select="Docs/returns"/>
  </xsl:if>

  <xsl:text>
  </xsl:text>

  <xsl:if test="Docs/exception">
    <b><xsl:text>Exceptions:</xsl:text></b>
    <table border="0" cellspacing="16">
    <xsl:apply-templates select="Docs/exception"/>
    </table>
  </xsl:if>

  <xsl:text>
  </xsl:text>
  
  <xsl:if test="Docs/value">
    <p><b><xsl:text>Value:</xsl:text></b></p>
    <xsl:apply-templates select="Docs/value"/>
  </xsl:if>

  <xsl:text>
  </xsl:text>

  <xsl:if test="Docs/remarks">
    <p><b><xsl:text>Remarks:</xsl:text></b></p>
    <xsl:apply-templates select="Docs/remarks"/>
  </xsl:if>

  <xsl:text>
  </xsl:text>

  <xsl:if test="Docs/permission">
    <p><b><xsl:text>Permission:</xsl:text></b></p>
    <xsl:apply-templates select="Docs/permission"/>
  </xsl:if>

  <xsl:text>
  </xsl:text>

  <xsl:if test="Docs/example">
    <p><b><xsl:text>Example:</xsl:text></b></p>
    <xsl:apply-templates select="Docs/example"/>
  </xsl:if>

  <xsl:text>
  </xsl:text>

  <xsl:if test="not(position()=last())">
    <hr></hr>
  </xsl:if>
</xsl:template>


<xsl:template match="Parameters/Parameter">
  <xsl:call-template name="full-type-link">
    <xsl:with-param name="nsroot" select="$nsroot"/>
    <xsl:with-param name="fulltype" select="@Type"/>
  </xsl:call-template>
  <xsl:text> </xsl:text>
  <i><xsl:value-of select="@Name"/></i>
  <xsl:if test="not(position()=last())">
    <xsl:text>, </xsl:text>
  </xsl:if>
  <xsl:text>
  </xsl:text>
</xsl:template>

<xsl:template match="Docs/summary">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

<xsl:template match="Docs/param">
  <xsl:variable name="anchor">
    <xsl:apply-templates select="ancestor::Member" mode="anchor"/>
    <!-- see "bug-space01" in BUGS for why this translate is necessary -->
    <xsl:value-of select="concat('-', translate(@name,'&amp; ','_'))"/>
  </xsl:variable>

  <tr><td valign="top">
    <b><i><a name="{$anchor}"><xsl:value-of select="@name"/></a></i></b>
  </td>
  <xsl:text>
  </xsl:text>
  <td>
    <xsl:apply-templates mode="doc"/>
  </td></tr>
  <xsl:text>
  </xsl:text>
</xsl:template>

<xsl:template match="Docs/returns">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

<xsl:template match="Docs/exception">
  <tr>
  <td valign="top">
    <xsl:call-template name="handle-cref">
      <xsl:with-param name="cref" select="@cref"/>
    </xsl:call-template>
  </td>
  <xsl:text>
  </xsl:text>
  <td>
    <xsl:apply-templates mode="doc"/>
  </td>
  </tr>
  <xsl:text>
  </xsl:text>
</xsl:template>

<xsl:template match="Docs/value">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

<xsl:template match="Docs/remarks">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

<xsl:template match="Docs/permission">
  <!-- I'm not sure what to do with the cref here -->
  <p>cref: <xsl:value-of select="substring(@cref,3)"/></p>
  <xsl:apply-templates mode="doc"/>
</xsl:template>

<xsl:template match="Docs/example">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

</xsl:stylesheet>

