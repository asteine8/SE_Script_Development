
#region This goes in the programmable block
const string VERSION = "112.3.0";
const string DATE = "05/11/19";

/*
/ //// / Whip's Turret Slaver / //// /
_______________________________________________________________________

README: https://steamcommunity.com/sharedfiles/filedetails/?id=672678005

Read the fuckin' online instructions. I'm out of space in this script.
Post any questions, suggestions, or issues you have on the workshop page.

FOR THE LOVE OF GOD, DON'T EDIT VARIABLES IN THE CODE!
USE THE CUSTOM DATA OF THE PROGRAMMABLE BLOCK!

Code by Whiplash141
*/

////////////////////////////////////////////////////
//=================================================
//      No touchey anything below here
//=================================================
////////////////////////////////////////////////////

#region Variables that you should not touch
readonly TurretVariables defaultTurretVariables = new TurretVariables()
{
    ToleranceAngle = 5,
    ConvergenceRange = 800,
    EquilibriumRotationSpeed = 10,
    ProportionalGain = 75,
    IntegralGain = 0,
    IntegralDecayRatio = 0.25,
    DerivativeGain = 0,
    GameMaxSpeed = 100,
    TargetRefreshInterval = 2,
    RotorTurretGroupNameTag = "Turret Group",
    AiTurretGroupNameTag = "Slaved Group",
    ElevationRotorNameTag = "Elevation",
    AzimuthRotorNameTag = "Azimuth",
    DesignatorNameTag = "Designator",
    OnlyShootWhenDesignatorShoots = false,
};

class TurretVariables
{
    public double ToleranceAngle;
    public double ConvergenceRange;
    public double EquilibriumRotationSpeed;
    public double ProportionalGain;
    public double IntegralGain;
    public double DerivativeGain;
    public double IntegralDecayRatio;
    public double GameMaxSpeed;
    public double TargetRefreshInterval;
    public string RotorTurretGroupNameTag;
    public string AiTurretGroupNameTag;
    public string ElevationRotorNameTag;
    public string AzimuthRotorNameTag;
    public string DesignatorNameTag;
    public bool OnlyShootWhenDesignatorShoots;
}

const string INI_MUZZLE_VELOCITY = "muzzle_velocity";
const string INI_AVOID_FF = "avoid_friendly_fire";
const string INI_TOLERANCE = "fire_tolerance_deg";
const string INI_CONVERGENCE = "manual_convergence_range";
const string INI_KP = "proportional_gain";
const string INI_KI = "integral_gain";
const string INI_KD = "derivative_gain";
const string INI_INTEGRAL_DECAY_RATIO = "integral_decay_ratio";
const string INI_REST_RPM = "return_to_rest_rpm";
const string INI_GENERAL_SECTION = "General Parameters";
const string INI_MAX_SPEED = "max_game_speed";
const string INI_ROTOR_TURRET_NAME = "rotor_turret_group_tag";
const string INI_AI_TURRET_NAME = "ai_turret_group_tag";
const string INI_AZIMUTH_NAME = "azimuth_rotor_name_tag";
const string INI_ELEVATION_NAME = "elevation_rotor_name_tag";
const string INI_DESIGNATOR_NAME = "designator_name_tag";
const string INI_ENGAGEMENT_RANGE = "auto_fire_range";
const string INI_REST_TIME = "return_to_rest_delay";
const string INI_SHOOT_WHEN_DESIGNATOR_SHOOTS = "only_shoot_when_designator_shoots";

const double UPDATES_PER_SECOND = 10;
const double MAIN_UPDATE_INTERVAL = 1.0 / UPDATES_PER_SECOND;
const double RUNTIME_TO_REALTIME = 1.0 / 0.96; // because keen thats why
const double DEFAULT_PROJECTILE_SPEED = 400; //in m/s
const double TICK = 1.0 / 60.0;
const int MAX_BLOCKS_TO_CHECK_FOR_FF = 50;

readonly List<IMyLargeTurretBase> allDesignators = new List<IMyLargeTurretBase>();
readonly List<IMyShipController> shipControllers = new List<IMyShipController>();
readonly List<IMyTextPanel> debugPanels = new List<IMyTextPanel>();
readonly List<IMyBlockGroup> allCurrentGroups = new List<IMyBlockGroup>();
readonly List<IMyBlockGroup> currentRotorTurretGroups = new List<IMyBlockGroup>();
readonly List<IMyBlockGroup> currentAITurretGroups = new List<IMyBlockGroup>();
readonly List<TurretGroup> turretList = new List<TurretGroup>();
readonly MyIni generalIni = new MyIni();
readonly Scheduler scheduler;
readonly ScheduledAction ScheduledMainSetup;
readonly StringBuilder iniOutput = new StringBuilder();
readonly StringBuilder echoOutput = new StringBuilder();
readonly StringBuilder turretEchoBuilder = new StringBuilder();
readonly StringBuilder turretErrorBuilder = new StringBuilder();
readonly StringBuilder turretEchoOutput = new StringBuilder();
readonly StringBuilder turretErrorOutput = new StringBuilder();
readonly RuntimeTracker runtimeTracker;
readonly CircularBuffer<Action> turretBuffer;

IMyShipController reference = null;
Vector3D lastGridPosition = Vector3D.Zero, gridVelocity = Vector3D.Zero;
double turretRefreshTime;
bool useVelocityEstimation = true, debugMode = false, writtenTurretEcho = false;
int rotorTurretsCount = 0, aiTurretsCount = 0;

#endregion

#region Main Routine Methods
Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    runtimeTracker = new RuntimeTracker(this, 5 * 60);

    double step = UPDATES_PER_SECOND / 60.0;
    turretBuffer = new CircularBuffer<Action>(6);
    turretBuffer.Add(() => UpdateTurrets(0 * step, 1 * step));
    turretBuffer.Add(() => UpdateTurrets(1 * step, 2 * step));
    turretBuffer.Add(() => UpdateTurrets(2 * step, 3 * step));
    turretBuffer.Add(() => UpdateTurrets(3 * step, 4 * step));
    turretBuffer.Add(() => UpdateTurrets(4 * step, 5 * step));
    turretBuffer.Add(() => UpdateTurrets(5 * step, 6 * step));

    scheduler = new Scheduler(this);
    ScheduledMainSetup = new ScheduledAction(MainSetup, 0.1);
    scheduler.AddScheduledAction(ScheduledMainSetup);
    scheduler.AddScheduledAction(CalculateShooterVelocity, UPDATES_PER_SECOND);
    scheduler.AddScheduledAction(PrintDetailedInfo, 1);
    scheduler.AddScheduledAction(VerifyTurretGroupBlocks, 1, timeOffset: 0.5);
    //scheduler.AddScheduledAction(RefreshDesignatorTargeting, 1, timeOffset: 0.66);
    scheduler.AddScheduledAction(MoveNextTurrets, 60);

    MainSetup();
    base.Echo("Initializing...");
}

void Main(string arg, UpdateType updateType)
{
    runtimeTracker.AddRuntime();

    if ((updateType & (UpdateType.Script | UpdateType.Terminal | UpdateType.Trigger)) != 0)
        ArgumentHandling(arg);

    scheduler.Update();

    runtimeTracker.AddInstructions();
}

/*
 * Hiding default echo implementation so that we can display precisely when we want.
 */
new void Echo(string text)
{
    echoOutput.AppendLine(text);
}

void PrintEcho()
{
    base.Echo(echoOutput.ToString());
}

void PrintDetailedInfo()
{
    Echo($"WMI Turret Control Systems\n(Version {VERSION} - {DATE})");
    Echo("\nYou can customize turrets \n  individually in the Custom Data\n  of this block.");
    Echo($"\nNext block refresh in {Math.Max(0, ScheduledMainSetup.RunInterval - ScheduledMainSetup.TimeSinceLastRun):N0} second(s).\n");
    Echo($"Turret Summary:\n> {rotorTurretsCount} rotor turret group(s) found");
    Echo($"> {aiTurretsCount} slaved AI turret group(s) found\n");
    Echo($"Debug mode is: {(debugMode ? "ON" : "OFF")}\n> Toggle debug output with the\nargument: \"debug_toggle\".");
    if (debugMode)
        Echo($"> {debugPanels.Count} debug panel(s) found\n> Name a text panel \"DEBUG\" to \nsee debug text.");

    echoOutput.Append(turretErrorOutput);
    Echo(runtimeTracker.Write());

    if (debugMode)
    {
        string finalOutput = echoOutput.ToString() + turretEchoOutput.ToString();
        foreach (var block in debugPanels)
        {
            block.WriteText(finalOutput);
            block.ContentType = ContentType.TEXT_AND_IMAGE;
        }
    }

    PrintEcho();
    echoOutput.Clear();
}

void MoveNextTurrets()
{
    turretBuffer.MoveNext().Invoke();
}

void UpdateTurrets(double startProportion, double endProportion)
{
    int startInt = (int)Math.Round(startProportion * turretList.Count);
    int endInt = (int)Math.Round(endProportion * turretList.Count);

    for (int i = startInt; i < endInt; ++i)
    {
        var turretToUpdate = turretList[i];
        turretToUpdate.DoWork(gridVelocity);
        turretErrorBuilder.Append(turretToUpdate.ErrorOutput);

        if (debugMode)
            turretEchoBuilder.Append(turretToUpdate.EchoOutput);
    }

    // End of cycle
    if (endInt == turretList.Count && !writtenTurretEcho)
    {
        writtenTurretEcho = true;

        if (debugMode)
        {
            turretEchoOutput.Clear();
            turretEchoOutput.Append(turretEchoBuilder);
            turretEchoBuilder.Clear();
        }

        turretErrorOutput.Clear();
        turretErrorOutput.Append(turretErrorBuilder);

        turretErrorBuilder.Clear();
    }
    else
    {
        writtenTurretEcho = false;
    }
}

void MainSetup()
{
    ParseGeneralIni();
    GetAllGrids();
    GetBlockGroups();
    GetVelocityReference();
    BuildIniOutput();
}

void VerifyTurretGroupBlocks()
{
    foreach (var turret in turretList)
    {
        turret.VerifyAllGroupBlocks();
    }
}

void CalculateShooterVelocity()
{
    if (useVelocityEstimation)
    {
        var currentGridPosition = Me.CubeGrid.WorldAABB.Center; //get grid's bounding box center, decent approximation for CoM
        gridVelocity = (currentGridPosition - lastGridPosition) * UPDATES_PER_SECOND;
        lastGridPosition = currentGridPosition;
    }
    else
    {
        if (DoesBlockExist(reference))
        {
            gridVelocity = reference.GetShipVelocities().LinearVelocity;
        }
        else
        {
            GetVelocityReference();
        }
    }
}

void WriteGeneralIni()
{
    generalIni.Clear();
    generalIni.TryParse(Me.CustomData, INI_GENERAL_SECTION);
    generalIni.Set(INI_GENERAL_SECTION, INI_MAX_SPEED, defaultTurretVariables.GameMaxSpeed);
    generalIni.Set(INI_GENERAL_SECTION, INI_ROTOR_TURRET_NAME, defaultTurretVariables.RotorTurretGroupNameTag);
    generalIni.Set(INI_GENERAL_SECTION, INI_AI_TURRET_NAME, defaultTurretVariables.AiTurretGroupNameTag);
    generalIni.Set(INI_GENERAL_SECTION, INI_AZIMUTH_NAME, defaultTurretVariables.AzimuthRotorNameTag);
    generalIni.Set(INI_GENERAL_SECTION, INI_ELEVATION_NAME, defaultTurretVariables.ElevationRotorNameTag);
    generalIni.Set(INI_GENERAL_SECTION, INI_DESIGNATOR_NAME, defaultTurretVariables.DesignatorNameTag);
}

