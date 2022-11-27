using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game;

class MenuGUISystem : MySystem
{
    private GUIHandler _menuGUIHandler;

    private CoroutineRunner _runner = new();

    private bool _clicked;

    public override void Run(EcsSystems systems)
    {
        _runner.Update(sharedData.physicsData.deltaTime);
        _menuGUIHandler.Update();
    }

    private IEnumerator LoadGame()
    {
        while (true)
        {
            if (sharedData.eventBus.HasEventSingleton<StaticTransitionState>())
            {
                StaticTransitionState transitionState;

                while (true)
                {
                    transitionState = sharedData.eventBus.GetEventBodySingleton<StaticTransitionState>();

                    if (transitionState.r_covered)
                        break;

                    yield return null;
                }

                GroupState.Set("Game", true);
                GroupState.Set("Physics", true);

                yield return 0.4f;

                sharedData.eventBus.NewEvent<TransitionState>() = new(false, Mouse.ScreenPosition);
                GroupState.Set("Menu", false);
                _menuGUIHandler.Clear();
                _runner.StopAll();
            }

            yield return null;
        }
    }

    public override void OnGroupActivate()
    {
        _menuGUIHandler = new();
        Sprite playButtonSprite = new(Content.LoadTexture(@"Content\Textures\GUI\playButton.png"), new Vector2i(47, 16));
        playButtonSprite.layer = Layer.UI;

        playButtonSprite.AddAnimation("idle", 0, false, new int[1]);
        playButtonSprite.AddAnimation("collided", 0, false, new int[] { 1 });
        playButtonSprite.AddAnimation("clicked", 0, false, new int[] { 2 });
 
        playButtonSprite.alpha = 0;

        GUIItem item = new SpriteButton(playButtonSprite, Vector2.Zero, "buttonPressed");
        item.OnUpdate += () =>
        {
            if (playButtonSprite.alpha >= 1)
                return;

            playButtonSprite.alpha += 0.06f;
        };
        item.OnClick += () =>
        {
            if (_clicked)
                return;
            _clicked = true;

            sharedData.eventBus.NewEvent<TransitionState>() = new(true, Mouse.ScreenPosition);
         };

        _menuGUIHandler.AddItem(item);

        playButtonSprite.position = item.Transform.position;
        _runner.Run(LoadGame());
    }

    public override void OnGroupDiactivate()
    {
        Content.DeleteTexture("playButton");
        _menuGUIHandler.Clear();
        _clicked = false;
    }
}   