// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;
using Azure.Core;

namespace Azure.ResourceManager.Automation.Models
{
    /// <summary> The response model for the list job stream operation. </summary>
    internal partial class JobStreamListResult
    {
        /// <summary> Initializes a new instance of JobStreamListResult. </summary>
        internal JobStreamListResult()
        {
            Value = new ChangeTrackingList<JobStream>();
        }

        /// <summary> Initializes a new instance of JobStreamListResult. </summary>
        /// <param name="value"> A list of job streams. </param>
        /// <param name="nextLink"> Gets or sets the next link. </param>
        internal JobStreamListResult(IReadOnlyList<JobStream> value, string nextLink)
        {
            Value = value;
            NextLink = nextLink;
        }

        /// <summary> A list of job streams. </summary>
        public IReadOnlyList<JobStream> Value { get; }
        /// <summary> Gets or sets the next link. </summary>
        public string NextLink { get; }
    }
}