void ParseGeneralIni()
{
    generalIni.Clear();
    bool parsed = generalIni.TryParse(Me.CustomData, INI_GENERAL_SECTION);
    if (!parsed)
        return;
    defaultTurretVariables.GameMaxSpeed = generalIni.Get(INI_GENERAL_SECTION, INI_MAX_SPEED).ToDouble(defaultTurretVariables.GameMaxSpeed);
    defaultTurretVariables.RotorTurretGroupNameTag = generalIni.Get(INI_GENERAL_SECTION, INI_ROTOR_TURRET_NAME).ToString(defaultTurretVariables.RotorTurretGroupNameTag);
    defaultTurretVariables.AiTurretGroupNameTag = generalIni.Get(INI_GENERAL_SECTION, INI_AI_TURRET_NAME).ToString(defaultTurretVariables.AiTurretGroupNameTag);
    defaultTurretVariables.AzimuthRotorNameTag = generalIni.Get(INI_GENERAL_SECTION, INI_AZIMUTH_NAME).ToString(defaultTurretVariables.AzimuthRotorNameTag);
    defaultTurretVariables.ElevationRotorNameTag = generalIni.Get(INI_GENERAL_SECTION, INI_ELEVATION_NAME).ToString(defaultTurretVariables.ElevationRotorNameTag);
    defaultTurretVariables.DesignatorNameTag = generalIni.Get(INI_GENERAL_SECTION, INI_DESIGNATOR_NAME).ToString(defaultTurretVariables.DesignatorNameTag);
}

void BuildIniOutput()
{
    iniOutput.Clear();
    WriteGeneralIni();
    iniOutput.AppendLine(generalIni.ToString());

    foreach (TurretGroup turret in turretList)
    {
        iniOutput.Append(turret.IniOutput).Append(Environment.NewLine);
    }

    Me.CustomData = iniOutput.ToString();
}

bool CollectDesignatorsDebugAndMech(IMyTerminalBlock b)
{
    if (!b.IsSameConstructAs(Me))
        return false;

    var turret = b as IMyLargeTurretBase;
    if (turret != null && StringExtensions.Contains(b.CustomName, defaultTurretVariables.DesignatorNameTag))
    {
        allDesignators.Add(turret);
        return false;
    }

    var sc = b as IMyShipController;
    if (sc != null)
    {
        shipControllers.Add(sc);
        return false;
    }

    var mech = b as IMyMechanicalConnectionBlock;
    if (mech != null)
    {
        allMechanical.Add(mech);
        return false;
    }

    var text = b as IMyTextPanel;
    if (debugMode && b != null && b.CustomName.Contains("DEBUG"))
    {
        debugPanels.Add(text);
        return false;
    }

    return false;
}

bool CollectTurretGroups(IMyBlockGroup g)
{
    if (StringExtensions.Contains(g.Name, defaultTurretVariables.AiTurretGroupNameTag))
    {
        currentAITurretGroups.Add(g);
        allCurrentGroups.Add(g);
        return false;
    }
    else if (StringExtensions.Contains(g.Name, defaultTurretVariables.RotorTurretGroupNameTag))
    {
        currentRotorTurretGroups.Add(g);
        allCurrentGroups.Add(g);
        return false;
    }
    return false;
}

void GetBlockGroups()
{
    shipControllers.Clear();
    allDesignators.Clear();
    allMechanical.Clear();
    debugPanels.Clear();
    currentAITurretGroups.Clear();
    currentRotorTurretGroups.Clear();
    allCurrentGroups.Clear();

    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(null, CollectDesignatorsDebugAndMech);
    GridTerminalSystem.GetBlockGroups(null, CollectTurretGroups);
    turretRefreshTime = defaultTurretVariables.TargetRefreshInterval / allDesignators.Count();

    turretList.RemoveAll(x => !allCurrentGroups.Contains(x.ThisGroup));

    rotorTurretsCount = currentRotorTurretGroups.Count;
    aiTurretsCount = currentAITurretGroups.Count;

    //Update existing turrets
    foreach (var turret in turretList)
    {
        turret.GetTurretGroupBlocks(defaultTurretVariables: defaultTurretVariables);

        //Remove existing turrets from list
        if (turret.IsRotorTurret)
        {
            currentRotorTurretGroups.Remove(turret.ThisGroup);
        }
        else
        {
            currentAITurretGroups.Remove(turret.ThisGroup);
        }
    }

    //Add new turret groups to the master list
    foreach (var g in currentAITurretGroups)
    {
        var turret = new TurretGroup(g, defaultTurretVariables, this, false, _allShipGrids);
        turretList.Add(turret);
    }

    foreach (var g in currentRotorTurretGroups)
    {
        var turret = new TurretGroup(g, defaultTurretVariables, this, true, _allShipGrids);
        turretList.Add(turret);
    }
}

void GetVelocityReference()
{
    reference = shipControllers.Count > 0 ? shipControllers[0] : null;
    useVelocityEstimation = reference == null;
}

void RefreshDesignatorTargeting()
{
    foreach (var turret in allDesignators)
    {
        float range = turret.GetValue<float>("Range");
        turret.SetValue<float>("Range", range - 1);
        turret.SetValue<float>("Range", range);
    }
}

void ArgumentHandling(string arg)
{
    switch (arg.ToLower())
    {
        case "reset_targeting":
            ResetAllDesignatorTargeting();
            break;

        case "debug_toggle":
            debugMode = !debugMode;
            if (debugMode)
            {
                GetBlockGroups();
            }
            break;

        default:
            break;
    }
}

void ResetAllDesignatorTargeting()
{
    foreach (var thisTurret in allDesignators)
    {
        thisTurret.ResetTargetingToDefault();
        thisTurret.SetValue("Range", float.MaxValue); //still no damn setter for this
    }
}
#endregion

#region General Utilities
bool DoesBlockExist(IMyTerminalBlock block)
{
    return !Vector3D.IsZero(block.WorldMatrix.Translation);
}
#endregion

#region Turret Group Class
class TurretGroup
{
    #region Member Fields
    public readonly StringBuilder EchoOutput = new StringBuilder();
    public readonly StringBuilder ErrorOutput = new StringBuilder();
    public readonly StringBuilder IniOutput = new StringBuilder();
    public readonly MyIni Ini = new MyIni();
    public bool IsRotorTurret { get; }
    public IMyBlockGroup ThisGroup { get; private set; }

    const double RADS_TO_RPM = 30.0 / Math.PI;

    readonly Program _program;
    readonly Dictionary<long, float> _rotorRestAngles = new Dictionary<long, float>();
    readonly Dictionary<Vector3D, bool> _scannedBlocks = new Dictionary<Vector3D, bool>();
    readonly List<IMyMotorStator> _additionalElevationRotors = new List<IMyMotorStator>();
    readonly List<IMyCameraBlock> _cameras = new List<IMyCameraBlock>();
    readonly List<IMyShipToolBase> _tools = new List<IMyShipToolBase>();
    readonly List<IMyLightingBlock> _lights = new List<IMyLightingBlock>();
    readonly List<IMyUserControllableGun> _guns = new List<IMyUserControllableGun>();
    readonly List<IMyTimerBlock> _timers = new List<IMyTimerBlock>();
    readonly List<IMyTerminalBlock> _rotorWeaponsAndTools = new List<IMyTerminalBlock>();
    readonly List<IMyTerminalBlock> _groupBlocks = new List<IMyTerminalBlock>();
    readonly List<IMyTerminalBlock> _slavedTurrets = new List<IMyTerminalBlock>();
    readonly List<IMyLargeTurretBase> _turretDesignators = new List<IMyLargeTurretBase>();
    readonly HashSet<IMyCubeGrid> _shipGrids = new HashSet<IMyCubeGrid>();
    readonly HashSet<IMyCubeGrid> _thisTurretGrids = new HashSet<IMyCubeGrid>();

    DecayingIntegralPID _elevationPID, _azimuthPID;
    IMyTerminalBlock _rotorTurretReference;
    IMyMotorStator _mainElevationRotor;
    IMyMotorStator _azimuthRotor;
    IMyLargeTurretBase _designator;
    IMyTerminalBlock _rotorTurretLeadReference = null;
    IMyUserControllableGun _firstWeapon = null;
    Vector3D _gridVelocity, _targetVec, _averageWeaponPos, _lastTargetVelocity = Vector3D.Zero;
    MatrixD _lastAzimuthMatrix, _lastElevationMatrix;
    double? _muzzleVelocity = null;
    double _initialMuzzleVelocity = -1;
    double _toleranceDotProduct;
    double _proportionalGain;
    double _integralGain;
    double _derivativeGain;
    double _toleranceAngle;
    double _equilibriumRotationSpeed;
    double _convergenceRange;
    double _gameMaxSpeed;
    double _integralDecayRatio;
    double _autoEngagementRange = 800;
    bool _isSetup = false;
    bool _firstRun = true;
    bool _isRocket;
    bool _intersection = false;
    bool _avoidFriendlyFire = true;
    bool _isShooting = true;
    bool _toolsOn = true;
    bool _onlyShootWhenDesignatorShoots = false;
    long _lastTargetEntityId = 0;
    int _framesSinceLastLock = 141;
    int _returnToRestDelay = 20; // 2 seconds by default
    int _errorCount = 0;
    string _designatorName;
    string _rotorTurretGroupNameTag;
    string _aiTurretGroupNameTag;
    string _elevationRotorName;
    string _azimuthRotorName;
    #endregion

    #region Constructor
    public TurretGroup(IMyBlockGroup group, TurretVariables defaultTurretVariables, Program program, bool isRotorTurret, HashSet<IMyCubeGrid> shipGrids)
    {
        ThisGroup = group;
        IsRotorTurret = isRotorTurret;
        _program = program;
        _shipGrids = shipGrids;

        _rotorTurretGroupNameTag = defaultTurretVariables.RotorTurretGroupNameTag;
        _aiTurretGroupNameTag = defaultTurretVariables.AiTurretGroupNameTag;
        _elevationRotorName = defaultTurretVariables.ElevationRotorNameTag;
        _azimuthRotorName = defaultTurretVariables.AzimuthRotorNameTag;
        _designatorName = defaultTurretVariables.DesignatorNameTag;
        _gameMaxSpeed = defaultTurretVariables.GameMaxSpeed;
        

        _toleranceAngle = defaultTurretVariables.ToleranceAngle;
        _convergenceRange = defaultTurretVariables.ConvergenceRange;
        _toleranceDotProduct = Math.Cos(_toleranceAngle * Math.PI / 180);
        _onlyShootWhenDesignatorShoots = defaultTurretVariables.OnlyShootWhenDesignatorShoots;

        if (IsRotorTurret)
        {
            _proportionalGain = defaultTurretVariables.ProportionalGain;
            _integralGain = defaultTurretVariables.IntegralGain;
            _derivativeGain = defaultTurretVariables.DerivativeGain;
            _equilibriumRotationSpeed = defaultTurretVariables.EquilibriumRotationSpeed;
            _integralDecayRatio = defaultTurretVariables.IntegralDecayRatio;
            SetPidValues();
        }

        GetTurretGroupBlocks();
    }
    #endregion

    #region Ini Config
    void WriteIni()
    {
        Ini.Clear();
        Ini.TryParse(_program.Me.CustomData, ThisGroup.Name);
        Ini.Set(ThisGroup.Name, INI_MUZZLE_VELOCITY, _muzzleVelocity.Value);
        Ini.Set(ThisGroup.Name, INI_AVOID_FF, _avoidFriendlyFire);
        Ini.Set(ThisGroup.Name, INI_TOLERANCE, _toleranceAngle);
        Ini.Set(ThisGroup.Name, INI_CONVERGENCE, _convergenceRange);
        Ini.Set(ThisGroup.Name, INI_ENGAGEMENT_RANGE, _autoEngagementRange);
        if (IsRotorTurret)
        {
            Ini.Set(ThisGroup.Name, INI_KP, _proportionalGain);
            Ini.Set(ThisGroup.Name, INI_KI, _integralGain);
            Ini.Set(ThisGroup.Name, INI_INTEGRAL_DECAY_RATIO, _integralDecayRatio);
            Ini.Set(ThisGroup.Name, INI_KD, _derivativeGain);
            Ini.Set(ThisGroup.Name, INI_REST_RPM, _equilibriumRotationSpeed);
            Ini.Set(ThisGroup.Name, INI_REST_TIME, _returnToRestDelay / UPDATES_PER_SECOND);
        }
        Ini.Set(ThisGroup.Name, INI_SHOOT_WHEN_DESIGNATOR_SHOOTS, _onlyShootWhenDesignatorShoots);

        IniOutput.Clear();
        IniOutput.Append(Ini.ToString());
    }

