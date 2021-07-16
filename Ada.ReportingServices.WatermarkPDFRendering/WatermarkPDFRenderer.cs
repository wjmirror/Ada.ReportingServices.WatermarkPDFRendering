using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ReportingServices.OnDemandReportRendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Specialized;
using System.Collections;

namespace Ada.ReportingServices.WatermarkPDFRendering
{


    public class WatermarkPDFRenderer : IRenderingExtension
    {
        private IRenderingExtension pdfRenderer;
        private Stream intermediateStream;
        private string _name;
        private string _extension;
        private Encoding _encoding;
        private string _mimeType;
        private bool _willSeek;
        private Microsoft.ReportingServices.Interfaces.StreamOper _operation;

        public WatermarkPDFRenderer()
        {
            Type imgRenderType = Type.GetType("Microsoft.ReportingServices.Rendering.ImageRenderer.PDFRenderer,Microsoft.ReportingServices.ImageRendering, Version=14.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
            pdfRenderer = (IRenderingExtension)Activator.CreateInstance(imgRenderType, true);
            if (pdfRenderer == null)
                throw new Exception("Can not instance Microsoft.ReportingServices.Rendering.ImageRenderer.PDFRenderer.");
        }

        public Stream IntermediateCreateAndRegisterStream(string name, string extension, Encoding encoding, string mimeType, bool willSeek, Microsoft.ReportingServices.Interfaces.StreamOper operation)
        {
            _name = name;
            _encoding = encoding;
            _extension = extension;
            _mimeType = mimeType;
            _operation = operation;
            _willSeek = willSeek;
            intermediateStream = new MemoryStream();
            return intermediateStream;
        }

        public bool Render(Microsoft.ReportingServices.OnDemandReportRendering.Report report, 
                            NameValueCollection reportServerParameters, 
                            NameValueCollection deviceInfo, 
                            NameValueCollection clientCapabilities, 
                            ref Hashtable renderProperties, 
                            Microsoft.ReportingServices.Interfaces.CreateAndRegisterStream createAndRegisterStream)
        {
            pdfRenderer.Render(report, reportServerParameters, deviceInfo, clientCapabilities, ref renderProperties, new Microsoft.ReportingServices.Interfaces.CreateAndRegisterStream(IntermediateCreateAndRegisterStream));

            intermediateStream.Position = 0;

            Stream outputStream = createAndRegisterStream(_name, _extension, _encoding, _mimeType, _willSeek, _operation);
            if (deviceInfo.AllKeys.Contains("Watermark") && !string.IsNullOrEmpty(deviceInfo["Watermark"]))
            {
                string waterMark = deviceInfo["Watermark"];
                this.AddWaterMark(intermediateStream, outputStream, waterMark);
            }
            else
            {
                byte[] buffer = new byte[32768];
                while (true)
                {
                    int read = intermediateStream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
                    outputStream.Write(buffer, 0, read);
                }
            }
            intermediateStream.Close();
            intermediateStream = null;
            return false;
        }

        public void AddWaterMark(Stream intermediateStream, Stream outputStream, string waterMark)
        {
            PdfDocument document = PdfReader.Open(intermediateStream, PdfDocumentOpenMode.Modify);
            if (document.Version < 14)
                document.Version = 14;

            foreach (PdfPage page in document.Pages)
            {
                XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

                XFont font = new XFont("Arial", 50.0, XFontStyle.Bold);
                XSize size = gfx.MeasureString(waterMark, font);

                gfx.TranslateTransform(page.Width.Point / (double)2, page.Height.Point / (double)2);
                gfx.RotateTransform(-Math.Atan(page.Height.Point / (double)page.Width.Point) * 180 / Math.PI);
                gfx.TranslateTransform(-page.Width.Point / (double)2, -page.Height.Point / (double)2);


                XStringFormat format = new XStringFormat();
                format.Alignment = XStringAlignment.Near;
                format.LineAlignment = XLineAlignment.Near;

                XBrush brush = new XSolidBrush(XColor.FromArgb(32, 0, 0, 0));

                gfx.DrawString(waterMark, font, brush, new XPoint((page.Width.Point - size.Width) / (double)2, (page.Height.Point - size.Height) / (double)2), format);
            }
            document.Save(outputStream, false);
        }

        public bool RenderStream(string streamName, 
                                Microsoft.ReportingServices.OnDemandReportRendering.Report report, 
                                NameValueCollection reportServerParameters, 
                                NameValueCollection deviceInfo, 
                                NameValueCollection clientCapabilities, 
                                ref Hashtable renderProperties,
                                Microsoft.ReportingServices.Interfaces.CreateAndRegisterStream createAndRegisterStream)
        {
            return false;
        }

        public string LocalizedName
        {
            get
            {
                return "PDF with watermark";
            }
        }

        public void SetConfiguration(string configuration)
        {
        }

        public void GetRenderingResource(Microsoft.ReportingServices.Interfaces.CreateAndRegisterStream createAndRegisterStreamCallback, NameValueCollection deviceInfo)
        {
        }
    }

}
