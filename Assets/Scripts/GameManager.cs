public class GameManager : GenericSingletonClass<GameManager>
{
    public delegate void StartGame(GameMode gameMode);
    public event StartGame StartGameEvent;

    /**
     * Cleaning up all assets in the game
     */
    public delegate void DestroyGame();
    public event DestroyGame DestroyGameEvent;

    public bool isNewMap;
    public static int MAP_LENGTH = 10; // distance from origin to one edge of the map

    public GameMode gameMode { get; private set; }

    private new void Awake()
    {
        base.Awake();
        gameMode = GameMode.DebugFixedPath;
    }

    private void OnEnable()
    {
        StartGameEvent += FindObjectOfType<Map.LandmarkGenerator>().onStartGame;
        StartGameEvent += FindObjectOfType<Map.AiSpawnGenerator>().onStartGame;
        StartGameEvent += FindObjectOfType<Map.HumanSpawnGenerator>().onStartGame;
        DestroyGameEvent += FindObjectOfType<Tunnel.TunnelMap>().onDestroyGame;
        DestroyGameEvent += Tunnel.TunnelManager.Instance.onDestroyGame;
    }

    private void Start()
    {
        StartGameEvent(gameMode);
    }

    private void OnDisable()
    {
        if (FindObjectOfType<Map.SpawnGenerator>())
        {
            StartGameEvent -= FindObjectOfType<Map.AiSpawnGenerator>().onStartGame;
            DestroyGameEvent -= FindObjectOfType<Map.HumanSpawnGenerator>().onDestroyGame;
        }
        if (FindObjectOfType<Tunnel.TunnelMap>())
        {
            DestroyGameEvent -= FindObjectOfType<Tunnel.TunnelMap>().onDestroyGame;
        }
        if (FindObjectOfType<Map.LandmarkGenerator>())
        {
            StartGameEvent -= FindObjectOfType<Map.LandmarkGenerator>().onStartGame;
            DestroyGameEvent -= FindObjectOfType<Map.LandmarkGenerator>().onDestroyGame;
        }
        if (Tunnel.TunnelManager.Instance)
        {
            DestroyGameEvent -= Tunnel.TunnelManager.Instance.onDestroyGame;
        }
    }
}