    void ParseIni()
    {
        Ini.Clear();
        Ini.TryParse(_program.Me.CustomData, ThisGroup.Name);

        _muzzleVelocity = Ini.Get(ThisGroup.Name, INI_MUZZLE_VELOCITY).ToDouble(_muzzleVelocity.Value);
        _avoidFriendlyFire = Ini.Get(ThisGroup.Name, INI_AVOID_FF).ToBoolean(_avoidFriendlyFire);
        _convergenceRange = Ini.Get(ThisGroup.Name, INI_CONVERGENCE).ToDouble(_convergenceRange);
        _autoEngagementRange = Ini.Get(ThisGroup.Name, INI_ENGAGEMENT_RANGE).ToDouble(_autoEngagementRange);
        _onlyShootWhenDesignatorShoots = Ini.Get(ThisGroup.Name, INI_SHOOT_WHEN_DESIGNATOR_SHOOTS).ToBoolean(_onlyShootWhenDesignatorShoots);

        double t = _toleranceAngle;
        _toleranceAngle = Ini.Get(ThisGroup.Name, INI_TOLERANCE).ToDouble(_toleranceAngle);
        if (t != _toleranceAngle)
            _toleranceDotProduct = Math.Cos(_toleranceAngle * Math.PI / 180);

        if (IsRotorTurret)
        {
            double kp = _proportionalGain, ki = _integralGain, kd = _derivativeGain, decay = _integralDecayRatio;
            _proportionalGain = Ini.Get(ThisGroup.Name, INI_KP).ToDouble(_proportionalGain);
            _integralGain = Ini.Get(ThisGroup.Name, INI_KI).ToDouble(_integralGain);
            _derivativeGain = Ini.Get(ThisGroup.Name, INI_KD).ToDouble(_derivativeGain);
            _integralDecayRatio = Ini.Get(ThisGroup.Name, INI_INTEGRAL_DECAY_RATIO).ToDouble(_integralDecayRatio);
            _equilibriumRotationSpeed = Ini.Get(ThisGroup.Name, INI_REST_RPM).ToDouble(_equilibriumRotationSpeed);

            if (kp != _proportionalGain || ki != _integralGain || kd != _derivativeGain || decay != _integralDecayRatio)
            {
                SetPidValues();
            }

            _returnToRestDelay = (int)(Ini.Get(ThisGroup.Name, INI_REST_TIME).ToDouble(_returnToRestDelay / UPDATES_PER_SECOND) * UPDATES_PER_SECOND);
        }

        WriteIni();
    }
    #endregion

    #region Grabbing Blocks
    public void UpdateGeneralSettings(TurretVariables defaultTurretVariables)
    {
        if (defaultTurretVariables == null)
            return;

        _rotorTurretGroupNameTag = defaultTurretVariables.RotorTurretGroupNameTag;
        _aiTurretGroupNameTag = defaultTurretVariables.AiTurretGroupNameTag;
        _elevationRotorName = defaultTurretVariables.ElevationRotorNameTag;
        _azimuthRotorName = defaultTurretVariables.AzimuthRotorNameTag;
        _designatorName = defaultTurretVariables.DesignatorNameTag;
        _gameMaxSpeed = defaultTurretVariables.GameMaxSpeed;
    }

    public void GetTurretGroupBlocks(bool verbose = false, TurretVariables defaultTurretVariables = null)
    {
        UpdateGeneralSettings(defaultTurretVariables);
        ThisGroup.GetBlocks(_groupBlocks);

        if (IsRotorTurret)
            _isSetup = GrabBlocks(_groupBlocks, verbose);
        else
            _isSetup = GrabBlocksAI(_groupBlocks, verbose);

        if (!_isSetup)
            return;

        _rotorTurretLeadReference = GetLeadingWeapon(_guns);
        _isRocket = _rotorTurretLeadReference == null ? false : IsProjectileRocket(_rotorTurretLeadReference);

        if (!_muzzleVelocity.HasValue)
        {
            _muzzleVelocity = _rotorTurretLeadReference == null ? 3e8 : GetInitialMuzzleVelocity(_rotorTurretLeadReference);
            _initialMuzzleVelocity = _muzzleVelocity.Value;
        }

        ParseIni();
    }

    void AddRotorGridsToHash(IMyMotorStator rotor, bool addBase = true)
    {
        if (addBase)
            _thisTurretGrids.Add(rotor.CubeGrid);

        if (rotor.IsAttached)
            _thisTurretGrids.Add(rotor.TopGrid);
    }

    /*
     * I have these collection functions
     * (1) because they are cleaner and
     * (2) they save characters as they are tabulated less.
     */
    bool CollectWeaponsAndTools(IMyTerminalBlock block)
    {
        var weapon = block as IMyUserControllableGun;
        if (weapon != null)
        {
            if (weapon is IMyLargeTurretBase)
                return false;
            _guns.Add(weapon);
            if (_firstWeapon == null)
                _firstWeapon = weapon;
            return true;
        }

        var cam = block as IMyCameraBlock;
        if (cam != null)
        {
            _cameras.Add(cam);
            return true;
        }

        var tool = block as IMyShipToolBase;
        if (tool != null)
        {
            _tools.Add(tool);
            return true;
        }

        var light = block as IMyLightingBlock;
        if (light != null)
        {
            _lights.Add(light);
            return true;
        }

        var timer = block as IMyTimerBlock;
        if (timer != null)
        {
            _timers.Add(timer);
            return true;
        }

        return false;
    }

    bool CollectRotors(IMyTerminalBlock block)
    {
        var rotor = block as IMyMotorStator;
        if (rotor != null)
        {
            if (StringExtensions.Contains(block.CustomName, _elevationRotorName))
            {
                _additionalElevationRotors.Add(rotor);
                AddRotorGridsToHash(block as IMyMotorStator);
            }
            else if (StringExtensions.Contains(block.CustomName, _azimuthRotorName))
            {
                if (_azimuthRotor != null)
                {
                    Echo("WARN: Only one azimuth rotor is supported\nper turret. Additional ones will\nbe ignored.");
                    return true;
                }
                _azimuthRotor = block as IMyMotorStator;
                AddRotorGridsToHash(block as IMyMotorStator, false);

            }
            GetRotorRestAngle(rotor);
            return true;
        }
        return false;
    }

    void GetRotorRestAngle(IMyMotorStator rotor)
    {
        float restAngle = 0;
        if (float.TryParse(rotor.CustomData, out restAngle))
        {
            _rotorRestAngles[rotor.EntityId] = MathHelper.ToRadians(restAngle) % MathHelper.TwoPi;
        }
    }

    IMyTerminalBlock GetTurretReferenceOnRotorHead(IMyMotorStator rotor)
    {
        IMyTerminalBlock block = GetBlockOnSameGrid(rotor.TopGrid, _guns);
        if (block != null)
            return block;

        block = GetBlockOnSameGrid(rotor.TopGrid, _cameras);
        if (block != null)
            return block;

        block = GetBlockOnSameGrid(rotor.TopGrid, _tools);
        if (block != null)
            return block;

        block = GetBlockOnSameGrid(rotor.TopGrid, _lights);
        if (block != null)
            return block;

        return null;
    }

    IMyTerminalBlock GetBlockOnSameGrid<T>(IMyCubeGrid grid, List<T> list) where T : class, IMyTerminalBlock
    {
        foreach (IMyTerminalBlock block in list)
        {
            if (grid == block.CubeGrid)
            {
                return block;
            }
        }
        return null;
    }

    bool GrabBlocks(List<IMyTerminalBlock> _groupBlocks, bool verbose)
    {
        _mainElevationRotor = null;
        _azimuthRotor = null;
        _designator = null;
        _firstWeapon = null;
        _rotorTurretReference = null;

        _guns.Clear();
        _tools.Clear();
        _lights.Clear();
        _cameras.Clear();
        _timers.Clear();
        _thisTurretGrids.Clear();
        _turretDesignators.Clear();
        _additionalElevationRotors.Clear();
        _rotorRestAngles.Clear();

        ThisGroup.GetBlocks(_groupBlocks);

        ClearErrorOutput();

        foreach (IMyTerminalBlock thisBlock in _groupBlocks)
        {
            if (CollectWeaponsAndTools(thisBlock))
                continue;

            if (CollectRotors(thisBlock))
                continue;

            var turret = thisBlock as IMyLargeTurretBase;
            if (turret != null && StringExtensions.Contains(thisBlock.CustomName, _designatorName))
            {
                if (!turret.IsFunctional)
                    continue;
                _turretDesignators.Add(turret);
                EnableTurretAI(turret);
                continue;
            }
        }

        /*
         * Grabs elevation rotor by checking for a valid weapon/tool on its grid
         * and then saves the valid weapon/tool as a directional referencefor later.
         */
        foreach (var rotor in _additionalElevationRotors)
        {
            _rotorTurretReference = GetTurretReferenceOnRotorHead(rotor);
            if (_rotorTurretReference != null)
            {
                _mainElevationRotor = rotor;
                break;
            }
        }

        bool noErrors = true;
        if (_guns.Count == 0 && _tools.Count == 0 && _cameras.Count == 0 && _lights.Count == 0)
        {
            if (verbose)
                EchoError("No weapons, tools, lights or\ncameras found");
            noErrors = false;
        }

        if (_turretDesignators.Count == 0)
        {
            if (verbose)
                EchoError("No designators found");
            noErrors = false;
        }

        if (_azimuthRotor == null)
        {
            if (verbose)
                EchoError("No azimuth rotor found");
            noErrors = false;
        }

        if (_mainElevationRotor == null)
        {
            if (_additionalElevationRotors.Count == 0 && verbose)
                EchoError("No elevation rotor(s) found");
            else if (verbose)
                EchoError($"None of the {_additionalElevationRotors.Count} elevation\nrotor(s) has weapons/tools attached to them");
            noErrors = false;
        }
        else
        {
            _additionalElevationRotors.Remove(_mainElevationRotor); /* Remove main elevation rotor from the list so it isnt double counted. */
        }

        return noErrors;
    }

    bool GrabBlocksAI(List<IMyTerminalBlock> groupBlocks, bool verbose)
    {
        _designator = null;
        _slavedTurrets.Clear();
        _turretDesignators.Clear();
        ClearErrorOutput();

        foreach (IMyTerminalBlock thisBlock in groupBlocks)
        {
            var turret = thisBlock as IMyLargeTurretBase;
            if (turret == null)
                continue;

            if (StringExtensions.Contains(turret.CustomName, _designatorName) && turret.IsFunctional)
            {
                _turretDesignators.Add(turret);
                EnableTurretAI(turret);
            }
            else
            {
                turret.SetValue("Range", 1f);
                if (turret.EnableIdleRotation)
                    turret.EnableIdleRotation = false;
                _slavedTurrets.Add(turret);
            }

        }

        bool setupError = false;
        if (_slavedTurrets.Count == 0)
        {
            if (verbose)
                EchoError($"No slaved AI turrets found");
            setupError = true;
        }

        if (_turretDesignators.Count == 0) /* second null check (If STILL null) */
        {
            if (verbose)
                EchoError($"No designators found");
            setupError = true;
        }

        return !setupError;
    }

