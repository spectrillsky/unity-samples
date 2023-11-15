Changelog
All notable changes to this package will be documented in this file.
The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.7.0]
* Added Vertex Snapping
* Updated InputHandler to use new Input System

## [0.7.1]
* Fixed error when markers weren't set
* Changed the way levelobject guid were created, they can now be manually created through the inspector
* Fixed issue where grid lattice wasn't being made properly
* Virtualized many methods in the module to allow be override for project specific purposes
* Created additional events that can be listened to
* General improvements to Settings
* Removed LevelLoader because it wasn't deserving of its own class.
* Level class now has Clean method that will remove any LOs that aren't being referenced by the data
* Strict grid placement is now a GridPlacement type instead of a global setting
* Create and Destroy placeholder are now methods in LevelObject
* Created placeholder events that can now be listened to for implementing custom project logic for valid/invalid placements
* Fixed bug where the raycasting method was sorting the wrong way

## [0.7.2]
* Changed the context to Cursor.PlaceObject event to be the new levelobject. Was previously the placeholder.
* Made LevelGrid's CreateLatice and DestroyLattice methods public
* Created LevelGrid.ToggleLattice method 

## [0.7.3]
* Fixed the lattice transform to be just above the grid
* Updated placeholder to change skinnedmeshrenderer materials also
* Fix a bug in cursor that happens when the level is unloaded
* Opened up cursor methods for public use

## [0.7.5]
* Fixed bug where cursor position clamp method was erroring
* Changed default LevelData field values

## [0.7.6]
* Updated default camera logic
* Updated default UI
* Various QOL updates

## [0.7.7]
* Fixed bug in loadlevel method where currentlevel wasn't being set correctly

## [0.7.8]
* Renamed the Cursor.placingResource event to Cursor.createPlaceholder and updated the levelobject being passed in the event

## [0.7.9]
* Fixed issue where cursor marker visuals were still showing even if disabled

## [0.8.0]
* Fixed issue where builds were failing because of unused referenced libaries

## [0.8.1]
* Updated leveleditor input map

## [0.8.5]
* Strict grid placement now takes odd sized levels into account
* Created apply offset method for grid transform logic

## [0.8.8]
* Fixed bug with overlapping logic
* When loading a level in editor, the level objects now maintains link to prefab

## [0.8.9]
* Placeholder no longer change materials for spriterenderers

## [0.8.10]
* Fixed issue where loading map during runtime would lose currentLevel reference
* Fixed issue where levels weren't being saved properly
* Cursor will no longer update if there isn't a current level being edited

## [0.9.0]
* Changed leveldata field names
* Added LevelObject Handles that can be used to select level objects

## [0.9.1]
* Validation system (Level and LevelObject Validators)
* New Validation UI
* Updated GalleryItem features
 
## [0.9.8]
* Added option to always show levelobject editing visuals
* Added global show editing visuals option
* Fixed break with LevelObjectProfile IDs not saving properly

## [0.9.10]
* Changed GalleryItem to use level object DisplayName instead of name on the nameLabel text field.

## [0.9.11]
* Updated LevelObjectType to be hidden from GalleryFilter list

## [0.9.12]
* Fixed issue where editing visuals would disappear on deselect when they shouldn't

## [0.9.14]
* Fixed bug where LOV wasn't updating properly when an LO is destroyed in runtime build.

## [0.9.16]
* Added overloaded functionality for PlaceObject for Networking
 
## [0.10.0]
* Added functionality around using an available 'Level Layout'. This is acts as a scaffolding for the user to build upon.
* Added Layout Option to the Menu, assign your layouts inside your LevelEditorSettings instance.

## [0.10.7]
* Fixed issue where level doesn't create validator if no there is no data reference
* Added setting to disable validation
* Validator bugfixes