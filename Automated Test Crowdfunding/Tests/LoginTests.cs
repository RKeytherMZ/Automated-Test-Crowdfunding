using NUnit.Framework;
using OpenQA.Selenium;
using Automated_Test_Crowdfunding.Pages;
using Automated_Test_Crowdfunding.Drivers;
using System;
using System.IO;
using OpenQA.Selenium.Support.Extensions;

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

            Dispose();
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver = null;
        }

        private void TakeScreenshot(string testName)
        {
            try
            {
                var screenshotsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                Directory.CreateDirectory(screenshotsDir);

                string filePath = Path.Combine(screenshotsDir,
                    $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

                ((ITakesScreenshot)_driver).GetScreenshot().SaveAsFile(filePath);

                TestContext.AddTestAttachment(filePath, "Screenshot");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving screenshot: " + ex.Message);
            }
        }

        // -------------------- TESTS --------------------

        /// <summary>
        /// 1. Camino feliz: credenciales válidas
        /// </summary>
        [Test]
        public void Login_With_Valid_Credentials_Should_Succeed()
        {
            _loginPage.GoTo();
            _loginPage.Login("admin", "MyS34567IO");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.True,
                "El login debe ser exitoso con credenciales válidas.");
        }


        /// <summary>
        /// 2. Prueba negativa: credenciales incorrectas
        /// </summary>
        [Test]
        public void Login_With_Invalid_Credentials_Should_Fail()
        {
            _loginPage.GoTo();
            _loginPage.Login("admin", "password_incorrecta");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.False,
                "El login NO debe permitir credenciales incorrectas.");
        }


        /// <summary>
        /// 3. Prueba de límite: campos vacíos → debe fallar
        /// </summary>
        [Test]
        public void Login_With_Empty_Fields_Should_Fail()
        {
            _loginPage.GoTo();
            _loginPage.Login("", "");

            Assert.That(_loginPage.IsLoginSuccessful(), Is.False,
                "El login NO debe permitir campos vacíos.");
        }
    }
}