    IMyTerminalBlock GetLeadingWeapon<T>(List<T> weaponsAndTools) where T : class, IMyTerminalBlock
    {
        int projectileCount = 0;
        int rocketCount = 0;
        IMyTerminalBlock rocketReference = null;
        IMyTerminalBlock projectileReference = null;

        if (IsRotorTurret)
        {
            foreach (var block in weaponsAndTools)
            {
                if (block is IMySmallGatlingGun)
                {
                    projectileCount++;
                    projectileReference = block;
                }
                else if (block is IMySmallMissileLauncher)
                {
                    rocketCount++;
                    rocketReference = block;
                }
            }
        }
        else
        {
            foreach (var block in _slavedTurrets)
            {
                if (block is IMyLargeGatlingTurret || block is IMyLargeInteriorTurret)
                {
                    projectileCount++;
                    projectileReference = block;
                }
                else if (block is IMyLargeMissileTurret)
                {
                    rocketCount++;
                    rocketReference = block;
                }
            }
        }

        if (projectileCount >= rocketCount && projectileCount != 0)
            return projectileReference;
        else if (rocketCount > projectileCount)
            return rocketReference;
        else
            return null;
    }
    #endregion

    #region Main Entrypoint
    public void DoWork(Vector3D gridVelocity)
    {
        EchoOutput.Clear();

        Echo($"_____________________________\n\n'{ThisGroup.Name}'\n");

        // If the turret group is not functional, grab blocks and return
        if (!_isSetup)
        {
            GetTurretGroupBlocks(true);

            if (IsRotorTurret)
                StopRotorMovement();
            else
                ResetTurretTargeting(_slavedTurrets);

            return;
        }

        this._gridVelocity = gridVelocity;

        _averageWeaponPos = GetAverageWeaponPosition();
        _designator = GetDesignatorTurret(_turretDesignators, _averageWeaponPos);

        if (IsRotorTurret)
        {
            HandleRotorTurret();
        }
        else // AI turret slaving
        {
            HandleSlavedAiTurret();
        }

        Echo($"Instruction sum: {_program.Runtime.CurrentInstructionCount}");
    }

    void HandleRotorTurret()
    {
        if (_designator == null)
        {
            ToggleWeaponsAndTools(false, false);
            return;
        }

        if ((_designator.IsUnderControl || _designator.HasTarget) && _designator.IsWorking)
        {
            RotorTurretTargeting(ThisGroup);
            Echo($"Rotor turret is targeting");
            _framesSinceLastLock = 0;
        }
        else
        {
            ToggleWeaponsAndTools(false, false);

            if (_framesSinceLastLock < _returnToRestDelay)
            {
                _framesSinceLastLock++;
                StopRotorMovement();
            }
            else
                ReturnToEquilibrium();

            Echo($"Rotor turret is idle");
        }

        var num = _mainElevationRotor == null ? 0 : 1;
        Echo($"Targeting: {_designator.HasTarget || _designator.IsUnderControl}");
        Echo($"Grid intersection: {_intersection}");
        Echo($"Elevation rotors: {_additionalElevationRotors.Count + num}");
        Echo($"Weapons: {_guns.Count}");
        Echo($"Tools: {_tools.Count}");
        Echo($"Lights: {_lights.Count}");
        Echo($"Cameras: {_cameras.Count}");
        Echo($"Timers: {_timers.Count}");
        Echo($"Designators: {_turretDesignators.Count}");
        if (_muzzleVelocity.HasValue)
        {
            Echo($"Muzzle velocity: {_muzzleVelocity.Value} m/s");
        }
    }

    void HandleSlavedAiTurret()
    {
        if (_designator == null)
        {
            ToggleWeaponsAndTools(false, false);
            return;
        }

        if (_designator.EnableIdleRotation)
            _designator.EnableIdleRotation = false;

        //guide on target
        if ((_designator.IsUnderControl || _designator.HasTarget) && _designator.IsWorking)
        {
            SlavedTurretTargeting();
            Echo($"Slaved turret(s) targeting");
        }
        else
        {
            if (_isShooting != false)
            {
                foreach (IMyLargeTurretBase thisTurret in _slavedTurrets)
                {
                    thisTurret.SetValue("Shoot", false);

                    if (thisTurret.EnableIdleRotation)
                        thisTurret.EnableIdleRotation = false;
                }
                _isShooting = false;
            }

            Echo($"Slaved turret(s) idle");
        }

        Echo($"Targeting: {_designator.HasTarget || _designator.IsUnderControl}");
        Echo($"Grid intersection: {_intersection}");
        Echo($"Slaved turrets: {_slavedTurrets.Count}");
        Echo($"Designators: {_turretDesignators.Count}");
    }
    #endregion

    #region Helper Functions
    void SetPidValues()
    {
        _elevationPID = new DecayingIntegralPID(_proportionalGain, _integralGain, _derivativeGain, MAIN_UPDATE_INTERVAL, _integralDecayRatio);
        _azimuthPID = new DecayingIntegralPID(_proportionalGain, _integralGain, _derivativeGain, MAIN_UPDATE_INTERVAL, _integralDecayRatio);
    }

    public void VerifyAllGroupBlocks()
    {
        //check for broken blocks in setup groups
        if (_isSetup)
            _isSetup = VerifyBlocks(_groupBlocks);
    }

    private bool VerifyBlocks(List<IMyTerminalBlock> blocks)
    {
        foreach (var x in blocks)
        {
            if (IsClosed(x))
                return false;
        }
        return true;
    }

    public static bool IsClosed(IMyTerminalBlock block)
    {
        return block.WorldMatrix == MatrixD.Identity;
    }

    void Echo(string data)
    {
        EchoOutput.AppendLine(data);
    }

    void EchoError(string data)
    {
        if (ErrorOutput.Length == 0)
        {
            ErrorOutput.Append("_____________________________\nErrors for '").Append(ThisGroup.Name).AppendLine("'");
        }

        ErrorOutput.Append($"> Error {++_errorCount}: ").AppendLine(data);
    }

    void ClearErrorOutput()
    {
        ErrorOutput.Clear();
        _errorCount = 0;
    }

    #endregion

    #region Targeting Functions
    Vector3D GetTargetPoint(Vector3D shooterPosition, IMyLargeTurretBase designator)
    {
        if (designator.IsUnderControl)
        {
            _targetVec = designator.GetPosition() + VectorAzimuthElevation(designator) * _convergenceRange;
            _lastTargetEntityId = 0;
        }
        else if (designator.HasTarget)
        {
            var targetInfo = designator.GetTargetedEntity();

            /*
             * We reset our PID controllers and make acceleration compute to zero to handle switching off targets.
             */
            if (targetInfo.EntityId != _lastTargetEntityId)
            {
                _lastTargetVelocity = targetInfo.Velocity;
                if (IsRotorTurret)
                {
                    _azimuthPID.Reset();
                    _elevationPID.Reset();
                }
            }
            _lastTargetEntityId = targetInfo.EntityId;

            var targetPosition = targetInfo.Position + (targetInfo.Velocity - _gridVelocity) * MAIN_UPDATE_INTERVAL * 0.5;

            if (_isRocket && _muzzleVelocity.Value == _initialMuzzleVelocity)
                _targetVec = CalculateMissileInterceptPoint(
                        _muzzleVelocity.Value, 
                        UPDATES_PER_SECOND, 
                        _gridVelocity, 
                        shooterPosition, 
                        targetInfo.Velocity, 
                        targetPosition, 
                        _lastTargetVelocity);
            else
                _targetVec = CalculateProjectileInterceptPoint(
                        _muzzleVelocity.Value,
                        UPDATES_PER_SECOND,
                        _gridVelocity,
                        shooterPosition,
                        targetInfo.Velocity,
                        targetPosition,
                        _lastTargetVelocity);

            _lastTargetVelocity = targetInfo.Velocity;
        }
        else
        {
            _lastTargetEntityId = 0;
        }

        return _targetVec;
    }

