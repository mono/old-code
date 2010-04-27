<xsl:stylesheet version = "1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml">

   <xsl:output method="xml" indent="yes" /> 
   
   <xsl:template match = "/" >
      <html>
	 <head>
	    <meta name="GENERATOR" content="bd2xhtml.xsl" />
	 </head>
	 <body>
	    <!--
	    <object type="text/site properties">
	    <param name="Auto Generated" value="No" />
	    </object>
	    -->

	    <xsl:apply-templates select="*" />
	 </body>
      </html>
   </xsl:template>

   <!-- bundle.xml -->

   <xsl:template match="bundle">
      <h2><xsl:value-of select="@name" /> Bundle</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <xsl:if test="count(namespace)">
	 <h4>Namespaces:</h4>
	 
	 <blockquote>
	    <table border="1" cellpadding="6" width="100%">
	       <xsl:apply-templates select="namespace" mode="summary"/>
	    </table>
	 </blockquote>
      </xsl:if>

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- results -->

   <xsl:template match="result">
      <h2><xsl:value-of select="@name" /> Result</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <xsl:if test="count(dictitem)">
	 <h4>Dictionary Items:</h4>
      
	 <blockquote>
	    <table border="1" cellpadding="6" width="100%">
	       <xsl:apply-templates select="dictitem" mode="summary"/>
	    </table>
	 </blockquote>
      </xsl:if>

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- providers -->

   <xsl:template match="provider">
      <h2><xsl:value-of select="@name" /> Provider</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <xsl:if test="count(target)">
	 <h4>Targets:</h4>
	 
	 <blockquote>
	    <table border="1" cellpadding="6" width="100%">
	       <xsl:apply-templates select="target" mode="summary"/>
	    </table>
	 </blockquote>
      </xsl:if>

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- rules -->

   <xsl:template match="rule">
      <h2><xsl:value-of select="@name" /> Rule</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <xsl:if test="count(argument)">
	 <h4>Arguments:</h4>
      
	 <blockquote>
	    <table border="1" cellpadding="6" width="100%">
	       <xsl:apply-templates select="argument" mode="summary"/>
	    </table>
	 </blockquote>
      </xsl:if>

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- regex matcher -->

   <xsl:template match="regex_matcher">
      <h2><xsl:value-of select="@name" /> Regular Expression Matcher</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <h4>Behavior:</h4>

      <blockquote>
	 <p>This matcher maps names matching the regular expresion
	 "<xsl:value-of select="regex"/>" to
	 the rule <xsl:value-of select="rule"/>.</p>
      </blockquote>

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- regular matcher (never actually used...) -->

   <xsl:template match="matcher">
      <h2><xsl:value-of select="@name" /> Matcher</h2>

      <a name="summary" />
      <xsl:apply-templates select="docs/summary" />

      <a name="remarks" />
      <xsl:apply-templates select="docs/remarks" />
   </xsl:template>

   <!-- XmlSynchronizer stuff -->
   <!-- Any way to avoid the duplication? -->

   <xsl:template match="*" mode="summary">
      <tr>
	 <a name="item-{@name}"/>

	 <td><b><xsl:value-of select="@name" /></b></td>
	 <td>
	    <xsl:if test="@deprecated_since">
	       <p><b>[Deprecated since version <xsl:value-of select="@deprecated_since"/>]</b></p>
	    </xsl:if>
	    <xsl:if test="@exists_since">
	       <p><b>[Implemented since version <xsl:value-of select="@exists_since"/>]</b></p>
	    </xsl:if>

	    <xsl:apply-templates select="docs/summary"/>

	    <xsl:apply-templates select="docs/remarks"/>
	 </td>
      </tr>
   </xsl:template>

   <xsl:template match="target" mode="summary">
      <tr>
	 <a name="item-{@name}"/>

	 <td><b><xsl:value-of select="@name" /></b></td>
	 <td>
	    <xsl:if test="@deprecated_since">
	       <p><b>[Deprecated since version <xsl:value-of select="@deprecated_since"/>]</b></p>
	    </xsl:if>
	    <xsl:if test="@exists_since">
	       <p><b>[Implemented since version <xsl:value-of select="@exists_since"/>]</b></p>
	    </xsl:if>

	    <xsl:apply-templates select="docs/summary"/>

	    <h4>Rule:</h4>
	    <blockquote>
	       <p><xsl:value-of select="@name" /> is built with the rule <xsl:value-of select="rule" />.</p>
	    </blockquote>

	    <xsl:apply-templates select="docs/remarks"/>
	 </td>
      </tr>
   </xsl:template>

   <xsl:template match="argument" mode="summary">
      <tr>
	 <a name="item-{@name}"/>

	 <td>
	    <xsl:value-of select="flags" /> 
	    <xsl:value-of select="' '" /> 
	    <xsl:value-of select="type" /> 
	    <xsl:value-of select="' '" /> 
	    <b><xsl:value-of select="@name" /></b>
	 </td>
	 <td>
	    <xsl:if test="@deprecated_since">
	       <p><b>[Deprecated since version <xsl:value-of select="@deprecated_since"/>]</b></p>
	    </xsl:if>
	    <xsl:if test="@exists_since">
	       <p><b>[Implemented since version <xsl:value-of select="@exists_since"/>]</b></p>
	    </xsl:if>
	    <xsl:apply-templates select="docs/summary"/>

	    <xsl:apply-templates select="docs/remarks"/>
	 </td>
      </tr>
   </xsl:template>

   <!-- Docs, remarks -->

   <xsl:template match="summary">
      <p>
	 <xsl:apply-templates select="@*|node()"/>
      </p>
   </xsl:template>

   <xsl:template match="remarks">
      <h4>Remarks:</h4>
      <blockquote>
	 <xsl:apply-templates select="@*|node()"/>
      </blockquote>
   </xsl:template>

   <xsl:template match="code">
      <table class="CodeExampleTable" bgcolor="#f5f5dd" border="1" cellpadding="5" width="100%">
	 <tr>
	    <td><b><font size="-1"><xsl:value-of select="@lang"/> Example</font></b></td>
	 </tr>
	 <tr>
	    <td><font size="-1">
	    <!--
	 <xsl:value-of select="monodoc:Colorize(string(descendant-or-self::text()), 
	 string(@lang))" disable-output-escaping="yes" />
	 -->
	    <pre><xsl:value-of select="string(descendant-or-self::text())" disable-output-escaping="yes" /></pre>
	    </font></td>
	 </tr>
      </table>
   </xsl:template>

   <xsl:template match="item">
      <a href="#item-{text()}"><xsl:value-of select="text()"/></a>
   </xsl:template>

   <xsl:template match="lr"> <!-- local reference: @ns = namespace, @c = class -->
   <!-- gross evil hack: and it won't work from summary page -->
      <a href="../{@ns}/{@c}.xhtml">
	 <xsl:value-of select="@ns" />
	 <xsl:value-of select="'.'" />
	 <xsl:value-of select="@c" />
      </a>
   </xsl:template>

   <xsl:template match="gr"> <!-- global reference -->
      <a href="{@href}">
	 <xsl:value-of select="text()" />
      </a>
   </xsl:template>

   <xsl:template match="p|i|em|u|b|a">
      <xsl:copy>
	 <xsl:apply-templates select="@*|node()"/>
      </xsl:copy>
   </xsl:template>
</xsl:stylesheet> 
