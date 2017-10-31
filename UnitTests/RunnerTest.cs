using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfCompressorLibrary;

namespace UnitTests
{
    [TestClass]
    public class RunnerTest
    {
        [TestMethod]
        public void RunCompression()
        {
            var filename = new[] { "T_3034-15.pdf" };
            PdfCompressor.Run(filename);
        }
    }
}
