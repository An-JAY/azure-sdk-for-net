﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using System.Linq;
using System.Collections.Generic;

namespace Azure.Identity
{
    /// <summary>
    /// Enables authentication to Azure Active Directory using client secret, or username and password,
    /// details configured in the following environment variables:
    /// <list type="table">
    /// <listheader><term>Variable</term><description>Description</description></listheader>
    /// <item><term>AZURE_TENANT_ID</term><description>The Azure Active Directory tenant(directory) ID.</description></item>
    /// <item><term>AZURE_CLIENT_ID</term><description>The client(application) ID of an App Registration in the tenant.</description></item>
    /// <item><term>AZURE_CLIENT_SECRET</term><description>A client secret that was generated for the App Registration.</description></item>
    /// <item><term>AZURE_CLIENT_CERTIFICATE_PATH</term><description>A path to certificate and private key pair in PEM or PFX format, which can authenticate the App Registration.</description></item>
    /// <item><term>AZURE_CLIENT_SEND_CERTIFICATE_CHAIN</term><description>Specifies whether an authentication request will include an x5c header to support subject name / issuer based authentication. When set to `true` or `1`, authentication requests include the x5c header.</description></item>
    /// <item><term>AZURE_USERNAME</term><description>The username, also known as upn, of an Azure Active Directory user account.</description></item>
    /// <item><term>AZURE_PASSWORD</term><description>The password of the Azure Active Directory user account. Note this does not support accounts with MFA enabled.</description></item>
    /// </list>
    /// This credential ultimately uses a <see cref="ClientSecretCredential"/>, <see cref="ClientCertificateCredential"/>, or <see cref="UsernamePasswordCredential"/> to
    /// perform the authentication using these details. Please consult the
    /// documentation of that class for more details.
    /// </summary>
    public class EnvironmentCredential : TokenCredential
    {
        private const string UnavailableErrorMessage = "EnvironmentCredential authentication unavailable. Environment variables are not fully configured. See the troubleshooting guide for more information. https://aka.ms/azsdk/net/identity/environmentcredential/troubleshoot";
        private readonly CredentialPipeline _pipeline;
        private readonly TokenCredentialOptions _options;

        internal TokenCredential Credential { get; }

        /// <summary>
        /// Creates an instance of the EnvironmentCredential class and reads client secret details from environment variables.
        /// If the expected environment variables are not found at this time, the GetToken method will return the default <see cref="AccessToken"/> when invoked.
        /// </summary>
        public EnvironmentCredential()
            : this(CredentialPipeline.GetInstance(null))
        { }

        /// <summary>
        /// Creates an instance of the EnvironmentCredential class and reads client secret details from environment variables.
        /// If the expected environment variables are not found at this time, the GetToken method will return the default <see cref="AccessToken"/> when invoked.
        /// </summary>
        /// <param name="options">Options that allow to configure the management of the requests sent to the Azure Active Directory service.</param>
        public EnvironmentCredential(TokenCredentialOptions options)
            : this(CredentialPipeline.GetInstance(options), options)
        { }

