using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfCompressorLibrary.Utility;

namespace PdfCompressorLibrary.ImageCompressor
{
    internal class DctCompressor : BaseCompressor, ICompressElement
    {
        private float _recommendedCompression = 0.37f;
        private ImageFormat _imageOutputFormat = ImageFormat.Png;

        public void Compress(PdfObject pdfObject, PdfDictionary pdfImageObject, PdfStamper stamper, int pageNum)
        {
            var image = new PdfImageObject((PRStream)pdfImageObject);
            var oldBytes = image.GetImageAsBytes();

            var compressedBitmapInfo = CompressImage(oldBytes, CompressionQuality.Low, pageNum);

            Bitmap bitmap = compressedBitmapInfo.Item1;
            if (compressedBitmapInfo.Item1 == null)
            {
                return;
            }

            iTextSharp.text.Image compressedImage;
            using (var shrinkedBitmap = ShrinkImage(bitmap, _recommendedCompression))
            {
                if (shrinkedBitmap == null)
                {
                    return;
                }

                Image palettedImage = shrinkedBitmap;

                if (compressedBitmapInfo.Item2 != PixelFormat.Undefined)
                {
                    var bitsPerPixel = GetIntPalette(compressedBitmapInfo.Item2);

                    if (bitsPerPixel > 0)
                    {
                        palettedImage = GdiPaletteConverter.ConvertBitmapTo1Or8Bpp(shrinkedBitmap, bitsPerPixel);
                    }
                }

                using (var msInternal = new MemoryStream())
                {
                    //Checked for T_ file, resolution: Png format took less space up to 10% rather JPEG
                    //var newBytes = ConvertImageToBytes(palettedImage, 90);
                    palettedImage.Save(msInternal, _imageOutputFormat);
                    var newBytes = msInternal.ToArray();
                    compressedImage = iTextSharp.text.Image.GetInstance(newBytes);
                }

                shrinkedBitmap.Dispose();
                if (compressedBitmapInfo.Item1 != null)
                {
                    compressedBitmapInfo.Item1.Dispose();
                }
                palettedImage.Dispose();
            }
            
            PdfReader.KillIndirect(pdfObject);
            var mask = compressedImage.ImageMask;
            if (mask != null)
            {
                stamper.Writer.AddDirectImageSimple(mask);
            }
            stamper.Writer.AddDirectImageSimple(compressedImage, (PRIndirectReference)pdfObject);
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
