using System.Drawing;
using System.Globalization;
using System.Numerics;
using Chroma;
using Chroma.Diagnostics.Logging;
using Chroma.Graphics;
using Chroma.Input;
using Chroma.FlexTerm;
using Chroma.SabreVGA;
using Color = Chroma.Graphics.Color;

namespace SharpApple;
public class EmuMain : Game
{
    private RenderTarget _target;
    public static Terminal Terminal = new Terminal(
        new Vector2(20, 20),
        new Size(320, 192),
        TerminalFont.IBM_CGA_8x8
    );
    private static Terminal _menu = new Terminal(
        new Vector2(20, 20),
        new Size(320, 192),
        TerminalFont.IBM_CGA_8x8
    );

    private Memory _mem;
    private MOS6502 _cpu;

    public static Log Log { get; } = LogManager.GetForCurrentAssembly();

    private bool _isInMenu;
    private byte _selection;
    private ushort _addr, _addrEnd;
    private Inputs _input;

    public EmuMain()
        : base(new(false, false))
    {
        ushort ramSize = 16384;
        ushort width = 1024;
        ushort height = 768;
        // parse commandline
        string[] commandline = Environment.CommandLine.Split(" ");
        for (int i = 0; i < commandline.Length - 1; i++)
        {
            if (commandline[i] == "--ram" || commandline[i] == "-r")
                ramSize = ushort.Parse(commandline[i + 1]);
            if (commandline[i] == "--height" || commandline[i] == "-h")
                height = ushort.Parse(commandline[i + 1]);
            if (commandline[i] == "--width" || commandline[i] == "-w")
                width = ushort.Parse(commandline[i + 1]);
        }

        Window.Title = "SharpApple";
        Window.Mode.SetWindowed(width, height);
        _target = new RenderTarget(360, 232);
        _target.FilteringMode = TextureFilteringMode.NearestNeighbor;
        _target.VirtualResolution = new Size(Window.Width, Window.Height);
        FixedTimeStepTarget = 60; // set internal clock to 60 hz, this way screen will refresh every clock cycle,
                                  // and 6502 will run 17050 (1 MHz / 60 Hz) cycles every frame

        //initialize emulated machine
        _mem = new Memory(ramSize, File.ReadAllBytes("apple1.rom"), File.ReadAllBytes("basic.rom"));
        _cpu = new MOS6502(_mem);
        Terminal.EchoInput = false;
        Terminal.VgaScreen.ActiveForegroundColor = Color.Lime;

        _menu.EchoInput = true;
        _menu.VgaScreen.ActiveForegroundColor = Color.Lime;
        _menu.InputReceived += MenuOnInputReceived;
        Terminal.Write($"SharpApple - Apple 1 emulator\n" +
                       $"Press 'Tab' to access menu\n" +
                       $"{_mem.RamSize/1024}K RAM\n");

        //Log.Debug($"Stack pointer is at {_cpu.state.s.ToString("X")}");
        _cpu.Reset();
    }


    protected override void TextInput(TextInputEventArgs e)
    {
        if (!_isInMenu)
        {
            foreach (char chr in e.Text)
            {
                _mem.Kbd = Char.ToUpper(chr);
            }
        }
        else
        {
            _menu.TextInput(e);
        }
    }

    protected override void Update(float delta)
    {
        (_isInMenu ? _menu : Terminal).Update(delta);
        Terminal.VgaScreen.Cursor.IsVisible = true;
        (_isInMenu ? _menu : Terminal).VgaScreen.Cursor.Color = Color.Lime;
    }

    protected override void Draw(RenderContext context)
    {
        //TODO: Render image
        context.RenderTo(_target, (ctx, _) =>
        {
            (_isInMenu ? _menu : Terminal).Draw(ctx);
            if (_isInMenu)
            {
                if (_input == Inputs.None) RenderMenu();
            }
        });
        context.DrawTexture(
            _target,
            Vector2.Zero,
            Vector2.One,
            Vector2.Zero,
            0
        );
    }