    private Vector3D CalculateProjectileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity)
    {
        double temp = 0;
        return CalculateProjectileInterceptPoint(
                projectileSpeed,
                updateFrequency,
                shooterVelocity,
                shooterPosition,
                targetVelocity,
                targetPosition,
                lastTargetVelocity,
                out temp);
    }

    private Vector3D CalculateProjectileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity,
            out double timeToIntercept)
    {
        timeToIntercept = -1;

        var directHeading = targetPosition - shooterPosition;
        var directHeadingNorm = Vector3D.Normalize(directHeading);
        var distanceToTarget = Vector3D.Dot(directHeading, directHeadingNorm);

        var relativeVelocity = targetVelocity - shooterVelocity;

        var parallelVelocity = relativeVelocity.Dot(directHeadingNorm) * directHeadingNorm;
        var normalVelocity = relativeVelocity - parallelVelocity;
        var diff = projectileSpeed * projectileSpeed - normalVelocity.LengthSquared();
        if (diff < 0)
            return targetPosition;

        var projectileForwardSpeed = Math.Sqrt(diff);
        var projectileForwardVelocity = projectileForwardSpeed * directHeadingNorm;
        timeToIntercept = distanceToTarget / projectileForwardSpeed;

        var interceptPoint = shooterPosition + (projectileForwardVelocity + normalVelocity) * timeToIntercept;
        var targetAcceleration = updateFrequency * (targetVelocity - lastTargetVelocity);

        /*
         * We return here if we are at or over the max speed as predicting acceleration becomes an exercise in folly
         * as the solution becomes numerical and not analytical. We also return if acceleration is really close to
         * zero for obvious reasons.
         */
        if (targetVelocity.LengthSquared() >= _gameMaxSpeed * _gameMaxSpeed || Vector3D.IsZero(targetAcceleration, 1e-3))
            return interceptPoint;

        /*
         * Getting our time to critcal point where we hit the speed cap.
         * vf = vi + a*t
         * (vf - vi) / a
         */
        var velocityInAccelDirn = VectorMath.Projection(targetVelocity, targetAcceleration).Length() * Math.Sign(Vector3D.Dot(targetVelocity, targetAcceleration));
        var timeToSpeedCap = (_gameMaxSpeed - velocityInAccelDirn) / targetAcceleration.Length();

        /*
         * This is our estimate adding on the displacement due to the target acceleration UNTIL
         * it hits the speed cap.
         * vf^2 = vi^2 + 2*a*d
         * d = v * t + .5 * a * t^2
         */
        var timeAcceleration = Math.Min(timeToSpeedCap, timeToIntercept);
        var timePostAcceleration = timeToIntercept - timeAcceleration;
        var adjustedInterceptPoint = interceptPoint + 0.5 * targetAcceleration * timeAcceleration * timeAcceleration;
        var parallelAccelerationRatio = 1; //Math.Abs(VectorMath.CosBetween(targetVelocity, targetAcceleration));
        return (1 - parallelAccelerationRatio) * interceptPoint + parallelAccelerationRatio * adjustedInterceptPoint;
    }

    private Vector3D CalculateMissileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity)
    {
        double interceptTimeEstimate = 0.0;
        var firstInterceptGuess = CalculateProjectileInterceptPoint(
            projectileSpeed,
            updateFrequency,
            Vector3D.Zero,
            shooterPosition,
            targetVelocity,
            targetPosition,
            lastTargetVelocity,
            out interceptTimeEstimate);

        /*
         * In this method, we use two empirical regression equations to predict how missiles will
         * behave once they hit the speed cap. One for average velocity and one for lateral displacement.
         */
        var forwardDirection = firstInterceptGuess - shooterPosition;
        var lateralShooterVelocity = VectorMath.Rejection(shooterVelocity, forwardDirection);
        var forwardShooterVelocity = shooterVelocity - lateralShooterVelocity;
        var lateralShooterSpeed = lateralShooterVelocity.Length();
        var forwardShooterSpeed = forwardShooterVelocity.Length() * Math.Sign(forwardShooterVelocity.Dot(forwardDirection));
        var averageMissileVelocity = CalculateMissileAverageVelocity(forwardShooterSpeed, lateralShooterSpeed, interceptTimeEstimate);
        var displacement = CalculateMissileLateralDisplacement(forwardShooterSpeed, lateralShooterSpeed, interceptTimeEstimate);
        var firstDisplacementVec = lateralShooterSpeed == 0 ? Vector3D.Zero : -displacement * lateralShooterVelocity / lateralShooterSpeed;
        return firstInterceptGuess + firstDisplacementVec;
    }

    //Whip's CalculateMissileLateralDisplacement Method v3 - 5/4/18
    #region Empirical Curve Fits
    /*
     * These nasty bastards were compiled by numerically simulating the behavior of missiles under differing initial
     * conditions and then using regression to interpolate missile flight characteristics for given inputs.
     */
    static double[] coeffs = new double[0];
    static readonly double[] coeffs800 = new double[] { 8.273620e-07, -4.074818e-04, -1.664885e-03, -4.890871e-05, 5.584003e-01, -1.036071e-01 };
    static readonly double[] coeffs700 = new double[] { 8.345401e-07, -4.074575e-04, -1.664838e-03, -4.894641e-05, 5.583784e-01, -1.036277e-01 };
    static readonly double[] coeffs600 = new double[] { 8.574764e-07, -4.073949e-04, -1.664709e-03, -4.901365e-05, 5.583180e-01, -1.036942e-01 };
    static readonly double[] coeffs500 = new double[] { 9.285435e-07, -4.072316e-04, -1.664338e-03, -4.918805e-05, 5.581436e-01, -1.039112e-01 };
    static readonly double[] coeffs400 = new double[] { 1.095375e-06, -4.068213e-04, -1.663140e-03, -5.029227e-05, 5.576205e-01, -1.044476e-01 };
    static readonly double[] coeffs300 = new double[] { 1.667607e-06, -4.056386e-04, -1.659285e-03, -5.870287e-05, 5.559914e-01, -1.062660e-01 };
    static readonly double[] coeffs200 = new double[] { 3.244393e-06, -4.037420e-04, -1.646552e-03, -9.560673e-05, 5.508962e-01, -1.113855e-01 };
    static readonly double[] coeffs100 = new double[] { 8.703641e-06, -4.043099e-04, -1.600068e-03, -3.163374e-04, 5.347254e-01, -1.310425e-01 };
    static readonly double[] coeffs050 = new double[] { 1.715572e-05, -4.076979e-04, -1.520712e-03, -7.353804e-04, 5.084648e-01, -1.620291e-01 };

    private static double CalculateMissileLateralDisplacement(double forwardVelocity, double lateralVelocity, double timeToIntercept = 4)
    {
        if (timeToIntercept > 4)
            coeffs = coeffs800;
        else if (timeToIntercept > 3.5)
            coeffs = coeffs700;
        else if (timeToIntercept > 3)
            coeffs = coeffs600;
        else if (timeToIntercept > 2.5)
            coeffs = coeffs500;
        else if (timeToIntercept > 2)
            coeffs = coeffs400;
        else if (timeToIntercept > 1.5)
            coeffs = coeffs300;
        else if (timeToIntercept > 1)
            coeffs = coeffs200;
        else if (timeToIntercept > 0.5)
            coeffs = coeffs100;
        else
            coeffs = coeffs050;

        var num1 = coeffs[0] * forwardVelocity * forwardVelocity;
        var num2 = coeffs[1] * lateralVelocity * lateralVelocity;
        var num3 = coeffs[2] * lateralVelocity * forwardVelocity;
        var num4 = coeffs[3] * forwardVelocity;
        var num5 = coeffs[4] * lateralVelocity;
        var num6 = coeffs[5];
        return num1 + num2 + num3 + num4 + num5 + num6;
    }

    static double[] coeffsAvgVel = new double[0];
    static readonly double[] coeffsAvgVel800 = new double[] { -1.723360e-04, 1.230321e-04, -2.023321e-04, 5.127529e-02, 4.642541e-03, 2.097784e+02 };
    static readonly double[] coeffsAvgVel700 = new double[] { -1.968387e-04, 1.405248e-04, -2.310997e-04, 5.856561e-02, 5.302618e-03, 2.093470e+02 };
    static readonly double[] coeffsAvgVel600 = new double[] { -2.294639e-04, 1.638162e-04, -2.694036e-04, 6.827262e-02, 6.181505e-03, 2.087726e+02 };
    static readonly double[] coeffsAvgVel500 = new double[] { -2.750528e-04, 1.963625e-04, -3.229274e-04, 8.183671e-02, 7.409618e-03, 2.079700e+02 };
    static readonly double[] coeffsAvgVel400 = new double[] { -3.432477e-04, 2.450474e-04, -4.029921e-04, 1.021268e-01, 9.246714e-03, 2.067693e+02 };
    static readonly double[] coeffsAvgVel300 = new double[] { -4.564063e-04, 3.258322e-04, -5.358466e-04, 1.357950e-01, 1.229508e-02, 2.047771e+02 };
    static readonly double[] coeffsAvgVel200 = new double[] { -6.808684e-04, 4.860776e-04, -7.993778e-04, 2.025794e-01, 1.834184e-02, 2.008252e+02 };
    static readonly double[] coeffsAvgVel100 = new double[] { -1.339773e-03, 9.564752e-04, -1.572969e-03, 3.986240e-01, 3.609201e-02, 1.892246e+02 };
    static readonly double[] coeffsAvgVel050 = new double[] { -2.247077e-03, 1.795438e-03, -2.796223e-03, 7.398802e-01, 6.514080e-02, 1.670806e+02 };

    private static double CalculateMissileAverageVelocity(double forwardVelocity, double lateralVelocity, double timeToIntercept = 4)
    {
        if (timeToIntercept > 4)
            coeffsAvgVel = coeffsAvgVel800;
        else if (timeToIntercept > 3)
            coeffsAvgVel = coeffsAvgVel700;
        else if (timeToIntercept > 2.5)
            coeffsAvgVel = coeffsAvgVel600;
        else if (timeToIntercept > 2)
            coeffsAvgVel = coeffsAvgVel500;
        else if (timeToIntercept > 1.5)
            coeffsAvgVel = coeffsAvgVel400;
        else if (timeToIntercept > 1)
            coeffsAvgVel = coeffsAvgVel300;
        else if (timeToIntercept > 0.5)
            coeffsAvgVel = coeffsAvgVel200;
        else if (timeToIntercept > 0.25)
            coeffsAvgVel = coeffsAvgVel100;
        else
            coeffsAvgVel = coeffsAvgVel050;

        var num1 = coeffsAvgVel[0] * forwardVelocity * forwardVelocity;
        var num2 = coeffsAvgVel[1] * lateralVelocity * lateralVelocity;
        var num3 = coeffsAvgVel[2] * lateralVelocity * forwardVelocity;
        var num4 = coeffsAvgVel[3] * forwardVelocity;
        var num5 = coeffsAvgVel[4] * lateralVelocity;
        var num6 = coeffsAvgVel[5];
        return num1 + num2 + num3 + num4 + num5 + num6;
    }
    #endregion

    static bool IsProjectileRocket(IMyTerminalBlock block)
    {
        return block is IMyLargeMissileTurret || block is IMySmallMissileLauncher;
    }

    double GetInitialMuzzleVelocity(IMyTerminalBlock block)
    {
        if (block is IMyLargeGatlingTurret || block is IMySmallGatlingGun)
            return 400;
        else if (block is IMyLargeInteriorTurret)
            return 300;
        else if (block is IMyLargeMissileTurret || block is IMySmallMissileLauncher)
            return 212.8125;
        else
            return DEFAULT_PROJECTILE_SPEED;
    }

    Vector3D GetAverageTurretPosition()
    {
        Vector3D positionSum = Vector3D.Zero;
        if (_slavedTurrets.Count == 0)
            return positionSum;

        foreach (var block in _slavedTurrets) { positionSum += block.GetPosition(); }
        return positionSum / _slavedTurrets.Count;
    }

    Vector3D GetAverageWeaponPosition()
    {
        Vector3D positionSum = Vector3D.Zero;

        if (_guns.Count != 0)
        {
            foreach (var block in _guns) { positionSum += block.GetPosition(); }
            return positionSum / _guns.Count;
        }

        /*
         * This is a fall-through in case the user has no guns. The code will use the
         * tools for alignment instead.
         */
        int toolCount = _lights.Count + _cameras.Count + _tools.Count;
        if (toolCount == 0)
            return positionSum;
        foreach (var block in _lights) { positionSum += block.GetPosition(); }
        foreach (var block in _cameras) { positionSum += block.GetPosition(); }
        foreach (var block in _tools) { positionSum += block.GetPosition(); }
        return positionSum / toolCount;
    }

    static void EnableTurretAI(IMyLargeTurretBase turret)
    {
        //if (turret.AIEnabled)
        //    return;

        turret.ResetTargetingToDefault();
        turret.EnableIdleRotation = false;
    }
    #endregion

    #region Weapon Control
    void ToggleWeaponsAndTools(bool toggleWeapons, bool toggleLightsAndTools)
    {
        /*
         * This attempts to avoid spamming terminal actions if we have already set the shoot state.
         */
        if (_isShooting != toggleWeapons)
        {
            foreach (var weapon in _guns)
                weapon.SetValue("Shoot", toggleWeapons);

            _isShooting = toggleWeapons;
        }

        if (_toolsOn != toggleLightsAndTools)
        {
            ChangePowerState(_tools, toggleLightsAndTools);
            ChangePowerState(_lights, toggleLightsAndTools);
            _toolsOn = toggleLightsAndTools;
        }

        foreach (var timer in _timers)
        {
            if (toggleWeapons)
            {
                if (timer.IsCountingDown)
                    continue;
                else
                    timer.StartCountdown();
            }
        }
    }

    static void ChangePowerState<T>(List<T> list, bool stateToSet) where T : class, IMyFunctionalBlock
    {
        foreach (IMyFunctionalBlock block in list)
        {
            if (block.Enabled != stateToSet)
                block.Enabled = stateToSet;
        }
    }
    #endregion

    #region Designator Selection
    IMyLargeTurretBase GetDesignatorTurret(List<IMyLargeTurretBase> turretDesignators, Vector3D referencePos)
    {
        IMyLargeTurretBase closestTurret = null;
        double closestDistanceSq = double.MaxValue;
        foreach (var block in turretDesignators)
        {
            if (block.IsUnderControl)
                return block;

            if (block.HasTarget)
            {
                var distanceSq = Vector3D.DistanceSquared(block.GetPosition(), referencePos);
                if (distanceSq + 1e-3 < closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    closestTurret = block;
                }
            }
        }

        if (closestTurret == null)
        {
            closestTurret = turretDesignators.Count == 0 ? null : turretDesignators[0];
        }
        return closestTurret;
    }
    #endregion

    #region Rotor Turret Control
    void RotorTurretTargeting(IMyBlockGroup ThisGroup)
    {
        Vector3D aimPosition = GetTargetPoint(_averageWeaponPos, _designator);
        Vector3D targetDirection = aimPosition - _averageWeaponPos;
        Vector3D targetDirectionNorm = Vector3D.IsZero(targetDirection) ? Vector3D.Zero : Vector3D.Normalize(targetDirection);

        if (_avoidFriendlyFire)
            _intersection = CheckForFF(_averageWeaponPos, targetDirectionNorm, _azimuthRotor);
        else
            _intersection = false;

        Vector3D turretFrontVec = _rotorTurretReference.WorldMatrix.Forward;
        Vector3D absUpVec = _azimuthRotor.WorldMatrix.Up;
        Vector3D turretSideVec = _mainElevationRotor.WorldMatrix.Up;
        Vector3D turretFrontCrossSide = turretFrontVec.Cross(turretSideVec);
        Vector3D baseUp = absUpVec;
        Vector3D baseLeft = baseUp.Cross(turretFrontVec);
        Vector3D baseForward = baseLeft.Cross(baseUp);

        /*
         * Here we check elevation rotor orientation w.r.t. reference to correct for turrets
         * mounted opposite of assumed convention.
         */
        Vector3D turretUpVec;
        Vector3D turretLeftVec;
        if (absUpVec.Dot(turretFrontCrossSide) >= 0)
        {
            turretUpVec = turretFrontCrossSide;
            turretLeftVec = turretSideVec;
        }
        else
        {
            turretUpVec = -1 * turretFrontCrossSide;
            turretLeftVec = -1 * turretSideVec;
        }

        /*
         * We need 2 sets of angles to be able to prevent the turret from trying to rotate over 90°
         * vertical to get to a target behind it. This ensures that the elevation angle is always
         * lies in the domain: -90° < elevation < 90°.
         */
        double desiredAzimuthAngle, desiredElevationAngle, currentAzimuthAngle, currentElevationAngle, azimuthAngle, elevationAngle;
        GetRotationAngles(targetDirection, baseForward, baseLeft, baseUp, out desiredAzimuthAngle, out desiredElevationAngle);
        GetRotationAngles(turretFrontVec, baseForward, baseLeft, baseUp, out currentAzimuthAngle, out currentElevationAngle);
        elevationAngle = (desiredElevationAngle - currentElevationAngle) * -Math.Sign(absUpVec.Dot(turretFrontCrossSide));
        azimuthAngle = desiredAzimuthAngle - currentAzimuthAngle;
        azimuthAngle = GetAllowedRotationAngle(azimuthAngle, _azimuthRotor);

        double azimuthError, elevationError;
        ComputeShipHeadingError(out azimuthError, out elevationError);

        double azimuthSpeed = _azimuthPID.Control(azimuthAngle - azimuthError);
        double elevationSpeed = _elevationPID.Control(elevationAngle - elevationError);

        /*
         * Negative because we want to cancel the positive angle via our movements.
         */
        _azimuthRotor.TargetVelocityRPM = -(float)azimuthSpeed;
        _mainElevationRotor.TargetVelocityRPM = -(float)elevationSpeed;

        if (!_azimuthRotor.Enabled)
            _azimuthRotor.Enabled = true;

        if (!_mainElevationRotor.Enabled)
            _mainElevationRotor.Enabled = true;

        bool inRange = _autoEngagementRange * _autoEngagementRange >= targetDirection.LengthSquared();
        bool angleWithinTolerance = VectorMath.IsDotProductWithinTolerance(turretFrontVec, targetDirection, _toleranceDotProduct);
        bool toggleLights = _designator.IsUnderControl || _designator.HasTarget;
        bool shootWeapons = false;

        if (_designator.IsUnderControl && !_intersection && angleWithinTolerance && _designator.IsShooting) // If manually controlled
            shootWeapons = true;
        else if (!_onlyShootWhenDesignatorShoots && _designator.HasTarget && !_intersection && angleWithinTolerance && inRange) // If AI controlled
            shootWeapons = true;
        else if (_onlyShootWhenDesignatorShoots && _designator.HasTarget && _designator.IsShooting && !_intersection && angleWithinTolerance && inRange) // If AI controlled
            shootWeapons = true;

        ToggleWeaponsAndTools(shootWeapons, toggleLights);

        foreach (var rotor in _additionalElevationRotors)
        {
            if (!rotor.IsAttached)
            {
                Echo($"Warning: No rotor head for additional elevation\nrotor named '{rotor.CustomName}'\nSkipping this rotor...");
                continue;
            }

            IMyTerminalBlock reference = GetTurretReferenceOnRotorHead(rotor);
            if (reference == null)
            {
                Echo($"Warning: No weapons, tools, cameras, or lights\non elevation rotor named\n'{rotor.CustomName}'\nSkipping this rotor...");
                continue;
            }

            if (!rotor.Enabled)
                rotor.Enabled = true;

            var desiredFrontVec = reference.WorldMatrix.Forward;

            float multiplier = Math.Sign(rotor.WorldMatrix.Up.Dot(_mainElevationRotor.WorldMatrix.Up));

            var diff = (float)VectorMath.AngleBetween(desiredFrontVec, turretFrontVec) * Math.Sign(desiredFrontVec.Dot(turretFrontCrossSide)) * 100;
            rotor.TargetVelocityRPM = (float)elevationSpeed - multiplier * diff;

            if (!rotor.Enabled)
                rotor.Enabled = true;
        }
    }

    void ComputeShipHeadingError(out double azimuthError, out double elevationError)
    {
        if (_firstRun)
        {
            _firstRun = false;
            _lastElevationMatrix = _mainElevationRotor.WorldMatrix;
            _lastAzimuthMatrix = _azimuthRotor.WorldMatrix;
            azimuthError = 0;
            elevationError = 0;
            return;
        }

        azimuthError = CalculateRotorDeviationAngle(_azimuthRotor.WorldMatrix.Forward, _lastAzimuthMatrix);
        elevationError = CalculateRotorDeviationAngle(_mainElevationRotor.WorldMatrix.Forward, _lastElevationMatrix);

        _lastElevationMatrix = _mainElevationRotor.WorldMatrix;
        _lastAzimuthMatrix = _azimuthRotor.WorldMatrix;
    }

    static double GetAllowedRotationAngle(double initialAngle, IMyMotorStator rotor)
    {
        if (rotor.LowerLimitRad >= -MathHelper.TwoPi && rotor.UpperLimitRad <= MathHelper.TwoPi && rotor.UpperLimitRad - rotor.LowerLimitRad > Math.PI)
        {
            var currentAngleVector = rotor.Top.WorldMatrix.Backward; //GetVectorFromRotorAngle(rotor.Angle, rotor);
            var lowerLimitVector = GetVectorFromRotorAngle(rotor.LowerLimitRad, rotor);
            var upperLimitVector = GetVectorFromRotorAngle(rotor.UpperLimitRad, rotor);

            var upAxis = Vector3D.Cross(upperLimitVector, lowerLimitVector);
            var currentCrossLower = Vector3D.Cross(currentAngleVector, lowerLimitVector);
            var currentCrossUpper = Vector3D.Cross(currentAngleVector, lowerLimitVector);

            var angleToLowerLimit = Math.Acos(Vector3D.Dot(lowerLimitVector, currentAngleVector));
            if (Vector3D.Dot(upAxis, currentCrossLower) > 0)
                angleToLowerLimit = MathHelper.TwoPi - angleToLowerLimit;

            var angleToUpperLimit = Math.Acos(Vector3D.Dot(upperLimitVector, currentAngleVector));
            if (Vector3D.Dot(upAxis, currentCrossUpper) < 0)
                angleToUpperLimit = MathHelper.TwoPi - angleToUpperLimit;

            if (initialAngle > 0) //rotating towards lower bound
            {
                if (angleToLowerLimit < Math.Abs(initialAngle))
                {
                    var newAngle = -MathHelper.TwoPi + initialAngle;
                    if (angleToUpperLimit < Math.Abs(newAngle))
                        return 0;

                    return newAngle; //rotate opposite direction
                }
            }
            else
            {
                if (angleToUpperLimit < Math.Abs(initialAngle))
                {
                    var newAngle = MathHelper.TwoPi + initialAngle;
                    if (angleToLowerLimit < Math.Abs(newAngle))
                        return 0;

                    return newAngle;//rotate opposite direction
                }
            }

            return initialAngle; //conditional fall-through
        }
        else
            return initialAngle;
    }

    void ReturnToEquilibrium()
    {
        MoveRotorToEquilibrium(_azimuthRotor);
        MoveRotorToEquilibrium(_mainElevationRotor);

        foreach (var block in _additionalElevationRotors)
        {
            MoveRotorToEquilibrium(block);
        }
    }

    void MoveRotorToEquilibrium(IMyMotorStator rotor)
    {
        if (rotor == null)
            return;

        if (!rotor.Enabled)
            rotor.Enabled = true;

        float restAngle = 0;
        float currentAngle = rotor.Angle;
        float lowerLimitRad = rotor.LowerLimitRad;
        float upperLimitRad = rotor.UpperLimitRad;

        if (_rotorRestAngles.TryGetValue(rotor.EntityId, out restAngle))
        {
            if (lowerLimitRad >= -MathHelper.TwoPi && upperLimitRad <= MathHelper.TwoPi)
            {
                if (restAngle > upperLimitRad)
                    restAngle -= MathHelper.TwoPi;
                else if (restAngle < lowerLimitRad)
                    restAngle += MathHelper.TwoPi;
            }
            else
            {
                if (restAngle > currentAngle + MathHelper.Pi)
                    restAngle -= MathHelper.TwoPi;
                else if (restAngle < currentAngle - MathHelper.Pi)
                    restAngle += MathHelper.TwoPi;
            }

            float angularDeviation = (restAngle - currentAngle);
            float targetVelocity = (float)Math.Round(angularDeviation * _equilibriumRotationSpeed, 2);

            if (Math.Abs(angularDeviation) < 1e-2)
            {
                rotor.TargetVelocityRPM = 0;
            }
            else
            {
                rotor.TargetVelocityRPM = targetVelocity;
            }
        }
        else if (lowerLimitRad >= -MathHelper.TwoPi && upperLimitRad <= MathHelper.TwoPi)
        {
            float avgAngle = (lowerLimitRad + upperLimitRad) * 0.5f;
            avgAngle %= MathHelper.TwoPi;

            var angularDeviation = (avgAngle - currentAngle);
            var targetVelocity = (float)Math.Round(angularDeviation * _equilibriumRotationSpeed, 2);

            if (Math.Abs(angularDeviation) < 1e-2)
            {
                rotor.TargetVelocityRPM = 0;
            }
            else
            {
                rotor.TargetVelocityRPM = targetVelocity;
            }
        }
        else
        {
            rotor.TargetVelocityRPM = 0f;
        }
    }

    void StopRotorMovement()
    {
        if (_azimuthRotor != null)
            _azimuthRotor.TargetVelocityRPM = 0;

        if (_mainElevationRotor != null)
            _mainElevationRotor.TargetVelocityRPM = 0;

        foreach (var additionalElevationRotor in _additionalElevationRotors)
        {
            additionalElevationRotor.TargetVelocityRPM = 0f;
        }
    }
    #endregion

    #region Slaved Turret Control
    void SlavedTurretTargeting()
    {
        _isShooting = false;
        foreach (IMyLargeTurretBase thisTurret in _slavedTurrets)
        {
            Vector3D aimPosition = GetTargetPoint(thisTurret.GetPosition(), _designator);
            MatrixD turretMatrix = thisTurret.WorldMatrix;
            Vector3D turretDirection = VectorAzimuthElevation(thisTurret);
            Vector3D targetDirectionNorm = Vector3D.Normalize(aimPosition - turretMatrix.Translation);

            if (_avoidFriendlyFire)
                _intersection = CheckForFF(thisTurret.GetPosition(), targetDirectionNorm, thisTurret);
            else
                _intersection = false;

            //This shit is broke yo
            //thisTurret.SetTarget(aimPosition);
            double azimuth = 0; double elevation = 0;
            GetRotationAngles(targetDirectionNorm, turretMatrix.Forward, turretMatrix.Left, turretMatrix.Up, out azimuth, out elevation);
            thisTurret.Azimuth = (float)azimuth;
            thisTurret.Elevation = (float)elevation;
            SyncTurretAngles(thisTurret);

            bool inRange = _autoEngagementRange * _autoEngagementRange > Vector3D.DistanceSquared(aimPosition, turretMatrix.Translation);
            bool withinAngleTolerance = VectorMath.IsDotProductWithinTolerance(turretDirection, turretDirection, _toleranceDotProduct);
            bool shouldShoot = false;
            if (withinAngleTolerance && !_intersection)
            {
                if (_designator.IsUnderControl && _designator.IsShooting)
                    shouldShoot = true;
                else if (_designator.HasTarget)
                {
                    if (inRange)
                    {
                        if (!_onlyShootWhenDesignatorShoots)
                            shouldShoot = true;
                        else if (_designator.IsShooting)
                            shouldShoot = true;
                    }
                }
            }

            if (thisTurret.GetValue<bool>("Shoot") != shouldShoot)
            {
                thisTurret.SetValue("Shoot", shouldShoot);
            }

            _isShooting |= shouldShoot;

            if (thisTurret.EnableIdleRotation)
                thisTurret.EnableIdleRotation = false;
        }
    }

    static void SyncTurretAngles(IMyLargeTurretBase turret)
    {
        turret.SyncAzimuth(); //this syncs both angles
                              //turret.SyncEnableIdleRotation(); //this does nothing
    }

    static void ResetTurretTargeting(List<IMyTerminalBlock> turrets)
    {
        foreach (var block in turrets)
        {
            var thisTurret = block as IMyLargeTurretBase;

            if (thisTurret == null)
                continue;

            if (!thisTurret.AIEnabled)
            {
                thisTurret.ResetTargetingToDefault();
                thisTurret.EnableIdleRotation = false;
                thisTurret.ApplyAction("Shoot_Off"); //still no damn setter for this
                thisTurret.SetValue("Range", float.MaxValue); //still no damn setter for this
            }
        }
    }
    #endregion

    #region Vector Math Functions
    static void WrapAngleAroundPI(ref float angle)
    {
        angle %= MathHelper.TwoPi;

        if (angle > Math.PI)
            angle = -MathHelper.TwoPi + angle;
        else if (angle < -Math.PI)
            angle = MathHelper.TwoPi + angle;
    }

    static Vector3D GetVectorFromRotorAngle(float angle, IMyMotorStator rotor)
    {
        double x = MyMath.FastSin(angle);
        double y = MyMath.FastCos(angle);
        var rotorMatrix = rotor.WorldMatrix;
        return rotorMatrix.Backward * y + rotor.WorldMatrix.Left * x;
    }

    static Vector3D VectorAverage(Vector3D a, Vector3D b, double bias = 0.5)
    {
        return a * bias + b * (1 - bias);
    }

    static double CalculateRotorDeviationAngle(Vector3D forwardVector, MatrixD lastOrientation)
    {
        var flattenedForwardVector = VectorMath.Rejection(forwardVector, lastOrientation.Up);
        return VectorMath.AngleBetween(flattenedForwardVector, lastOrientation.Forward) * Math.Sign(flattenedForwardVector.Dot(lastOrientation.Left));
    }

    static Vector3D VectorAzimuthElevation(IMyLargeTurretBase turret)
    {
        double el = turret.Elevation;
        double az = turret.Azimuth;
        Vector3D targetDirection;
        Vector3D.CreateFromAzimuthAndElevation(az, el, out targetDirection);
        return Vector3D.TransformNormal(targetDirection, turret.WorldMatrix);
    }

    /*
     * Whip's Get Rotation Angles Method v12 - 2/16/18
     * Dependencies: VectorMath.AngleBetween()
     * Modified yaw sign for application
     */
    static void GetRotationAngles(Vector3D targetVector, Vector3D frontVec, Vector3D leftVec, Vector3D upVec, out double yaw, out double pitch)
    {
        var matrix = MatrixD.Zero;
        matrix.Forward = frontVec; matrix.Left = leftVec; matrix.Up = upVec;

        var localTargetVector = Vector3D.TransformNormal(targetVector, MatrixD.Transpose(matrix));
        var flattenedTargetVector = new Vector3D(localTargetVector.X, 0, localTargetVector.Z);

        yaw = VectorMath.AngleBetween(Vector3D.Forward, flattenedTargetVector) * -Math.Sign(localTargetVector.X); //right is positive
        if (Math.Abs(yaw) < 1E-6 && localTargetVector.Z > 0) //check for straight back case
            yaw = Math.PI;

        if (Vector3D.IsZero(flattenedTargetVector)) //check for straight up case
            pitch = MathHelper.PiOver2 * Math.Sign(localTargetVector.Y);
        else
            pitch = VectorMath.AngleBetween(localTargetVector, flattenedTargetVector) * Math.Sign(localTargetVector.Y); //up is positive
    }
    #endregion

    #region Intersection Checks
    List<Vector3I> scannedPoints = new List<Vector3I>();

    bool CheckForFF(Vector3D start, Vector3D dirnNorm, IMyTerminalBlock ignoredBlock)
    {
        bool intersection = false;
        Vector3D end = start; //This may not be precise if target is off axis by a bunch
        if (_isRocket)
            end += (dirnNorm * (_muzzleVelocity.Value - _gridVelocity.Dot(dirnNorm)) - _gridVelocity) * 5;
        else
            end += dirnNorm * 1000;

        foreach (var grid in _shipGrids)
        {
            if (_thisTurretGrids.Contains(grid))
                continue;

            intersection = CheckGridIntersection(start, end, grid, ignoredBlock, true, MAX_BLOCKS_TO_CHECK_FOR_FF);
            if (intersection)
                break;
        }

        return intersection;
    }

    /*
     * Checks for intersection of a line through a grid's blocks. Returns true if there is an intersection.
     */
    bool CheckGridIntersection(Vector3D startPosWorld, Vector3D endPosWorld, IMyCubeGrid cubeGrid, IMyTerminalBlock originBlock, bool checkFast = false, int maxIterations = 50)
    {
        Vector3D startPosGrid = WorldToGridVec(startPosWorld, cubeGrid);
        Vector3D endPosGrid = WorldToGridVec(endPosWorld, cubeGrid);
        var line = new LineD(startPosGrid, endPosGrid);

        double padding = cubeGrid.GridSizeEnum == MyCubeSize.Large ? 1.0 : 5.0; // Approx 2.5 meters of padding
        var box = new BoundingBoxD((Vector3D)cubeGrid.Min - 0.5 - padding, (Vector3D)cubeGrid.Max + 0.5 + padding);

        // Check fast if possible to save on runtime
        if (checkFast && !PointInBox(startPosGrid, box.Min, box.Max))
        {
            return CheckGridIntersectionFast(startPosGrid, endPosGrid, box);
        }

        var intersectedLine = new LineD();
        if (!box.Intersect(ref line, out intersectedLine))
        {
            Echo($"No intersection");
            return false;
        }

        Vector3I startInt = Vector3I.Round(intersectedLine.From);
        Vector3I endInt = Vector3I.Round(intersectedLine.To);

        IMySlimBlock slim = originBlock.CubeGrid.GetCubeBlock(originBlock.Position);

        Vector3D diff = endInt - startInt;
        Vector3I sign = Vector3I.Sign(diff);
        Vector3D diff_abs = diff * (Vector3D)sign;
        Vector3D dirn = Vector3D.Normalize(diff);
        Vector3D tMaxVec = 0.5 / diff_abs;
        scannedPoints.Clear();

        Vector3I point = startInt;
        for (int i = 0; i < maxIterations; i++)
        {
            scannedPoints.Add(point);
            if (BlockExistsAtPoint(cubeGrid, point, slim))
                return true;

            if (!PointInBox(point, box.Min, box.Max))
                return false;

            int minIndex = GetMinIndex(tMaxVec);

            point += (Vector3I)GetIncrement(sign, minIndex);
            tMaxVec += GetIncrement(2 * tMaxVec, minIndex);
        }
        return false;
    }

    bool CheckGridIntersectionFast(Vector3D startPosGrid, Vector3D endPosGrid, BoundingBoxD box)
    {
        var line = new LineD(startPosGrid, endPosGrid);
        return box.Intersects(ref line);
    }

    int GetMinIndex(Vector3D vec)
    {
        var min = vec.Min();
        if (min == vec.X) return 0;
        if (min == vec.Y) return 1;
        return 2;
    }

    Vector3D GetIncrement(Vector3D vec, int i)
    {
        switch (i)
        {
            case 0:
                return new Vector3D(vec.X, 0, 0);
            case 1:
                return new Vector3D(0, vec.Y, 0);
            case 2:
                return new Vector3D(0, 0, vec.Z);
            default:
                return Vector3D.Zero;
        }
    }


    static Vector3D WorldToGridVec(Vector3D position, IMyCubeGrid cubeGrid)
    {
        var direction = position - cubeGrid.GetPosition();
        return Vector3D.TransformNormal(direction, MatrixD.Transpose(cubeGrid.WorldMatrix)) / cubeGrid.GridSize;
    }

    static bool BlockExistsAtPoint(IMyCubeGrid cubeGrid, Vector3I point, IMySlimBlock blockToIgnore = null)
    {
        if (!cubeGrid.CubeExists(point))
            return false;

        var slim = cubeGrid.GetCubeBlock(point);
        return slim != blockToIgnore;
    }

    static bool PointInBox(Vector3D point, Vector3D boxMin, Vector3D boxMax)
    {
        if (boxMin.X <= point.X && point.X <= boxMax.X &&
            boxMin.Y <= point.Y && point.Y <= boxMax.Y &&
            boxMin.Z <= point.Z && point.Z <= boxMax.Z)
        {
            return true;
        }
        return false;
    }
    #endregion
}
#endregion

