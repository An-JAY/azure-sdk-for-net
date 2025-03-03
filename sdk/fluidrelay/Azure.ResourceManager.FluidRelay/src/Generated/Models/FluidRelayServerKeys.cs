// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

namespace Azure.ResourceManager.FluidRelay.Models
{
    /// <summary> The set of available keys for this server. </summary>
    public partial class FluidRelayServerKeys
    {
        /// <summary> Initializes a new instance of FluidRelayServerKeys. </summary>
        internal FluidRelayServerKeys()
        {
        }

        /// <summary> Initializes a new instance of FluidRelayServerKeys. </summary>
        /// <param name="primaryKey"> The primary key for this server. </param>
        /// <param name="secondaryKey"> The secondary key for this server. </param>
        internal FluidRelayServerKeys(string primaryKey, string secondaryKey)
        {
            PrimaryKey = primaryKey;
            SecondaryKey = secondaryKey;
        }

        /// <summary> The primary key for this server. </summary>
        public string PrimaryKey { get; }
        /// <summary> The secondary key for this server. </summary>
        public string SecondaryKey { get; }
    }
}
