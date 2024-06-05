﻿
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core.Capabilities;
 
// main plugin file, controls central hooking
// defers to warden, lr and sd
[MinimumApiVersion(239)]
public class JailPlugin : BasePlugin, IPluginConfig<JailConfig>
{
    // Global event settings, used to filter plugin activits
    // during warday and SD
    bool isEventActive = false;

    public JailConfig Config  { get; set; } = new JailConfig();
    public static IInterOperabilityAPI? _api;
	public static PluginCapability<IInterOperabilityAPI> IOCapability { get; } = new("InterOperability:Api");
	public static PluginCapability<IWardenService> wardenService {get; } = new ("jailbreak:warden_service");

    public static bool IsWarden(CCSPlayerController? player)
    {
        return warden.IsWarden(player);
    }

    public static bool EventActive()
    {
        return globalCtx.isEventActive;
    }

    public static void StartEvent()
    {
        globalCtx.isEventActive = true;
    }

    public static void EndEvent()
    {
        globalCtx.isEventActive = false;
    }

    public static void WinLR(CCSPlayerController? player,LastRequest.LRType type)
    {
        WardenService?.LRWin(player, type);
        jailStats.Win(player,type);
    }

    public static void LoseLR(CCSPlayerController? player, LastRequest.LRType type)
    {
		WardenService?.LRLost(player, type);
		jailStats.Loss(player,type);
    }

    public static void PurgePlayerStats(CCSPlayerController? player)
    {
        jailStats.PurgePlayer(player);
    }

    public override string ModuleName => "CS2 Jailbreak - destoer";

    public override string ModuleVersion => "v0.4.5c t9";
    public static WardenApi? WardenService { get; set; } = null;
    public override void Load(bool hotReload)
    {
        globalCtx = this;
        logs = new Logs(this); 

        WardenService = new WardenApi(this);
        Capabilities.RegisterPluginCapability(wardenService,() => WardenService);

        RegisterCommands();
        
        RegisterHooks();

        RegisterListeners();

        LocalizePrefix();

        JailPlayer.SetupDB();

        Console.WriteLine("Sucessfully started JB");

        AddTimer(Warden.LASER_TIME,warden.LaserTick,CSTimer.TimerFlags.REPEAT);
    }
	public override void OnAllPluginsLoaded(bool hotReload) {
        _api = IOCapability.Get();
        if (_api == null) return;
	}

	void LocalizePrefix()
    {
        LastRequest.LR_PREFIX = Chat.Localize(/*"lr.lr_prefix"*/"Jail.Prefix");
        Entity.DOOR_PREFIX = Chat.Localize(/*"warden.door_prefix"*/"Jail.Prefix");

        SpecialDay.SPECIALDAY_PREFIX = Chat.Localize(/*"sd.sd_prefix"*/"Jail.Prefix");
        JailPlayer.REBEL_PREFIX = Chat.Localize(/*"rebel.rebel_prefix"*/ "Jail.Prefix");

        Mute.MUTE_PREFIX = Chat.Localize(/*"mute.mute_prefix"*/"Jail.Prefix");
        Warden.TEAM_PREFIX = Chat.Localize(/*"warden.team_prefix"*/ "Jail.Prefix");
        
        Warday.WARDAY_PREFIX = Chat.Localize(/*"warday.warday_prefix"*/ "Jail.Prefix");
        Warden.WARDEN_PREFIX = Chat.Localize(/*"warden.warden_prefix"*/ "Jail.Prefix");    
    }

    void StatDBReload()
    {
        Task.Run(async () => 
        {
            var database = await jailStats.ConnectDB();

            jailStats.SetupDB(database);
        });
    }

    public void OnConfigParsed(JailConfig config)
    {
        // give each sub plugin the config
        this.Config = config;
        
        jailStats.Config = config;
        lr.Config = config;

        warden.Config = config;
        warden.mute.Config = config;
        warden.warday.Config = config;
        warden.block.Config = config;
        JailPlayer.Config = config;

        sd.Config = config;

        lr.LRConfigReload();
        StatDBReload();
    }

    void RegisterListeners()
    {
        RegisterListener<Listeners.OnEntitySpawned>(entity =>
        {
            lr.EntCreated(entity);
            sd.EntCreated(entity);
        });
    }

    void AddLocalizedCmd(String base_name,String desc,CommandInfo.CommandCallback callback)
    {
        AddCommand("css_" + Localizer[base_name],desc,callback);
    }

