<xsl:stylesheet version = "1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns="http://www.w3.org/1999/xhtml"
	xmlns:monodoc="http://www.go-mono.org/xml/monodoc">
	
<xsl:output method="xml" indent="yes" /> 

<xsl:param name="locale" select="'en'" />

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
						<param name="Name" value="The Mono Handbook" />
						<param name="Local" value="new/{$locale}/index.html" />
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
			<param name="Local" value="new/{$locale}/empty.html" />
		</object>
	</li>
	<xsl:if test="count(*)">
		<ul>
			<xsl:apply-templates select="*" />
		</ul>
	</xsl:if>
</xsl:template>

<xsl:template match="part|chapter|doc">
	<xsl:variable name="localized" select="concat('new/', $locale, '/', @href)" />
	<li>
		<object type="text/sitemap">
			<param name="Name" value="{@name}" />
			<xsl:choose>
				<xsl:when test="@href != ''" >
					<xsl:choose>
						<xsl:when test="document($localized)">
							<!-- <xsl:message>found localized document: <xsl:value-of select="$localized" /></xsl:message> -->
							<param name="Local" value="{$localized}" />
						</xsl:when>
						<xsl:otherwise>
							<param name="Local" value="new/en/{@href}" />
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise >
					<xsl:choose>
						<xsl:when test="document('new/{$locale}/empty.html')">
							<param name="Local" value="new/{$locale}/empty.html" />
						</xsl:when>
						<xsl:otherwise>
							<param name="Local" value="new/en/empty.html" />
						</xsl:otherwise>
					</xsl:choose>
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
