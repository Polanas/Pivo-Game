using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Game;

class InputData
{
    [JsonInclude]
    public Dictionary<string, Keys> KeyboardInputs { get; private set; }

    [JsonInclude]
    public Dictionary<string, MouseButton> MouseInputs { get; private set; }

    public InputData()
    {
        KeyboardInputs = new();
        MouseInputs = new();
    }
}

class Input
{

    private static Input _input;

    public InputData InputData { get => _input._inputData; }

    private readonly string _inputPath = MyPath.Join(MyPath.SavesDirectory, "input.json");

    private InputData _inputData;

    public static Input New()
    {
        _input = new Input();

        if (!File.Exists(_input._inputPath))
        {
            var defaultInputData = GetDefaultInputData();
            string serializedInput = JsonSerializer.Serialize(defaultInputData);

            using (var streamWriter = new StreamWriter(_input._inputPath))
            {
                streamWriter.Write(serializedInput);
            }

            _input._inputData = defaultInputData;
        }
        else
        {
            using (var streamReader = new StreamReader(_input._inputPath))
            {
                string serializedInput = streamReader.ReadToEnd();
                var input = JsonSerializer.Deserialize<InputData>(serializedInput);
                _input._inputData = input;
            }
        }

        return _input;
    }

    public static void Save()
    {
        using (var streamWriter = new StreamWriter(_input._inputPath))
        {
            string serializedInput = JsonSerializer.Serialize(_input._inputData);
            streamWriter.Write(serializedInput);
        }
    }

    public static KeyData GetKeyData(string inputName) =>
        new KeyData(inputName, Pressed(inputName), Down(inputName), Released(inputName));
    
    public static bool Down(string inputName)
    {
        if (_input._inputData.KeyboardInputs.ContainsKey(inputName))
            return Keyboard.Down(_input._inputData.KeyboardInputs[inputName]);
        return Mouse.Down(_input._inputData.MouseInputs[inputName]);
    }

    public static bool Pressed(string inputName)
    {
        if (_input._inputData.KeyboardInputs.ContainsKey(inputName))
            return Keyboard.Pressed(_input._inputData.KeyboardInputs[inputName]);
        return Mouse.Pressed(_input._inputData.MouseInputs[inputName]);
    }

    public static bool Released(string inputName)
    {
        if (_input._inputData.KeyboardInputs.ContainsKey(inputName))
            return Keyboard.Released(_input._inputData.KeyboardInputs[inputName]);
        return Mouse.Released(_input._inputData.MouseInputs[inputName]);
    }

    private static InputData GetDefaultInputData()
    {
        var inputData = new InputData();

        inputData.MouseInputs["menuLeft"] = MouseButton.Left;
        inputData.MouseInputs["useTongue"] = MouseButton.Left;
        inputData.KeyboardInputs["left"] = Keys.Left;
        inputData.KeyboardInputs["right"] = Keys.Right;
        inputData.KeyboardInputs["up"] = Keys.Up;
        inputData.KeyboardInputs["down"] = Keys.Down;

        return inputData;
    }
}