    public static void KillCmd(CCSPlayerController? invoke, CommandInfo command)
    {  
        if(invoke.IsLegalAlive())
        {
            Chat.LocalizeAnnounce("","jail.kill_cmd",invoke.PlayerName);
            invoke.Slay();
        }
    }

    void RegisterCommands()
    {
        AddCommand("kill","kill self",KillCmd);

        // reg warden comamnds
        AddLocalizedCmd("warden.take_warden_cmd", "take warden", warden.TakeWardenCmd);
        AddLocalizedCmd("warden.leave_warden_cmd", "leave warden", warden.LeaveWardenCmd);
        AddLocalizedCmd("warden.remove_warden_cmd", "remove warden", warden.RemoveWardenCmd);
        AddLocalizedCmd("warden.remove_marker_cmd","remove warden marker",warden.RemoveMarkerCmd);

        AddLocalizedCmd("warden.marker_colour_cmd", "set marker colour", warden.MarkerColourCmd);
        AddLocalizedCmd("warden.laser_colour_cmd", "set laser colour", warden.LaserColourCmd);

        AddLocalizedCmd("warden.colour_cmd","set player colour",warden.ColourCmd);

        AddLocalizedCmd("warden.no_block_cmd","warden : disable block",warden.WubCmd);
        AddLocalizedCmd("warden.block_cmd","warden : enable block",warden.WbCmd);

        AddLocalizedCmd("warden.sd_cmd","warden : call a special day",sd.WardenSDCmd);
        AddLocalizedCmd("warden.sd_ff_cmd","warden : call a friendly fire special day",sd.WardenSDFFCmd);

        AddLocalizedCmd("warden.swap_guard","admin : move a player to ct",warden.SwapGuardCmd);

        AddLocalizedCmd("warden.warday_cmd","warden : start warday",warden.WardayCmd);
        AddLocalizedCmd("warden.list_cmd", "warden : show all commands",warden.CmdInfo);
        AddLocalizedCmd("warden.time_cmd","how long as warden been active?",warden.WardenTimeCmd);

        AddLocalizedCmd("warden.gun_cmd","give ct guns",warden.CmdCtGuns);

        AddLocalizedCmd("warden.force_open_cmd","force open every door and vent",warden.ForceOpenCmd);
        AddLocalizedCmd("warden.force_close_cmd","force close every door",warden.ForceCloseCmd);

        AddLocalizedCmd("warden.fire_guard_cmd","admin : Remove all guards apart from warden",warden.FireGuardCmd);

        AddLocalizedCmd("warden.give_freeday_cmd","give t a freeday",warden.GiveFreedayCmd);
        AddLocalizedCmd("warden.give_pardon_cmd","give t a pardon",warden.GivePardonCmd);

        AddLocalizedCmd("warden.countdown_cmd","start a countdown",warden.CountdownCmd);
        AddLocalizedCmd("warden.countdown_abort_cmd","abort a countdown",warden.CountdownAbortCmd);

		AddLocalizedCmd("warden.mute_cmd", "do a warden mute", warden.WardenMuteCmd);

		AddLocalizedCmd("warden.deputy_cmd", "take deputy", warden.DeputyCmd);
		AddLocalizedCmd("warden.force_warden_cmd", "set warden", warden.ForceWardenCmd);

        // reg lr commands
        AddLocalizedCmd("lr.start_lr_cmd","start an lr",lr.LRCmd);
        AddLocalizedCmd("lr.cancel_lr_cmd","admin : cancel lr",lr.CancelLRCmd);
        AddLocalizedCmd("lr.stats_cmd","list lr stats",jailStats.LRStatsCmd);

        // reg sd commands
        AddLocalizedCmd("sd.start_cmd","start a sd",sd.SDCmd);
        AddLocalizedCmd("sd.start_ff_cmd","start a ff sd",sd.SDFFCmd);
        AddLocalizedCmd("sd.cancel_cmd","cancel an sd",sd.CancelSDCmd);

        AddLocalizedCmd("logs.logs_cmd", "show round logs", logs.LogsCommand);

        // debug 
        if(Debug.enable)
        {
            AddCommand("nuke","debug : kill every player",Debug.Nuke);
            AddCommand("is_rebel","debug : print rebel state to console",warden.IsRebelCmd);
            AddCommand("lr_debug","debug : start an lr without restriction",lr.LRDebugCmd);
            AddCommand("is_blocked","debug : print block state",warden.block.IsBlocked);
            AddCommand("test_laser","test laser",Debug.TestLaser);
            AddCommand("test_player","testt player",Debug.TestPlayer);
            AddCommand("test_strip","test weapon strip",Debug.TestStripCmd);
            AddCommand("join_ct_debug","debug : force join ct",Debug.JoinCtCmd);
            AddCommand("hide_weapon_debug","debug : hide player weapon on back",Debug.HideWeaponCmd);
            AddCommand("rig","debug : force player to boss on sd",sd.SDRigCmd);
            AddCommand("is_muted","debug : print voice flags",Debug.IsMutedCmd);
            AddCommand("spam_db","debug : spam db",Debug.TestLRInc);
            AddCommand("wsd_enable","debug : enable wsd",Debug.WSDEnableCmd);
            AddCommand("test_noblock","debug : enable wsd",Debug.TestNoblockCmd);
        }
    }

