using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using FreeImageAPI;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using PdfCompressorLibrary.Infrastructure;

namespace PdfCompressorLibrary
{
    public class PdfCompressor
    {
        private static string SourceFolder { get; set; }
        private static string DestinationFolder { get; set; }

        /// <summary>
        /// Here the parameter you worth to play with:
        /// Accepted values are: [0.01, ..., 0.99]
        /// The value closer to Zero - more compression, less quality
        /// The value closer to One - less compression, more quality
        /// </summary>
        private static float compressionLevel = 0.37f;  

        public static void Run(string[] args)
        {
            ReadConfigSettings();

            if (args == null || args.Length == 0)
            {
                throw new Exception("Please, provide the PDf filename to start compression!");
            }
            var filename = args[0];
            var sourcePath = SourceFolder + filename;
            if (!File.Exists(sourcePath))
            {
                throw new ArgumentException(string.Format("PDF file doesn't exist in file system. Validate the full path: \"{0}\"", sourcePath));
            }

            var destinationPath = DestinationFolder + filename;
            if (!FreeImage.IsAvailable())
            {
                throw new ApplicationException("FreeImage library isn't available");
            }
            var _factory = new ImageCompressorFactory();

            Logger.LogInfo(string.Format("Start processing of file \"{0}\"", filename));
            var timing = Stopwatch.StartNew();

            var pdfReader = new PdfReader(sourcePath);
            using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var pdfStamper = new PdfStamper(pdfReader, fs))
                {
                    var wasCompressed = false;
                    var totalPages = pdfReader.NumberOfPages + 1;
                    Logger.LogInfo(string.Format("Total number of pages is: {0}", totalPages - 1));
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
                }
            }

            Logger.LogInfo(string.Format("Compression of file \"{0}\" completed on \"{1}\"",filename, timing.Elapsed));
            var compression = CalculateCompression(sourcePath, destinationPath);
            Logger.LogInfo(string.Format("File was compressed on {0} %", compression));
        }

        private static void ReadConfigSettings()
        {
            var config = ConfigurationManager.OpenExeConfiguration("PdfCompressorLibrary.dll");
            if (config.AppSettings.Settings.Count == 0)
            {
                var appName = AppDomain.CurrentDomain.BaseDirectory;

                var fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = appName + "\\" + "App.config"
                };
                config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            }

            SourceFolder = config.AppSettings.Settings["sourcePdfFolder"].Value;
            DestinationFolder = config.AppSettings.Settings["destinationPdfFolder"].Value;
        }

        private static int CalculateCompression(string source, string dest)
        {
            FileInfo fi = new FileInfo(source);
            var sourcFileLength = fi.Length;
            fi = new FileInfo(dest);
            var destFileLength = fi.Length;
            return (int)(sourcFileLength / destFileLength) * 100;
        }
    }
}
