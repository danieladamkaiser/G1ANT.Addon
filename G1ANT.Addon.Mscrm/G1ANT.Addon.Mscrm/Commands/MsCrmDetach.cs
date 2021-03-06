﻿/**
*    Copyright(C) G1ANT Ltd, All rights reserved
*    Solution G1ANT.Addon, Project G1ANT.Addon.Mscrm
*    www.g1ant.com
*
*    Licensed under the G1ANT license.
*    See License.txt file in the project root for full license information.
*
*/
using System;
using G1ANT.Language;


namespace G1ANT.Addon.Mscrm
{
    [Command(Name = "mscrm.detach",Tooltip = "This command disconnect from instance of CRM attached by 'mscrm.attach'.")]
    class MsCrmDetachCommand : Command
    {
        public class Arguments : CommandArguments
        {
            [Argument]
            public VariableStructure Result { get; set; } = new VariableStructure("result"); 
        }
       public MsCrmDetachCommand(AbstractScripter scripter) : base(scripter)
        { }
        public void Execute(Arguments arguments)
        {
            try
            {
                if (MsCrmManager.CurrentCRM != null)
                {
                    MsCrmManager.Detach(MsCrmManager.CurrentCRM);
                }
                Scripter.Variables.SetVariableValue(arguments.Result.Value, new BooleanStructure(true));
            }
            catch
            {
                throw new ApplicationException("Unable to attach to CRM");
            }
        }
    }
}
