<xsl:stylesheet version = "1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml"
	xmlns:monodoc="http://www.go-mono.org/xml/monodoc">

   <xsl:param name="htmlbase" />
   <xsl:param name="title" />
	
   <xsl:output method="xml" indent="yes" /> 
   
   <xsl:template match = "/" >
      <html>
	 <head>
	    <meta name="GENERATOR" content="mkmstoc.xsl" />
	 </head>
	 <body>
	    <object type="text/site properties">
	       <param name="Auto Generated" value="No" />
	    </object>
	    
	    <ul>
	       <li>
		  <object type="text/sitemap">
		     <param name="Name" value="{$title}" />
		     <param name="Local" value="{$htmlbase}/index.html" />
		  </object>
	       </li>
	       
	       <ul>
		  <xsl:apply-templates select="monkeyguide/*" />
	       </ul>
	    </ul>
	 </body>
      </html>
   </xsl:template>

   <xsl:template match="appendices">
      <li>
	 <object type="text/sitemap">
	    <param name="Name" value="Appendices" />
	    <param name="Local" value="html/en/empty.html" />
	 </object>
      </li>
      <xsl:if test="count(*)">
	 <ul>
	    <xsl:apply-templates select="*" />
	 </ul>
      </xsl:if>
   </xsl:template>
   
   <xsl:template match="part|chapter|doc">
      <li>
	 <object type="text/sitemap">
	    <param name="Name" value="{@name}" />
	    <xsl:choose>
	       <xsl:when test="@href != ''" >
		  <param name="Local" value="{$htmlbase}/{@href}" />
	       </xsl:when>
	       <xsl:otherwise >
		  <!-- special value used by monodoc: no $htmlbase -->
		  <param name="Local" value="html/en/empty.html" />
	       </xsl:otherwise>
	    </xsl:choose>
	 </object>
      </li>
      
      <xsl:if test="count(*)">
	 <ul>
	    <xsl:apply-templates select="*" />
	 </ul>
      </xsl:if>
   </xsl:template>

</xsl:stylesheet> 
