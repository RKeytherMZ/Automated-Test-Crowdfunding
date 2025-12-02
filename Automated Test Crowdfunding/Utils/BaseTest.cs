using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;

namespace Automated_Test_Crowdfunding.Utils
{
    [SetUpFixture]
    public class TestReportManager
    {
        public static ExtentReports Extent;
        public static ExtentTest Test;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            Directory.CreateDirectory("Reports");
            Directory.CreateDirectory("Screenshots");

            var reporter = new ExtentSparkReporter("Reports/TestReport.html");
            Extent = new ExtentReports();
            Extent.AttachReporter(reporter);
        }

        [OneTimeTearDown]
        public void GlobalTearDown()
        {
            Extent.Flush();
        }
    }

    public class BaseTest
    {
        protected IWebDriver _driver;

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
            _driver.Manage().Window.Maximize();

            TestReportManager.Test = TestReportManager.Extent.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {
            var status = TestContext.CurrentContext.Result.Outcome.Status;
            var error = TestContext.CurrentContext.Result.Message;

            if (status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                string screenshotPath = TakeScreenshot();
                TestReportManager.Test.Fail("❌ Test Failed");
                TestReportManager.Test.Fail(error);
                TestReportManager.Test.AddScreenCaptureFromPath(screenshotPath);
            }
            else
            {
                TestReportManager.Test.Pass("✔ Test Passed");
            }

            _driver.Dispose();
        }

        private string TakeScreenshot()
        {
            string fileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:HH-mm-ss}.png";
            string path = Path.Combine("Screenshots", fileName);

            ITakesScreenshot ts = (ITakesScreenshot)_driver;
            Screenshot screenshot = ts.GetScreenshot();
            screenshot.SaveAsFile(path);

            return path;
        }
    }
}
