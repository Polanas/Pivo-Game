namespace Game;

/// <summary>
/// Than one is for collision filterting
/// </summary>
enum PhysicsCategory
{
    Other = 1,
    Beer = 1 << 1,
    TongueSensor = 1 << 2,
}

/// <summary>
/// That one is for manual collision checks (could've used entites but nah)
/// </summary>
enum PhysicsBodyType
{
    Nothing,
    Other,
    Block,
    PlayerSensor,
    TongueSensor,
    Player,
    Eatable
}
