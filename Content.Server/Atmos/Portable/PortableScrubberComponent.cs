// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Guidebook;

namespace Content.Server.Atmos.Portable
{
    [RegisterComponent]
    public sealed partial class PortableScrubberComponent : Component
    {
        /// <summary>
        /// The air inside this machine.
        /// </summary>
        [DataField("gasMixture"), ViewVariables(VVAccess.ReadWrite)]
        public GasMixture Air { get; private set; } = new();

        [DataField("port"), ViewVariables(VVAccess.ReadWrite)]
        public string PortName { get; set; } = "port";

        /// <summary>
        /// Which gases this machine will scrub out.
        /// Unlike fixed scrubbers controlled by an air alarm,
        /// this can't be changed in game.
        /// </summary>
        [DataField("filterGases")]
        public HashSet<Gas> FilterGases = new()
        {
            Gas.CarbonDioxide,
            Gas.Plasma,
            Gas.Tritium,
            Gas.WaterVapor,
            Gas.Ammonia,
            Gas.NitrousOxide,
            Gas.Frezon,
            Gas.BZ, // Assmos - /tg/ gases
            Gas.Healium, // Assmos - /tg/ gases
            Gas.Nitrium, // Assmos - /tg/ gases
            Gas.Hydrogen, // Assmos - /tg/ gases
            Gas.HyperNoblium, // Assmos - /tg/ gases
            Gas.ProtoNitrate, // Assmos - /tg/ gases
            Gas.Zauker, // Assmos - /tg/ gases
            Gas.Halon, // Assmos - /tg/ gases
            Gas.Helium, // Assmos - /tg/ gases
            Gas.AntiNoblium, // Assmos - /tg/ gases
        };

        [ViewVariables(VVAccess.ReadWrite)]
        public bool Enabled = true;

        /// <summary>
        /// Maximum internal pressure before it refuses to take more.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MaxPressure = 2500;

        /// <summary>
        /// The speed at which gas is scrubbed from the environment.
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float TransferRate = 800;

        #region GuidebookData

        [GuidebookData]
        public float Volume => Air.Volume;

        #endregion
    }
}