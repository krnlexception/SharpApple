# SharpApple

Not quick, but definitely dirty emulator of Apple 1 computer written in C#

Powered by [Chroma Framework](https://github.com/Chroma-2D/Chroma/)

## Usage
Just start it, to access menu, press 'Tab' at any time.

In menu, you can move around with arrow keys, to select an option, press 'Enter'/'Return', to return to main menu press 'Escape'.

To reset emulated CPU, press 'Escape' while not in menu.

### Commandline args
- `--ram`/`-r` - RAM to allocate to emulated computer. Has to be smaller than 53263.
- `--width`/`-w` and `--height`/`-h` - Width and height of window.

## License stuff
### Libraries used
- CPU emulator: [Omnicrash/EMU6502](https://github.com/Omnicrash/EMU6502) - license included in [SharpApple/MOS6502.cs](SharpApple/MOS6502.cs)
- Framework: [Chroma-2D/Chroma](https://github.com/Chroma-2D/Chroma) - [license](https://github.com/Chroma-2D/Chroma/blob/master/LICENSE.md)
