<!-- members.xsl   Scott Bronson   24 Oct 2002 -->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:include href="doc.xsl"/>

<xsl:param name="l"/>
<!-- view: 't' -> view by type, 'a' -> view by access, 'n' -> view by name -->
<xsl:param name="view"/>

<xsl:variable name="fulltype" select="/Type/@FullName"/>
<xsl:variable name="assembly" select="/Type/AssemblyInfo/AssemblyName"/>


<xsl:template match="/Type">
<html>
<head>
  <title><xsl:value-of select="@Name"/> Members</title>
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

<xsl:call-template name="full-summary-link">
  <xsl:with-param name="nsroot" select="$nsroot"/>
  <xsl:with-param name="fulltype" select="@FullName"/>
  <xsl:with-param name="content">
    <xsl:value-of select="@Name"/>
  </xsl:with-param>
</xsl:call-template>
</h2>
</center>

<xsl:choose>
  <xsl:when test="$view='t'">
    <xsl:call-template name="generate-type-table"/>
  </xsl:when>
  <xsl:when test="$view='a'">
    <xsl:call-template name="generate-access-table"/>
  </xsl:when>
  <xsl:when test="$view='n'">
    <xsl:call-template name="generate-name-table"/>
  </xsl:when>
  <xsl:otherwise>
    <xsl:message terminate="yes">
      You must specify a view parameter!
    </xsl:message>
  </xsl:otherwise>
</xsl:choose>

</body>
</html>
</xsl:template>


<!-- - - - - - -   TYPES   - - - - - - -->

<xsl:template name="generate-type-table">
  <xsl:call-template name="type-member-table">
    <xsl:with-param name="type" select="'Property'"/>
    <xsl:with-param name="readabletype" select="'Properties'"/>
  </xsl:call-template>
  
  <xsl:call-template name="type-member-table">
    <xsl:with-param name="type" select="'Field'"/>
    <xsl:with-param name="readabletype" select="'Fields'"/>
  </xsl:call-template>
  
  <xsl:call-template name="type-member-table">
    <xsl:with-param name="type" select="'Event'"/>
    <xsl:with-param name="readabletype" select="'Events'"/>
  </xsl:call-template>
  
  <xsl:call-template name="type-member-table">
    <xsl:with-param name="type" select="'Constructor'"/>
    <xsl:with-param name="readabletype" select="'Constructors'"/>
  </xsl:call-template>
  
  <xsl:call-template name="type-member-table">
    <xsl:with-param name="type" select="'Method'"/>
    <xsl:with-param name="readabletype" select="'Methods'"/>
  </xsl:call-template>
</xsl:template>


<xsl:template name="type-member-table">
  <xsl:param name="type"/>
  <xsl:param name="readabletype"/>

  <xsl:choose>
    <xsl:when test="./Members/Member[MemberType=$type]">
      <a name="{$type}">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3><xsl:value-of select="$readabletype"/></h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="$type"/>
	</xsl:call-template>
      </a>
      <table border="1">
        <tr><th>
	  <xsl:call-template name="members-by-name-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Name'"/>
	  </xsl:call-template>
	</th><th>
	  <xsl:call-template name="members-by-access-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Access'"/>
	  </xsl:call-template>
	</th><th>
          Summary
        </th></tr>
        <xsl:for-each select="./Members/Member[MemberType=$type]">
          <xsl:sort select="@MemberName"/>
          <xsl:call-template name="type-member-line"/>
        </xsl:for-each>
      </table>
    </xsl:when>
    <xsl:otherwise>
      <a name="{$type}">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3>(no <xsl:value-of select="$readabletype"/>)</h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="$type"/>
	</xsl:call-template>
      </a>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="type-member-line">
  <xsl:variable name="access">
    <xsl:call-template name="find-access"/>
  </xsl:variable>

  <tr>
    <td>
      <xsl:call-template name="member-link">
        <xsl:with-param name="name" select="@MemberName"/>
        <xsl:with-param name="type" select="MemberType"/>
      </xsl:call-template>
    </td>
    <td>
      <xsl:call-template name="members-by-access-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
	<xsl:with-param name="fulltype" select="$fulltype"/>
	<xsl:with-param name="content" select="$access"/>
	<xsl:with-param name="access" select="$access"/>
      </xsl:call-template>
    </td>
    <td><xsl:apply-templates select="Docs/summary"/></td>
  </tr>
