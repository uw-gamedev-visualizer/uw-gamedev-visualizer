# UW Gamedev Visualizer

## Operation
If in editor, open and run the scene `Core/Scenes/Main.unity`.

- Clicking the `<` button will pull out the main menu
  - Clicking the `>` button will make the main menu disappear
  - Clicking a visualizer will toggle it on and off
  - `Modify Scale` will affect the intensity of the visualizers
    - The checkbox toggles if this effect is on or off
    - The slider determines the strength of the effect, left is lower and right is higher
  - `Background` sets the background
    - The checkbox toggles if the background is one of the listed items vs black
    - The dropdown menu changes the background when the effect is on
  - `Audio Device` sets the audio device used by the visualizer
    - See the config section
- The meeting time can be set by typing on the keyboard
  - Press up to two number keys, then enter
    - Meeting time is in minutes
    - 0 is a valid amount of time
  - When the timer gets to 0, "Meeting will begin momentarily" is displayed
- Escape will exit the program

## Configuring audio devices
Currently only Windows 10 is supported.

### Windows 10

#### Stereo Mix
Comes by default on most Windows machines
- Right click the sound icon in the system tray and choose `Sounds`
  - Open the `Recording` tab
    - Enable `Stereo Mix` and ensure it is outputting sound (the right bars will turn green)

#### VB-CABLE
Audio synchronizes better
- Install VB-CABLE (https://vb-audio.com/Cable/)
- Right click the sound icon in the system tray and choose `Sounds`
  - Open the `Playback` tab
    - Remember which audio device is set as default
    - Enable `CABLE Input` and set as default device
  - Open the `Recording` tab
    - Enable `CABLE Output`
    - Right click `CABLE Output` and select `Properties`
      - Open the `Listen` tab
        - Check the box `Listen to this device`
        - Set `Playback through this device` to what was previously the default device

## Creating a visualizer
- Create a new folder in the `Visualizers` folder
- Create a new script in the folder
  - Extend `VisualizerModule`
  - Above the class declaration add `[CreateAssetMenu(menuName = "Visualizers/VisualizerName")]`
  - `Name` is the name shown in the list
  - `Scale` is a boolean that is `true` if the visualizer should fit in the Monado and `false` otherwise
  - `Spawn(Transform transform)` is called when the visualizer is spawned by the main menu
    - `transform` must be the parent of any `GameObject`s the visualizer spawns!
      - For scaled visualizers, the Monado ring is a circle of relative radius 1
  - `UpdateVisuals()` is called every `Update()`
- Add assets in the folder
- Create a `ScriptableObject` asset by right clicking in the folder and going to `Create > Visualizers > VisualizerName`
  - Add configuration details
  - Add the asset to the `VisualizerList` on the object `Core/Scenes/Main.unity > Visualiser`

### Accessing audio information
`VisualizerCore` is a static class and provides access to all the audio data
- `float Sample(int index, FilterType filter)` gets one sample
- `float[] Sample(FilterType filter)` gets all samples
- `float Spectrum(int index)` gets spectrum data for one index
- `float[] Spectrum(int startIndex, int endIndex)` gets a range of spectrum data, including `startIndex` and excluding `endIndex`
- `float[] Spectrum()` gets all the spectrum data


## Old version
https://github.com/ssvegaraju/GDC_Title_Screen
