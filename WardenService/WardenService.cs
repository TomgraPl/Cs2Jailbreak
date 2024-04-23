
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities;

public interface IWardenService
{
	/// <summary>
	/// Event triggered when LR was won
	/// </summary>
	public event Action<CCSPlayerController?, int>? OnLRWin;
	/// <summary>
	/// Event triggered when LR was lost
	/// </summary>
	public event Action<CCSPlayerController?, int>? OnLRLost;
	public bool IsWarden(CCSPlayerController? player);
    public CCSPlayerController? GetWarden();
    public void SetWarden(CCSPlayerController player);
    public bool IsDeputy(CCSPlayerController? player);
	public CCSPlayerController? GetDeputy();
}