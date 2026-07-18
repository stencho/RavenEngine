# Raven
A simplified, unified, jack-of-all-trades image editor, where each layer can be a different kind of media, and the UI will change to reflect the currently selected layer(s). Aimed at everyone from amateur to pro, Linux, Windows, or Mac.


## Core layer types
- Raster editor - aiming for feature parity somewhere between Photoshop CS 8.0 and CS6. No AI. Photoshop-style hotkeys.
- Vector editor - as stated. Once again aiming for feature parity roughly on par with early Illustrator versions
- 3D model display + lighting renderer + rasterizer and compositor
- Digital painting focused raster editor - A krita type. Lower priority, not my tempo, but would love to learn.
- Node-based texture editor - Substance Designer my beloved
- Script-based texture editor
- Shader-based texture editor


## Layer attachments
- Cropping masks - attach to any of the above to crop them to a shape
- Node-based layer filters - attach to other layers to do node-based alterations to their colors
- Script-based layer filters
- Shader-based layer filters, with on-demand recompilation

- Blend modes always available to every layer. Add a selection list which displays little micro-previews of each blend mode. Generate these previews in the background over several frames, always keep several differently sized lower-resolution versions of each layer in memory for things like performance while rendering extremely large images


## Major features
- A full 3D game engine built in, technically, because the user interface and window manager system are part of my game engine, Raven. It's designed to handle like an immersive sim, similar to games like Deus Ex, Morrowind (shut up), Arx Fatalis. Hit a button, time freezes, inventory, stats, equipment, etc. appears, drag items onto the world to drop them, that kinda deal. As such:
- A window and form building framework based on Windows Forms.
- A window manager.
- As it turns out, it's also just a good way to develop a bunch of base UI Forms while also building up a robust Lua UI framework

- A system for storing custom shader/script/node filters and textures as custom menu items, with binds assigned if the user wants them. The goal will be to have a very customizable UI, with custom submenus in the top menu, customizable quick access bars below the top menu, and quick access to individual settings when multiple layers are selected, with parts of the UI multiplying themselves (up to a point).

- A VERY customizable UI and keybinds, but very usable out of the box. Full control over UI color scheme, patterns, panel and menu layout, button order, status bar info, etc. either through the settings menu, or the fully auto-commented and auto-structured 'gvars' file. Main drop-down menus contain more than just buttons and submenus. Sliders, color pickers, pictures of cats, whatever. Each menu item reserves some height and sets a width then draws to that region, and the menu sizes itself to the items. This should ultimately be configurable via Lua, with the entire menu system and several other parts of the UI stored in the config, freely editable.


- A combination layer focus system - Select multiple layers, then quickly switch between editing all of them at once, or editing a specific one of them, by using keys to either jump directly between them, or to move up and down the layer stack. Color-coded and named layers, and also numbered in order of selection.

- High resolution image handling - A system for splitting very large images into smaller chunks, and for rendering only the currently visible parts of them, also used aforementioned low as rendering lower resolution versions of each layer instead of full resolution when zoomed out. When doing very large brush edits of very large images, while zoomed out, preview the edit ASAP by editing a far smaller version as though it was the full size version, then handle the actual edit in the background through the thread dispatcher queue.

- Animations would be nice, but not an immediate focus. Once they make it in, video segment importing, frame-by-frame editing, and webm/mp4/av1 exporting should be next. Maybe even perhaps some audio editing with SoundFlow? It would be cool to basically turn the whole thing into an NLE the moment you choose to make an image have more than one frame.


## Immediate needs
- minimize/maximize/close buttons on the MenuStrip, depending on gvars, and maybe platform? also the rest of the menustrip
- a working text editor, probably steal swoop's keyboard text input class, maybe its entire text editor. fixing that prolly beats writing a new one
- a color picker would be cool, a color picker UIWindow made with it would be very cool
- MenuStrip drop-down, replace List<ButtonFlat> with a List<some_class_containing_buttonflat_and_menu_info>
- File importing. Custom file browser? not like I haven't done it several times before, and it's the best way to make it A) match aesthetically, B) not require another library C) be good. I need to support a looooot of different file types
- Layers. Shouldn't be too bad, mostly just need a List<ILayer> in each Canvas and then a chunky interface
- Project saving. Again, not too bad. Might be worth putting together a custom format to bake both the paths and the actual files referenced in each project into a .bird file or something idk lol. that way, if the file exists and changes externally, I can ask the user if they want to update it, and allow manual updating.
- Proper exporting, with sRGB support and such. Look into libraries. Probably gonna need ImageMagick and GhostScript again.
- bezier line drawing. I don't even think it'll be that hard tbh, but making vectors out of them might be. maybe I can use my SDF shape drawing code as a base tho
- FONT RENDERER. probably not too bad not TOO bad not bad. ooohhhh. this one is daunting but required so very required :(

- Might be worth looking into geting Vulkan going cos lmao opengl