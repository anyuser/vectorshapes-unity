# Vector Shapes for Unity

VectorShapes is a simple and easy to use library for drawing vector shapes in Unity.

Created by [Mario von Rickenbach](http://mariov.ch)

![](Docs~/Resources/overview.png?)

## Features
- Draw shapes by hand or from code.
- Supported shape types: Rectangle, Ellipse/Circle, Polygon (closed, open)
- Polygon Point Types: Corner, Bezier, Bezier Continuous, Smooth
- Shapes can be saved in scene or as asset reference.

### Stroke features
- Stroke rendering in shader (fast) or CPU (slow)
- Shader-based stroke rendering works with any number of cameras
- Stroke render types: Screen space pixel, Screen space relative, Shape space
- Stroke corner types: Bevel, Extend and cut, Extend and miter
- Texture modes: Normalized, Absolute
- Stroke points can have independent widths
- Antialiasing can be activated in stroke shader, no full-screen AA required

### Fill features
- Supports plain color fills
- Texture modes: Normalized, Absolute
- Fill material can use any shader


## Compatibility
Works with unity 2019.3+

## Installation
Install this package in the package manager: "+" -> "Add package from git URL":  

``
https://github.com/anyuser/vectorshapes-unity.git
``  

## Getting started
VectorShapes can be used completely without coding. Shapes can be created by adding a Shape and ShapeRenderer component to a GameObject.

### Create shape
Basic shapes can be created by clicking "GameObject/2D Object/XYZ Shape" in the Unity menu bar. Alternatively, a shape can be created by adding a Shape and a Shape Renderer component to a game object.

### Shapes
Shape is a component that can be attached to GameObjects. It defines the properties of a shape and can be rendered by a ShapeRenderer. By default, shape data is directly saved in the object. Alternatively, the shape data can be a referenced from a Shape Asset.

### Shape renderers
Shapes are either rendered by a ShapeRenderer attached to the GameObject, or by a ShapeRenderer in a parent object. If you want to render multiple shapes with the same materials, it's best to create only one renderer and attach Shapes to the transform hierarchy under that ShapeRenderer. If more than one parent object of a Shape has a ShapeRenderer, the closest activated one in the hierarchy is used to render the object.

### Shape assets
Shape Asset is an asset which contains shape data, saved in the project as an asset. It can be referenced by a Shape comonent to reuse content.

## Shape Properties

- Shape types:
    - Rectangle
    - Ellipse/Circle
    - Polygon (closed, open)

- Polygon Point Types:
    - Corner: Corner point
    - Bezier: Bezier point with independent tangents
    - Bezier Continuous: Bezier point with aligned tangents
    - Smooth: Bezier point with automatic tangents

- Stroke render types:
    - Screen space pixel: screen space, 1 stroke width unit is one pixel
    - Screen space relative: screen space, 1 stroke width unit is screen height
    - Shape space: relative to shape z direction

- Stroke corner types:
    - Bevel: simple line segments connected to close the corner
    - Extend and cut: extend until miter limit is reached, cut if above
    - Extend and miter: extend until miter limit is reached, miter if above

- Stroke Texture modes:
    - Normalized: Texture is stretched to the full line length
    - Absolute: Texture is laid onto stroke in absolute object space size

- Stroke Texture offset & scale: Offset & scale textures based on texture mode

- Fill color: Color

- Fill Texture modes:
  - Normalized: Texture is scaled to fill the bounds of the shape and starts in the center of it
  - Absolute: Texture is applied in object space, starting at the pivot point

- Fill Stroke Texture offset & scale: Offset & scale textures based on texture mode

## Optimizing performance

### Performance considerations:
- Shapes are generated meshes. Each time a shape changes, the mesh will be regenerated. Only change shapes in Update when really necessary.
- For best performance, use the line shader included. If the line material is not using the line shader, stroke rendering will fall back to CPU mode.
-  As long as the shape doesn’t change, there isn’t any performance cost in addition to rendering the mesh. The lines are transformed in the vertex shader.
- Changing a filled shape can generate allocation garbage, because the triangulation library (LibTessDotNet) is currently not optimized for that.
- Fill meshes are only rebuilt when the
- Scene view is always rendered in cpu mode, to make the lines selectable.
- LibTessDotNet allocates memory when triangulating meshes.

### CPU mode
- If the stroke material doesn't use the included stroke shader, stroke rendering will fall back to CPU mode.
- In CPU mode, each mesh additionally has to be regenerated each time the camera changes its transformation matrix. Therefore, CPU mode is by magnitudes slower than GPU in almost all cases.
- There is one case where a CPU shape is faster than the shader version: For shapes that don't change, with StrokeRenderType set to ShapeSpace.
- CPU mode only supports one camera, the one set in the shape renderer.


## Known issues / limitations

### Render order
- render order not defined (TODO: Graphics.DrawMesh? materialpropertyblock)

### Corners
- miter mode glitch

### Antialiasing
- Stroke antialiasing is done in shader
- No antialiasing (except builtin) for fills. workaround 1px line
- Antialiasing in shapespace mode is not so nice
- Antialiasing doesn't work well in miter mode in extreme cases
- Antialiasing Y UV for cutted lines

### Variable stroke width & color
- Variable stroke width has to be used with caution: Corners don't react well to all geometries.
- Stroke points can have colors, currently
- linear interpolation mobile
- Stroke gradient show the mesh triangulation in some corner modes. Use "Bevel" corner mode for best gradients.

### Stroke textures
- stroke texture distorted
- textures mapped on bezier curves are distorted
- texture mapped on looping lines are glitchy

### Import & export
- Currently there's no import or export from other formats like SVG.
