# netDxf
netDxf Copyright(C) 2009-2023 Daniel Carvajal, licensed under MIT License
## Description
netDxf is a .net library programmed in C# to read and write AutoCAD DXF files. It supports AutoCad2000, AutoCad2004, AutoCad2007, AutoCad2010,  AutoCad2013, and AutoCad2018 DXF database versions, in both text and binary format.

The library is easy to use and I tried to keep the procedures as straightforward as possible, for example you will not need to fill up the table section with layers, styles or line type definitions. The DxfDocument will take care of that every time a new item is added.

If you need more information, you can find the official DXF documentation [here](https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-235B22E0-A567-4CF6-92D3-38A2306D73F3).

Code example:

```c#
public static void Main()
{
	// your DXF file name
	string file = "sample.dxf";

	// create a new document, by default it will create an AutoCad2000 DXF version
	DxfDocument doc = new DxfDocument();
	// an entity
	Line entity = new Line(new Vector2(5, 5), new Vector2(10, 5));
	// add your entities here
	doc.Entities.Add(entity);
	// save to file
	doc.Save(file);

	// this check is optional but recommended before loading a DXF file
	DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file);
	// netDxf is only compatible with AutoCad2000 and higher DXF versions
	if (dxfVersion < DxfVersion.AutoCad2000) return;
	// load file
	DxfDocument loaded = DxfDocument.Load(file);
}
```

## Samples and Demos 
Are contained in the source code.
Well, at the moment they are just tests for the work in progress.
## Dependencies and distribution 
Multitarget project, predefined frameworks for Net Framework 4.8, Net Standard 2.1, Net Core 3.1, NET 5.0, and NET 6.0.
Additionally the Net Standard 2.1 build requires a reference to the external library "System.Text.Encoding.CodePages".
## Compiling
Visual Studio 2022. The solution file is still usable by Visual Studio 2019 but it does not support NET 6.0.
netDxf is compatible with any net version from Net Framework 4.0.
If your desired version is not listed among the predefined frameworks manually edit the netdxf.csproj file and set the TargetFrameworks accordingly.
When compiling for any of the Net Framework 4 family make sure that the constant NET4X is defined for your selected framework.
## Development Status 
See [changelog.txt](https://github.com/haplokuon/netDxf/blob/master/doc/Changelog.txt) or the [wiki page](https://github.com/haplokuon/netDxf/wiki) for information on the latest changes.
## Supported DXF entities
* 3dFace
* Arc
* Circle
* Dimensions (aligned, linear, radial, diametric, 3 point angular, 2 line angular, arc length, and ordinate)
* Ellipse
* Hatch (including Gradient patterns)
* Image
* Insert (block references and attributes, dynamic blocks are not supported)
* Leader
* Line
* LwPolyline (light weight polyline)
* Mesh
* MLine
* MText
* Point
* Polyline (Polyline2D, Polyline3D, PolyfaceMesh, and PolygonMesh)
* Ray
* Shape
* Solid
* Spline
* Text
* Tolerance
* Trace
* Underlay (DGN, DWF, and PDF underlays)
* Wipeout
* XLine (aka construction line)

All entities can be grouped.
All DXF objects may contain extended data information. 
AutoCad Table entities will be imported as Inserts (block references).
Both simple and complex line types are supported.
The library will never be able to read some entities like REGIONs, SURFACEs, and 3DSOLIDs, since they depend on undocumented proprietary data.
