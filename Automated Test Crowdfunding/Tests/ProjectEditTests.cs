using NUnit.Framework;
using System;
using System.IO;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Pages;
using Automated_Test_Crowdfunding.Drivers;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class ProjectEditTests : IDisposable
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
                Path.Combine(reportPath, $"ProjectEditTests_{DateTime.Now:yyyyMMdd_HHmmss}.html"));

            htmlReporter.Config.DocumentTitle = "Project Edit Tests Report - Crowdfunding";
            htmlReporter.Config.ReportName = "Project Update Test Execution Report";
            htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

            _extent = new ExtentReports();
            _extent.AttachReporter(htmlReporter);
            _extent.AddSystemInfo("Application", "Crowdfunding Platform");
            _extent.AddSystemInfo("Module", "Project Edition/Update");
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

                Assert.That(_loginPage.IsLoginSuccessful(), Is.True, "El login de precondición falló.");

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

        // -------------------- TESTS --------------------

        // ----------------------------------------------------
        // HAPPY PATH
        // ----------------------------------------------------
        [Test]
        [Category("Smoke")]
        [Category("Positive")]
        [Category("CRUD")]
        public void Project_Update_HappyPath_ShouldModifyTitleSuccessfully()
        {
            string originalTitle = "Proyecto a Editar " + Guid.NewGuid().ToString().Substring(0, 5);
            string newTitle = "PROYECTO ACTUALIZADO " + Guid.NewGuid().ToString().Substring(0, 5);

            try
            {
                _test.Log(Status.Info, "Starting project update happy path test");
                _test.Log(Status.Info, $"Original title: {originalTitle}");
                _test.Log(Status.Info, $"New title: {newTitle}");

                // --- 1. PRECONDICIÓN: Crear un proyecto para editar ---
                _test.Log(Status.Info, "PRECONDITION: Creating project to be edited");
                _projectListPage.NavigateToProjectsSection();

                _test.Log(Status.Info, "Filling creation form with original data");
                _createPage.FillForm(originalTitle, "Descripción original", "5000", "01-12-2025", "02-12-2025", "2", "Active");
                _createPage.SubmitForm();
                _createPage.GetAndAcceptAlertText();

                Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True,
                    "Precondición fallida: El proyecto original no existe.");
                _test.Log(Status.Pass, "Project created successfully for editing");

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_AfterCreate");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "After Original Project Creation");
                }

                // --- 2. ACCIÓN: Editar el proyecto ---
                _test.Log(Status.Info, "ACTION: Starting project edit");
                string projectId = _projectListPage.GetProjectId(originalTitle);
                _test.Log(Status.Info, $"Project ID: {projectId}");

                _test.Log(Status.Info, "Clicking edit button");
                _projectListPage.ClickEdit(projectId);

                screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_EditMode");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Edit Form Loaded");
                }

                _test.Log(Status.Info, "Filling form with updated data");
                _createPage.FillForm(newTitle, "Nueva descripción editada", "6000", "01-12-2025", "03-12-2025", "4", "Active");

                _test.Log(Status.Info, "Submitting update form");
                _createPage.SubmitForm();

                string alertMessage = _createPage.GetAndAcceptAlertText();
                _test.Log(Status.Info, $"Alert message: {alertMessage}");

                // --- 3. ASERSIONES ---
                _test.Log(Status.Info, "ASSERTIONS: Verifying update results");

                _test.Log(Status.Info, "Assertion 1: Verify success message");
                Assert.That(alertMessage, Contains.Substring("actualizado exitosamente"),
                    "La alerta de éxito de la actualización no apareció o no contiene el mensaje esperado.");
                _test.Log(Status.Pass, "Success message verified");

                _test.Log(Status.Info, "Assertion 2: Verify old title no longer exists");
                Assert.That(_projectListPage.ProjectExists(originalTitle), Is.False,
                    "El proyecto antiguo sigue visible después de la edición.");
                _test.Log(Status.Pass, "Old title removed from list");

                _test.Log(Status.Info, "Assertion 3: Verify new title exists");
                Assert.That(_projectListPage.ProjectExists(newTitle), Is.True,
                    "El proyecto actualizado no aparece con el nuevo título.");
                _test.Log(Status.Pass, "New title appears in list");

                _test.Log(Status.Pass, $"Project successfully updated from '{originalTitle}' to '{newTitle}'");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        // ----------------------------------------------------
        // NEGATIVE TEST
        // ----------------------------------------------------
        [Test]
        [Category("Negative")]
        public void Project_Update_Negative_ShouldFailWhenGoalIsMissing()
        {
            string originalTitle = "Proyecto para fallo" + Guid.NewGuid().ToString().Substring(0, 5);
            string originalGoal = "5000";

            try
            {
                _test.Log(Status.Info, "Starting negative test: update with invalid goal");
                _test.Log(Status.Info, $"Original project: {originalTitle}");

                // --- 1. PRECONDICIÓN: Crear un proyecto válido ---
                _test.Log(Status.Info, "PRECONDITION: Creating valid project");
                _projectListPage.NavigateToProjectsSection();

                _createPage.FillForm(originalTitle, "Descripción original", originalGoal, "01-12-2025", "02-12-2025", "2", "Active");
                _createPage.SubmitForm();
                _createPage.GetAndAcceptAlertText();

                Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True,
                    "Precondición fallida: El proyecto original no existe.");
                _test.Log(Status.Pass, "Valid project created successfully");

                string projectId = _projectListPage.GetProjectId(originalTitle);
                _test.Log(Status.Info, $"Project ID: {projectId}");

                // --- 2. ACCIÓN: Intentar editar con meta negativa ---
                _test.Log(Status.Info, "ACTION: Attempting to update with negative goal (-5000)");
                _projectListPage.ClickEdit(projectId);

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_BeforeInvalidUpdate");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Before Invalid Update");
                }

                _createPage.FillForm(originalTitle, "Descripción editada", "-5000", "01-12-2025", "02-12-2025", "2", "Active");
                _createPage.SubmitForm();

                // --- 3. ASERSIONES ---
                _test.Log(Status.Info, "ASSERTIONS: Verifying validation error");

                string alertMessage = _createPage.GetAndAcceptAlertText();
                _test.Log(Status.Info, $"Alert message: {alertMessage}");

                _test.Log(Status.Info, "Assertion 1: Verify Bad Request error");
                Assert.That(alertMessage, Contains.Substring("Bad Request"),
                    "El test negativo falló: la alerta no contenía 'Bad Request' al intentar guardar una Meta de Financiamiento negativa.");
                _test.Log(Status.Pass, "Bad Request error correctly returned");

                _test.Log(Status.Info, "Assertion 2: Verify original project remains unchanged");
                Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True,
                    "El proyecto original fue alterado o eliminado a pesar del error de validación.");
                _test.Log(Status.Pass, "Original project remains unchanged");

                _test.Log(Status.Pass, "System correctly rejected negative funding goal");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }

        // ----------------------------------------------------
        // BOUNDARY TEST
        // ----------------------------------------------------
        [Test]
        [Category("Boundary")]
        [Category("Positive")]
        public void Project_Update_Boundary_ShouldAcceptMaxTitleLength()
        {
            const int MAX_LENGTH = 100;
            string originalTitle = "Base Edit Limit " + Guid.NewGuid().ToString().Substring(0, 5);
            string longTitle = new string('A', MAX_LENGTH);

            try
            {
                _test.Log(Status.Info, "Starting boundary test: maximum title length");
                _test.Log(Status.Info, $"Max title length: {MAX_LENGTH} characters");
                _test.Log(Status.Info, $"Original title: {originalTitle}");

                // --- 1. PRECONDICIÓN: Crear un proyecto ---
                _test.Log(Status.Info, "PRECONDITION: Creating base project");
                _projectListPage.NavigateToProjectsSection();

                _createPage.FillForm(originalTitle, "Descripción", "5000", "01-12-2025", "02-12-2025", "2", "Active");
                _createPage.SubmitForm();
                _createPage.GetAndAcceptAlertText();

                string projectId = _projectListPage.GetProjectId(originalTitle);
                _test.Log(Status.Info, $"Project ID: {projectId}");
                _test.Log(Status.Pass, "Base project created successfully");

                // --- 2. ACCIÓN: Editar con el máximo de caracteres ---
                _test.Log(Status.Info, $"ACTION: Updating with {MAX_LENGTH}-character title");
                _projectListPage.ClickEdit(projectId);

                var screenshotPath = TakeScreenshot($"{TestContext.CurrentContext.Test.Name}_BeforeMaxLength");
                if (!string.IsNullOrEmpty(screenshotPath))
                {
                    _test.AddScreenCaptureFromPath(screenshotPath, "Before Max Length Update");
                }

                _test.Log(Status.Info, "Filling form with maximum length title");
                _createPage.FillForm(longTitle, "Descripción editada", "5000", "01-12-2025", "02-12-2025", "2", "Active");
                _createPage.SubmitForm();
                _createPage.GetAndAcceptAlertText();

                // --- 3. ASERSIONES ---
                _test.Log(Status.Info, "ASSERTIONS: Verifying max length title was saved");

                Assert.That(_projectListPage.ProjectExists(longTitle), Is.True,
                    "El proyecto no se guardó con el título de longitud máxima.");

                _test.Log(Status.Pass, $"Project successfully updated with {MAX_LENGTH}-character title");
                _test.Log(Status.Pass, "System accepts maximum title length boundary");
            }
            catch (Exception ex)
            {
                _test.Log(Status.Fail, "Exception occurred: " + ex.Message);
                throw;
            }
        }
    }
}