    public HookResult JoinTeam(CCSPlayerController? invoke, CommandInfo command)
    {
        jailStats.LoadPlayer(invoke);

        JailPlayer? jailPlayer = warden.JailPlayerFromPlayer(invoke);

        if(jailPlayer != null)
        {
            jailPlayer.LoadPlayer(invoke);
        }        

        if(!warden.JoinTeam(invoke,command))
        {
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    
    void RegisterHooks()
    {
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd,HookMode.Pre);
        RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventTeamchangePending>(OnSwitchTeam);
        RegisterEventHandler<EventMapTransition>(OnMapChange);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath,HookMode.Pre);
        RegisterEventHandler<EventItemEquip>(OnItemEquip);
        RegisterEventHandler<EventGrenadeThrown>(OnGrenadeThrown);
        RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
        RegisterEventHandler<EventWeaponZoom>(OnWeaponZoom);
        RegisterEventHandler<EventPlayerPing>(OnPlayerPing);

        // take damage causes crashes on windows
        // cant figure out why because the windows cs2 console wont log
        // before it dies
        if(!Lib.IsWindows())
        {
            VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage,HookMode.Pre);
        }
        
        HookEntityOutput("func_button", "OnPressed", OnButtonPressed);
        
        RegisterListener<Listeners.OnClientVoice>(OnClientVoice);
        RegisterListener<Listeners.OnClientAuthorized>(OnClientAuthorized);

        AddCommandListener("jointeam",JoinTeam);
        AddCommandListener("player_ping",PlayerPingCmd);

        AddCommandListener("say", OnPlayerChat);

