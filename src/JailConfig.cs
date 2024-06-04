using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

	public class JailConfig : BasePluginConfig {
		[JsonPropertyName("username")]
		public String username { get; set; } = "";

		[JsonPropertyName("password")]
		public String password { get; set; } = "";

		[JsonPropertyName("server")]
		public String server { get; set; } = "127.0.0.1";

		[JsonPropertyName("port")]
		public String port { get; set; } = "3306";

		[JsonPropertyName("database")]
		public String database { get; set; } = "cs2_jail";

		[JsonPropertyName("no_block")]
		public bool noBlock { get; set; } = true;

		[JsonPropertyName("mute_dead")]
		public bool muteDead { get; set; } = true;

		[JsonPropertyName("warden_laser")]
		public bool wardenLaser { get; set; } = true;

		[JsonPropertyName("ct_voice_only")]
		public bool ctVoiceOnly { get; set; } = false;

		[JsonPropertyName("thirty_sec_mute")]
		public bool thirtySecMute { get; set; } = true;

		[JsonPropertyName("mute_t_allways")]
		public bool muteTAllways { get; set; } = false;

		[JsonPropertyName("warden_on_voice")]
		public bool wardenOnVoice { get; set; } = true;

		[JsonPropertyName("ct_swap_only")]
		public bool ctSwapOnly { get; set; } = false;

		[JsonPropertyName("ct_guns")]
		public bool ctGuns { get; set; } = true;

		[JsonPropertyName("ct_handicap")]
		public bool ctHandicap { get; set; } = false;

		[JsonPropertyName("ct_gun_menu")]
		public bool ctGunMenu { get; set; } = true;

		[JsonPropertyName("ct_armour")]
		public bool ctArmour { get; set; } = true;

		[JsonPropertyName("warden_force_removal")]
		public bool wardenForceRemoval { get; set; } = true;

		[JsonPropertyName("strip_spawn_weapons")]
		public bool stripSpawnWeapons { get; set; } = true;

		[JsonPropertyName("warday_guns")]
		public bool wardayGuns { get; set; } = false;

		// ratio of t to CT
		[JsonPropertyName("bal_guards")]
		public int balGuards { get; set; } = 0;

		[JsonPropertyName("enable_riot")]
		public bool riotEnable { get; set; } = false;

		[JsonPropertyName("hide_kills")]
		public bool hideKills { get; set; } = false;

		[JsonPropertyName("restrict_ping")]
		public bool restrictPing { get; set; } = true;

		[JsonPropertyName("colour_rebel")]
		public bool colourRebel { get; set; } = false;

		[JsonPropertyName("rebel_cant_lr")]
		public bool rebelCantLr { get; set; } = false;

		[JsonPropertyName("lr_knife")]
		public bool lrKnife { get; set; } = true;

		[JsonPropertyName("lr_gun_toss")]
		public bool lrGunToss { get; set; } = true;

		[JsonPropertyName("lr_dodgeball")]
		public bool lrDodgeball { get; set; } = true;

		[JsonPropertyName("lr_no_scope")]
		public bool lrNoScope { get; set; } = true;

		[JsonPropertyName("lr_war")]
		public bool lrWar { get; set; } = true;

		[JsonPropertyName("lr_grenade")]
		public bool lrGrenade { get; set; } = true;

		[JsonPropertyName("lr_russian_roulette")]
		public bool lrRussianRoulette { get; set; } = true;

		[JsonPropertyName("lr_scout_knife")]
		public bool lrScoutKnife { get; set; } = true;

		[JsonPropertyName("lr_headshot_only")]
		public bool lrHeadshotOnly { get; set; } = true;

		[JsonPropertyName("lr_shot_for_shot")]
		public bool lrShotForShot { get; set; } = true;

		[JsonPropertyName("lr_mag_for_mag")]
		public bool lrMagForMag { get; set; } = true;

		[JsonPropertyName("lr_count")]
		public uint lrCount { get; set; } = 2;

		[JsonPropertyName("rebel_requirehit")]
		public bool rebelRequireHit { get; set; } = false;

		[JsonPropertyName("wsd_round")]
		public int wsdRound { get; set; } = 50;

		[JsonPropertyName("warden_deputy")]
		public bool wardenDeputy { get; set; } = true;

		[JsonPropertyName("warden_marker")]
		public bool wardenMarker { get; set; } = true;
	}