#region Getting All Grids
readonly List<IMyMechanicalConnectionBlock> allMechanical = new List<IMyMechanicalConnectionBlock>();
readonly HashSet<IMyCubeGrid> _allShipGrids = new HashSet<IMyCubeGrid>();
void GetAllGrids()
{
    _allShipGrids.Clear();
    _allShipGrids.Add(Me.CubeGrid);

    GridTerminalSystem.GetBlocksOfType(allMechanical);
    foreach (var block in allMechanical)
    {
        _allShipGrids.Add(block.CubeGrid);

        if (block.IsAttached && block.TopGrid != null)
            _allShipGrids.Add(block.TopGrid);
    }
}

bool GetIfGridIsBigger(IMyCubeGrid grid, ref double size)
{
    if (grid.WorldAABB.Volume > size)
    {
        size = grid.WorldAABB.Volume;
        return true;
    }
    return false;
}
#endregion

#region Circular Buffer
/// <summary>
/// A simple, generic circular buffer class with a fixed capacity.
/// </summary>
/// <typeparam name="T"></typeparam>
public class CircularBuffer<T>
{
    public readonly int Capacity;

    T[] _array = null;
    int _setIndex = 0;
    int _getIndex = 0;

    /// <summary>
    /// CircularBuffer ctor.
    /// </summary>
    /// <param name="capacity">Capacity of the CircularBuffer.</param>
    public CircularBuffer(int capacity)
    {
        if (capacity < 1)
            throw new Exception($"Capacity of CircularBuffer ({capacity}) can not be less than 1");
        Capacity = capacity;
        _array = new T[Capacity];
    }

