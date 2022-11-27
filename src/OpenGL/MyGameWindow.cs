#define ENABLE_LOGGING
//#define ENABLE_LOG_SAVE

#region stuff

using Box2DSharp.Dynamics;
using Leopotam.EcsLite.Di;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Game;

class MyGameWindow : GameWindow
{

    public static Vector2i ScreenSize { get; private set; }

    public static Vector2i PixelatedScreenSize { get; private set; }

    public static float FullToPixelatedRatio { get; private set; }

    public static int RefreshRate { get; private set; }

    public static TextWriter LogWriter => _writer;

    public EcsWorld World { get; private set; }

    public SharedData SharedData { get; private set; }

    private EcsSystems _gameSystems;

    private EcsSystems _renderSystems;

    private EcsSystems _physicsSystems;

#if DEBUG
    private double _summedRenderTime;

    private double _lastElapsedRenderTime;

    private int _framesCount;

    private double _averageTime;
#endif

    private FileStream _logFile;

    private const string USELESS_LOG = "will use VIDEO memory as the source for buffer object operations.";

    private const float GRAVITY = 9.8f;

    private const float PTM = 1f / 8f;

    public static TextWriter _writer;

    private static DebugProc _debugProcCallback = DebugCallBack;

    private static GCHandle _debugProcCallbackHandle;

    public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) :
     base(gameWindowSettings, nativeWindowSettings)
    {
        UpdateFrame += OnUpdate;
        RenderFrame += OnRender;

#if ENABLE_LOGGING
        InitLogger();
#endif
        Init();
    }

    protected override void Dispose(bool disposing)
    {
        Input.Save();

        base.Dispose(disposing);

#if ENABLE_LOG_SAVE
        _writer.Dispose();
        _logFile.Dispose();
#endif
    }

    protected void OnUpdate(FrameEventArgs e)
    {
        SharedData.renderData.frameEventArgs = e;

        _physicsSystems.Run();
        _gameSystems.Run();
    }

    protected void OnRender(FrameEventArgs args)
    {
#if DEBUG
        double lastTime = GLFW.GetTime();
        _framesCount++;
#endif

#if DEBUG
        double currentTime = GLFW.GetTime();
        double elapsed = currentTime - _lastElapsedRenderTime;

        _summedRenderTime += elapsed;

        if (elapsed >= 1.0)
        {
            _averageTime = _summedRenderTime / _framesCount;
            _lastElapsedRenderTime += elapsed;
        }

        //   Graphics.DrawText($"Average rendering time: {_averageTime} ms", "pixuf", new Vector2(20, 60), new Vector3i(0, 255, 0), FullToPixelatedRatio / 2, false);
#endif
        _renderSystems.Run();

        SwapBuffers();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    private void InitLogger()
    {
#if ENABLE_LOG_SAVE
        var currentDateTime = DateTime.Now.ToString().Replace(":", ".");

        _logFile = File.Create($@"Logs\{currentDateTime}.txt");
        _writer = new StreamWriter(_logFile);
#endif

        _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);

        GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
        GL.Enable(EnableCap.DebugOutput);
        GL.Enable(EnableCap.DebugOutputSynchronous);
    }

    private static void DebugCallBack(
       DebugSource source,
       DebugType type,
       int id,
       DebugSeverity severity,
       int length,
       IntPtr message,
       IntPtr userParam)
    {
        string messageStr = Marshal.PtrToStringAnsi(message, length);

        if (messageStr.Contains(USELESS_LOG))
            return;

#if ENABLE_LOG_SAVE
        _writer.WriteLine($"{severity} {type} | {messageStr}");
#endif
        Console.WriteLine($"{severity} {type} | {messageStr}");
    }

    #endregion

    private void Init()
    {
        int refreshRate;

        unsafe
        {
            var monitor = GLFW.GetPrimaryMonitor();
            var videoMode = GLFW.GetVideoMode(monitor);
            Vector2i monitorSize = new Vector2i(videoMode->Width, videoMode->Height);
            refreshRate = videoMode->RefreshRate;
            GLFW.SetWindowSize(WindowPtr, monitorSize.X, monitorSize.Y + 1);

            RefreshRate = refreshRate;

            GLFW.SetWindowPos(WindowPtr, 0, -1);
        }

        ScreenSize = Size - new Vector2i(0, 1);
        PixelatedScreenSize = new Vector2i(240, 135);
        GL.ClearColor(System.Drawing.Color.FromArgb(0, 100, 100, 100));

        FullToPixelatedRatio = MathF.Max(ScreenSize.X / PixelatedScreenSize.X, ScreenSize.Y / PixelatedScreenSize.X);

        CursorState = CursorState.Hidden;
        VSync = VSyncMode.On;
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
        GL.Viewport(0, 0, ScreenSize.X, ScreenSize.Y);

        float fixedDeltaTime = 1f / refreshRate;
        World = new EcsWorld();

        World b2World = new(new System.Numerics.Vector2(0, GRAVITY / PTM));

        SFX audioManager = SFX.New();
        SharedData sharedData = new(new GameData(new Camera(), this, audioManager, new GroupState(), new TextHelper()),
                                    new RenderData(new DebugDrawer(), new Graphics(), new ToTextureRenderer()),
                                    new PhysicsData(fixedDeltaTime, PTM, new PhysicsObjectsFactory(), b2World, 8, 3),
                                    null,
                                    new EventsBus());

        sharedData.gameData.Input = Input.New();
        sharedData.physicsData.contcactListener = new ContactListener();
        b2World.SetContactListener(sharedData.physicsData.contcactListener);

        SharedData = sharedData;

        Mouse.Init(this);
        Keyboard.Init(this);
        Extensions.Init(this);
        Utils.Init(this);
        ElementsBuffer.InitStatic();
        VertexBuffer.InitStatic();

        LoadShaders(Paths.ShadersDirectory);
        LoadShaders(Paths.MaterialsDirectory);
        LoadTextures(Paths.TexturesDirectory);
        LoadTextures(Paths.Combine(Paths.TexturesDirectory, "Tilesets"));
        LoadSounds(Paths.SoundsDirectory);

        _gameSystems = new EcsSystems(World, "game", sharedData);
        _renderSystems = new EcsSystems(World, "render", sharedData);
        _physicsSystems = new EcsSystems(World, "physics", sharedData);

        var gameDataFields = typeof(GameData).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
        var renderDataFields = typeof(RenderData).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
        var physicsDataFields = typeof(PhysicsData).GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();

        var gameRegularServices = gameDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(IRegularService)));
        var renderingRegularServices = renderDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(IRegularService)));
        var physicsRegularServices = physicsDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(IRegularService)));

        var gameSystemServices = gameDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(ISystemService)));
        var renderingSystemServices = renderDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(ISystemService)));
        var physicsSystemServices = physicsDataFields.FindAll(x => x.FieldType.IsAssignableTo(typeof(ISystemService)));

        InitServices(new (IGameData, List<FieldInfo>)[] { (sharedData.gameData, gameRegularServices),
                             (sharedData.renderData, renderingRegularServices),
                             (sharedData.physicsData, physicsRegularServices)}, ServiceState.Regular);
        _gameSystems
            .Add(new InitMatierialsSystem())
            .Add(new InitLayersSystem())
            .Add(new LoadFontsSystem())
            .Add(new TimeSystem())
            .AddGroupExt("PlayerCamera", false, new CameraFollowPlayerSystem())
           // .Add(new MoveCameraSystem())

            .Add(new SetMouseIngamePosSystem())

            .AddGroupExt("Intro", true, new Cool3DIntroSystem())
            .AddGroupExt("Cursor", false, new CursorSystem())
            .Add(new CameraSystem())
            .AddGroupExt("Menu", false, new MenuGUISystem())
            .Add(new TransitionSystem())
            .AddGroupExt("Game", false,
                new PlayerControllerSystem(),
                new PauseSystem(),
                //   new PauseMenuSystem(),
                new LevelsSystem(),
                new VoronoiWindSystem(),
                new PlayerSystem(),
                new TongueSystem(),
                new BeerSystem(),
                new DebugSystem(),
                new SubLevelTransitionSystem()
                )

            .AddGroupExt(typeof(SkyBackgroundSystem).Name, true,
                new SkyBackgroundSystem())

            //.Add(new DrawPhysicsObjectsSystem())
