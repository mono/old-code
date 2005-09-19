<xsl:stylesheet version = "1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml"
	xmlns:monodoc="http://www.go-mono.org/xml/monodoc">
	
<xsl:output method="xml" indent="yes" /> 

<xsl:template match = "/" >
	<html xmlns="http://www.w3.org/1999/xhtml" xmlns:monodoc="http://www.go-mono.org/xml/monodoc">
		<head>
			<title>The Mono Handbook</title>
		
			<meta name = "DC.Description" content="This is the Mono Handbook" />
			<link rel="stylesheet" type="text/css" href="style.css" />
		</head>
		<body>
		<!-- PLEASE DO NOT REMOVE - "dirty" fix for monodoc crash //-->
		<img src="images/empty.png" border="0" />
			<xsl:apply-templates select="monkeyguide/*" />
		</body>
	</html>
</xsl:template>

<xsl:template match="intro">
	<ul>
		<xsl:for-each select = "doc" >
			<li>
				<xsl:choose>
					<xsl:when test="@href != ''" >
						 <a href="{@href}"><xsl:value-of select="@name" /></a>
					</xsl:when>
					<xsl:otherwise >
						<xsl:value-of select="@name" />
					</xsl:otherwise>
				</xsl:choose>
			</li>
		</xsl:for-each>
	</ul>
</xsl:template>

<xsl:template match="appendices">
	<h2 class="part">Appendices</h2>
	<ul>
		<xsl:for-each select = "doc" >
			<li>
				<xsl:number format="A" /><xsl:text> </xsl:text>
				
				<xsl:choose>
					<xsl:when test="@href != ''" >
						<a href="{@href}"><xsl:value-of select="@name" /></a>
					</xsl:when>
					<xsl:otherwise >
						<xsl:value-of select="@name" />
					</xsl:otherwise>
				</xsl:choose>
			</li>
		</xsl:for-each>
	</ul>
</xsl:template>

<xsl:template match="part">
	<h2 class="part"><xsl:value-of select="@name"/></h2>
	
	<xsl:apply-templates select="chapter" />
</xsl:template>

<xsl:template match="chapter">
	<h4 class="chapter"><xsl:value-of select="@name"/></h4>
	
	<xsl:if test="count(doc)">
		<ul>
			<xsl:apply-templates select="doc" />
		</ul>
	</xsl:if>
</xsl:template>

<xsl:template match="doc">
	<li>
		<xsl:choose>
			<xsl:when test="@href != ''" >
				 <a href="{@href}"><xsl:value-of select="@name" /></a>
			</xsl:when>
			<xsl:otherwise >
				<xsl:value-of select="@name" />
			</xsl:otherwise>
		</xsl:choose>
	</li>
	
	<xsl:if test="count(doc)">
		<ul>
			<xsl:apply-templates select="doc" />
		</ul>
	</xsl:if>
</xsl:template>



</xsl:stylesheet> 