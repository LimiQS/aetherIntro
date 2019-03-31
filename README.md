# aetherIntro

A lightweight WPF-based tool for dynamic rendering V-Tuber standby screen.

aetherIntro applies acrylic effects to backgrounds, either image or video. 

aetherIntro supports hardware acceleration for dynamic rendering of video backgrounds.

aetherIntro is developed for AetherPray VTuber Project.



### Screenshot

![](https://raw.githubusercontent.com/LimiQS/aetherIntro/master/aetherIntroPreview.png)



### Usage

##### Keyboard shortcuts

Ctrl + Alt + W or Hold ESC: Quit

F5: Reload configuration



##### Configuration File

*The configuration file is **config.json** in the same directory as the program*

*If you are not able to find the file, you can start the program and then quit, which will automatically generate a default configuration file at the above mentioned location.*

**UseVideo:** `[bool]` Whether to use video as the background.

**DebugMode:** `[bool]` Whether to enable debug mode.

**HideLogo:** `[bool]` Whether to display logo at the center of screen.

**Fullscreen:** `[bool]` Whether to enter full screen mode.

**ImagePath:** `[string]` Path to the background image file.

**VideoPath:** `[string]` Path to the background video file.

**LogoPath:** `[string]` Path to the logo image file. Note that the logo image file must be in SVG format.

**WindowWidth:** `[int]` Width of the window. This field is invalid in full screen mode.

**WindowHeight:** `[int]` Height of the window. This field is invalid in full screen mode.

**TintOpacity:** `[float]` Opacity of the tint layer. Ranged from 0.0 to 1.0

**NoiseOpacity:** `[float]` Opacity of the noise layer. Ranged from 0.0 to 1.0

**VideoVolume:** `[float]` Audio volume of the background video. This field is invalid if the background is an image.

**LogoColorRGBA:** `[float]` RGBA value used to render logo image on the screen. Each component has a value range of 0 - 255. This field is invalid if logo is hidden.



##### Debug Mode

In debug mode, the following behavior will be different from default:

You will be able to move the window around. (Default: Locked)

You can scale the window size, maximize or minimize. (Default: Locked)

You can exit the program by press ESC. (Default: Hold ESC)

The window will not be always topmost.  (Default: Topmost)

Cursor will not be hidden. (Default: Hidden)

Title bar will be displayed. (Default: Hidden)



### License

Academic Public License 1.1
