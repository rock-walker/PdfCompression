using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using PdfCompressorLibrary.Infrastructure;

namespace PdfCompressorLibrary
{
    public class CompressorItextSharp : ICompress
    {
        private readonly ImageCompressorFactory _factory;

        public CompressorItextSharp()
        {
            _factory = new ImageCompressorFactory();
        }

        public void CompressFile(string sourcePath, string destinationPath, string filename, double? compressionLevel)
        {
            using (var pdfReader = new PdfReader(sourcePath))
            {
                Stream fs = null;
                CompressFileImpl(ref fs, pdfReader, destinationPath, filename, compressionLevel);
            }
        }

        public Stream CompressFile(Stream fileStream, string destinationPath, string filename, double? compressionLevel)
        {
            Stream fs = new MemoryStream();
            using (var pdfReader = new PdfReader(fileStream))
            {
                CompressFileImpl(ref fs, pdfReader, destinationPath, filename, compressionLevel);
            }
            fs.Position = 0;
            return fs;
        }

        public void ReportStatistics()
        {
            throw new NotImplementedException();
        }

        private void CompressFileImpl(ref Stream outStream, PdfReader pdfReader, string destinationPath, string filename, double? compressionLevel)
        {
            FileStream fs = null;
            PdfStamper pdfStamper;

            if (outStream == null)
            {
                fs = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                pdfStamper = new PdfStamper(pdfReader, fs);
            }
            else
            {
                pdfStamper = new PdfStamper(pdfReader, outStream);
            }
            var wasCompressed = false;
            var totalPages = pdfReader.NumberOfPages + 1;
            Logger.LogInfo(string.Format("Total number of pages is: {0}.", totalPages - 1));
            for (int i = 1; i < totalPages; i++)
            {
                PdfDictionary page = pdfReader.GetPageN(i);
                PdfDictionary resources = (PdfDictionary)PdfReader.GetPdfObject(page.Get(PdfName.RESOURCES));
                PdfDictionary xobject = (PdfDictionary)PdfReader.GetPdfObject(resources.Get(PdfName.XOBJECT));

                if (xobject != null)
                {
                    foreach (var name in xobject.Keys)
                    {
                        var obj = xobject.Get(name);
                        if (obj.IsIndirect())
                        {
                            var imgObject = (PdfDictionary)PdfReader.GetPdfObject(obj);

                            if (imgObject != null && imgObject.Get(PdfName.SUBTYPE).Equals(PdfName.IMAGE))
                            {
                                var filter = imgObject.Get(PdfName.FILTER);

                                Logger.LogDebug(string.Format("page num: {0}. Filter {1}", i, filter));

                                var compressor = _factory.Create(filter, filename, compressionLevel);
                                if (compressor != null)
                                {
                                    try
                                    {
                                        compressor.Compress(obj, imgObject, pdfStamper, i);
                                    }
                                    catch (UnsupportedPdfException ex)
                                    {
                                        Logger.LogError(string.Format("Exception at file \"{0}\" on page {1}. {2}", filename, i, ex));
                                    }
                                    catch (KeyNotFoundException ex)
                                    {
                                        Logger.LogError(string.Format("Exception at file \"{0}\" on page {1}. {2}", filename, i, ex));
                                    }
                                    catch (InvalidImageException ex)
                                    {
                                        Logger.LogError(string.Format("Exception at file \"{0}\" on page {1}. {2}", filename, i, ex));
                                    }
                                }
                                else
                                {
                                    Logger.LogWarning(string.Format("Skip compression for {0} type of image", filter));
                                }

                                Logger.Log(TraceEventType.Information, string.Format("Image on page {0} compressed successfully", i));
                                wasCompressed = true;
                            }
                        }
                    }
                }
                else
                {
                    pdfReader.SetPageContent(i, pdfReader.GetPageContent(i), PdfStream.BEST_COMPRESSION, true);
                }
            }

            if (!wasCompressed)
            {
                pdfReader.RemoveUnusedObjects();
                pdfReader.RemoveAnnotations();
            }

            if (fs == null)
            {
                pdfStamper.Writer.CloseStream = false;
            }

            pdfStamper.Close();
            pdfStamper.Dispose();

            if (fs != null)
            {
                fs.Close();
            }
        }
    }
}
