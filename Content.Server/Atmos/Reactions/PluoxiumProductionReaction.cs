// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.EntityEffects.Effects;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Consumes a tiny amount of tritium to convert CO2 and oxygen to pluoxium.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;
            
        var initO2 = mixture.GetMoles(Gas.Oxygen);
        var initCO2 = mixture.GetMoles(Gas.CarbonDioxide);
        var initTrit = mixture.GetMoles(Gas.Tritium);

        float[] efficiencies = {5f, initCO2, initO2 * 2f, initTrit * 100f};
        Array.Sort(efficiencies);
        var producedAmount = efficiencies[0];

        var co2Removed = producedAmount;
        var oxyRemoved = producedAmount * 0.5f;
        var tritRemoved = producedAmount * 0.01f;

        if (producedAmount <= 0 ||
            co2Removed > initCO2 ||
            oxyRemoved * 0.5 > initO2 ||
            tritRemoved * 0.01 > initTrit)
            return ReactionResult.NoReaction;

        var pluoxProduced = producedAmount;
        var hydroProduced = producedAmount * 0.01f;

        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Oxygen, -oxyRemoved);
        mixture.AdjustMoles(Gas.Tritium, -tritRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxProduced);
        mixture.AdjustMoles(Gas.Hydrogen, hydroProduced);

        var energyReleased = producedAmount * Atmospherics.PluoxiumProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}