    /// <summary>
    /// Adds an item to the buffer. If the buffer is full, it will overwrite the oldest value.
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        _array[_setIndex] = item;
        _setIndex = ++_setIndex % Capacity;
    }

    /// <summary>
    /// Retrieves the current item in the buffer and increments the buffer index.
    /// </summary>
    /// <returns></returns>
    public T MoveNext()
    {
        T val = _array[_getIndex];
        _getIndex = ++_getIndex % Capacity;
        return val;
    }

    /// <summary>
    /// Retrieves the current item in the buffer without incrementing the buffer index.
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
        return _array[_getIndex];
    }
}
#endregion

#region Scheduler
/// <summary>
/// Class for scheduling actions to occur at specific frequencies. Actions can be updated in parallel or in sequence (queued).
/// </summary>
public class Scheduler
{
    ScheduledAction _currentlyQueuedAction = null;
    bool _firstRun = true;

    readonly bool _ignoreFirstRun;
    readonly List<ScheduledAction> _scheduledActions = new List<ScheduledAction>();
    readonly List<ScheduledAction> _actionsToDispose = new List<ScheduledAction>();
    readonly Queue<ScheduledAction> _queuedActions = new Queue<ScheduledAction>();
    readonly Program _program;

    const double RUNTIME_TO_REALTIME = (1.0 / 60.0) / 0.0166666;