</xsl:template>




<!-- - - - - - -   ACCESS   - - - - - - -->

<xsl:template name="generate-access-table">
  <xsl:call-template name="access-member-table">
    <xsl:with-param name="access" select="'public'"/>
    <xsl:with-param name="readableaccess" select="'Public'"/>
  </xsl:call-template>
  
  <xsl:call-template name="access-member-table">
    <xsl:with-param name="access" select="'protected'"/>
    <xsl:with-param name="readableaccess" select="'Protected'"/>
  </xsl:call-template>
  
  <xsl:call-template name="access-member-table">
    <xsl:with-param name="access" select="'private'"/>
    <xsl:with-param name="readableaccess" select="'Private'"/>
  </xsl:call-template>
  
  <xsl:call-template name="access-member-table">
    <xsl:with-param name="access" select="'internal'"/>
    <xsl:with-param name="readableaccess" select="'Internal'"/>
  </xsl:call-template>
</xsl:template>


<xsl:template name="access-member-table">
  <xsl:param name="access"/>
  <xsl:param name="readableaccess"/>

  <xsl:choose>
    <xsl:when test="./Members/Member/MemberSignature[@Language='C#' and contains(@Value,$access)]">
      <a name="{$access}">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3><xsl:value-of select="$readableaccess"/> Members:</h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="$access"/>
	</xsl:call-template>
      </a>
      <table border="1">
        <tr><th>
	  <xsl:call-template name="members-by-name-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Name'"/>
	  </xsl:call-template>
	</th><th>
	  <xsl:call-template name="members-by-type-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Type'"/>
	    <xsl:with-param name="type"/>
	  </xsl:call-template>
	</th><th>
	  Summary
	</th></tr>

        <xsl:for-each select="./Members/Member/MemberSignature[@Language='C#' and contains(@Value,$access)]">
          <xsl:sort select="../@MemberName"/>
          <xsl:call-template name="access-member-line"/>
        </xsl:for-each>
      </table>
    </xsl:when>
    <xsl:otherwise>
      <a name="{$access}">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3>(no <xsl:value-of select="$readableaccess"/> members)</h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="$access"/>
	</xsl:call-template>
      </a>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="access-member-line">
  <tr>
    <td>
      <xsl:call-template name="member-link">
        <xsl:with-param name="name" select="../@MemberName"/>
        <xsl:with-param name="type" select="../MemberType"/>
      </xsl:call-template>
    </td>
    <td>
      <xsl:call-template name="members-by-type-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
	<xsl:with-param name="fulltype" select="$fulltype"/>
	<xsl:with-param name="content" select="../MemberType"/>
	<xsl:with-param name="type" select="../MemberType"/>
      </xsl:call-template>
    </td>
    <td><xsl:apply-templates select="../Docs/summary"/></td>
  </tr>
</xsl:template>


<!-- - - - - - -   NAME   - - - - - - -->

