# PdfCompression
Compress PDF documents with help of ITextSharp and FreeImage third party libs. 
Excellent point to start and customize for your particular cases.

My experience was based on scanned documents mostly black-and-white.
This project could help you to compress files up to 70%

# Settings
Build solution and restore Nuget packages ITextSharp and FreeImage.

1. Setup at first parameters SourceFolder and DestinationFolder at App.config file.
2. Copy FreeImage.dll from "\packages\VVVV.FreeImage.3.15.1.1\build\net40\freeimage\x86\FreeImage.dll" into "bin"
   folder at "PdfCompressorLibrary" and "UnitTests". 
   "FreeImage.dll" is the main C++ lib, which resample, resize images brilliantly.
   
# First launch
Run single test "RunCompression" and point fileName.
Verify compressed file in Destination folder. If quality/compressionLevel were not satisfied you, then try to play 
with "compressionLevel" parameter at "Runner.cs"
