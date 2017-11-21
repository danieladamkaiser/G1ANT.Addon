﻿using System;
using System.Collections.Generic;
using System.IO;

using G1ANT.Engine;
using GStruct = G1ANT.Language;

using NUnit.Framework;
using System.Reflection;
using G1ANT.Addon.Ocr.AbbyyFineReader.Tests.Properties;
using System.Threading;
using G1ANT.Language;

namespace G1ANT.Addon.Ocr.AbbyyFineReader.Tests
{
    [TestFixture]
    public class FindTests
    {
        [OneTimeSetUp]
        public void Initialize()
        {
            Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }
        [SetUp]
        public void Init()
        {
            Language.Addon addon = Language.Addon.Load(@"G1ANT.Addon.Ocr.AbbyyFineReader.dll");
        }
        [Test, Timeout(AbbyTests.TestsTimeout)]
        public void FindTest()
        {
            string path = Assembly.GetExecutingAssembly().UnpackResourceToFile(nameof(Resources.document1), "tif");
            string numberRegex = @"(\d{1,3}(,|\.))?(\d{3}(,|\.))*(\d{1,3})";
            List<GStruct.Structure> allNumbers = null;

            Scripter scripter = new Scripter();
            scripter.RunLine($"ocrabbyy.processfile {SpecialChars.Text}{path}{SpecialChars.Text}");
            scripter.RunLine($"ocrabbyy.find {SpecialChars.Text}{numberRegex}{SpecialChars.Text} result {nameof(allNumbers)}");
            allNumbers = scripter.Variables.GetVariableValue<List<GStruct.Structure>>(nameof(allNumbers));
            Assert.AreNotEqual(0, allNumbers.Count);
        }

        [Test, Timeout(AbbyTests.TestsTimeout)]
        public void FindWindowTitleTest()
        {
            string appTitle = "TestApp";
            Scripter scripter = new Scripter();
            System.Diagnostics.Process testerApp = null;
            List<GStruct.Structure> windowTitleRect = null;

            try
            {
                scripter = new Scripter();
                testerApp = AbbyTests.StartFormTester($"Title {appTitle}");
                IntPtr hTesterAppWindow = testerApp.MainWindowHandle;
                RobotWin32.Rect windowRect = new RobotWin32.Rect();
                RobotWin32.GetWindowRectangle(hTesterAppWindow, ref windowRect);
                int titleBarHeight = 24;

                scripter.RunLine($"ocrabbyy.processscreen area {SpecialChars.Text}{windowRect.Left},{windowRect.Top},{windowRect.Right},{windowRect.Bottom}{SpecialChars.Text}");
                scripter.RunLine($"ocrabbyy.find {SpecialChars.Text}{appTitle}{SpecialChars.Text} result {nameof(windowTitleRect)}");
                windowTitleRect = scripter.Variables.GetVariableValue<List<GStruct.Structure>>(nameof(windowTitleRect));
                Assert.AreNotEqual(0, windowTitleRect.Count);
                System.Drawing.Rectangle titleRect = ((GStruct.RectangleStructure)windowTitleRect[0]).Value;

                Assert.IsTrue(titleRect.Top > 0 & titleRect.Top < titleBarHeight, "Top edge position of found rectangle is incorrect");
                Assert.IsTrue(titleRect.Bottom > 0 & titleRect.Bottom <= titleBarHeight, "Bottom edge position of found rectangle is incorrect");
                Assert.IsTrue(titleRect.Left > 0 & titleRect.Left < 50, "Left edge position of found rectangle is incorrect");
                Assert.IsTrue(titleRect.Right > 0 & titleRect.Right < windowRect.Right);
            }
            finally
            {
                if (testerApp != null && !testerApp.HasExited)
                {
                    testerApp.Kill();
                }
            }
        }
    }
}
