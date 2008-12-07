<!-- doc.xsl   Scott Bronson   11 Oct 2002 -->

<!-- whoever designed  xsl entity handling was obviously totally deranged. -->
<!DOCTYPE xsl:stylesheet [
<!ENTITY frac14 "<xsl:text disable-output-escaping='yes'>&amp;frac14;</xsl:text>">
<!ENTITY frac12 "<xsl:text disable-output-escaping='yes'>&amp;frac12;</xsl:text>">
<!ENTITY frac34 "<xsl:text disable-output-escaping='yes'>&amp;frac34;</xsl:text>">
<!ENTITY permil "<xsl:text disable-output-escaping='yes'>&amp;permil;</xsl:text>">
<!ENTITY pi "<xsl:text disable-output-escaping='yes'>&amp;pi;</xsl:text>">
<!ENTITY theta "<xsl:text disable-output-escaping='yes'>&amp;theta;</xsl:text>">
]>

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<xsl:include href="link.xsl"/>

<!-- doc elements.  must be in doc mode to use these. -->
<!-- I wonder if doc mode was such a good idea...? -->

<xsl:variable name="nsroot">
  <xsl:call-template name="calc-root">
    <xsl:with-param name="string" select="/Type/@FullName"/>
  </xsl:call-template>
</xsl:variable>


<!-- prints the given namespace as a list with each one clickable -->
<xsl:template name="print-hierarchy">
  <xsl:param name="ns"/>

  <xsl:variable name="base">
    <xsl:call-template name="base-name">
      <xsl:with-param name="string" select="$ns"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:if test="contains($ns, '.')">
    <xsl:call-template name="print-hierarchy">
      <xsl:with-param name="ns" select="$base"/>
    </xsl:call-template>
  </xsl:if>

  <xsl:call-template name="namespace-link">
    <xsl:with-param name="nsroot" select="$nsroot"/>
    <xsl:with-param name="ns" select="$ns"/>
    <xsl:with-param name="assembly" select="$assembly"/>
    <xsl:with-param name="content">
      <xsl:call-template name="leaf-name">
        <xsl:with-param name="string" select="$ns"/>
      </xsl:call-template>
    </xsl:with-param>
  </xsl:call-template>
  <xsl:text> . </xsl:text>
</xsl:template>


<xsl:template match="block" mode="doc">
  <table border="0" cellspacing="8">
  <tr><td valign="top">
  <xsl:choose>
    <xsl:when test="@type='note'">
      <img src="http://www.rinspin.com/~bronson/notealert.png"/>
    </xsl:when>
    <xsl:when test="@type='behaviors'">
      <xsl:text>(behaviors)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='usage'">
      <xsl:text>(usage)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='overrides'">
      <xsl:text>(overrides)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='example'">
      <xsl:text>(example)</xsl:text>
    </xsl:when>
    <xsl:when test="@type='default'">
      <xsl:text>(default)</xsl:text>
    </xsl:when>
    <xsl:otherwise>
      <xsl:message terminate="yes">
        ERROR: Unrecognized block type.
      </xsl:message>
    </xsl:otherwise>
  </xsl:choose>
  </td><td>
  <xsl:apply-templates mode="doc"/>
  </td></tr></table>
</xsl:template>


<xsl:template match="c" mode="doc">
  <tt><xsl:apply-templates mode="doc"/></tt>
</xsl:template>


<xsl:template match="code" mode="doc">
  <xsl:choose>
    <xsl:when test="@lang='C#'">
      <table bgcolor="#E0E0E0"><tr><td>
      <pre><xsl:apply-templates mode="doc"/></pre>
      </td></tr></table>
    </xsl:when>
    <xsl:otherwise>
      <p><i>
      (code snippet in <xsl:value-of select="@lang"/> suppressed.)
      </i></p>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<!-- is this right??? see System/Decimal.xml -->
<xsl:template match="i" mode="doc">
  <i><xsl:apply-templates mode="doc"/></i>
</xsl:template>


<xsl:template match="list" mode="doc">
  <xsl:choose>
    <xsl:when test="@type='bullet'">
      <ul><xsl:apply-templates mode="doc-list"/></ul>
    </xsl:when>
    <xsl:when test="@type='number'">
      <ol><xsl:apply-templates mode="doc-list"/></ol>
    </xsl:when>
    <xsl:when test="@type='table'">
      <table border="1">
      <xsl:apply-templates mode="doc-table"/>
      </table>
    </xsl:when>
    <xsl:otherwise>
      <xsl:message terminate="yes">
        ERROR: Unrecognized list type.
      </xsl:message>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="list/item/term" mode="doc-list">
  <li><xsl:apply-templates mode="doc"/></li>
</xsl:template>

<xsl:template match="list/listheader" mode="doc-table">
  <tr><xsl:apply-templates mode="doc-table-th"/></tr>
</xsl:template>

<xsl:template match="list/item" mode="doc-table">
  <tr><xsl:apply-templates mode="doc-table"/></tr>
</xsl:template>

<xsl:template match="term|description" mode="doc-table-th">
  <th><xsl:apply-templates mode="doc"/></th>
</xsl:template>

<xsl:template match="term|description" mode="doc-table">
  <td><xsl:apply-templates mode="doc"/></td>
</xsl:template>


<xsl:template match="onequarter" mode="doc">&frac14;</xsl:template>


<xsl:template match="para" mode="doc">
  <p><xsl:apply-templates mode="doc"/></p>
</xsl:template>