    /// <summary>
    /// Constructs a scheduler object with timing based on the runtime of the input program.
    /// </summary>
    public Scheduler(Program program, bool ignoreFirstRun = false)
    {
        _program = program;
        _ignoreFirstRun = ignoreFirstRun;
    }

    /// <summary>
    /// Updates all ScheduledAcions in the schedule and the queue.
    /// </summary>
    public void Update()
    {
        double deltaTime = Math.Max(0, _program.Runtime.TimeSinceLastRun.TotalSeconds * RUNTIME_TO_REALTIME);

        if (_ignoreFirstRun && _firstRun)
            deltaTime = 0;

        _firstRun = false;
        _actionsToDispose.Clear();
        foreach (ScheduledAction action in _scheduledActions)
        {
            action.Update(deltaTime);
            if (action.JustRan && action.DisposeAfterRun)
            {
                _actionsToDispose.Add(action);
            }
        }

        // Remove all actions that we should dispose
        _scheduledActions.RemoveAll((x) => _actionsToDispose.Contains(x));

        if (_currentlyQueuedAction == null)
        {
            // If queue is not empty, populate current queued action
            if (_queuedActions.Count != 0)
                _currentlyQueuedAction = _queuedActions.Dequeue();
        }

        // If queued action is populated
        if (_currentlyQueuedAction != null)
        {
            _currentlyQueuedAction.Update(deltaTime);
            if (_currentlyQueuedAction.JustRan)
            {
                // Set the queued action to null for the next cycle
                _currentlyQueuedAction = null;
            }
        }
    }

    /// <summary>
    /// Adds an Action to the schedule. All actions are updated each update call.
    /// </summary>
    public void AddScheduledAction(Action action, double updateFrequency, bool disposeAfterRun = false, double timeOffset = 0)
    {
        ScheduledAction scheduledAction = new ScheduledAction(action, updateFrequency, disposeAfterRun, timeOffset);
        _scheduledActions.Add(scheduledAction);
    }

    /// <summary>
    /// Adds a ScheduledAction to the schedule. All actions are updated each update call.
    /// </summary>
    public void AddScheduledAction(ScheduledAction scheduledAction)
    {
        _scheduledActions.Add(scheduledAction);
    }

    /// <summary>
    /// Adds an Action to the queue. Queue is FIFO.
    /// </summary>
    public void AddQueuedAction(Action action, double updateInterval)
    {
        if (updateInterval <= 0)
        {
            updateInterval = 0.001; // avoids divide by zero
        }
        ScheduledAction scheduledAction = new ScheduledAction(action, 1.0 / updateInterval, true);
        _queuedActions.Enqueue(scheduledAction);
    }

    /// <summary>
    /// Adds a ScheduledAction to the queue. Queue is FIFO.
    /// </summary>
    public void AddQueuedAction(ScheduledAction scheduledAction)
    {
        _queuedActions.Enqueue(scheduledAction);
    }
}

public class ScheduledAction
{
    public bool JustRan { get; private set; } = false;
    public bool DisposeAfterRun { get; private set; } = false;
    public double TimeSinceLastRun { get; private set; } = 0;
    public readonly double RunInterval;

    readonly double _runFrequency;
    readonly Action _action;

    /// <summary>
    /// Class for scheduling an action to occur at a specified frequency (in Hz).
    /// </summary>
    /// <param name="action">Action to run</param>
    /// <param name="runFrequency">How often to run in Hz</param>
    public ScheduledAction(Action action, double runFrequency, bool removeAfterRun = false, double timeOffset = 0)
    {
        _action = action;
        _runFrequency = runFrequency;
        RunInterval = 1.0 / _runFrequency;
        DisposeAfterRun = removeAfterRun;
        TimeSinceLastRun = timeOffset;
    }

    public void Update(double deltaTime)
    {
        TimeSinceLastRun += deltaTime;

        if (TimeSinceLastRun >= RunInterval)
        {
            _action.Invoke();
            TimeSinceLastRun = 0;

            JustRan = true;
        }
        else
        {
            JustRan = false;
        }
    }
}
#endregion

#region PID Class
/// <summary>
/// Discrete time PID controller class.
/// </summary>
public class PID
{
    readonly double _kP = 0;
    readonly double _kI = 0;
    readonly double _kD = 0;

    double _timeStep = 0;
    double _inverseTimeStep = 0;
    double _errorSum = 0;
    double _lastError = 0;
    bool _firstRun = true;

    public double Value { get; private set; }

    public PID(double kP, double kI, double kD, double timeStep)
    {
        _kP = kP;
        _kI = kI;
        _kD = kD;
        _timeStep = timeStep;
        _inverseTimeStep = 1 / _timeStep;
    }

    protected virtual double GetIntegral(double currentError, double errorSum, double timeStep)
    {
        return errorSum + currentError * timeStep;
    }

    public double Control(double error)
    {
        //Compute derivative term
        var errorDerivative = (error - _lastError) * _inverseTimeStep;

        if (_firstRun)
        {
            errorDerivative = 0;
            _firstRun = false;
        }

        //Get error sum
        _errorSum = GetIntegral(error, _errorSum, _timeStep);

        //Store this error as last error
        _lastError = error;

        //Construct output
        this.Value = _kP * error + _kI * _errorSum + _kD * errorDerivative;
        return this.Value;
    }

    public double Control(double error, double timeStep)
    {
        if (timeStep != _timeStep)
        {
            _timeStep = timeStep;
            _inverseTimeStep = 1 / _timeStep;
        }
        return Control(error);
    }

    public void Reset()
    {
        _errorSum = 0;
        _lastError = 0;
        _firstRun = true;
    }
}

public class DecayingIntegralPID : PID
{
    readonly double _decayRatio;

    public DecayingIntegralPID(double kP, double kI, double kD, double timeStep, double decayRatio) : base(kP, kI, kD, timeStep)
    {
        _decayRatio = decayRatio;
    }

    protected override double GetIntegral(double currentError, double errorSum, double timeStep)
    {
        return errorSum = errorSum * (1.0 - _decayRatio) + currentError * timeStep;
    }
}

public class ClampedIntegralPID : PID
{
    readonly double _upperBound;
    readonly double _lowerBound;

    public ClampedIntegralPID(double kP, double kI, double kD, double timeStep, double lowerBound, double upperBound) : base(kP, kI, kD, timeStep)
    {
        _upperBound = upperBound;
        _lowerBound = lowerBound;
    }

    protected override double GetIntegral(double currentError, double errorSum, double timeStep)
    {
        errorSum = errorSum + currentError * timeStep;
        return Math.Min(_upperBound, Math.Max(errorSum, _lowerBound));
    }
}

public class BufferedIntegralPID : PID
{
    readonly Queue<double> _integralBuffer = new Queue<double>();
    readonly int _bufferSize = 0;

    public BufferedIntegralPID(double kP, double kI, double kD, double timeStep, int bufferSize) : base(kP, kI, kD, timeStep)
    {
        _bufferSize = bufferSize;
    }

    protected override double GetIntegral(double currentError, double errorSum, double timeStep)
    {
        if (_integralBuffer.Count == _bufferSize)
            _integralBuffer.Dequeue();
        _integralBuffer.Enqueue(currentError);
        return _integralBuffer.Sum() * timeStep;
    }
}

#endregion

#region Helper Classes
public static class StringExtensions
{
    public static bool Contains(string source, string toCheck, StringComparison comp = StringComparison.OrdinalIgnoreCase)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }
}

public static class VectorMath
{
    public static Vector3D SafeNormalize(Vector3D a)
    {
        if (Vector3D.IsZero(a))
            return Vector3D.Zero;

        if (Vector3D.IsUnit(ref a))
            return a;

        return Vector3D.Normalize(a);
    }

    public static Vector3D Reflection(Vector3D a, Vector3D b, double rejectionFactor = 1) //reflect a over b
    {
        Vector3D project_a = Projection(a, b);
        Vector3D reject_a = a - project_a;
        return project_a - reject_a * rejectionFactor;
    }

    public static Vector3D Rejection(Vector3D a, Vector3D b) //reject a on b
    {
        if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
            return Vector3D.Zero;

        return a - a.Dot(b) / b.LengthSquared() * b;
    }

    public static Vector3D Projection(Vector3D a, Vector3D b)
    {
        if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
            return Vector3D.Zero;

        return a.Dot(b) / b.LengthSquared() * b;
    }

    public static double ScalarProjection(Vector3D a, Vector3D b)
    {
        if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
            return 0;

        if (Vector3D.IsUnit(ref b))
            return a.Dot(b);

        return a.Dot(b) / b.Length();
    }

    public static double AngleBetween(Vector3D a, Vector3D b) //returns radians
    {
        if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
            return 0;
        else
            return Math.Acos(MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
    }

    public static double CosBetween(Vector3D a, Vector3D b, bool useSmallestAngle = false) //returns radians
    {
        if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
            return 0;
        else
            return MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1);
    }

    public static bool IsDotProductWithinTolerance(Vector3D a, Vector3D b, double tolerance)
    {
        double dot = Vector3D.Dot(a, b);
        double num = a.LengthSquared() * b.LengthSquared() * tolerance * tolerance;
        return dot * dot > num;
    }
}


/// <summary>
/// Class that tracks runtime history.
/// </summary>
public class RuntimeTracker
{
    public int Capacity { get; set; }
    public double Sensitivity { get; set; }
    public double MaxRuntime { get; private set; }
    public double MaxInstructions { get; private set; }
    public double AverageRuntime { get; private set; }
    public double AverageInstructions { get; private set; }

    private readonly Queue<double> _runtimes = new Queue<double>();
    private readonly Queue<double> _instructions = new Queue<double>();
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly int _instructionLimit;
    private readonly Program _program;

    public RuntimeTracker(Program program, int capacity = 100, double sensitivity = 0.01)
    {
        _program = program;
        Capacity = capacity;
        Sensitivity = sensitivity;
        _instructionLimit = _program.Runtime.MaxInstructionCount;
    }

    public void AddRuntime()
    {
        double runtime = _program.Runtime.LastRunTimeMs;
        AverageRuntime = Sensitivity * (runtime - AverageRuntime) + AverageRuntime;

        _runtimes.Enqueue(runtime);
        if (_runtimes.Count == Capacity)
        {
            _runtimes.Dequeue();
        }

        MaxRuntime = _runtimes.Max();
    }

    public void AddInstructions()
    {
        double instructions = _program.Runtime.CurrentInstructionCount;
        AverageInstructions = Sensitivity * (instructions - AverageInstructions) + AverageInstructions;

        _instructions.Enqueue(instructions);
        if (_instructions.Count == Capacity)
        {
            _instructions.Dequeue();
        }

        MaxInstructions = _instructions.Max();
    }

    public string Write()
    {
        _sb.Clear();
        _sb.AppendLine("\n_____________________________\nGeneral Runtime Info\n");
        _sb.AppendLine($"Avg instructions: {AverageInstructions:n2}");
        _sb.AppendLine($"Max instructions: {MaxInstructions:n0}");
        _sb.AppendLine($"Avg complexity: {MaxInstructions / _instructionLimit:0.000}%");
        _sb.AppendLine($"Avg runtime: {AverageRuntime:n4} ms");
        _sb.AppendLine($"Max runtime: {MaxRuntime:n4} ms");
        return _sb.ToString();
    }
}
#endregion
#endregion
