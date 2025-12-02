using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using NUnit.Framework;
using System.IO;

namespace Automated_Test_Crowdfunding.Utils
{
    [SetUpFixture]
    public class ReportManager
    {
        public static ExtentReports Extent;
        public static ExtentTest CurrentTest;

        [OneTimeSetUp]
        public void InitReport()
        {
            Directory.CreateDirectory("Reports");
            Directory.CreateDirectory("Screenshots");

            var reporter = new ExtentSparkReporter("Reports/TestReport.html");

            Extent = new ExtentReports();
            Extent.AttachReporter(reporter);
        }

        [OneTimeTearDown]
        public void CloseReport()
        {
            Extent.Flush();
        }
    }
}
