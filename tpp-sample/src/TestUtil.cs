﻿using System;
using System.Collections.Generic;
using Tokenio.Proto.Common.AliasProtos;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Proto.Common.TokenProtos;
using Tokenio.Security;
using Tokenio.Tpp.Security;
using Tokenio.User;
using Tokenio.Utils;
using UserMember = Tokenio.User.Member;

namespace Tokenio.Sample.Tpp
{
    public abstract class TestUtil
    {
        private static string DEV_KEY = "f3982819-5d8d-4123-9601-886df2780f42";
        private static string TOKEN_REALM = "token";

        /// <summary>
        /// Generates random user name to be used for testing.
        /// </summary>
        /// <returns>The alias.</returns>
        public static Alias RandomAlias()
        {
            return new Alias
            {
                Value = "alias-" + Util.Nonce().ToLower() + "+noverify@example.com",
                Type = Alias.Types.Type.Domain,
                Realm = TOKEN_REALM
            };
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <returns>The client.</returns>
        public static Tokenio.Tpp.TokenClient CreateClient()
        {
            return Tokenio.Tpp.TokenClient.Create(Tokenio.TokenCluster.DEVELOPMENT, DEV_KEY);
        }

        /// <summary>
        /// Creates the client with Builder.
        /// </summary>
        /// <returns>The client.</returns>
        public static Tokenio.Tpp.TokenClient CreateClient(TokenCryptoEngineFactory cryptoEngineFactory)
        {
            return Tokenio.Tpp.TokenClient.NewBuilder()
                .ConnectTo(Tokenio.TokenCluster.DEVELOPMENT)
                .DeveloperKey(DEV_KEY)
                .WithCryptoEngine(cryptoEngineFactory)
                .Build();
        }

        /// <summary>
        /// Creates the client with Builder.
        /// </summary>
        /// <returns>The client.</returns>
        public static Tokenio.Tpp.TokenClient CreateClient(EidasCryptoEngineFactory cryptoEngineFactory)
        {
            return Tokenio.Tpp.TokenClient.NewBuilder()
                .ConnectTo(Tokenio.TokenCluster.DEVELOPMENT)
                .DeveloperKey(DEV_KEY)
                .WithCryptoEngine(cryptoEngineFactory)
                .Build();
        }

        /// <summary>
        /// Creates the user member.
        /// </summary>
        /// <returns>The user member.</returns>
        public static UserMember CreateUserMember()
        {
            Tokenio.User.TokenClient
                client = Tokenio.User.TokenClient.Create(Tokenio.TokenCluster.DEVELOPMENT, DEV_KEY);
            Alias alias = RandomAlias();
            UserMember member = client.CreateMemberBlocking(alias);
            member.CreateTestBankAccountBlocking(1000.0, "EUR");
            return member;
        }

        /// <summary>
        /// Creates the access token.
        /// </summary>
        /// <returns>The access token.</returns>
        /// <param name="grantor">Grantor.</param>
        /// <param name="accountId">Account identifier.</param>
        /// <param name="granteeAlias">Grantee alias.</param>
        public static Token CreateAccessToken(
            UserMember grantor,
            string accountId,
            Alias granteeAlias)
        {
            // Create an access token for the grantee to access bank
            // account names of the grantor.
            Token accessToken = grantor.CreateAccessTokenBlocking(
                AccessTokenBuilder
                    .Create(granteeAlias)
                    .ForAccount(accountId)
                    .ForAccountBalances(accountId));

            // Grantor endorses a token to a grantee by signing it
            // with her secure private key.
            accessToken = grantor.EndorseTokenBlocking(
                accessToken,
                Key.Types.Level.Standard).Token;

            return accessToken;
        }

        /// <summary>
        /// Creates the transfer token.
        /// </summary>
        /// <returns>The transfer token.</returns>
        /// <param name="payer">Payer.</param>
        /// <param name="payeeAlias">Payee alias.</param>
        public static Token CreateTransferToken(
            UserMember payer,
            Alias payeeAlias)
        {
            // We'll use this as a reference ID. Normally, a payer who
            // explicitly sets a reference ID would use an ID from a db.
            // E.g., a bill-paying service might use ID of a "purchase".
            // We don't have a db, so we fake it with a random string:
            string purchaseId = Tokenio.User.Utils.Util.Nonce();

            // Create a transfer token.
            var tokenPayload = payer.CreateTransferTokenBuilder(
                    100.0, // amount
                    "EUR") // currency // source account:
                .SetAccountId(payer.GetAccountsBlocking()[0].Id())
                // payee token alias:
                .SetToAlias(payeeAlias)
                // optional description:
                .SetDescription("Book purchase")
                // ref id (if not set, will get random ID)
                .SetRefId(purchaseId)
                .BuildPayload();
            var transferToken = payer.CreateTokenBlocking(tokenPayload, new List<Signature>());

            // Payer endorses a token to a payee by signing it
            // with her secure private key.
            transferToken = payer.EndorseTokenBlocking(transferToken, Key.Types.Level.Standard).Token;

            return transferToken;
        }


        public static string RandomNumeric(int size)
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, size);
        }
    }
}