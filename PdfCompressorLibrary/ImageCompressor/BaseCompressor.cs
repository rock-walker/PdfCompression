using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using FreeImageAPI;

namespace PdfCompressorLibrary.ImageCompressor
{
    internal class BaseCompressor
    {
        protected Tuple<Bitmap, PixelFormat> CompressImage(byte[] originBytes,
            CompressionQuality imgQuality,
            int pageIndex,
            int recursionChance = 0, //skip for outer call
            float compressionLevel = 0.6f)
        {
            Bitmap compressedBitmap = null;
            PixelFormat sourcePalette = PixelFormat.Undefined;
            using (var sourceMs = new MemoryStream(originBytes))
            {
                FREE_IMAGE_LOAD_FLAGS loadFlag;
                FREE_IMAGE_FORMAT saveFlag;
                FREE_IMAGE_SAVE_FLAGS qualityFlag;

                var imageType = FreeImage.GetFileTypeFromStream(sourceMs);

                switch (imageType)
                {
                    case FREE_IMAGE_FORMAT.FIF_PNG:
                        loadFlag = FREE_IMAGE_LOAD_FLAGS.PNG_IGNOREGAMMA;
                        saveFlag = FREE_IMAGE_FORMAT.FIF_PNG;
                        qualityFlag = FREE_IMAGE_SAVE_FLAGS.PNG_Z_BEST_COMPRESSION;
                        break;
                    case FREE_IMAGE_FORMAT.FIF_JPEG:
                        loadFlag = FREE_IMAGE_LOAD_FLAGS.JPEG_FAST;
                        saveFlag = FREE_IMAGE_FORMAT.FIF_JPEG;
                        qualityFlag = imgQuality == CompressionQuality.Average
                            ? FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYAVERAGE
                            : FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYBAD;
                        break;
                    default:
                        loadFlag = FREE_IMAGE_LOAD_FLAGS.JPEG_FAST;
                        saveFlag = FREE_IMAGE_FORMAT.FIF_JPEG;
                        qualityFlag = FREE_IMAGE_SAVE_FLAGS.JPEG_QUALITYBAD;
                        break;
                }

                var sourceBitmap = FreeImage.LoadFromStream(sourceMs, loadFlag);

                if (!sourceBitmap.IsNull)
                {
                    sourcePalette = FreeImage.GetPixelFormat(sourceBitmap);

                    //var dotnetBitmap = FreeImage.GetBitmap(sourceBitmap);
                    //for TEST
                    //FreeImage.Save(saveFlag, sourceBitmap, string.Format("page{0}.jpg", Guid.NewGuid()),qualityFlag);
                    //if (!sourceBitmap.IsNull)
                    //{
                    using (var convertedMs = new MemoryStream())
                    {
                        FreeImage.SaveToStream(sourceBitmap, convertedMs, saveFlag, qualityFlag);
                        compressedBitmap = new Bitmap(convertedMs);
                        //compressedBytes = convertedMs.ToArray();
                    }

                    FreeImage.Unload(sourceBitmap);
                }
            }

            //return new  compressedBytes;
            return Tuple.Create(compressedBitmap, sourcePalette);
        }

        protected Bitmap ShrinkImage(Image sourceImage, float scaleFactor)
        {
            int newWidth = Convert.ToInt32(sourceImage.Width * scaleFactor);
            int newHeight = Convert.ToInt32(sourceImage.Height * scaleFactor);

            if (newHeight == 0 || newWidth == 0)
            {
                return null;
            }

            var thumbnailBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(thumbnailBitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.InterpolationMode = InterpolationMode.Bilinear;
                g.PixelOffsetMode = PixelOffsetMode.Default;

                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                g.DrawImage(sourceImage, imageRectangle, 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel);
            }
            sourceImage.Dispose();

            return thumbnailBitmap;
        }

        protected byte[] ConvertImageToBytes(Image image, long compressionLevel)
        {
            if (compressionLevel < 0)
            {
                compressionLevel = 0;
            }
            else if (compressionLevel > 100)
            {
                compressionLevel = 100;
            }
            var format = GetImageFormat(image);
            ImageCodecInfo jgpEncoder = GetEncoder(format);

            Encoder myEncoder = Encoder.ScanMethod;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, compressionLevel);
            myEncoderParameters.Param[0] = myEncoderParameter;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, jgpEncoder, myEncoderParameters);
                return ms.ToArray();
            }
        }

        protected int DefineBitPerPixel(FIBITMAP bitmap)
        {
            switch (FreeImage.GetPixelFormat(bitmap))
            {
                case PixelFormat.Format1bppIndexed:
                    return 1;
                case PixelFormat.Format8bppIndexed:
                    return 8;
            }
            return 16;
        }

        protected int GetIntPalette(PixelFormat bitsPerPixel)
        {
            var pixels = 0;

            switch (bitsPerPixel)
            {
                case PixelFormat.Format1bppIndexed:
                    pixels = 1;
                    break;
                case PixelFormat.Format8bppIndexed:
                    pixels = 8;
                    break;
            }

            return pixels;
        }

        private static ImageFormat GetImageFormat(Image image)
        {
            if (ImageFormat.Jpeg.Equals(image.RawFormat))
            {
                return ImageFormat.Jpeg;
            }
            else if (ImageFormat.Png.Equals(image.RawFormat))
            {
                return ImageFormat.Png;
            }
            else if (ImageFormat.Tiff.Equals(image.RawFormat))
            {
                return ImageFormat.Tiff;
            }
            return ImageFormat.Jpeg;
        }

        //standard code from MSDN
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return codecs[6]; //PNG
        }
    }
}
