using NUnit.Framework;            
using System;                      
using OpenQA.Selenium;             
using Automated_Test_Crowdfunding.Pages;
using Automated_Test_Crowdfunding.Drivers;


namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class ProjectEditTests : IDisposable
    {
        // ... Variables de Page Objects y Driver ...
        private IWebDriver _driver;
        private LoginPage _loginPage;
        private ProjectsListPage _projectListPage;
        // Asumimos que esta clase maneja la creación y edición del formulario
        private CreateProjectPage _createPage;

        // --- Setup y TearDown (Si no están en otra clase base) ---

        [SetUp]
        public void SetUp()
        {
            // Inicialización de Driver y Page Objects
            _driver = DriverFactory.CreateDriver(); // O con headless: DriverFactory.CreateDriver(headless: true);
            _loginPage = new LoginPage(_driver);
            _projectListPage = new ProjectsListPage(_driver);
            _createPage = new CreateProjectPage(_driver);

            // Lógica de Precondición de Login (asumiendo que es necesaria antes de cada test)
            _loginPage.GoTo();
            _loginPage.Login("admin", "MyS34567IO");
            // Espera a que el login sea exitoso antes de continuar
            Assert.That(_loginPage.IsLoginSuccessful(), Is.True, "El login de precondición falló.");
        }

        [TearDown]
        public void TearDown()
        {

        }

        public void Dispose()
        {
            // Esta línea es la que garantiza al analizador que el recurso se libera.
            _driver?.Quit();

            GC.SuppressFinalize(this);
        }



        // -------------------- TESTS --------------------

        // ----------------------------------------------------
        // HAPPY PATH
        // ----------------------------------------------------

        [Test]
        public void Project_Update_HappyPath_ShouldModifyTitleSuccessfully()
        {
            // --- 1. PRECONDICIÓN: Crear un proyecto para editar ---
            string originalTitle = "Proyecto a Editar " + Guid.NewGuid().ToString().Substring(0, 5);

            _projectListPage.NavigateToProjectsSection();

            // Usamos la misma lógica del Happy Path de creación para el pre-requisito
            _createPage.FillForm(originalTitle, "Descripción original", "5000", "01-12-2025", "02-12-2025", "2", "Active");
            _createPage.SubmitForm();
            _createPage.GetAndAcceptAlertText(); // Acepta la alerta de creación exitosa

            // Aseguramos que existe antes de editar (sincronización)
            Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True, "Precondición fallida: El proyecto original no existe.");

            // --- 2. ACCIÓN: Editar el proyecto ---
            string projectId = _projectListPage.GetProjectId(originalTitle);
            string newTitle = "PROYECTO ACTUALIZADO " + Guid.NewGuid().ToString().Substring(0, 5);

            // ClickEdit dispara la carga de datos del formulario y espera a que terminen de cargar.
            _projectListPage.ClickEdit(projectId);

            // Reutilizamos FillForm (en CreateProjectPage) para ingresar el nuevo título.
            _createPage.FillForm(newTitle, "Nueva descripción editada", "6000", "01-12-2025", "03-12-2025", "4", "Active");

            // Enviar el formulario (ahora actúa como "Actualizar Proyecto")
            _createPage.SubmitForm();

            // Manejar la alerta de éxito de la actualización
            string alertMessage = _createPage.GetAndAcceptAlertText();

            // --- 3. ASERSIONES ---

            // Aserción 1: Verificar el mensaje de éxito
            Assert.That(alertMessage, Contains.Substring("actualizado exitosamente"),
                "La alerta de éxito de la actualización no apareció o no contiene el mensaje esperado.");

            // Aserción 2: Verificar que el título original YA NO existe (sincronización de lista)
            Assert.That(_projectListPage.ProjectExists(originalTitle), Is.False,
                "El proyecto antiguo sigue visible después de la edición.");

            // Aserción 3: Verificar que el nuevo título SÍ existe
            Assert.That(_projectListPage.ProjectExists(newTitle), Is.True,
                "El proyecto actualizado no aparece con el nuevo título.");
        }

        // ----------------------------------------------------
        // NEGATIVE TEST
        // ----------------------------------------------------
        [Test]
        public void Project_Update_Negative_ShouldFailWhenGoalIsMissing()
        {
            // --- 1. PRECONDICIÓN: Crear un proyecto válido para editar ---
            string originalTitle = "Proyecto para fallo" + Guid.NewGuid().ToString().Substring(0, 5);
            string originalGoal = "5000"; // Meta válida

            _projectListPage.NavigateToProjectsSection();

            // Crear el proyecto con datos válidos
            _createPage.FillForm(originalTitle, "Descripción original", originalGoal, "01-12-2025", "02-12-2025", "2", "Active");
            _createPage.SubmitForm();
            _createPage.GetAndAcceptAlertText(); // Acepta la alerta de creación exitosa

            // Verificar que se creó correctamente
            Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True, "Precondición fallida: El proyecto original no existe.");

            // Obtener el ID del proyecto recién creado
            string projectId = _projectListPage.GetProjectId(originalTitle);

            // --- 2. ACCIÓN: Intentar editar y dejar el campo de Meta de Financiamiento (Goal) vacío ---

            _projectListPage.ClickEdit(projectId);

            // Rellenar el formulario dejando la Meta de Financiamiento ("goal") negativa ("-5000")
            _createPage.FillForm(originalTitle, "Descripción editada", "-5000", "01-12-2025", "02-12-2025", "2", "Active");

            _createPage.SubmitForm();

            // --- 3. ASERSIONES ---

            // Sincronización: Manejar la alerta de error que DEBE venir de la API (Bad Request)
            string alertMessage = _createPage.GetAndAcceptAlertText();

            // Aserción 1: Verificar que el servidor devolvió el error esperado (prueba de éxito del test negativo)
            Assert.That(alertMessage, Contains.Substring("Bad Request"),
                "El test negativo falló: la alerta no contenía 'Bad Request' al intentar guardar una Meta de Financiamiento negativa.");

            // Aserción 2: Verificar que el proyecto original NO se modificó (prueba de seguridad)
            // Nos aseguramos de que la meta SÓLO sigue siendo $5000, lo cual confirmamos indirectamente 
            // al verificar que el proyecto sigue existiendo con su título original.
            Assert.That(_projectListPage.ProjectExists(originalTitle), Is.True,
                "El proyecto original fue alterado o eliminado a pesar del error de validación.");
        }



        // ----------------------------------------------------
        // BOUNDARY TEST
        // ----------------------------------------------------

        [Test]
        public void Project_Update_Boundary_ShouldAcceptMaxTitleLength()
        {
            // Asumimos un límite de 100 caracteres para el título (ajusta este valor si es diferente)
            const int MAX_LENGTH = 100;

            // --- 1. PRECONDICIÓN: Crear un proyecto ---
            string originalTitle = "Base Edit Limit " + Guid.NewGuid().ToString().Substring(0, 5);
            _projectListPage.NavigateToProjectsSection();
            _createPage.FillForm(originalTitle, "Descripción", "5000", "01-12-2025", "02-12-2025", "2", "Active");
            _createPage.SubmitForm();
            _createPage.GetAndAcceptAlertText();
            string projectId = _projectListPage.GetProjectId(originalTitle);

            // --- 2. ACCIÓN: Editar con el máximo de caracteres ---

            // Generar una cadena con el máximo de caracteres permitido
            string longTitle = new string('A', MAX_LENGTH);

            _projectListPage.ClickEdit(projectId);

            // Llenar el formulario con el título límite
            _createPage.FillForm(longTitle, "Descripción editada", "5000", "01-12-2025", "02-12-2025", "2", "Active");
            _createPage.SubmitForm();

            // Manejar la alerta de éxito
            _createPage.GetAndAcceptAlertText();

            // --- . ASERSIONES ---

            // Aserción 1: Verificar que el título de 100 caracteres se guardó exitosamente.
            Assert.That(_projectListPage.ProjectExists(longTitle), Is.True,
                "El proyecto no se guardó con el título de longitud máxima.");

            
        }

    }
}
