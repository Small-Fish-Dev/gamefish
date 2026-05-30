namespace GameFish;

/// <summary>
/// A collection of string constants for damage types.
/// </summary>
public static partial class Damage
{
	/// <summary>
	/// 💖 A bit counter-intuitive, but hey.
	/// </summary>
	public const string HEAL = "heal";
	public const string HEALING = HEAL;

	/// <summary>
	/// ⚫ Fell out of bounds.
	/// </summary>
	public const string ABYSS = "abyss";

	/// <summary>
	/// 🧱 Squeezed between a rock and a hard place.
	/// </summary>
	public const string CRUSH = "crush";
	/// <inheritdoc cref="CRUSH" />
	public const string CRUSHING = CRUSH;

	/// <summary>
	/// 👇 Hitting the ground hard.
	/// </summary>
	public const string FALL = "fall";
	/// <inheritdoc cref="FALL" />
	public const string FALLING = FALL;

	/// <summary>
	/// 👢 Cratered some fool with your phat ass.
	/// </summary>
	public const string GOOMBA = "goomba";

	/// <summary>
	/// ✨ Teleporting into someone/somewhere lethally.
	/// </summary>
	public const string TELEFRAG = "tele";
	/// <inheritdoc cref="TELEFRAG" />
	public const string TELEPORT = TELEFRAG;

	/// <summary>
	/// 💢 Hit with some fast moving object.
	/// </summary>
	public const string IMPACT = "impact";
	/// <inheritdoc cref="IMPACT" />
	public const string IMPACTED = IMPACT;

	/// <summary>
	/// 🚀 Hit by something from/for a launcher.
	/// </summary>
	public const string PROJECTILE = "projectile";
	/// <inheritdoc cref="PROJECTILE" />
	public const string PROJECTILES = PROJECTILE;

	/// <summary>
	/// 🚅 Shot by something from/for a gun.
	/// </summary>
	public const string BULLET = "bullet";
	/// <inheritdoc cref="BULLET" />
	public const string BULLETS = "bullet";

	/// <summary>
	/// 💥 A sudden boom.
	/// </summary>
	public const string EXPLODE = "explosive";
	/// <inheritdoc cref="EXPLODE" />
	public const string EXPLODING = EXPLODE;
	/// <inheritdoc cref="EXPLODE" />
	public const string EXPLOSIVE = EXPLODE;

	/// <summary>
	/// ✨🧙‍♂️ The delightfully impossible.
	/// </summary>
	public const string MAGIC = "magic";
	/// <inheritdoc cref="MAGIC" />
	public const string MAGICAL = MAGIC;

	/// <summary>
	/// 🥋 General melee attack tag.
	/// </summary>
	public const string MELEE = "melee";

	/// <summary>
	/// 👊 Hitting with your fist(s).
	/// </summary>
	public const string PUNCH = "punch";
	/// <inheritdoc cref="PUNCH" />
	public const string PUNCHING = PUNCH;

	/// <summary>
	/// 🦵 A leg strike.
	/// </summary>
	public const string KICK = "kick";
	/// <inheritdoc cref="KICK" />
	public const string KICKING = KICK;

	/// <summary>
	/// 🦶 Hit with those digits.
	/// </summary>
	public const string FOOT = "foot";
	/// <inheritdoc cref="FOOT" />
	public const string FEET = FOOT;

	/// <summary>
	/// ⚔ Cut with the broad side of a blade.
	/// </summary>
	public const string SLASH = "slash";
	/// <inheritdoc cref="SLASH" />
	public const string SLASHING = SLASH;

	/// <summary>
	/// 🔪 Hit with the pointy end.
	/// </summary>
	public const string STAB = "stab";
	/// <inheritdoc cref="STAB" />
	public const string STABBING = STAB;

	/// <summary>
	/// 🤺 A penetrative force.
	/// </summary>
	public const string PIERCE = "pierce";
	/// <inheritdoc cref="PIERCE" />
	public const string PIERCED = PIERCE;
	/// <inheritdoc cref="PIERCE" />
	public const string PIERCING = PIERCE;

	/// <summary>
	/// 🍗 Hit with the broad, possibly heavy part of something.
	/// </summary>
	public const string BLUNT = "blunt";
	/// <inheritdoc cref="BLUNT" />
	public const string BLUNTED = BLUNT;

