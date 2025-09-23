using UnityEngine;

public class GuardBrain : MonoBehaviour
{
	[Header("Config & Start")]
	public GuardConfig config;
	public StateAsset startState;

	[Header("Refs")]
	public Transform player;
	public Mover mover;
	public GuardPerception perception;
	public GuardDetector detector;
	public WaypointPath path;

	public GuardContext Ctx { get; private set; }

	private StateAsset currentAsset;
	private IState currentRuntime;

	void Awake()
	{
		Ctx = new GuardContext(transform, config, mover, perception, detector, path, player);
	}

	void Start()
	{
		Switch(startState);
	}

	void Update()
	{
		detector?.Tick();
		currentRuntime?.Tick();
	}

	public void Switch(StateAsset next)
	{
		currentRuntime?.Exit();
		currentAsset = next;
		currentRuntime = next?.CreateRuntime(this);
		currentRuntime?.Enter();
	}
}

public class GuardContext
{
	public Transform Self { get; }
	public GuardConfig Config { get; }
	public Mover Mover { get; }
	public GuardPerception Perception { get; }
	public GuardDetector Detector { get; }
	public WaypointPath Path { get; }
	public Transform Player { get; }

	public NoiseInbox Noise { get; private set; }
	public void WriteNoise(Vector2 point, float time, float strength = 1f)
	{
		Noise = new NoiseInbox { HasSignal = true, Point = point, Time = time, Strength = strength };
	}
	public void ClearNoise() => Noise.Clear();


	public GuardContext(Transform self, GuardConfig config, Mover mover, GuardPerception perception, GuardDetector detector, WaypointPath path, Transform player)
	{
		Self = self;
		Config = config;
		Mover = mover;
		Perception = perception;
		Detector = detector;
		Path = path;
		Player = player;
	}
}