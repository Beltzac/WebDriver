using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Utils
{
    public static class WebDriverExtensions
    {
        public static Action<string, Color?> LogAction { get; set; }

        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            LogAction?.Invoke($"Finding element by {by} with timeout {timeoutInSeconds}s", null);
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                var element = wait.Until(drv => drv.FindElement(by));
                LogAction?.Invoke($"Found element by {by}", Color.Green);
                return element;
            }
            var immediateElement = driver.FindElement(by);
            LogAction?.Invoke($"Found element by {by} (no wait)", Color.Green);
            return immediateElement;
        }

        public static ReadOnlyCollection<IWebElement> FindElements(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            LogAction?.Invoke($"Finding elements by {by} with timeout {timeoutInSeconds}s", null);
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                var elements = wait.Until(drv => (drv.FindElements(by).Count > 0) ? drv.FindElements(by) : null);
                LogAction?.Invoke($"Found {elements?.Count ?? 0} elements by {by}", elements?.Count > 0 ? Color.Green : Color.Orange);
                return elements;
            }
            var immediateElements = driver.FindElements(by);
            LogAction?.Invoke($"Found {immediateElements.Count} elements by {by} (no wait)", immediateElements.Count > 0 ? Color.Green : Color.Orange);
            return immediateElements;
        }

        public static void WaitForElement(this IWebElement element, IWebDriver driver, int timeoutSeconds = 10)
        {
            LogAction?.Invoke($"Waiting for element (timeout: {timeoutSeconds}s)", null);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(d =>
            {
                try
                {
                    var isReady = element.Displayed && element.Enabled;
                    if (isReady) LogAction?.Invoke("Element is ready", Color.Green);
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
