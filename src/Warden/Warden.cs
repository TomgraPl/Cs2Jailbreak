

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
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Core.Translations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using CSTimer = CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;

public partial class Warden
{
    public Warden()
    {
        for(int p = 0; p < jailPlayers.Length; p++)
        {
            jailPlayers[p] = new JailPlayer();
        }
    }

    // Give a player warden
    public void SetWarden(int slot) {
        if (deputySlot == slot) {
            deputySlot = INVALID_SLOT;
        }

        wardenSlot = slot;

        var player = Utilities.GetPlayerFromSlot(wardenSlot);

        // one last saftey check
        if (!player.IsLegal()) {
            wardenSlot = INVALID_SLOT;
            return;
        }

        Chat.LocalizeAnnounce(WARDEN_PREFIX, "warden.took_warden", player.PlayerName);

        player.LocalizeAnnounce(WARDEN_PREFIX, "warden.wcommand");

        wardenTimestamp = Lib.CurTimestamp();

        // set model
        player.SetModel(WardenModelPath);

        // change player color!
        /*Server.RunOnTick(Server.TickCount + 64, () => {
            if (IsWarden(player)) player.SetColour(Color.FromArgb(255, 0, 0, 255));
        });*/

        // add health
        JailPlugin._api?.AddHealth("JB - Role", player, 15, true, false);

        JailPlugin.logs.AddLocalized("warden.took_warden", player.PlayerName);
		Lib.PlaySoundAll("sounds/jaileng/warden.vsnd");
	}
    public void SetDeputy(int slot) {
		deputySlot = slot;

		var player = Utilities.GetPlayerFromSlot(deputySlot);

		// one last saftey check
		if (!player.IsLegal()) {
			deputySlot = INVALID_SLOT;
            Server.PrintToChatAll("dupa");
			return;
		}

		Chat.LocalizeAnnounce(WARDEN_PREFIX, "warden.took_deputy", player.PlayerName);

		// add health
		JailPlugin._api?.AddHealth("JB - Role", player, 5, true, false);

		player.LocalizeAnnounce(WARDEN_PREFIX, "warden.dcommand");

	}

	public bool IsWarden(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return false;
        }

        return player.Slot == wardenSlot;
    }

	public bool IsDeputy(CCSPlayerController? player) {
		if (!player.IsLegal()) {
			return false;
		}

		return player.Slot == deputySlot;
	}

	void RemoveWardenInternal()
    {
        Lib.PlaySoundAll("sounds/jaileng/unwarden.vsnd");
        var player = Utilities.GetPlayerFromSlot(wardenSlot);
        if (player.IsLegal()) JailPlugin._api?.AddHealth("JB - Role", player, 0, true, false);
		wardenSlot = INVALID_SLOT;
        wardenTimestamp = -1;
    }

    public void RemoveWarden()
    {
        var player = Utilities.GetPlayerFromSlot(wardenSlot);

        if(player.IsLegal())
        {
            if (player.Team == CsTeam.CounterTerrorist) player.SetModel(CTModelPath);
            player.SetColour(Player.DEFAULT_COLOUR);
            Chat.LocalizeAnnounce(WARDEN_PREFIX,"warden.removed",player.PlayerName);
            JailPlugin.logs.AddLocalized("warden.removed", player.PlayerName);
        }

        RemoveWardenInternal();
    }

    public void RemoveIfWarden(CCSPlayerController? player)
    {
        if(IsWarden(player))
        {
            RemoveWarden();
        }
    }


    public bool PlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        String text = info.GetArg(1);

        if(text.StartsWith("/") || text.StartsWith("!") || String.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if(player.IsLegalAlive() && IsWarden(player))
        {
            Server.PrintToChatAll($"{WARDEN_PREFIX} {player.PlayerName}: {text}");
            return false;
        }   

        return true;
    }

    // reset variables for a new round
    void PurgeRound()
    {
        RemoveLaser();

        if(Config.wardenForceRemoval)
        {
            RemoveWardenInternal();
            var player = Utilities.GetPlayerFromSlot(deputySlot);
            if (player.IsLegal()) JailPlugin._api?.AddHealth("JB - Role", player, 0, true, false);
            deputySlot = INVALID_SLOT;
		}

        // reset player structs
        foreach(JailPlayer jailPlayer in jailPlayers)
        {
            jailPlayer.PurgeRound();
        }
    }

    void SetWardenIfLast(bool onDeath = false)
    {
        return;/*
		// dont override the warden if there is no death removal
		// also don't do it if an event is running because it's annoying
		if (!Config.wardenForceRemoval || JailPlugin.EventActive()) {
            return;
        }
        // if there is only one ct automatically give them warden!
        var ctPlayers = Lib.GetAliveCt();

        if(ctPlayers.Count == 1)
        {
            if(onDeath)
            {
                // play sfx for last ct
                // TODO: this is too loud as there is no way to control volume..
                //Lib.PlaySound_all("sounds/vo/agents/sas/lastmanstanding03");
            }
        
            int slot = ctPlayers[0].Slot;
            SetWarden(slot);
        }*/
    }

    public void SetupPlayerGuns(CCSPlayerController? player)
    {
        // dont intefere with spawn guns if an event is running
        if(!player.IsLegalAlive() || JailPlugin.EventActive())
        {
            return;
        }

        // strip weapons just in case
        if(Config.stripSpawnWeapons)
        {
            player.StripWeapons();
        }

        if(player.IsCt())
        {
            if(Config.ctGuns)
            {
                var jailPlayer = JailPlayerFromPlayer(player);

                player.GiveWeapon("deagle");

                if(jailPlayer != null)
                {
                    player.GiveMenuWeapon(jailPlayer.ctGun);
                }
            }

            if(Config.ctArmour)
            {  
                player.GiveArmour();
            }
        } 
    }

    // util func to get a jail player
    public JailPlayer? JailPlayerFromPlayer(CCSPlayerController? player)
    {
        if(!player.IsLegal())
        {
            return null;
        }

        return jailPlayers[player.Slot];
    }
    
    public CCSPlayerController? GetWarden()
    {
        if(wardenSlot == INVALID_SLOT)
        {
            return null;
        }

        return Utilities.GetPlayerFromSlot(wardenSlot);
    }

    Countdown<int> chatCountdown = new Countdown<int>();
	CSTimer.Timer? tmpMuteTimer = null;
	long tmpMuteTimestamp = 0;
	CSTimer.Timer? openCellTimer = null;

	const int INVALID_SLOT = -3;

    public int deputySlot { get; private set; } = INVALID_SLOT;
    int wardenSlot { get; set; } = INVALID_SLOT;

    public static String WARDEN_PREFIX { get; set; } = JailPlugin.Localize("warden.warden_prefix");

    long wardenTimestamp = -1;

    public JailConfig Config = new JailConfig();

    public JailPlayer[] jailPlayers = new JailPlayer[64];

    // slot for player for warden colour
    int colourSlot = -1;

    bool ctHandicap = false;

    public Warday warday = new Warday();
    public Block block = new Block();
    public Mute mute = new Mute();
	public static string WardenModelPath { get; set; } = "characters/models/nozb1/jail_police_player_model/jail_police_player_model.vmdl";
	public static string CTModelPath { get; set; } = "characters/models/nozb1/policeman_player_model/policeman_player_model.vmdl";
};