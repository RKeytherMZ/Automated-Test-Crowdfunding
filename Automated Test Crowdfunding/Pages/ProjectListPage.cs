using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System;

namespace Automated_Test_Crowdfunding.Pages
{
    public class ProjectsListPage
    {
        private readonly IWebDriver _driver;

        // 🟢 VARIABLE DECLARADA: Necesaria para el método NavigateToProjectsSection()
        private readonly By ProjectsContainerLocator = By.Id("projects-container");

        public ProjectsListPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // --- Selectores ---
        // Se definen los selectores como propiedades para reutilizar el By.
        private IWebElement ProjectsContainer => _driver.FindElement(ProjectsContainerLocator);


        // --- Métodos de Navegación y Sincronización ---

        // 🟢 MÉTODO MODIFICADO: Solo ejecuta la lógica de navegación de la SPA
        public void NavigateToProjectsSection()
        {
            // Ejecutar el routing de la SPA para cargar la sección 'projects'.
            ((IJavaScriptExecutor)_driver)
                .ExecuteScript("showSection('projects');");

            // 2. Usar WebDriverWait para esperar a que el elemento de la sección sea visible.
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            try
            {
                // Espera a que el contenedor principal de proyectos sea visible.
                wait.Until(ExpectedConditions.ElementIsVisible(ProjectsContainerLocator));
            }
            catch (WebDriverTimeoutException)
            {
                // Manejo de error si la sección no carga a tiempo.
                throw new Exception("Error de sincronización: El contenedor de proyectos no se mostró después de 10 segundos.");
            }
        }

        // --- Métodos de Aserción y Utilidad ---

        public bool ProjectExists(string title, int timeoutInSeconds = 5)
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));

            try
            {
                // Espera a que el texto del título aparezca dentro del contenedor principal de proyectos
                // Usamos el By.XPath para ser más específico y buscar el título dentro de un 'h2' o 'p'.
                // Aquí asumimos que el título es visible dentro del 'project-card'
                By projectTitleLocator = By.XPath($"//div[@class='project-card']/h2[contains(text(), '{title}')]");

                // Si el elemento es visible antes del timeout, devuelve true.
                wait.Until(ExpectedConditions.ElementIsVisible(projectTitleLocator));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                // Si el elemento no es visible en 5 segundos, la espera falla y devolvemos false.
                return false;
            }
        }

        public string GetProjectId(string title)
        {
            // Asumiendo que estamos en la página de proyectos y ya cargó
            var cards = _driver.FindElements(By.ClassName("project-card"));

            foreach (var card in cards)
            {
                if (card.Text.Contains(title))
                {
                    return card.FindElement(By.ClassName("delete-btn"))
                               .GetAttribute("data-id");
                }
            }

            return null;
        }

        // --- Métodos de Interacción ---

        
        public void ClickEdit(string id)
        {
           
            _driver.FindElement(By.CssSelector($"button.edit-btn[data-id='{id}']")).Click();

        }
        public void ClickDelete(string id)
        {
            _driver.FindElement(By.CssSelector($"button.delete-btn[data-id='{id}']")).Click();

           
        }
    }
}
