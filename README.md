# Unity-2D-Conveyor-System
A simple 2D top down conveyor system asset for use with the Unity Game Engine.
This asset comes with a default set of sprites and animations, but you can create custom themes to suit your game!

Comes with support for the Unity Inspector, as well as a public API to alter the conveyor system behaviour via script.

## Overview
The conveyor system has 3 levels of hierarchy, Conveyor Segment, Conveyor Belt, and Conveyor Group.

* Conveyor Semgents can be grouped into a Conveyor Belt, OR exist by itself. 
* Group multiple Conveyor Belts into a single Conveyor Group.
* Grouping segments and belts together helps the design process as you can overwrite the settings of all the Conveyor Segments in a single click!
  
Furthermore, customise the look and feel of your conveyor system via the Conveyor Theme!

### Installation
Simply download the asset into your project folder!

## Editor

<img src="https://github.com/Specturnal/Unity-2D-Conveyor-System/blob/DemoAssets/Conveyor%20Segment%20Inspector.png" alt="Conveyor Segment" width="350" title="Conveyor Segment">

*The Conveyor Segment inspector*

<img src="https://github.com/Specturnal/Unity-2D-Conveyor-System/blob/DemoAssets/Conveyor%20Belt%20Inspector.png" alt="Conveyor Belt" width="350" title="Conveyor Belt">

*The Conveyor Belt inspector*

<img src="https://github.com/Specturnal/Unity-2D-Conveyor-System/blob/DemoAssets/Conveyor%20Group%20Inspector.png" alt="Conveyor Group" width="350" title="Conveyor Group">

*The Conveyor Group inspector*

With Unity's inspector, you can customise the segments' alignment(direction), activation, and physics settings without touching a single bit of code! Furthermore, Conveyor Belts and Conveyor Groups can overwrite their children's behaviour, essentially a multi editing tool.

## Example Code
```csharp
public class DemoConveyor : MonoBehaviour {
    public ConveyorSegment segment;

    private void OnTriggerEnter2D(Collider2D collision) {
        segment.Activate(true);
    }
    private void OnTriggerExit2D(Collider2D collision) {
        segment.Activate(false);
    }
}
```
The above code demonstrates how to activate a Conveyor Segment when an object enters it, and deactivates it as the object leaves.

## Example Scene
<img src="https://github.com/Specturnal/Unity-2D-Conveyor-System/blob/DemoAssets/Example%20Scene.png" alt="Example Scene" width="350" title="Example Scene">

*The example scene included in the asset*

A simple scene with a lone Conveyor Segment, a Conveyor Belt, a Conveyor Group, and a fun little contraption.
