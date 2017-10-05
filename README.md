# netDxf
netDxf 2.0.3 Copyright(C) 2009-2017 Daniel Carvajal, Licensed under LGPL
## Description
netDxf is a .net library programmed in C# to read and write AutoCAD dxf files. It supports AutoCad2000, AutoCad2004, AutoCad2007, AutoCad2010,  AutoCad2013, and AutoCad2018 dxf database versions, in both text and binary format.

I tried to keep the use as simple as posible, for example you will not need to fill up the table section with layers, styles or line type definitions. The DxfDocument will take care of that everytime a new item is added and the load and save its done with just one line of code.
If you need more information, you can find the official dxf documentation in
[http://help.autodesk.com/view/ACD/2016/ENU/](http://help.autodesk.com/view/ACD/2016/ENU/)

Code example:

```c#
public static void Main()
   { 
      // by default it will create an AutoCad2000 dxf version
      DxfDocument dxf = new DxfDocument();
      // add your entities here
      dxf.AddEntity(entity);
      // save to file
      dxf.Save(filename);

      // load file
      DxfDocument dxfLoad = DxfDocument.Load(filename);
   } 
```

### Samples and Demos 
Are contained in the source code.
Well, at the moment they are just tests for the work in progress.
### Dependencies and distribution 
* .NET Framework 4.5
## Compiling
To compile the source code you will need Visual Studio 2015.

## Development Status 
Stable.
### Supported entities
* 3dFace
* Arc
* Circle
* Dimensions (aligned, linear, radial, diametric, 3 point angular, 2 line angular, and ordinate)
* Ellipse
* Hatch (including Gradient patterns)
* Image
* Insert (block references and attributes)
* Leader
* Line
* LwPolyline (light weight polyline)
* Mesh
* MLine
* MText
* Point
* PolyfaceMesh
* Polyline
* Ray
* Solid
* Spline
* Text
* Tolerance
* Trace
* Underlay (DGN, DWF, and PDF underlays)
* Wipeout
* XLine (aka construction line)

All entities can be grouped and may contain extended data information.
AutoCad Table entities will be imported as Inserts (block references).
The libray will never be able to read some entities like Regions and 3dSolids, since they depend on proprietary data.
