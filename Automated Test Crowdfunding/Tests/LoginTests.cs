using NUnit.Framework;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Pages;
using Automated_Test_Crowdfunding.Drivers;
using System;
using System.IO;
using OpenQA.Selenium.Support.Extensions;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class LoginTests : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private static ExtentReports _extent;
        private ExtentTest _test;

        [OneTimeSetUp]
        public void InitializeReport()
        {
            // Usa la carpeta Reports existente en la raíz del proyecto
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            var reportPath = Path.Combine(projectRoot, "Reports");

            var htmlReporter = new ExtentSparkReporter(
                Path.Combine(reportPath, $"TestReport_{DateTime.Now:yyyyMMdd_HHmmss}.html"));

            htmlReporter.Config.DocumentTitle = "Automated Test Report - Crowdfunding";
            htmlReporter.Config.ReportName = "Login Tests Execution Report";
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Application", "Crowdfunding Platform");
            _extent.AddSystemInfo("Environment", "QA");
            _extent.AddSystemInfo("Tester", Environment.UserName);
        }

        [SetUp]
        public void SetUp()
        {
            _driver = DriverFactory.CreateDriver();
            _loginPage = new LoginPage(_driver);
            _test = _extent.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {
            var outcome = TestContext.CurrentContext.Result.Outcome.Status;
            var testName = TestContext.CurrentContext.Test.Name;

            if (outcome == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                _test.Log(Status.Fail, "Test Failed");
                _test.Log(Status.Fail, TestContext.CurrentContext.Result.Message);

                var screenshotPath = TakeScreenshot(testName);
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Failure Screenshot");
                }
            }
            else if (outcome == NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                _test.Log(Status.Pass, "Test Passed Successfully");

                var screenshotPath = TakeScreenshot(testName);
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Success Screenshot");
                }
            }
            else if (outcome == NUnit.Framework.Interfaces.TestStatus.Skipped)
            {
                _test.Log(Status.Skip, "Test Skipped");
            }

            Dispose();
        }

        [OneTimeTearDown]
        public void FlushReport()
        {
            _extent.Flush();
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver = null;
        }

        private string TakeScreenshot(string testName)
        {
            try
            {
                // Usa la carpeta Screenshots existente en la raíz del proyecto
                var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                var screenshotsDir = Path.Combine(projectRoot, "Screenshots");

                string fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                string filePath = Path.Combine(screenshotsDir, fileName);

                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(filePath);
                TestContext.AddTestAttachment(filePath, "Screenshot");

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving screenshot: " + ex.Message);
                _test.Log(Status.Warning, "Could not capture screenshot: " + ex.Message);
                return null;
            }
        }

        // -------------------- TESTS --------------------

        /// <summary>
        /// 1. Camino feliz: credenciales válidas
        /// </summary>
        [Test]
        [Category("Smoke")]
        [Category("Positive")]
        public void Login_With_Valid_Credentials_Should_Succeed()
        {
            try
            {
                _test.Log(Status.Info, "Starting login test with valid credentials");

                _test.Log(Status.Info, "Navigating to login page");
                _loginPage.GoTo();

                _test.Log(Status.Info, "Entering credentials: admin / MyS34567IO");
                _loginPage.Login("admin", "MyS34567IO");

                _test.Log(Status.Info, "Verifying successful login");
                Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                    "El login debe ser exitoso con credenciales válidas.");

                _test.Log(Status.Pass, "Login successful with valid credentials");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 2. Prueba negativa: credenciales incorrectas
        /// </summary>
        [Test]
        [Category("Negative")]
        public void Login_With_Invalid_Credentials_Should_Fail()
        {
            try
            {
                _test.Log(Status.Info, "Starting login test with invalid credentials");

                _test.Log(Status.Info, "Navigating to login page");
                _loginPage.GoTo();

                _test.Log(Status.Info, "Entering invalid credentials: admin / password_incorrecta");
                _loginPage.Login("admin", "password_incorrecta");

                _test.Log(Status.Info, "Verifying login failure");
                Assert.That(_loginPage.IsLoginSuccessful(), Is.False,
                    "El login NO debe permitir credenciales incorrectas.");

                _test.Log(Status.Pass, "Login correctly rejected invalid credentials");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 3. Prueba de límite: campos vacíos → debe fallar
        /// </summary>
        [Test]
        [Category("Boundary")]
        [Category("Negative")]
        public void Login_With_Empty_Fields_Should_Fail()
        {
            try
            {
                _test.Log(Status.Info, "Starting login test with empty fields");

                _test.Log(Status.Info, "Navigating to login page");
                _loginPage.GoTo();

                _test.Log(Status.Info, "Attempting login with empty username and password");
                _loginPage.Login("", "");

                _test.Log(Status.Info, "Verifying login failure with empty fields");
                Assert.That(_loginPage.IsLoginSuccessful(), Is.False,
                    "El login NO debe permitir campos vacíos.");

                _test.Log(Status.Pass, "Login correctly rejected empty fields");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }
    }
}