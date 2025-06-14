// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 yavuz <58685802+yahay505@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Content.Goobstation.Shared.Supermatter.Systems;
using Content.Shared.Atmos;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Shared.Supermatter.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SupermatterComponent : Component
{
    #region SM Base

    /// <summary>
    ///     The SM will only cycle if activated.
    /// </summary>
    [DataField("activated")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Activated = false;

    [DataField("supermatterSliverPrototype")]
    public string SliverPrototypeId = "SupermatterSliver";

    /// <summary>
    ///     Affects delamination timer. If removed - delamination timer is divided by 2.
    /// </summary>
    [DataField("sliverRemoved")]
    [ViewVariables(VVAccess.ReadWrite)]
    public bool SliverRemoved = false;

    [DataField("whitelist")]
    public EntityWhitelist Whitelist = new();
    public string IdTag = "EmitterBolt";

    public string[] LightningPrototypes =
    {
        "Lightning",
        "ChargedLightning",
        "SuperchargedLightning",
        "HyperchargedLightning"
    };

    [DataField("singularitySpawnPrototype")]
    public string SingularityPrototypeId = "Singularity";

    [DataField("teslaSpawnPrototype")]
    public string TeslaPrototypeId = "TeslaEnergyBall";

    [DataField("supermatterKudzuSpawnPrototype")]
    public string SupermatterKudzuPrototypeId = "SupermatterKudzu";

    [ViewVariables(VVAccess.ReadWrite)]
    public float Power;

    /// <summary>
    /// The amount of damage we have currently
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Damage = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float MatterPower;

    [ViewVariables(VVAccess.ReadWrite)]
    public float MatterPowerConversion = 10f;

    /// <summary>
    /// The portion of the gasmix we're on
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float GasEfficiency = 0.15f;

    /// <summary>
    /// The amount of heat we apply scaled
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float HeatThreshold = 2500f;

    [DataField("radiationOutputFactor")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float RadiationOutputFactor = 0.03f;
    #endregion SM Base

    #region SM Sound
    /// <summary>
    /// Current stream of SM audio.
    /// </summary>
    public EntityUid? AudioStream;

    public SharedSupermatterSystem.SuperMatterSound? SmSound;

    [DataField("dustSound")]
    public SoundSpecifier DustSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/supermatter.ogg");

    [DataField("delamSound")]
    public SoundSpecifier DelamSound = new SoundPathSpecifier("/Audio/_Goobstation/Supermatter/delamming.ogg");

    [DataField("delamAlarm")]
    public SoundSpecifier DelamAlarm = new SoundPathSpecifier("/Audio/Machines/alarm.ogg");

    #endregion SM Sound

    #region SM Calculation

    /// <summary>
    /// Based on co2 percentage, slowly moves between
    /// 0 and 1. We use it to calc the powerloss_inhibitor
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float PowerlossDynamicScaling;

    /// <summary>
    /// Affects the amount of damage and minimum point
    /// at which the sm takes heat damage
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float DynamicHeatResistance = 1;

    /// <summary>
    /// Multiplier on damage the core takes from absorbing hot gas
    /// Default is ~1/350
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float MoleHeatPenalty = 0.00286f;

    /// <summary>
    /// Inverse of MoleHeatPenalty
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float MoleHeatThreshold = 350f;

    /// <summary>
    /// Multiplier on power generated by nuclear reactions
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("reactionpowerModifier")]
    public float ReactionPowerModifier = 0.55f;

    /// <summary>
    /// Acts as a multiplier on the amount that nuclear reactions increase the supermatter core temperature
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("thermalreleaseModifier")]
    public float ThermalReleaseModifier = 0.2f;

    /// <summary>
    /// Multiplier on how much plasma is released during supermatter reactions
    /// Default is ~1/750
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("plasmareleaseModifier")]
    public float PlasmaReleaseModifier = 0.001333f;

    /// <summary>
    /// Multiplier on how much oxygen is released during supermatter reactions.
    /// Default is ~1/325
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("oxygenreleaseModifier")]
    public float OxygenReleaseEfficiencyModifier = 0.0031f;

    #endregion SM Calculation

    #region SM Timer

    /// <summary>
    /// The point at which we should start sending messeges
    /// about the damage to the engi channels.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("WarningPoint")]
    public float WarningPoint = 50;

    /// <summary>
    /// The point at which we start sending messages to the common channel
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("emergencyPoint")]
    public float EmergencyPoint = 500;

    /// <summary>
    /// we yell if over 50 damage every YellTimer Seconds
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float YellTimer = 60f;

    /// <summary>
    /// set to YellTimer at first so it doesnt yell a minute after being hit
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float YellAccumulator = 60f;

    /// <summary>
    ///     Timer for delam
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float DelamTimerAccumulator;

    /// <summary>
    ///     Time until delam
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("delamTimer")]
    public float DelamTimer = 120f;

    /// <summary>
    ///     The message timer
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float SpeakAccumulator = 60f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float UpdateAccumulator = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float UpdateTimer = 1f;

    [ViewVariables(VVAccess.ReadOnly)]
    public float ZapAccumulator = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ZapTimer = 10f;
    #endregion SM Timer

    #region SM Threshold

    /// <summary>
    /// Higher == Higher percentage of inhibitor gas needed
    /// before the charge inertia chain reaction effect starts.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerlossinhibitiongasThreshold")]
    public float PowerlossInhibitionGasThreshold = 0.20f;

    /// <summary>
    /// Higher == More moles of the gas are needed before the charge
    /// inertia chain reaction effect starts.
    /// Scales powerloss inhibition down until this amount of moles is reached
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerlossinhibitionmoleThreshold")]
    public float PowerlossInhibitionMoleThreshold = 20f;

    /// <summary>
    /// bonus powerloss inhibition boost if this amount of moles is reached
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerlossinhibitionmoleboostThreshold")]
    public float PowerlossInhibitionMoleBoostThreshold = 500f;

    /// <summary>
    /// Above this value we can get lord singulo and independent mol damage,
    /// below it we can heal damage
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("molepenaltyThreshold")]
    public float MolePenaltyThreshold = 900f;

    /// <summary>
    /// more moles of gases are harder to heat than fewer,
    /// so let's scale heat damage around them
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("moleheatpenaltyThreshold")]
    public float MoleHeatPenaltyThreshold;

    /// <summary>
    /// The cutoff on power properly doing damage, pulling shit around,
    /// and delamming into a tesla. Low chance of pyro anomalies, +2 bolts of electricity
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("powerPenaltyThreshold")]
    public float PowerPenaltyThreshold = 2500f;

    /// <summary>
    /// Maximum safe operational temperature in degrees Celsius. Supermatter begins taking damage above this temperature.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("heatpenaltyThreshold")]
    public float HeatPenaltyThreshold = 40f;

    /// <summary>
    /// The damage we had before this cycle. Used to limit the damage we can take each cycle, and for safe alert
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float DamageArchived = 0f;

    /// <summary>
    /// is multiplied by ExplosionPoint to cap
    /// evironmental damage per cycle
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float DamageHardcap = 0.002f;

    /// <summary>
    /// environmental damage is scaled by this
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("damageincreaseMultiplier")]
    public float DamageIncreaseMultiplier = 0.25f;

    /// <summary>
    /// if spaced sm wont take more than 2 damage per cycle
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("maxspaceexposureDamage")]
    public float MaxSpaceExposureDamage = 2;

    #endregion SM Threshold

    #region SM Delamm

    public bool DelamAnnounced = false;

    /// <summary>
    /// The point at which we delamm
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("explosionPoint")]
    public int DelaminationPoint = 900;

    //Are we delamming?
    [ViewVariables(VVAccess.ReadOnly)]
    public bool Delamming = false;

    //Explosion totalIntensity value
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("totalIntensity")]
    public float TotalIntensity = 50000f;

    //Explosion radius value
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("radius")]
    public float Radius = 50f;

    /// <summary>
    /// These would be what you would get at point blank, decreases with distance
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("detonationRads")]
    public float DetonationRads = 200f;

    #endregion SM Delamm

    #region SM Gas
    /// <summary>
    /// Is used to store gas
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("gasStorage")]
    public Dictionary<Gas, float> GasStorage = new Dictionary<Gas, float>()
    {
        {Gas.Oxygen, 0f},
        {Gas.Nitrogen, 0f},
        {Gas.CarbonDioxide, 0f},
        {Gas.Plasma, 0f},
        {Gas.Tritium, 0f},
        {Gas.WaterVapor, 0f},
        {Gas.BZ, 0f}, // Assmos - /tg/ gases
        {Gas.Healium, 0f}, // Assmos - /tg/ gases
        {Gas.Pluoxium, 0f} // Assmos - /tg/ gases
    };

    /// <summary>
    ///     Stores each gas facts
    /// </summary>
    public readonly Dictionary<Gas, (float TransmitModifier, float HeatPenalty, float PowerMixRatio)> GasDataFields = new()
    {
        [Gas.Oxygen] = (TransmitModifier: 1.5f, HeatPenalty: 1f, PowerMixRatio: 1f),
        [Gas.Nitrogen] = (TransmitModifier: 0f, HeatPenalty: -1.5f, PowerMixRatio: -1f),
        [Gas.CarbonDioxide] = (TransmitModifier: 0f, HeatPenalty: 0.1f, PowerMixRatio: 1f),
        [Gas.Plasma] = (TransmitModifier: 4f, HeatPenalty: 15f, PowerMixRatio: 1f),
        [Gas.Tritium] = (TransmitModifier: 30f, HeatPenalty: 10f, PowerMixRatio: 1f),
        [Gas.WaterVapor] = (TransmitModifier: 2f, HeatPenalty: 12f, PowerMixRatio: 1f),
        [Gas.Frezon] = (TransmitModifier: 3f, HeatPenalty: -10f, PowerMixRatio: -1f),
        [Gas.Ammonia] = (TransmitModifier: 0f, HeatPenalty: .5f, PowerMixRatio: 1f),
        [Gas.NitrousOxide] = (TransmitModifier: 0f, HeatPenalty: -5f, PowerMixRatio: -1f),

        [Gas.BZ] = (TransmitModifier: 0f, HeatPenalty: 5f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.Healium] = (TransmitModifier: 2.4f, HeatPenalty: 4f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.Pluoxium] = (TransmitModifier: 0f, HeatPenalty: -2.5f, PowerMixRatio: -1f), // Assmos - /tg/ gases
        [Gas.Nitrium] = (TransmitModifier: 30f, HeatPenalty: 10f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.Hydrogen] = (TransmitModifier: 20f, HeatPenalty: 10f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.HyperNoblium] = (TransmitModifier: 30f, HeatPenalty: -9f, PowerMixRatio: -1f), // Assmos - /tg/ gases
        [Gas.ProtoNitrate] = (TransmitModifier: 15f, HeatPenalty: -4f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.Zauker] = (TransmitModifier: 2f, HeatPenalty: 4f, PowerMixRatio: 1f), // Assmos - /tg/ gases
        [Gas.Halon] = (TransmitModifier: 0.1f, HeatPenalty: 0.1f, PowerMixRatio: 0.1f), // Assmos - /tg/ gases // 0.1f heatres
        [Gas.Helium] = (TransmitModifier: 0.1f, HeatPenalty: 0.1f, PowerMixRatio: 0.1f), // Assmos - /tg/ gases // 0.1f heatres
        [Gas.AntiNoblium] = (TransmitModifier: -0.5f, HeatPenalty: 14f, PowerMixRatio: 1f), // Assmos - /tg/ gases
    };

    #endregion SM Gas
}

[Serializable, NetSerializable]
public sealed partial class SupermatterDoAfterEvent : SimpleDoAfterEvent
{

}
