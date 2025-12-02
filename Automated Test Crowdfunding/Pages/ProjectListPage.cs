using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers; // Para usar ExpectedConditions
using System;

namespace Automated_Test_Crowdfunding.Pages
{
    public class ProjectsListPage
    {
        private readonly IWebDriver _driver;
        // Asume que ProjectsContainerLocator está definido arriba, ej: By.Id("projects-container")
        private readonly By ProjectsContainerLocator = By.Id("projects-container");

        public ProjectsListPage(IWebDriver driver)
        {
            _driver = driver;
        }

        // --- Navegación ---
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
                // 🟢 CORRECCIÓN: Usar un selector XPath más permisivo. 
                // Busca la tarjeta completa (div.project-card) que contenga el texto del título en CUALQUIER lugar dentro (//text()).
                By projectCardLocator = By.XPath($"//div[@class='project-card'][.//text()[contains(., '{title}')]]");

                // Si el elemento es visible antes del timeout, devuelve true.
                wait.Until(ExpectedConditions.ElementIsVisible(projectCardLocator));
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                // Si el elemento no es visible en el timeout, devolvemos false.
                return false;
            }
        }

        public bool ProjectDoesNotExist(string title, int timeoutInSeconds = 5)
        {
            // Espera a que el proyecto desaparezca de la lista
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));

            By projectCardLocator = By.XPath($"//div[@class='project-card'][.//text()[contains(., '{title}')]]");

            try
            {
                // Espera hasta que el elemento NO esté presente en el DOM
                return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(projectCardLocator));
            }
            catch (WebDriverTimeoutException)
            {
                // Si el elemento sigue visible después del timeout, falla esta espera, devolvemos false (sigue existiendo).
                return false;
            }
        }

        public string GetProjectId(string title)
        {
            // 1. 🟢 SINCRONIZACIÓN: Asegurarse de que el proyecto existe antes de buscar el ID.
            if (!ProjectExists(title, 10)) // Damos un poco más de tiempo aquí, por si acaso.
            {
                throw new NoSuchElementException($"No se pudo encontrar el proyecto con título '{title}' para obtener su ID.");
            }

            // 2. Localizar la tarjeta completa usando el selector robusto (el que sabemos que ya cargó).
            By projectCardLocator = By.XPath($"//div[@class='project-card'][.//text()[contains(., '{title}')]]");
            IWebElement card = _driver.FindElement(projectCardLocator);

            // 3. Obtener el ID
            return card.FindElement(By.ClassName("delete-btn"))
                       .GetAttribute("data-id");
        }

        // --- Métodos de Interacción ---

        public void ClickEdit(string id)
        {
            // ... (El resto del código de ClickEdit es correcto y lo mantenemos) ...
            By editButtonLocator = By.CssSelector($"button.edit-btn[data-id='{id}']");

            // 1. Click en el botón de edición
            WebDriverWait waitClick = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
            waitClick.Until(ExpectedConditions.ElementToBeClickable(editButtonLocator)).Click();

            // 2. SINCRONIZACIÓN CLAVE: Esperar a que el campo Título tenga datos.
            WebDriverWait waitDataLoad = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            By titleInputLocator = By.Id("title");

            waitDataLoad.Until(d =>
            {
                IWebElement titleElement = d.FindElement(titleInputLocator);
                // Espera hasta que el campo NO esté vacío.
                return !string.IsNullOrEmpty(titleElement.GetAttribute("value"));
            });
        }

        public void ClickDelete(string id)
        {
            By deleteButtonLocator = By.CssSelector($"button.delete-btn[data-id='{id}']");
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));

            // 1. Click en el botón de eliminar
            wait.Until(ExpectedConditions.ElementToBeClickable(deleteButtonLocator)).Click();

            // 2. MANEJAR LA ALERTA DE CONFIRMACIÓN (confirm())
            try
            {
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("Error de sincronización: La alerta de confirmación NO apareció.");
            }

            // 3. MANEJAR LA ALERTA DE ÉXITO (alert())
            string successMessage;
            try
            {
                IAlert successAlert = wait.Until(ExpectedConditions.AlertIsPresent());
                successMessage = successAlert.Text;
                successAlert.Accept();

                if (!successMessage.Contains("eliminado exitosamente"))
                {
                    throw new Exception($"El proyecto fue eliminado, pero el mensaje de éxito no fue el esperado: {successMessage}");
                }
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception("Error de sincronización: La alerta de 'eliminado exitosamente' NO apareció.");
            }
        }
    }
}


