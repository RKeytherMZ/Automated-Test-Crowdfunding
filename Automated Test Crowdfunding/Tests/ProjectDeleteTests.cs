using System;
using System.IO;
using Automated_Test_Crowdfunding.Drivers;
using Automated_Test_Crowdfunding.Pages;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class ProjectDeleteTests : IDisposable
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
                Path.Combine(reportPath, $"ProjectDeleteTests_{DateTime.Now:yyyyMMdd_HHmmss}.html"));

            htmlReporter.Config.DocumentTitle = "Project Delete Tests Report - Crowdfunding";
            htmlReporter.Config.ReportName = "Project Deletion Test Execution Report";
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Application", "Crowdfunding Platform");
            _extent.AddSystemInfo("Module", "Project Deletion");
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

                _loginPage.GoTo();
                _loginPage.Login("admin", "MyS34567IO");

                Assert.That(_loginPage.IsLoginSuccessful(), Is.True, "El login falló.");

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
        }

        [OneTimeTearDown]
        public void FlushReport()
        {
            _extent.Flush();
        }

        public void Dispose()
        {
            _driver?.Quit();
            GC.SuppressFinalize(this);
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

        // ===============================
        // MÉTODO PARA ESPERAR Y ACEPTAR ALERTAS
        // ===============================
        private void WaitAndAcceptAlert()
        {
            try
            {
                _test.Log(Status.Info, "Waiting for alert to appear");
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
                _driver.SwitchTo().Alert().Accept();
                _test.Log(Status.Info, "Alert accepted successfully");
            }
            catch (WebDriverTimeoutException)
            {
                _test.Log(Status.Info, "No alert appeared within 5 seconds");
            }
        }

        // =================================================================
        // MÉTODO NECESARIO PARA QUE SELENIUM NO SE BLOQUEE CON ALERTAS
        // =================================================================
        private void AcceptAlertIfPresent()
        {
            try
            {
                _test.Log(Status.Info, "Checking for alert presence");
                var alert = _driver.SwitchTo().Alert();
                alert.Accept();
                _test.Log(Status.Info, "Alert accepted");
            }
            catch (NoAlertPresentException)
            {
                _test.Log(Status.Info, "No alert present");
            }
        }

        // =================== TESTS ===================
        [Test]
        [Category("Smoke")]
        [Category("Positive")]
        [Category("CRUD")]
        public void Project_Delete_ShouldRemoveProjectFromList()
        {
            string titleToDelete = "Proyecto a Eliminar " + Guid.NewGuid().ToString().Substring(0, 5);

            try
            {
                _test.Log(Status.Info, "Starting project deletion test");
                _test.Log(Status.Info, $"Project to delete: {titleToDelete}");

                // Crear proyecto primero
                _test.Log(Status.Info, "Step 1: Creating project to be deleted");
                _projectListPage.NavigateToProjectsSection();

                _test.Log(Status.Info, "Filling project creation form");
                _createPage.FillForm(
                    titleToDelete,
                    "Descripción breve",
                    "1000",
                    "01/12/2025",
                    "02/12/2025",
                    "2",
                    "Active"
                );

                _test.Log(Status.Info, "Submitting project creation form");
                _createPage.SubmitForm();

                // Screenshot después de crear
                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_AfterCreate");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "After Project Creation");
                }

                // Cerrar alerta
                WaitAndAcceptAlert();

                _test.Log(Status.Info, "Verifying project was created successfully");
                Assert.That(_projectListPage.ProjectExists(titleToDelete), Is.True,
                    $"El proyecto '{titleToDelete}' no se creó correctamente");
                _test.Log(Status.Pass, "Project created and verified in list");

                // Obtener ID y eliminar
                _test.Log(Status.Info, "Step 2: Deleting the project");
                string projectId = _projectListPage.GetProjectId(titleToDelete);
                _test.Log(Status.Info, $"Project ID obtained: {projectId}");

                // Screenshot antes de eliminar
                screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_BeforeDelete");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Before Deletion");
                }

                _test.Log(Status.Info, "Clicking delete button");
                _projectListPage.ClickDelete(projectId);

                // Cerrar alerta de confirmación
                AcceptAlertIfPresent();

                // Screenshot después de eliminar
                screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_AfterDelete");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "After Deletion");
                }

                _test.Log(Status.Info, "Verifying project was deleted from list");
                Assert.That(_projectListPage.ProjectDoesNotExist(titleToDelete), Is.True,
                    $"El proyecto '{titleToDelete}' todavía existe después de eliminarlo");

                _test.Log(Status.Pass, $"Project '{titleToDelete}' successfully deleted and removed from list");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }
    }
}