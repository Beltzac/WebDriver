using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Utils
{
    public static class WebDriverExtensions
    {
        public static Action<string, Color?> LogAction { get; set; }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            LogAction?.Invoke($"Procurando elemento por {by} com timeout de {timeoutInSeconds}s", null);
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                //wait.IgnoreExceptionTypes(typeof(COMException));
                //var element = wait.Until();

                //IWebElement element = null;
                
                var element = wait.Until(d =>
                {
                    try
                    {
                        return d.FindElement(by);
                    }
                    catch
                    {
                        return null;
                    }
                });

                LogAction?.Invoke($"Elemento encontrado por {by}", Color.Green);
                return element;
            }
            var immediateElement = driver.FindElement(by);
            LogAction?.Invoke($"Elemento encontrado por {by} (sem espera)", Color.Green);
            return immediateElement;
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            LogAction?.Invoke($"Procurando elementos por {by} com timeout de {timeoutInSeconds}s", null);
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                //wait.IgnoreExceptionTypes(typeof(COMException));
                //var elements = wait.Until();


                var elements = wait.Until(d =>
                {
                    try
                    {
                        return (d.FindElements(by).Count > 0) ? d.FindElements(by) : null;
                    }
                    catch
                    {
                        return null;
                    }
                });


                LogAction?.Invoke($"Encontrados {elements?.Count ?? 0} elementos por {by}", elements?.Count > 0 ? Color.Green : Color.Orange);
                return elements;
            }
            var immediateElements = driver.FindElements(by);
            LogAction?.Invoke($"Encontrados {immediateElements.Count} elementos por {by} (sem espera)", immediateElements.Count > 0 ? Color.Green : Color.Orange);
            return immediateElements;
        }

        public static void WaitForElement(this IWebElement element, IWebDriver driver, int timeoutSeconds = 10)
        {
            LogAction?.Invoke($"Aguardando elemento (timeout: {timeoutSeconds}s)", null);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(d =>
            {
                try
                {
                    var isReady = element.Displayed && element.Enabled;
                    if (isReady) LogAction?.Invoke("Elemento está pronto", Color.Green);
                    return isReady;
                }
                catch
                {
                    return false;
                }
            });
        }
    }
}
