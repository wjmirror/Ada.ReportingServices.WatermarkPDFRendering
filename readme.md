### Ada.ReportingServices.WatermarkPDFRender is a Reporting Services PDF render which adds a watermark text on each page.

## Adapt to Different Reporting Services Version 
The source code is for Reporting service 2016, version 14.0.  
You may change the Reporting service version with 
* Change the Reference dlls, Microsoft.ReportingServices.ImageRendering.dll, Microsoft.ReportingServices.Interfaces.dll and Microsoft.ReportingServices.ProcessingCore.dll from your target Reporting Service installation folder. It usually is C:\Program Files\Microsoft SQL Server Reporting Services\SSRS\ReportServer\bin\ . 
* Change WatermarkPDFRender.cs, line 29, the Type name of Microsoft.ReportingServices.Rendering.ImageRenderer.PDFRenderer, change the dll version as your target reporting service dll version. 

## Installation 
1. Compile the project, Copy the Ada.ReportingServices.WatermarkPDFRendering.dll into reporing service installation bin folder. Usually, it is C:\Program Files\Microsoft SQL Server Reporting Services\SSRS\ReportServer\bin\. 
2. Change the rsreportserver.config, comment out the original PDF render ```<Configuration><Extensions><Render><!--Extension Name="PDF" .../-->```  
    Add new WatermarkPDFRenderer in ``` <Configuration><Extensions><Render> ``` as,  
    ```xml
	<Extension Name="PDF" Type="Ada.ReportingServices.WatermarkPDFRendering.WatermarkPDFRenderer,Ada.ReportingServices.WatermarkPDFRendering">
		<Configuration>
			<DeviceInfo>
				<Watermark>Ada.ReportingServices.WatermarkPDFRendering.WatermarkPDFRenderer</Watermark>
			</DeviceInfo>
		</Configuration>
	</Extension> 
	```

3. Change rssrvpolicy.config, to add following CodeGroup section in the last inner CodeGroup section. 
	```xml
	<CodeGroup class="UnionCodeGroup" version="1" PermissionSetName="FullTrust" Name="WatermarkPDFRendering" Description="This code group grants Ada.ReportingServices.WatermarkPDFRendering FullTrust. ">
		<IMembershipCondition class="UrlMembershipCondition" version="1" Url="C:\Program Files\Microsoft SQL Server Reporting Services\SSRS\ReportServer\bin\Ada.ReportingServices.WatermarkPDFRendering.dll"/>
    </CodeGroup>
	```
4. The watermark text is configured in DeviceInfo. 
5. Restart Reporting service, you will see "PDF with Watermark' in the Export options. 
    