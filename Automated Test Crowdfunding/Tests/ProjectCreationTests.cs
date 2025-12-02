using NUnit.Framework;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Drivers;
using Automated_Test_Crowdfunding.Pages;
using System;
using System.IO;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class CreateProjectTests : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ProjectsListPage _projectListPage;
        private CreateProjectPage _createPage;
        private static ExtentReports _extent;
        private ExtentTest _test;

        [OneTimeSetUp]
        public void InitializeReport()
        {
            // Usa la carpeta Reports existente en la raíz del proyecto
            var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            var reportPath = Path.Combine(projectRoot, "Reports");

            var htmlReporter = new ExtentSparkReporter(
                Path.Combine(reportPath, $"CreateProjectTests_{DateTime.Now:yyyyMMdd_HHmmss}.html"));

            htmlReporter.Config.DocumentTitle = "Create Project Tests Report - Crowdfunding";
            htmlReporter.Config.ReportName = "Project Creation Test Execution Report";
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Application", "Crowdfunding Platform");
            _extent.AddSystemInfo("Module", "Project Creation");
            _extent.AddSystemInfo("Environment", "QA");
            _extent.AddSystemInfo("Tester", Environment.UserName);
        }

        [SetUp]
        public void SetUp()
        {
            _driver = DriverFactory.CreateDriver();

            _loginPage = new LoginPage(_driver);
            _projectListPage = new ProjectsListPage(_driver);
            _createPage = new CreateProjectPage(_driver);

            _test = _extent.CreateTest(TestContext.CurrentContext.Test.Name);

            try
            {
                _test.Log(Status.Info, "Performing login for test setup");

                // LOGIN FIRST
                _loginPage.GoTo();
                _loginPage.Login("admin", "MyS34567IO");

                Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                    "ERROR CRÍTICO EN SETUP: Falló el inicio de sesión con credenciales válidas. Las pruebas de proyecto no pueden continuar.");

                _test.Log(Status.Pass, "Setup completed: User logged in successfully");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Setup failed: " + ex.Message);
                var screenshotPath = TakeScreenshot("Setup_Failed");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Setup Failure");
                }
                throw;
            }
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

        // ----------------------------------------------------
        // HAPPY PATH
        // ----------------------------------------------------
        [Test]
        [Category("Smoke")]
        [Category("Positive")]
        public void CreateProject_HappyPath_ShouldCreateSuccessfully()
        {
            string projectName = "Proyecto Prueba " + Guid.NewGuid().ToString().Substring(0, 5);

            try
            {
                _test.Log(Status.Info, "Starting happy path test for project creation");
                _test.Log(Status.Info, $"Project name: {projectName}");

                _test.Log(Status.Info, "Navigating to Projects section");
                _projectListPage.NavigateToProjectsSection();

                _test.Log(Status.Info, "Filling project creation form with valid data");
                _createPage.FillForm(
                    projectName,
                    "Descripción válida",
                    "5000",
                    "01-12-2025",
                    "02-12-2025",
                    "2, 4",
                    "Active"
                );

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_FormFilled");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Form Filled");
                }

                _test.Log(Status.Info, "Submitting project creation form");
                _createPage.SubmitForm();

                _test.Log(Status.Info, "Capturing and accepting alert message");
                string alertMessage = _createPage.GetAndAcceptAlertText();
                _test.Log(Status.Info, $"Alert message received: {alertMessage}");

                _test.Log(Status.Info, "Verifying project exists in the list");
                Assert.That(_projectListPage.ProjectExists(projectName), Is.True,
                    $"Error de sincronización: El proyecto '{projectName}' no apareció en la lista después de crearse.");

                _test.Log(Status.Pass, $"Project '{projectName}' created successfully and verified in list");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        // ----------------------------------------------------
        // NEGATIVE TEST
        // Faltan campos obligatorios
        // ----------------------------------------------------
        [Test]
        [Category("Negative")]
        public void CreateProject_Negative_ShouldFail_WhenMissingRequiredFields()
        {
            try
            {
                _test.Log(Status.Info, "Starting negative test: missing required fields");

                _test.Log(Status.Info, "Navigating to Projects section");
                _projectListPage.NavigateToProjectsSection();

                _test.Log(Status.Info, "Attempting to create project with empty title (missing required field)");
                _createPage.FillForm(
                    "",
                    "Descripción válida",
                    "5000",
                    "01-12-2025",
                    "02-12-2025",
                    "2",
                    "Active"
                );

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_BeforeSubmit");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Form with Missing Field");
                }

                _test.Log(Status.Info, "Submitting form with missing required field");
                _createPage.SubmitForm();

                _test.Log(Status.Info, "Capturing alert message");
                string alertMessage = _createPage.GetAndAcceptAlertText();
                _test.Log(Status.Info, $"Alert message received: {alertMessage}");

                _test.Log(Status.Info, "Verifying alert contains 'Bad Request'");
                Assert.That(alertMessage, Contains.Substring("Bad Request"),
                    "El test negativo falló: la alerta no contenía 'Bad Request'.");

                _test.Log(Status.Pass, "System correctly rejected project creation with missing required fields");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        // ----------------------------------------------------
        // BOUNDARY TEST
        // Meta de financiamiento = 0 (límite)
        // ----------------------------------------------------
        [Test]
        [Category("Boundary")]
        [Category("Negative")]
        public void CreateProject_Boundary_ShouldFail_WhenFundingGoalIsZero()
        {
            string randomName = "Proyecto_Limite_" + Guid.NewGuid().ToString().Substring(0, 4);

            try
            {
                _test.Log(Status.Info, "Starting boundary test: funding goal = 0");
                _test.Log(Status.Info, $"Project name: {randomName}");

                _test.Log(Status.Info, "Navigating to Projects section");
                _projectListPage.NavigateToProjectsSection();

                _test.Log(Status.Info, "Filling form with funding goal = 0 (boundary limit)");
                _createPage.FillForm(
                    randomName,
                    "Descripción válida",
                    "0",                 // BOUNDARY LIMIT
                    "01-12-2025",
                    "02-12-2025",
                    "2",
                    "Active"
                );

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_ZeroFunding");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Zero Funding Goal Form");
                }

                _test.Log(Status.Info, "Submitting form with zero funding goal");
                _createPage.SubmitForm();

                _test.Log(Status.Info, "Capturing alert message");
                string alertMessage = _createPage.GetAndAcceptAlertText();
                _test.Log(Status.Info, $"Alert message received: {alertMessage}");

                _test.Log(Status.Info, "Verifying project does NOT exist in the list");
                Assert.That(
                    _projectListPage.ProjectExists(randomName), Is.False,
                    "No se debe permitir meta de financiamiento 0."
                );

                _test.Log(Status.Pass, "System correctly rejected project with zero funding goal");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }
    }
}