using NUnit.Framework;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Drivers;
using Automated_Test_Crowdfunding.Pages;
using System;

namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class CreateProjectTests : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ProjectsListPage _projectListPage;
        private CreateProjectPage _createPage;

        [SetUp]
        public void SetUp()
        {
            _driver = DriverFactory.CreateDriver();

            _loginPage = new LoginPage(_driver);
            _projectListPage = new ProjectsListPage(_driver);
            _createPage = new CreateProjectPage(_driver);

            // LOGIN FIRST
            _loginPage.GoTo();
            _loginPage.Login("admin", "MyS34567IO");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
        "ERROR CRÍTICO EN SETUP: Falló el inicio de sesión con credenciales válidas. Las pruebas de proyecto no pueden continuar.");
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver = null;
        }

        // ----------------------------------------------------
        // HAPPY PATH
        // ----------------------------------------------------
        [Test]
        public void CreateProject_HappyPath_ShouldCreateSuccessfully()
        {
            string projectName = "Proyecto Prueba " + Guid.NewGuid().ToString().Substring(0, 5);

            _projectListPage.NavigateToProjectsSection();

            _createPage.FillForm(
                projectName,
                "Descripción válida",
                "5000",
                "01-12-2025",
                "02-12-2025",
                "2, 4",
                "Active"
            );

            _createPage.SubmitForm();

            string alertMessage = _createPage.GetAndAcceptAlertText();

            Assert.That(_projectListPage.ProjectExists(projectName), Is.True,
        $"Error de sincronización: El proyecto '{projectName}' no apareció en la lista después de crearse.");
        }
        

        // ----------------------------------------------------
        // NEGATIVE TEST
        // Faltan campos obligatorios
        // ----------------------------------------------------
        [Test]
        public void CreateProject_Negative_ShouldFail_WhenMissingRequiredFields()
        {
            _projectListPage.NavigateToProjectsSection();

            // Intentar crear sin título
            _createPage.FillForm(
                "",
                "Descripción válida",
                "5000",
                "01-12-2025",
                "02-12-2025",
                "2",
                "Active"
            );

            _createPage.SubmitForm();

            string alertMessage = _createPage.GetAndAcceptAlertText();

            
            Assert.That(alertMessage, Contains.Substring("Bad Request"),
                "El test negativo falló: la alerta no contenía 'Bad Request'.");

           
        }

        // ----------------------------------------------------
        // BOUNDARY TEST
        // Meta de financiamiento = 0 (límite)
        // ----------------------------------------------------
        [Test]
        public void CreateProject_Boundary_ShouldFail_WhenFundingGoalIsZero()
        {
            string randomName = "Proyecto_Limite_" + Guid.NewGuid().ToString().Substring(0, 4);

            _projectListPage.NavigateToProjectsSection();

            _createPage.FillForm(
                randomName,
                "Descripción válida",
                "0",                 // BOUNDARY LIMIT
                "01-12-2025",
                "02-12-2025",
                "2",
                "Active"
            );

            _createPage.SubmitForm();

            string alertMessage = _createPage.GetAndAcceptAlertText();

            Assert.That(
                _projectListPage.ProjectExists(randomName), Is.False,
                "No se debe permitir meta de financiamiento 0."
            );
        }
    }
}