#if DEBUG
            .Add(new DebugCursorToolsSystem())
#endif
            .Inject(new LevelsService())
            .Init();

        _physicsSystems
            .AddGroupExt("Physics", false, new UpdatePhysicsSystem())
            .Inject()
            .Init();

        _renderSystems
            .AddExt(new RenderSpritesSystem())
            .AddExt(new RenderTextSystem())
            //  .Add(new LightingSystem())
            .Add(new SetLayerProjectionsSystem())
            .Add(new RenderLayersSystem())
            .Add(new FinalRenderSystem())
            //   .Add(new ClearTextLayerSystem())
            .Inject()
            .Init();

        InitServices(new (IGameData, List<FieldInfo>)[] { (sharedData.gameData, gameSystemServices),
                             (sharedData.renderData, renderingSystemServices),
                             (sharedData.physicsData, physicsSystemServices)}, ServiceState.System);
    }

    public void AddGroupSystems(string name, IEcsSystem[] systems) =>
        SharedData.gameData.groupSystems[name] = systems;

    private void InitServices((IGameData data, List<FieldInfo> fieldsInfo)[] systemsInfo, ServiceState state)
    {
        foreach (var systemInfo in systemsInfo)
        {
            foreach (var field in systemInfo.fieldsInfo)
            {
                if (!field.FieldType.IsAssignableTo(typeof(Service)))
                    continue;

                var instance = (Service)field.GetValue(systemInfo.data);
                instance.Init(SharedData, World, state);
            }
        }
    }

    private void LoadTextures(string dirPath)
    {
        DirectoryInfo directoryInfo = new(dirPath);

        foreach (var file in directoryInfo.GetFiles())
            Content.LoadTexture(file.FullName);
    }

    private void LoadSounds(string dirPath)
    {
        DirectoryInfo directoryInfo = new(dirPath);

        foreach (var file in directoryInfo.GetFiles())
            SFX.Add(file.FullName);

    }

    private void LoadShaders(string dirPath)
    {
        DirectoryInfo directoryInfo = new(dirPath);

        var directories = directoryInfo.GetDirectories();
        foreach (var dir in directories)
        {
            var files = dir.GetFiles().ToList();

            var vertFile = files.Find(file => file.Extension == ".vert");
            var fragFile = files.Find(file => file.Extension == ".frag");
            var geomFile = files.Find(files => files.Extension == ".geom");

            if (vertFile == null || fragFile == null)
                continue;

            var name = dir.Name;

            Content.LoadShader(name, vertFile.FullName, fragFile.FullName, geomFile?.FullName);
        }

        GL.UseProgram(0);
    }
}