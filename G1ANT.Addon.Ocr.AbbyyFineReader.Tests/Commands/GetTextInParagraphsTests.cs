﻿using System;
using System.Collections.Generic;
using System.IO;

using G1ANT.Engine;
using G1ANT.Language.Semantic;
using GStruct = G1ANT.Language;

using NUnit.Framework;
using G1ANT.Addon.Ocr.AbbyyFineReader.Tests.Properties;
using System.Reflection;
using G1ANT.Language;

namespace G1ANT.Addon.Ocr.AbbyyFineReader.Tests
{
    [TestFixture]
    public class GetTextInParagraphsTests
    {
        string path = Assembly.GetExecutingAssembly().UnpackResourceToFile(nameof(Resources.document2), "jpg");
        Scripter scripter = new Scripter();

        [OneTimeSetUp]
        public void Initialize()
        {
            System.Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
        }
        [SetUp]
        public void Init()
        {
            Language.Addon addon = Language.Addon.Load(@"G1ANT.Addon.Ocr.AbbyyFineReader.dll");
        }
        [Test, Timeout(AbbyTests.TestsTimeout)]
        public void GetParagraphsTest()
        {
            scripter.RunLine($"ocrabbyy.processfile {SpecialChars.Text}{path}{SpecialChars.Text}");
            List<GStruct.Structure> paragraphs;
            scripter.RunLine($"ocrabbyy.gettextparagraphs result {nameof(paragraphs)}");
            paragraphs = scripter.Variables.GetVariableValue<List<GStruct.Structure>>(nameof(paragraphs));
            Assert.IsNotNull(paragraphs);
            Assert.AreNotEqual(0, paragraphs.Count);
            Assert.AreEqual(8, paragraphs.Count);
            Assert.IsTrue(((GStruct.TextStructure)paragraphs[0]).Value.StartsWith("In 1929 Gustav Tauschek obtained a patent on OCR in Germany, followed"));
            Assert.IsTrue(((GStruct.TextStructure)paragraphs[5]).Value.StartsWith("In about 1965, Reader's Digest and RCA collaborated to build an OCR Document"));
        }
    }
}