        internal EnvironmentCredential(CredentialPipeline pipeline, TokenCredentialOptions options = null)
        {
            _pipeline = pipeline;
            _options = options ?? new TokenCredentialOptions();

            string tenantId = EnvironmentVariables.TenantId;
            string clientId = EnvironmentVariables.ClientId;
            string clientSecret = EnvironmentVariables.ClientSecret;
            string clientCertificatePath = EnvironmentVariables.ClientCertificatePath;
            string clientSendCertificateChain = EnvironmentVariables.ClientSendCertificateChain;
            string username = EnvironmentVariables.Username;
            string password = EnvironmentVariables.Password;

            // Since the AdditionallyAllowedTenantsCore is internal it cannot be set by the application.
            // Currently this is only set by the DefaultAzureCredential where it will default to the value
            // of EnvironmentVariables.AdditionallyAllowedTenants, but can also be altered by the application.
            // In either case we don't want to alter it.
            if (_options.AdditionallyAllowedTenantsCore.Count == 0)
            {
                _options.AdditionallyAllowedTenantsCore = EnvironmentVariables.AdditionallyAllowedTenants;
            }

            if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(clientId))
            {
                if (!string.IsNullOrEmpty(clientSecret))
                {
                    Credential = new ClientSecretCredential(tenantId, clientId, clientSecret, _options, _pipeline, null);
                }
                else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    Credential = new UsernamePasswordCredential(username, password, tenantId, clientId, _options, _pipeline, null);
                }
                else if (!string.IsNullOrEmpty(clientCertificatePath))
                {
                    bool sendCertificateChain = !string.IsNullOrEmpty(clientSendCertificateChain) &&
                        (clientSendCertificateChain == "1" || clientSendCertificateChain == "true");

                    ClientCertificateCredentialOptions clientCertificateCredentialOptions = new ClientCertificateCredentialOptions
                    {
                        AuthorityHost = _options.AuthorityHost,
                        IsLoggingPIIEnabled = _options.IsLoggingPIIEnabled,
                        Transport = _options.Transport,
                        AdditionallyAllowedTenantsCore = new List<string>(_options.AdditionallyAllowedTenantsCore),
                        SendCertificateChain = sendCertificateChain
                    };
                    Credential = new ClientCertificateCredential(tenantId, clientId, clientCertificatePath, clientCertificateCredentialOptions, _pipeline, null);
                }
            }
        }

        internal EnvironmentCredential(CredentialPipeline pipeline, TokenCredential credential)
        {
            _pipeline = pipeline;
            Credential = credential;
        }

        /// <summary>
        /// Obtains a token from the Azure Active Directory service, using the specified client details specified in the environment variables
        /// AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET or AZURE_USERNAME and AZURE_PASSWORD to authenticate.
        /// Acquired tokens are cached by the credential instance. Token lifetime and refreshing is handled automatically. Where possible,
        /// reuse credential instances to optimize cache effectiveness.
        /// </summary>
        /// <remarks>
        /// If the environment variables AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET are not specified, the default <see cref="AccessToken"/>
        /// </remarks>
        /// <param name="requestContext">The details of the authentication request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls.</returns>
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            return GetTokenImplAsync(false, requestContext, cancellationToken).EnsureCompleted();
        }

        /// <summary>
        /// Obtains a token from the Azure Active Directory service, using the specified client details specified in the environment variables
        /// AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET or AZURE_USERNAME and AZURE_PASSWORD to authenticate.
        /// Acquired tokens are cached by the credential instance. Token lifetime and refreshing is handled automatically. Where possible,
        /// reuse credential instances to optimize cache effectiveness.
        /// </summary>
        /// <remarks>
        /// If the environment variables AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET are not specified, the default <see cref="AccessToken"/>
        /// </remarks>
        /// <param name="requestContext">The details of the authentication request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> controlling the request lifetime.</param>
        /// <returns>An <see cref="AccessToken"/> which can be used to authenticate service client calls, or a default <see cref="AccessToken"/>.</returns>
        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken = default)
        {
            return await GetTokenImplAsync(true, requestContext, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask<AccessToken> GetTokenImplAsync(bool async, TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            using CredentialDiagnosticScope scope = _pipeline.StartGetTokenScope("EnvironmentCredential.GetToken", requestContext);

            if (Credential is null)
            {
                throw scope.FailWrapAndThrow(new CredentialUnavailableException(UnavailableErrorMessage));
            }

            try
            {
                AccessToken token = async
                    ? await Credential.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false)
                    : Credential.GetToken(requestContext, cancellationToken);

                return scope.Succeeded(token);
            }
            catch (Exception e)
            {
                 throw scope.FailWrapAndThrow(e);
            }
        }
    }
}
