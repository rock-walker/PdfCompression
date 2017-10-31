using System.Drawing.Imaging;
using iTextSharp.text.pdf;
using PdfCompressorLibrary.ImageCompressor;

namespace PdfCompressorLibrary
{
    public class ImageCompressorFactory
    {
        private readonly ICompressElement _ccitFaxCompressor;
        private readonly ICompressElement _dctCompressor;
        internal ImageCompressorFactory()
        {
            _ccitFaxCompressor = new CcitFaxCompressor();
            _dctCompressor = new DctCompressor();
        }

        public ICompressElement Create(PdfObject pdfObj, string filename, double? compressionLevel)
        {
            string clearFilter;

            if (pdfObj == null)
            {
                pdfObj = PdfName.CCITTFAXDECODE;
                clearFilter = "";
            }
            else
            {
                clearFilter = pdfObj.ToString().Trim('[', ']');
            }

            if (pdfObj.Equals(PdfName.CCITTFAXDECODE))
            {
                var compressionPercent = compressionLevel ?? SetBestCompressionByDocName(filename);
                if (compressionPercent != null)
                {
                    _ccitFaxCompressor.SetCompressionPercent((float)compressionPercent.Value);
                }
                return _ccitFaxCompressor;
            }

            if (pdfObj.Equals(PdfName.DCTDECODE) || clearFilter.Equals(PdfName.DCTDECODE.ToString())
                || pdfObj.Equals(PdfName.FLATEDECODE)
                || pdfObj.IsArray()) //when we have two filters simultaneously: Flat and DCTD
                //|| pdfObj.Equals(PdfName.JBIG2DECODE)) that filter doesn't supported in .net, iTextSharp, FreeImage
            {
                var compressionPercent = compressionLevel ?? SetBestCompressionByDocName(filename);
                if (compressionPercent != null)
                {
                    _dctCompressor.SetCompressionPercent((float) compressionPercent.Value);
                }

                var preferredImageFormat = SetPreferredOutImageFormat(filename);
                if (preferredImageFormat != null)
                {
                    _dctCompressor.SetPreferredOutputImageType(preferredImageFormat);
                }

                return _dctCompressor;
            }

            return null;
        }

        private static float? SetBestCompressionByDocName(string docName)
        {
            if (docName.StartsWith("Ds_"))
            {
                return 0.37f;
            }
            if (docName.StartsWith("KRS_"))
            {
                return 0.6f;
            }
            if (docName.StartsWith("T_"))
            {
                return 0.37f;
            }
            if (docName.StartsWith("SOU"))
            {
                return 0.65f;
            }
            if (docName.StartsWith("Bet_"))
            {
                return 0.55f;
            }
            if (docName.StartsWith("MALMO_"))
            {
                return 0.5f;
            }
            if (docName.StartsWith("skr"))
            {
                return 0.48f;
            }

            return null;
        }

        private static ImageFormat SetPreferredOutImageFormat(string docName )
        {
            if (docName.StartsWith("SOU"))
            {
                return ImageFormat.Jpeg;
            }

            if (docName.StartsWith("Bet_"))
            {
                return ImageFormat.Jpeg;
            }

            if (docName.StartsWith("MALMO_"))
            {
                return ImageFormat.Jpeg;
            }

            return null;
        }
    }
}
