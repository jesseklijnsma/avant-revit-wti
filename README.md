# Water Technical Installation Generator for Revit
Made by Jesse Klijnsma for [Avant Projects](https://avantprojects.nl)




## Images

![Window](assets/window1.png)

## Usage

### Installation

 - Download the latest [release](/jesseklijnsma/avant-revit-wti/releases/latest)
 - Copy the contents of the zip file to '**%appdata%\\Autodesk\\Revit\\Addins**\\*year*\\'
 - (Re)Start Revit
 - Enable Add-in in popup

## Development
To make changes to the code, open the Solution in Visual Studio

**Make sure you do the following:**
<br>
<br>
Relink the Revit API References

![Add References](assets/dev-setup-1.png)

![Add References](assets/dev-setup-2.png)

**Setup debugging**


Double click *Properties*

![Add References](assets/dev-setup-3.png)
<br>
<br>

Make sure the path to *Revit.exe* is correct.<br>
Also, set the *Command line arguments* to the path of a default project to test in.

![Add References](assets/dev-setup-4.png)

<br>
To automatically copy the compiled add-in to the Revit Add-in location,

make sure this path is the same as the path used in [Installation](#installation)

![Add References](assets/dev-setup-5.png)

