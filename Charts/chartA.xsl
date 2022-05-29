<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">	
	<xsl:template match="/chart">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
		    <xsl:apply-templates>
				<xsl:sort select="@value" data-type="number" order="ascending"/>				
			</xsl:apply-templates>
		</xsl:copy>		
	</xsl:template>	
	<xsl:template match="set">
		<xsl:copy>
			<xsl:copy-of select="@*"/>
		</xsl:copy>
	</xsl:template>
</xsl:stylesheet>