<xsl:template name="generate-name-table">
  <xsl:choose>
    <xsl:when test="./Members/Member">
      <a name="all">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3>All Members:</h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="all"/>
	</xsl:call-template>
      </a>
      <table border="1">
        <tr><th>
	  <xsl:call-template name="members-by-name-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Name'"/>
	  </xsl:call-template>
	</th><th>
	  <xsl:call-template name="members-by-access-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Access'"/>
	    <xsl:with-param name="type"/>
	  </xsl:call-template>
	</th><th>
	  <xsl:call-template name="members-by-type-link">
            <xsl:with-param name="nsroot" select="$nsroot"/>
            <xsl:with-param name="fulltype" select="$fulltype"/>
            <xsl:with-param name="content" select="'Type'"/>
	    <xsl:with-param name="type"/>
	  </xsl:call-template>
	</th><th>
	  Summary
	</th></tr>

        <xsl:for-each select="./Members/Member">
          <xsl:sort select="@MemberName"/>
          <xsl:call-template name="name-member-line"/>
        </xsl:for-each>
      </table>
    </xsl:when>
    <xsl:otherwise>
      <a name="all">
        <xsl:call-template name="full-summary-link">
          <xsl:with-param name="nsroot" select="$nsroot"/>
          <xsl:with-param name="fulltype" select="$fulltype"/>
          <xsl:with-param name="content">
            <h3>(no members!)</h3>
          </xsl:with-param>
	  <xsl:with-param name="mark" select="all"/>
	</xsl:call-template>
      </a>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template name="name-member-line">
  <xsl:variable name="access">
    <xsl:call-template name="find-access"/>
  </xsl:variable>

  <tr><td>
    <xsl:call-template name="member-link">
      <xsl:with-param name="name" select="@MemberName"/>
      <xsl:with-param name="type" select="MemberType"/>
    </xsl:call-template>
  </td><td>
    <xsl:call-template name="members-by-access-link">
      <xsl:with-param name="nsroot" select="$nsroot"/>
      <xsl:with-param name="fulltype" select="$fulltype"/>
      <xsl:with-param name="content" select="$access"/>
      <xsl:with-param name="access" select="$access"/>
    </xsl:call-template>
  </td><td>
    <xsl:call-template name="members-by-type-link">
      <xsl:with-param name="nsroot" select="$nsroot"/>
      <xsl:with-param name="fulltype" select="$fulltype"/>
      <xsl:with-param name="content" select="MemberType"/>
      <xsl:with-param name="type" select="MemberType"/>
    </xsl:call-template>
  </td><td>
    <xsl:apply-templates select="Docs/summary"/>
  </td></tr>
</xsl:template>


<!-- creates a link to the current Member element, whatever its type -->
<xsl:template name="member-link">
  <xsl:param name="name"/>
  <xsl:param name="type"/>

  <!-- don't use FQ name, even if one is supplied -->
  <xsl:variable name="shortname">
    <xsl:choose>
      <xsl:when test="substring($name, 1+string-length($name)-string-length('.ctor'))='.ctor'">
        <xsl:value-of select="'.ctor'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="leaf-name">
          <xsl:with-param name="string" select="$name"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="fullname" select="concat($fulltype, '.', $shortname)"/>
  <xsl:choose>
    <xsl:when test="$type='Method' or $type='Constructor'">
      <xsl:call-template name="full-method-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullmethod" select="$fullname"/>
	<xsl:with-param name="overload">
	  <xsl:call-template name="calculate-anchor"/>
	</xsl:with-param>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="$type='Property'">
      <xsl:call-template name="full-property-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullprop" select="$fullname"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="$type='Field'">
      <xsl:call-template name="full-field-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullfield" select="$fullname"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="$type='Event'">
      <xsl:call-template name="full-event-link">
        <xsl:with-param name="nsroot" select="$nsroot"/>
        <xsl:with-param name="fullevent" select="$fullname"/>
      </xsl:call-template>
    </xsl:when>
    <xsl:otherwise>
      <xsl:message terminate="yes">
        Unrecognized member type "<xsl:value-of select="$type"/>"!
      </xsl:message>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>


<xsl:template name="find-access">
  <xsl:variable name="val" select="MemberSignature[@Language='C#']/@Value"/>
  <xsl:choose>
    <xsl:when test="contains($val,'public')">
      <xsl:value-of select="'public'"/>
    </xsl:when>
    <xsl:when test="contains($val,'protected')">
      <xsl:value-of select="'protected'"/>
    </xsl:when>
    <xsl:when test="contains($val,'private')">
      <xsl:value-of select="'private'"/>
    </xsl:when>
    <xsl:when test="contains($val,'internal')">
      <xsl:value-of select="'internal'"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'(public?)'"/>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

<xsl:template match="Docs/summary">
  <xsl:apply-templates mode="doc"/>
</xsl:template>

</xsl:stylesheet>

