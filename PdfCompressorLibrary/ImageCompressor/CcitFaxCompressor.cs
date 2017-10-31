using System.Drawing.Imaging;
using System.IO;
using FreeImageAPI;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfCompressorLibrary.Utility;

namespace PdfCompressorLibrary.ImageCompressor
{
    internal class CcitFaxCompressor : BaseCompressor, ICompressElement
    {
        private float _recommendedCompression = 0.55f; //for SthlmHRDom... documents;
        private ImageFormat _imageOutputFormat = ImageFormat.Png; //for T_.., Sthlm.. files 

        public void Compress(PdfObject pdfObject, PdfDictionary pdfImageObject, PdfStamper stamper, int pageNum)
        {
            var stream = (PRStream) pdfImageObject;
            var image = new PdfImageObject(stream);

            var imageBytes = image.GetImageAsBytes();

            using (var ms = new MemoryStream(imageBytes))
            {
                var fileType = FreeImage.GetFileTypeFromStream(ms);
                var sourceBitmap = FreeImage.LoadFromStream(ms, FREE_IMAGE_LOAD_FLAGS.PNG_IGNOREGAMMA, ref fileType);
                var dotnetBitmap = FreeImage.GetBitmap(sourceBitmap);

                iTextSharp.text.Image compressedImage;
                using (var shrinkedBitmap = ShrinkImage(dotnetBitmap, _recommendedCompression))
                {
                    if (shrinkedBitmap == null)
                    {
                        return;
                    }

                    System.Drawing.Image newImage = shrinkedBitmap;
                    if (DefineBitPerPixel(sourceBitmap) <= 8)
                    {
                        newImage = GdiPaletteConverter.ConvertBitmapTo1Or8Bpp(shrinkedBitmap, 1);
                    }

                    using (var msInternal = new MemoryStream())
                    {
                        newImage.Save(msInternal, _imageOutputFormat);
                        var newBytes = msInternal.ToArray();
                        compressedImage = iTextSharp.text.Image.GetInstance(newBytes);
                    }

                    shrinkedBitmap.Dispose();
                    newImage.Dispose();
                }

                dotnetBitmap.Dispose();
                FreeImage.Unload(sourceBitmap);
                sourceBitmap.SetNull();

                PdfReader.KillIndirect(pdfObject);
                stamper.Writer.CompressionLevel = PdfStream.BEST_COMPRESSION;

                var mask = compressedImage.ImageMask;
                if (mask != null)
                {
                    stamper.Writer.AddDirectImageSimple(mask);
                }
                stamper.Writer.AddDirectImageSimple(compressedImage, (PRIndirectReference)pdfObject);
            }
        }

        public void SetCompressionPercent(float compressionPercent)
        {
            _recommendedCompression = compressionPercent;
        }

        public void SetPreferredOutputImageType(ImageFormat format)
        {
            _imageOutputFormat = format;
        }
    }
}
