# IscFluenceOptimizer
Varian Eclipse Scripting API (Varian Medical Systems, USA) for semi-automatic Irregular surface compensator (ISC) fluence modification 

## Procedures
1. Download the repository.
1. Compile using Visual Studio (2017 or later are recomended).
1. Project for Eclipse Scrpting API Binary Plug-in is *IscFluenceOptimizerBinaryPlugin*. DLLs will be stored in folder bin/x64.
1. Run the binary plugin-in from the Eclipse Scripting interface.
    * This script is only applicable to a treatment plan including beams with fluence maps.
1. Enter a new plan ID, Dose threshold [%] relative to the prescription dose, and the number of steps for fluence reduction, and push OK button. 

## Requirements
* Eclipse version 13.6 (Varian Medical Systems, USA).
    * Probaly, this can be executed in different versions with minor modifications.
* VMS.TPS.Common.Model.Types.dll and VMS.TPS.Common.Model.API.dll, which can be installed using  the Eclipse Scripting API installer.
