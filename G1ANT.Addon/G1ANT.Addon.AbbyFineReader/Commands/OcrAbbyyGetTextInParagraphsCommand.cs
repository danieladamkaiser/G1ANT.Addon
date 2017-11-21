﻿





using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FREngine;
using System.Text.RegularExpressions;
using G1ANT.Language;

namespace G1ANT.Addon.Ocr.AbbyyFineReader
{
    [Command(Name = "ocrabbyy.gettextparagraphs", Tooltip = "This command extract paragraphs containing text from specified file")]
    public class OcrAbbyyGetTextInParagraphsCommand : Command
    {
        public class Arguments : CommandArguments
        {
            [Argument(Required = false, Tooltip = "Id of a processed document returned by a call to processfile command. If not specified last processed document is used.")]
            public IntegerStructure DocumentID { get; set; } = null;

            [Argument]
            public TextStructure Result { get; set; } = new TextStructure("result");

 
        }
        public OcrAbbyyGetTextInParagraphsCommand(AbstractScripter scripter) : base(scripter)
        {
        }
        public void Execute(Arguments arguments)
        {
            AbbyyManager manager = AbbyyManager.Instance;
            int docID = arguments.DocumentID == null ? manager.CurentDocumentCount : arguments.DocumentID.Value;
            FineReaderDocument document = manager.GetDocument(docID);

            document.ExtractData();
            List<FineReaderParagraph> paragraphs = document.Paragraphs;

            ListStructure paragraphsList = new ListStructure(new List<Structure>());

            foreach (FineReaderParagraph p in paragraphs)
                paragraphsList.Value.Add(new TextStructure(p.Text));

            Scripter.Variables.SetVariableValue(arguments.Result.Value, paragraphsList);
        }
    }
}