    protected override void KeyPressed(KeyEventArgs e)
    {
        if (!_isInMenu)
        {
            switch (e.KeyCode)
            {
                case KeyCode.Return:
                    _mem.Kbd = (char)0x8D;
                    break;
                case KeyCode.Backspace:
                    _mem.Kbd = (char)0x08;
                    break;
                case KeyCode.Escape:
                    _cpu.Reset();
                    break;
            }
        }
        else
        {
            if (e.KeyCode == KeyCode.Down)
            {
                if (_selection == 1) _selection = 0;
                else _selection++;
            }
            else if (e.KeyCode == KeyCode.Up)
            {
                if (_selection == 0) _selection = 1;
                else _selection--;
            }
            else if (e.KeyCode == KeyCode.Return && _input == Inputs.None)
            {
                _menu.VgaScreen.ClearToColor(Color.Lime, Color.Black);
                switch (_selection)
                {
                    case 0:
                        _menu.Write("Address to load into:\n");
                        _input = Inputs.Addr;
                        _menu.ReadLine();
                        break;
                    case 1:
                        _menu.Write("Starting address:\n");
                        _input = Inputs.AddrStart;
                        _menu.ReadLine();
                        break;
                }
            }
            else if (e.KeyCode == KeyCode.Escape && _input != Inputs.None)
            {
                _input = Inputs.None;
            }
            else _menu.KeyPressed(e);
        }

        if (e.KeyCode == KeyCode.Tab)
            _isInMenu = !_isInMenu;
    }

    protected override void FixedUpdate(float delta)
    {
        //TODO: Run CPU clock cycles
        //TODO: Process each character from queue of new characters this frame

        if (!_isInMenu)
        {
            for (int i = 0; i < 17050; i++)
            {
                _cpu.Process();
            }
        }
        (_isInMenu ? _menu : Terminal).FixedUpdate(delta);
    }

    void RenderMenu()
    {
        _menu.VgaScreen.ClearToColor(Color.Lime, Color.Black);
        _menu.Write(_selection == 0 ? "> Load\n" : "Load\n");
        _menu.Write(_selection == 1 ? "> Save\n" : "Save\n");
    }

    private void MenuOnInputReceived(object? sender, TerminalInputEventArgs e)
    {
        switch (_input)
        {
            case Inputs.Addr:
            {
                try
                {
                    _addr = ushort.Parse(e.Text, NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    _menu.Write("*** Format error\n");
                    break;
                }

                if (_addr > _mem.RamSize - 1)
                {
                    _menu.Write("*** Outside of usable RAM\n");
                    break;
                }
                _menu.Write("File to load:\n");
                _input = Inputs.FileOpen;
                _menu.ReadLine();
                break;
            }
            case Inputs.FileOpen:
            {
                byte[] file = File.ReadAllBytes(e.Text);
                if (file.Length > _mem.RamSize - 1 - _addr)
                {
                    _menu.Write("*** File too large\n");
                    break;
                }
                _menu.Write("Loading file...\n");
                Array.Copy(file, 0, _mem.Ram, _addr, file.Length);
                _menu.Write("File loaded\n");
                break;
            }
            case Inputs.AddrStart:
            {
                try
                {
                    _addr = ushort.Parse(e.Text, NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    _menu.Write("*** Format error\n");
                    break;
                }

                if (_addr < 0 || _addr > _mem.RamSize - 1)
                {
                    _menu.Write("*** Outside of usable RAM\n");
                    break;
                }
                _menu.Write("Ending address:\n");
                _input = Inputs.AddrEnd;
                _menu.ReadLine();
                break;
            }
            case Inputs.AddrEnd:
            {
                try
                {
                    _addrEnd = ushort.Parse(e.Text, NumberStyles.HexNumber);
                }
                catch (FormatException)
                {
                    _menu.Write("*** Format error\n");
                    break;
                }

                if (_addrEnd < 0 || _addrEnd > _mem.RamSize - 1)
                {
                    _menu.Write("*** Outside of usable RAM\n");
                    break;
                }

                if (_addrEnd - _addr <= 0)
                {
                    _menu.Write("*** Too small region\n");
                    break;
                }

                _menu.Write("File to write:\n");
                _input = Inputs.FileWrite;
                _menu.ReadLine();
                break;
            }
            case Inputs.FileWrite:
            {
                byte[] region = new byte[_addrEnd - _addr + 1];
                _menu.Write("Copying RAM...\n");
                Array.Copy(_mem.Ram, _addr, region, 0, _addrEnd - _addr + 1);
                _menu.Write("Saving to file...\n");
                File.WriteAllBytes(e.Text, region);
                _menu.Write("File saved\n");
                break;
            }
        }
    }
}

public enum Inputs
{
    None,
    Addr,
    FileOpen,
    AddrStart,
    AddrEnd,
    FileWrite
}