	/// <summary>
	/// 🌡 Being in too high a temperature.
	/// </summary>
	public const string HOT = "burn";
	/// <inheritdoc cref="HOT" />
	public const string HEAT = HOT;
	/// <inheritdoc cref="HOT" />
	public const string BURN = HOT;
	/// <inheritdoc cref="HOT" />
	public const string BURNING = HOT;

	/// <summary>
	/// 🌨 Being in too low a temperature.
	/// </summary>
	public const string COLD = "cold";
	/// <inheritdoc cref="COLD" />
	public const string FREEZE = COLD;
	/// <inheritdoc cref="COLD" />
	public const string FREEZING = COLD;

	/// <summary>
	/// 🔥 The element of burning stuff.
	/// </summary>
	public const string FIRE = "fire";
	/// <inheritdoc cref="FIRE" />
	public const string FLAME = FIRE;
	/// <inheritdoc cref="FIRE" />
	public const string FLAMING = FIRE;

	/// <summary>
	/// 🧊 The element of stuff being frozen.
	/// </summary>
	public const string ICE = "ice";
	/// <inheritdoc cref="ICE" />
	public const string ICY = "ice";

	/// <summary>
	/// ⚡ An electrick shock.
	/// </summary>
	public const string ZAP = "zap";
	/// <inheritdoc cref="ZAP" />
	public const string SHOCK = ZAP;
	/// <inheritdoc cref="ZAP" />
	public const string SHOCKING = ZAP;
	/// <inheritdoc cref="ZAP" />
	public const string ELECTRIC = ZAP;
	/// <inheritdoc cref="ZAP" />
	public const string ELECTRICITY = ZAP;

	/// <summary>
	/// ❗ A particularly intense beam of light.
	/// </summary>
	public const string LASER = "laser";
	/// <inheritdoc cref="LASER" />
	public const string BEAM = LASER;

	/// <summary>
	/// 💡 The good side.. or bleary eyes when you first wake up.
	/// </summary>
	public const string LIGHT = "dark";

	/// <summary>
	/// 🌑 The element of generally being a jerk.
	/// </summary>
	public const string DARK = "dark";
	/// <inheritdoc cref="DARK" />
	public const string DARKNESS = DARK;

	/// <summary>
	/// 💀 The element of being unalive.
	/// </summary>
	public const string DEATH = "death";

	/// <summary>
	/// 🟢 Like bleach or toxic water.
	/// </summary>
	public const string POISON = "poison";
	/// <inheritdoc cref="POISON" />
	public const string POISONED = POISON;
	/// <inheritdoc cref="POISON" />
	public const string POISONOUS = POISON;

	/// <summary>
	/// ☢ Radioactive elements.
	/// </summary>
	public const string RADS = "rads";
	/// <inheritdoc cref="RADS" />
	public const string RADIATION = RADS;

	/// <summary>
	/// 🔊 A really loud/disruptive sound.
	/// </summary>
	public const string SONIC = "sonic";
	/// <inheritdoc cref="SONIC" />
	public const string SOUND = SONIC;

	/// <summary>
	/// 🕳 Lack of pressure.
	/// </summary>
	public const string VOID = "void";
	/// <inheritdoc cref="VOID" />
	public const string SPACE = VOID;

	/// <summary>
	/// 🍽 Not eating for too long.
	/// </summary>
	public const string HUNGER = "hunger";
	/// <inheritdoc cref="HUNGER" />
	public const string STARVE = HUNGER;
	/// <inheritdoc cref="HUNGER" />
	public const string STARVING = HUNGER;
	/// <inheritdoc cref="HUNGER" />
	public const string STARVATION = HUNGER;

	/// <summary>
	/// 🥤 Not drinking for too long.
	/// </summary>
	public const string THIRST = "thirst";

	/// <summary>
	/// 🏊 Not being amphibious.
	/// </summary>
	public const string DROWN = "drown";
	/// <inheritdoc cref="DROWN" />
	public const string DROWNING = DROWN;

	/// <summary>
	/// 💯 A repeated hit of some kind.
	/// </summary>
	public const string COMBO = "combo";
}