<xsl:template match="paramref" mode="doc">
  <xsl:choose>
    <xsl:when test="contains(@name, ' ')">
      <i><xsl:value-of select="@name"/></i>
    </xsl:when>
    <xsl:when test="1">
      <i><xsl:value-of select="@name"/></i>
    </xsl:when>
    <!-- dammitalltohell.  I had paramrefs linking to their params
    _perfectly_.  Then along come all these goddam paramrefs that
    don't actually refer to a param.  Should call them varrefs or
    something. bug-paramref -->
    <xsl:otherwise>
      <xsl:call-template name="full-parameter-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <!-- we rely on the member variable being set up by the caller -->
        <xsl:with-param name="fullmethod" select="concat($fulltype, '.', $member)"/>
        <xsl:with-param name="overload">
          <!-- unfortunately we can't pass the anchor in to this template -->
          <!-- therefore we must calculate it by moving back up the tree.  ugh. -->
          <xsl:apply-templates select="ancestor::Member" mode="anchor"/>
        </xsl:with-param>
        <xsl:with-param name="parameter" select="@name"/>
      </xsl:call-template>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="Member" mode="anchor">
  <xsl:call-template name="calculate-anchor"/>
</xsl:template>

<!-- assumes the current node is "Member" -->
<xsl:template name="calculate-anchor">
  <xsl:variable name="anch">
    <xsl:for-each select="Parameters/Parameter">
      <xsl:value-of select="@Type"/>
      <xsl:if test="not(position()=last())">
        <xsl:text>,</xsl:text>
      </xsl:if>
    </xsl:for-each>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="string-length($anch)>0">
      <xsl:value-of select="translate($anch,'[]&amp;','___')"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'null'"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template match="permille" mode="doc">&permil;</xsl:template>
<xsl:template match="pi" mode="doc">&pi;</xsl:template>

<!-- use of the PRE element is retarded.  bug-useofpre -->
<xsl:template match="PRE" mode="doc">
  <xsl:apply-templates mode="doc"/>
</xsl:template>


<xsl:template match="see" mode="doc">
  <xsl:choose>
    <xsl:when test="@langword">
      <xsl:call-template name="langword-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="langword" select="@langword"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="@cref">
      <xsl:call-template name="handle-cref">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="cref" select="@cref"/>
	<xsl:with-param name="qualify" select="@qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:otherwise>
      <xsl:message terminate="yes">
        ERROR: Unrecognized format for 'see' tag.
      </xsl:message>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="handle-cref">
  <xsl:param name="cref"/>     <!-- the text from the cref attribute -->
  <xsl:param name="qualify"/>  <!-- optional, "true" means fully qualify -->

  <xsl:choose>
    <xsl:when test="substring($cref,1,2)='E:'">
      <!-- cref E (event) -->
      <xsl:call-template name="full-event-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullevent" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="substring($cref,1,2)='F:'">
      <!-- cref F (field) -->
      <xsl:call-template name="full-field-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullfield" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="substring($cref,1,2)='M:'">
      <!-- cref M (method) -->
      <xsl:call-template name="full-method-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullmethod" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="substring($cref,1,2)='N:'">
      <!-- cref N (namespace) -->
      <xsl:call-template name="full-method-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="ns" select="substring($cref,3)"/>
	<xsl:with-param name="assembly" select="$assembly"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="substring($cref,1,2)='P:'">
      <!-- cref P (property) -->
      <xsl:call-template name="full-property-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullprop" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <xsl:when test="substring($cref,1,2)='T:'">
      <!-- cref T (type) -->
      <xsl:call-template name="full-type-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fulltype" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>

    <!-- cref ! (doc generator error) -->
    <xsl:when test="substring($cref,1,2)='!:'">
      <xsl:call-template name="full-bang-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullbang" select="substring($cref,3)"/>
	<xsl:with-param name="qualify" select="$qualify"/>
      </xsl:call-template>
    </xsl:when>
  </xsl:choose>
</xsl:template>


<!-- use of the SPAN element is retarded.  bug-useofspan -->
<xsl:template match="SPAN" mode="doc">
  <xsl:apply-templates mode="doc"/>
</xsl:template>


<xsl:template match="sup" mode="doc">
  <sup><xsl:apply-templates mode="doc"/></sup>
</xsl:template>

<!-- see bug-superscript-elem -->
<xsl:template match="superscript" mode="doc">
  <sup><xsl:value-of select="@term"/></sup>
</xsl:template>

<xsl:template match="sub" mode="doc">
  <sub><xsl:apply-templates mode="doc"/></sub>
</xsl:template>

<!-- this element is idiotic.  All usage should be replaced
by <sub> above.  See bug-subscript-elem  -->
<xsl:template match="subscript" mode="doc">
  <sub><xsl:value-of select="@term"/></sub>
</xsl:template>


<xsl:template match="theta" mode="doc">&theta;</xsl:template>


<!-- if not in doc mode, then we barf -->
<xsl:template match="block|c|code|list|para|paramref|see|sup|sub">
  <xsl:message terminate="yes">
    element: <xsl:value-of select="name()"/>
    ERROR: used a doc element without being in doc mode!
  </xsl:message>
</xsl:template>


<xsl:template match="*" mode="doc">
  <xsl:message terminate="yes">
    element: <xsl:value-of select="name()"/>
    ERROR: Unrecognized documentation element!
  </xsl:message>
</xsl:template>


<!-- calculates path to the root, i.e. "../.." for "System.Sub.Obj2" -->
<!-- now uses monodoc flat hierarchy, much easier this way... -->
<xsl:template name="calc-root">
  <xsl:param name="string"/>
  <xsl:text>../</xsl:text>
</xsl:template>


</xsl:stylesheet>

