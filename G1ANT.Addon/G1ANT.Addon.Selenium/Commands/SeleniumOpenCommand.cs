﻿using G1ANT.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G1ANT.Addon.Selenium
{
    [Command(Name = "selenium.open", Tooltip = "Opens new instance of chosen web browser and optionally navigate to specified url address.")]
    public class SeleniumOpenCommand : Command
    {
        public class Arguments : CommandArguments
        {
            [Argument(Required = true, Tooltip = "Defines which web browser we use")]
            public TextStructure Type { get; set; }

            [Argument(Tooltip = "Webpage address to load")]
            public TextStructure Url { get; set; } = new TextStructure(string.Empty);

            [Argument(DefaultVariable = "timeoutselenium")]
            public override int Timeout { get; set; } = (SeleniumSettings.SeleniumTimeout);

            [Argument]
            public BooleanStructure NoWait { get; set; } = new BooleanStructure(false);

            [Argument]
            public TextStructure Result { get; set; } = new TextStructure("result");
        }
        public SeleniumOpenCommand(AbstractScripter scripter) : base(scripter)
        {
        }
        public void Execute(Arguments arguments)
        {
            try
            {
                SeleniumWrapper wrapper = SeleniumManager.CreateWrapper(
                        arguments.Type.Value,
                        arguments.Url?.Value,
                        arguments.Timeout / 1000,
                        arguments.NoWait.Value);
                OnScriptEnd = () =>
                {
                    SeleniumManager.DisposeAllOpenedDrivers();
                    SeleniumManager.RemoveWrapper(wrapper);
                };
                Scripter.Variables.SetVariableValue(arguments.Result.Value, new IntegerStructure(wrapper.Id));
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error occured while opening new selenium instance. Url '{arguments.Url.Value}'. Message: {ex.Message}", ex);
            }
        }
    }
}