        // TODO: need to hook weapon drop
    }


    public HookResult OnPlayerChat(CCSPlayerController? invoke, CommandInfo command)
    {
        // dont print chat, warden is handling it
        if(!warden.PlayerChat(invoke,command))
        {
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    public HookResult PlayerPingCmd(CCSPlayerController? invoke, CommandInfo command)
    {
        // if player is not warden ignore the ping
        if(Config.restrictPing && !warden.IsWarden(invoke))
        {
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerPing(EventPlayerPing  @event, GameEventInfo info)
    {
        var player = @event.Userid;

        if(player.IsLegal())
        {
            warden.Ping(player,@event.X,@event.Y,@event.Z);
        }

        return HookResult.Continue;
    }

    void OnClientVoice(int slot)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

        if(player.IsLegal())
        {
            warden.Voice(player);
        }
    }

    // button log
    HookResult OnButtonPressed(CEntityIOOutput output, String name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
    {
        CCSPlayerController? player = new CBaseEntity(activator.Handle).Player();

        // grab player controller from pawn
        CBaseEntity? ent =  Utilities.GetEntityFromIndex<CBaseEntity>((int)caller.Index);

        if(player.IsLegal() && ent != null && ent.IsValid)
        {
            logs.AddLocalized(player, "logs.format.button", ent.Entity?.Name ?? "Unlabeled", output?.Connections?.TargetDesc ?? "None");
        }

        return HookResult.Continue;
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage,HookMode.Pre);
    }

    HookResult OnGrenadeThrown(EventGrenadeThrown @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player.IsLegal())
        {
            lr.GrenadeThrown(player);
            sd.GrenadeThrown(player);
            logs.AddLocalized(player, "logs.format.grenade", @event.Weapon); 
        }

        return HookResult.Continue;
    }
  
    HookResult OnWeaponZoom(EventWeaponZoom @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player.IsLegal())
        {
            lr.WeaponZoom(player);
        }

        return HookResult.Continue;
    }

    HookResult OnItemEquip(EventItemEquip @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player.IsLegal())
        {
            lr.WeaponEquip(player,@event.Item);
            sd.WeaponEquip(player,@event.Item);
        }

        return HookResult.Continue;
    }

    HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        CCSPlayerController? attacker = @event.Attacker;

        int damage = @event.DmgHealth;
        int health = @event.Health;
        int hitgroup = @event.Hitgroup;

        if(player.IsLegal())
        {
            lr.PlayerHurt(player,attacker,damage,health,hitgroup);
            warden.PlayerHurt(player,attacker,damage,health);
            sd.PlayerHurt(player,attacker,damage,health,hitgroup);
        }

        return HookResult.Continue;
    }

    HookResult OnTakeDamage(DynamicHook handle)
    {
        CEntityInstance victim = handle.GetParam<CEntityInstance>(0);
        CTakeDamageInfo damage_info = handle.GetParam<CTakeDamageInfo>(1);

        CHandle<CBaseEntity> dealer = damage_info.Attacker;

        // get player and attacker
        CCSPlayerController? player = new CBaseEntity(victim.Handle).Player();
        CCSPlayerController? attacker = dealer.Player();

        if(player.IsLegal())
        {
            warden.TakeDamage(player,attacker,ref damage_info.Damage);
            sd.TakeDamage(player,attacker,ref damage_info.Damage);
            lr.TakeDamage(player,attacker,ref damage_info.Damage);
        }
        
        return HookResult.Continue;
    }

    HookResult OnMapChange(EventMapTransition @event, GameEventInfo info)
    {
        warden.MapStart();

        return HookResult.Continue;
    }

    HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        warden.RoundStart();
        lr.RoundStart();
        sd.RoundStart();

        return HookResult.Continue;
    }

    HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? victim = @event.Userid;
        CCSPlayerController? killer = @event.Attacker;

        // NOTE: have to check IsConnected incase this is tripped by a dc
    
        // hide t killing ct
        if(Config.hideKills && victim.IsConnected() && killer.IsConnected() && killer.IsT() && victim.IsCt())
        {
            killer.Announce(Warden.WARDEN_PREFIX,$"You killed: {victim.PlayerName}");
            info.DontBroadcast = true;
        }
    
        if(victim.IsLegal() && victim.IsConnected())
        {
            warden.Death(victim,killer);
            lr.Death(victim);
            sd.Death(victim,killer,@event.Weapon);
        }
        return HookResult.Continue;
    }

    HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player.IsLegal())
        {
            int slot = player.Slot;

            AddTimer(0.5f,() =>  
            {
                warden.Spawn(Utilities.GetPlayerFromSlot(slot));
            });
            
        }

        return HookResult.Continue;
    }

    HookResult OnSwitchTeam(EventTeamchangePending @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        int new_team = @event.Toteam;

        if(player.IsLegal())
        {
            // close menu on team switch to prevent illegal usage
            //MenuManager.CloseActiveMenu(player);
            warden.SwitchTeam(player,new_team);
        }

        return HookResult.Continue;
    }

    public void OnClientAuthorized(int slot, SteamID steamid)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

        if(player.IsLegal())
        {
            // load in player stats
            jailStats.LoadPlayer(player);
            
            JailPlayer? jailPlayer = warden.JailPlayerFromPlayer(player);

            if(jailPlayer != null)
            {
                jailPlayer.LoadPlayer(player);
            }
        }
    }

    HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if(player.IsLegal())
        {
            warden.Disconnect(player);
            lr.Disconnect(player);
            sd.Disconnect(player);
        }

        return HookResult.Continue;
    }

    HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        warden.RoundEnd();
        lr.RoundEnd();
        sd.RoundEnd();

        return HookResult.Continue;
    }

    HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        // attempt to get player and weapon
        var player = @event.Userid;
        String name = @event.Weapon;

        if(player.IsLegalAlive())
        {
            warden.WeaponFire(player,name);
            lr.WeaponFire(player,name);
        }

        return HookResult.Continue;
    }

    public static String Localize(string name,params Object[] args)
    {
        return String.Format(globalCtx.Localizer[name],args);
    }

    public static Warden warden = new Warden();
    public static LastRequest lr = new LastRequest();
    public static SpecialDay sd = new SpecialDay();
    public static JailStats jailStats = new JailStats();

    // in practice these wont be null
    #pragma warning disable CS8618 
    public static Logs logs;

    // workaround to query global state!
    public static JailPlugin globalCtx;

    #pragma warning restore CS8618
}
