<!-- namespace.xsl   Scott Bronson   11 Oct 2002 -->

<!-- This outputs an index of the given directory. -->

<!-- An obscure bug in Xalan 1.2 prevents it from generating the root.
     Dunno if it's fixed in 1.3 or 1.4.
     Sablotron 0.95 seems to process it OK. -->


<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:include href="link.xsl"/>

<!-- specify the namespace of the index to generate on the command line -->
<xsl:param name="ns"/>
<xsl:param name="l"/>


<xsl:variable name="assembly" select="/masterdoc/@assembly"/>

<xsl:template match="/masterdoc">
<html>
<head>
<title>
<xsl:call-template name="print-namespace"/>
</title>
</head>
<body>
<h3> <center>
<xsl:call-template name="print-namespace"/>
</center> </h3>

<table border="1">
<tr><th>Subnamespaces</th></tr>
<tr><td>
<xsl:choose>
  <xsl:when test="$ns">
    <xsl:call-template name="namespace-link">
      <xsl:with-param name="ns">
        <xsl:call-template name="base-name">
	  <xsl:with-param name="string" select="$ns"/>
	</xsl:call-template>
      </xsl:with-param>
      <xsl:with-param name="assembly" select="$assembly"/>
      <xsl:with-param name="content" select="string('.. (parent)')"/>
    </xsl:call-template>
  </xsl:when>
  <xsl:otherwise>
    <i>at root</i>
  </xsl:otherwise>
</xsl:choose>
</td></tr>
<xsl:apply-templates select="/masterdoc" mode="list"/>
</table>

<p></p>

<table border="1">
<tr><th>Class</th><th>Description</th></tr>
  <xsl:choose>
    <xsl:when test="count(/masterdoc/class[@namespace=$ns])=0">
      <tr><td colspan="2"><i>No classes!</i></td></tr>
    </xsl:when>
    <xsl:otherwise>
      <xsl:for-each select="/masterdoc/class[@namespace=$ns]">
        <xsl:sort select="@name"/>
        <tr>

	  <td>
	    <xsl:call-template name="full-summary-link">
    	      <xsl:with-param name="fulltype" select="concat(@namespace, '.', @name)"/>
	    </xsl:call-template>
	  </td>

	  <td>
	    (unavailable)
	  </td>

	</tr>
      </xsl:for-each>
    </xsl:otherwise>
  </xsl:choose>
</table>

</body>
</html>

</xsl:template>


<!-- outputs all the namespaces immediately after this one -->
<!-- thanks to David McNally -->
<xsl:template match="/masterdoc" mode="list">
  <xsl:variable name="thisns">
    <xsl:if test="$ns">
      <xsl:value-of select="concat($ns,'.')"/>
    </xsl:if>
  </xsl:variable>

  <xsl:for-each select="class[starts-with(@namespace,$ns)]">
    <xsl:sort select="@namespace"/>
    <xsl:variable name="nsafter" select="substring-before(concat(substring-after(@namespace,$thisns),'.'),'.')"/>
    <xsl:if test="not(preceding-sibling::class[starts-with(@namespace,concat($thisns,$nsafter))]) and ($thisns='' or not($nsafter=''))">
      <tr><td>
	<xsl:call-template name="namespace-link">
    	  <xsl:with-param name="ns" select="concat($thisns,$nsafter)"/>
	  <xsl:with-param name="assembly" select="$assembly"/>
	  <xsl:with-param name="content" select="$nsafter"/>
	</xsl:call-template>
      </td></tr>
    </xsl:if>
  </xsl:for-each>
</xsl:template>


<xsl:template name="print-namespace">
  <xsl:choose>
    <xsl:when test="string-length($ns)=0">
      (root namespace)
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$ns"/> namespace
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


</xsl:stylesheet>

