using NUnit.Framework;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Pages;
using Automated_Test_Crowdfunding.Drivers;
using System;
using System.IO;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;



namespace Automated_Test_Crowdfunding.Tests
{
    [TestFixture]
    public class LoginTests : IDisposable
    {
        private IWebDriver _driver;
        private LoginPage _loginPage;

        [SetUp]
        public void SetUp()
        {
            _driver = DriverFactory.CreateDriver();
            _loginPage = new LoginPage(_driver);
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                TakeScreenshot(TestContext.CurrentContext.Test.Name);
            }

            // Llama a Dispose
            Dispose();
        }

        // Nuevo método para satisfacer IDisposable
        public void Dispose()
        {
            // Verifica si el driver existe y luego lo cierra.
            _driver?.Quit();
            _driver = null;
        }

        private void TakeScreenshot(string testName)
        {
            try
            {
                var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                string filePath = Path.Combine(screenshotsDir, $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                var ss = ((ITakesScreenshot)_driver).GetScreenshot();
                ss.SaveAsFile(filePath);

                TestContext.AddTestAttachment(filePath, "Screenshot on Failure");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving screenshot: " + ex.Message);
            }
        }

        // -------------------- TESTS --------------------

        [Test]
        public void Login_With_Valid_Credentials_Should_Succeed()
        {
            _loginPage.GoTo();

            _loginPage.Login("admin", "MyS3cureP@ssword");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                 "El login NO fue exitoso con credenciales válidas.");
        }

        [Test]
        public void Login_With_Invalid_Credentials_Should_Fail()
        {
            _loginPage.GoTo();

            _loginPage.Login("admin", "password_incorrecta");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.False,
                 "El login DEBIÓ fallar pero pasó.");
        }
    }
}
