﻿/**
*    Copyright(C) G1ANT Ltd, All rights reserved
*    Solution G1ANT.Addon, Project G1ANT.Addon.Selenium
*    www.g1ant.com
*
*    Licensed under the G1ANT license.
*    See License.txt file in the project root for full license information.
*
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using System.Threading;
using OpenQA.Selenium.IE;
using System.Runtime.InteropServices;
using G1ANT.Language;

namespace G1ANT.Addon.Selenium
{
    public class SeleniumWrapper
    {
        private IWebDriver webDriver = null;
        private int scriptSeconds = 4;
        private IntPtr mainWindowHandle = IntPtr.Zero;
        AbstractLogger logger = null;

        public class NewPopupWindowHandler
        {
            protected IWebDriver webDriver = null;
            protected int initialWindowHandlesCount = 0;
            protected bool waitForNewWindow = false;
            protected TimeSpan timeout;
            public NewPopupWindowHandler(IWebDriver driver, bool _waitForNewWindow = false, TimeSpan _timeout = new TimeSpan())
            {
                webDriver = driver;
                waitForNewWindow = _waitForNewWindow;
                timeout = _timeout;
                initialWindowHandlesCount = webDriver.WindowHandles.Count;
            }

            ~NewPopupWindowHandler()
            {
                try
                {
                    if (waitForNewWindow == true)
                    {
                        var wait = new WebDriverWait(webDriver, timeout);
                        wait.Until(driver =>
                        {
                            return driver.WindowHandles.Count != initialWindowHandlesCount;
                        });
                        webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
                    }
                    else if (webDriver.WindowHandles.Count != initialWindowHandlesCount)
                    {
                        webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
                    }
                }
                catch
                { }
            }
        }


        public BrowserType BrowserType {get; set;}

        public IntPtr MainWindowHandle
        {
            get
            {
                return mainWindowHandle;
            }
        }

        public int Id { get; set; }

        public string Title
        {
            get
            {
                return webDriver.Title;
            }
        }

        public SeleniumWrapper(IWebDriver webDriver, IntPtr mainWindowHandle, BrowserType type, AbstractLogger scr)
        {
            this.logger = scr;
            this.mainWindowHandle = mainWindowHandle;
            this.webDriver = webDriver;
            this.BrowserType = type;
            webDriver.Manage().Timeouts().AsynchronousJavaScript = new TimeSpan(0, 0, scriptSeconds);
        }

        private IWebElement FindElement(string search, string by, TimeSpan timeout)
        {
            IWebElement element = null;
            ElementSearchBy searchBy;
            if (Enum.TryParse<ElementSearchBy>(by.CapitalizeFirstLetter(), out searchBy) == false)
            {
                throw new ArgumentException("Argument 'By' was not recognized");
            }
            try
            {
                switch (searchBy)
                {
                    case ElementSearchBy.Id:
                        search = search.StartsWith("#") ? search.TrimStart(new char[] { '#' }) : search;
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id(search)));
                        break;
                    case ElementSearchBy.Class:
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.ClassName(search)));
                        break;
                    case ElementSearchBy.CssSelector:
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.CssSelector(search)));
                        break;
                    case ElementSearchBy.Tag:
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.TagName(search)));
                        break;
                    case ElementSearchBy.Xpath:
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(search)));
                        break;
                    case ElementSearchBy.Name:
                        element = new WebDriverWait(webDriver, timeout).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Name(search)));
                        break;
                    case ElementSearchBy.Query:
                        element = new WebDriverWait(webDriver, timeout).Until(CustomExpectedConditions.ElementExistsByJavaScript(search));
                        break;
                    case ElementSearchBy.Jquery:
                        element = new WebDriverWait(webDriver, timeout).Until(CustomExpectedConditions.ElementExistsByJquery(search));
                        break;
                }
            }
            catch
            {
                string errorMessage = $"Timeout occured while waiting for element. Search phrase: '{search}', by: '{by}' {(webDriver is InternetExplorerDriver ? ". It might be necessary to disable protection mode and lower security level in Internet Explorer." : string.Empty)}";
                throw new TimeoutException(errorMessage);
            }
            return element;
        }

        private static string ValidateUrl(string url)
        {
            try
            {                 
                return new UriBuilder(url).ToString();
            }
            catch
            {
                throw new UriFormatException($"Specified url '{url}' is in a wrong format.");
            }
        }

        private void PreCheckCurrentWindowHandle()
        {
            string found = null;
            try
            {
                found = webDriver.WindowHandles.Where(x => x == webDriver.CurrentWindowHandle).FirstOrDefault();
            }
            catch
            { }
            if (found == null)
            {
                webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
            }
        }

        public void Navigate(string url, TimeSpan timeout, bool noWait)
        {
            url = ValidateUrl(url);
            if (!noWait)
            {
                webDriver.Navigate().GoToUrl(url);
                WebDriverWait wait = new WebDriverWait(webDriver, timeout);
                wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }
            else
            {
                try
                {
                    webDriver.JavaScriptExecutor().ExecuteAsyncScript($"window.location.href = '{url}';");
                }
                catch (Exception ex)
                {
                    logger.Log(AbstractLogger.Level.Error,$"Problem while navigating to url: '{url}' :  {ex.Message}");
                }
            }
        }

        public void Refresh()
        {
            webDriver.Navigate().Refresh();
        }

        public void Quit()
        {
            webDriver.Quit();
        }

        public void Dispose()
        {
            webDriver.Dispose();
        }

        public string RunScript(string script)
        {
            NewPopupWindowHandler popupHandler = new NewPopupWindowHandler(webDriver);
            PreCheckCurrentWindowHandle();
            object result = webDriver.JavaScriptExecutor().ExecuteScript(script);
            return result?.ToString() ?? string.Empty;
        }

        public void CloseTab(TimeSpan timeout)
        {
            PreCheckCurrentWindowHandle();
            switch (BrowserType)
            {
                case BrowserType.Edge:
                case BrowserType.InternetExplorer:
                    throw new ApplicationException("CloseTab command is not supported by Edge and Internet Explorer selenium driver.");
                case BrowserType.Firefox:
                case BrowserType.Chrome:
                    RunScript(string.Format($"window.close();"));
                    break;
            }
            webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
        }

        public void NewTab(TimeSpan timeout, string url, bool noWait)
        {
            url = ValidateUrl(url);
            switch (BrowserType)
            {
                case BrowserType.Edge:
                case BrowserType.InternetExplorer:
                    throw new ApplicationException("NewTab command is not supported by Edge and Internet Explorer selenium driver.");
                case BrowserType.Firefox:
                case BrowserType.Chrome:
                    RunScript(string.Format($"window.open('','_blank');"));
                    break;
            }            
            webDriver.SwitchTo().Window(webDriver.WindowHandles.Last());
            if (!string.IsNullOrEmpty(url))
            {
                Navigate(url, timeout, noWait);
            }
        }

        public void ActivateTab(string phrase, string by)
        {
            if (BrowserType == BrowserType.Edge ||
                BrowserType == BrowserType.InternetExplorer)
            {
                throw new ApplicationException("Activating tabs in Edge and Internet Explorer is not supported.");
            }
            by = by.ToLower();
            phrase = phrase.ToLower();
            string originalHandler = webDriver.CurrentWindowHandle;
            foreach (var handler in webDriver.WindowHandles)
            {
                IWebDriver currentHandler = webDriver.SwitchTo().Window(handler);
                switch (by)
                {
                    case "url":
                        if (currentHandler.Url.ToLower().Contains(phrase))
                        {
                            return;
                        }
                        break;

                    case "title":
                        if (currentHandler.Title.ToLower().Contains(phrase))
                        {
                            return;
                        }
                        break;
                    default:
                        throw new ArgumentException("Argument 'by' not recognized. It accepts either 'title' or 'url' value.");
                }
            }
            webDriver.SwitchTo().Window(originalHandler);
            throw new ArgumentException($"Specified tab (by: '{by}, search: '{phrase}') not found.");
        }

        public void AlertPerformAction(string action, TimeSpan timeout)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, timeout);
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.AlertIsPresent());
            var alert = webDriver.SwitchTo().Alert();
            switch (action)
            {
                case "accept":
                    alert.Accept();
                    break;
                case "decline":
                    alert.Dismiss();
                    break;
                default:
                    throw new NotSupportedException($"Action '{action}' is not supported.");
            }
        }

        public void Click(string search, string by, TimeSpan timeout, bool waitForNewWindow = false)
        {
            NewPopupWindowHandler popupHandler = new NewPopupWindowHandler(webDriver, waitForNewWindow, timeout);
            PreCheckCurrentWindowHandle();
            IWebElement elem = FindElement(search, by, timeout);
            elem.Click();
        }

        public void TypeText(string text, string search, string by, TimeSpan timeout)
        {
            PreCheckCurrentWindowHandle();
            IWebElement elem = FindElement(search, by, timeout);
            elem.SendKeys(text);
        }

        public void PressKey(string keyText, string search, string by, TimeSpan timeout)
        {
            NewPopupWindowHandler popupHandler = new NewPopupWindowHandler(webDriver);
            PreCheckCurrentWindowHandle();
            IWebElement elem = FindElement(search, by, timeout);
            string convertedText = typeof(Keys).GetFields().Where(x => x.Name.ToLower() == keyText.ToLower()).FirstOrDefault()?.GetValue(null) as string;
            if (convertedText == null)
            {
                throw new ArgumentException($"Wrong key argument '{keyText}' specified. Please use keys allowed by selenium library.");
            }
            elem.SendKeys(convertedText);
        }

        public string GetAttributeValue(string attributeName, string search, string by, TimeSpan timeout)
        {
            PreCheckCurrentWindowHandle();
            IWebElement element = FindElement(search, by, timeout);
            return element?.GetAttribute(attributeName) ?? string.Empty;
        }

        public void SetAttributeValue(string attributeName, string attributeValue, string search, string by, TimeSpan timeout)
        {
            PreCheckCurrentWindowHandle();
            IWebElement element = FindElement(search, by, timeout);
            element?.SetAttribute(attributeName, attributeValue);
        }

        public void CallFunction(string functionName, object[] arguments, string type, string search, string by, TimeSpan timeout)
        {
            NewPopupWindowHandler popupHandler = new NewPopupWindowHandler(webDriver);
            PreCheckCurrentWindowHandle();
            IWebElement element = FindElement(search, by, timeout);
            element?.CallFunction(functionName, arguments, type);
        }

        public void BringWindowToForeground()
        {
            if (MainWindowHandle != IntPtr.Zero)
            {
                Language.RobotWin32.SetForegroundWindow(MainWindowHandle);
            }
        }
    }
}
