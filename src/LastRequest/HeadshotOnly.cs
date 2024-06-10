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
using System.Numerics;

public class LRHeadshotOnly : LRBase
{
    public LRHeadshotOnly(LastRequest manager,LastRequest.LRType type,int LRSlot, int playerSlot, String choice) : base(manager,type,LRSlot,playerSlot,choice)
    {

    }

    public override void InitPlayer(CCSPlayerController player)
    {    
        weaponRestrict = "deagle";

        player.GiveWeapon("deagle");
    }

    public override void PlayerHurt(int health,int damage, int hitgroup) {
		// dont allow damage when its not to head
		if (hitgroup != Lib.HITGROUP_HEAD && hitgroup != (int)HitGroup_t.HITGROUP_GENERIC)
        {
            CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);
            player.RestoreHP(damage,health);
        }
    }
}