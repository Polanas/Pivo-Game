using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using System.Collections;
using Coroutines;

namespace Game;

class Sound
{

    public readonly int buffer;

    public readonly int channels;

    public readonly int bitsPerSample;

    public readonly int sampleRate;

    public readonly byte[] data;

    public Sound(int buffer, int channels, int bitsPerSample, int sampleRate, byte[] data, int source)
    {
        this.buffer = buffer;
        this.channels = channels;
        this.bitsPerSample = bitsPerSample;
        this.sampleRate = sampleRate;
        this.data = data;
    }
}

class SFX
{

    private static SFX _sfx;

    private ALContext _context;

    private ALDevice _device;

    private Dictionary<string, Sound> _sounds = new();

    private int[] _sources = new int[20];

    private int _sourceIndex;

    public unsafe static SFX New()
    {
        _sfx = new SFX();

        _sfx._device = ALC.OpenDevice(null);
        _sfx._context = ALC.CreateContext(_sfx._device, (int*)null);
        ALC.MakeContextCurrent(_sfx._context);

        for (int i = 0; i < _sfx._sources.Length; i++)
            _sfx._sources[i] = AL.GenSource();

        return _sfx;
    }

    public static void Add(string path)
    {
        byte[] soundData;
        int channels, bits, rate;

        using (FileStream stream = new(path, FileMode.Open))
            soundData = Content.LoadWave(stream, out channels, out bits, out rate);

        var name = Path.GetFileNameWithoutExtension(path);
        _sfx._sounds.Add(name, new Sound(AL.GenBuffer(), channels, bits, rate, soundData, AL.GenSource()));
        var sound = _sfx._sounds[name];

        AL.BufferData(sound.buffer, GetSoundFormat(sound.channels, sound.bitsPerSample), ref sound.data[0], sound.data.Length, sound.sampleRate);
    }

    public static void Play(string name, float volume = 1)
    {
        if (!_sfx._sounds.ContainsKey(name))
        {
#if DEBUG
            throw new ArgumentException($"Error: sound with name {name} not found!");
#else
            return;
#endif
        }

        var sound = _sfx._sounds[name];

        _sfx._sourceIndex++;

        AL.SourceStop(_sfx._sources[Math.Min(_sfx._sourceIndex + 1, _sfx._sources.Length-1)]);

        if (_sfx._sourceIndex == _sfx._sources.Length)
            _sfx._sourceIndex = 0;

        AL.GetSource(_sfx._sources[_sfx._sourceIndex], ALGetSourcei.SourceState, out int state);

        if ((ALSourceState)state != ALSourceState.Playing)
            AL.SourceStop(_sfx._sources[_sfx._sourceIndex]);

        AL.Source(_sfx._sources[_sfx._sourceIndex], ALSourcei.Buffer, sound.buffer);
        AL.Source(_sfx._sources[_sfx._sourceIndex], ALSourcef.Gain, volume);

        AL.SourcePlay(_sfx._sources[_sfx._sourceIndex]);
    }

    private static ALFormat GetSoundFormat(int channels, int bits)
    {
        return channels switch
        {
            1 => bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16,
            2 => bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16,
            _ => throw new NotSupportedException("The specified sound format is not supported."),
        };
    }

    ~SFX()
    {
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
    }
}