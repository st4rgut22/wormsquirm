using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : GenericSingletonClass<GameManager>
{
    public delegate void CreateWorm(string wormTag);
    public event CreateWorm CreateWormEvent;

    public delegate void ResetTunnelNetwork();
    public event ResetTunnelNetwork ResetTunnelNetworkEvent;

    public bool isNewMap;

    public GameMode gameMode { get; private set; }

    private void Awake()
    {
        gameMode = GameMode.GamePlay;        
    }

    private void OnEnable()
    {
        CreateWormEvent += FindObjectOfType<Worm.Factory.WormFactory>().onCreateWorm;
        ResetTunnelNetworkEvent += FindObjectOfType<Tunnel.Map>().onResetTunnelNetwork;
        ResetTunnelNetworkEvent += FindObjectOfType<Map.MapLandmarks>().onResetTunnelNetwork;
        ResetTunnelNetworkEvent += Tunnel.TunnelManager.Instance.onResetTunnelNetwork;
    }

    public void onObjectiveReached()
    {
        ResetTunnelNetworkEvent();
    }

    private void Start()
    {
        if (gameMode == GameMode.GamePlay)
        {
            CreateWormEvent(Worm.WormManager.WORM_PLAYER_TAG);
        }
        else
        {
            CreateWormEvent(Worm.WormManager.WORM_AI_TAG);
        }
    }

    private void OnDisable()
    {
        CreateWormEvent -= FindObjectOfType<Worm.Factory.WormFactory>().onCreateWorm;
        ResetTunnelNetworkEvent -= FindObjectOfType<Tunnel.Map>().onResetTunnelNetwork;
        ResetTunnelNetworkEvent -= FindObjectOfType<Map.MapLandmarks>().onResetTunnelNetwork;
        ResetTunnelNetworkEvent -= Tunnel.TunnelManager.Instance.onResetTunnelNetwork;
    }
}
