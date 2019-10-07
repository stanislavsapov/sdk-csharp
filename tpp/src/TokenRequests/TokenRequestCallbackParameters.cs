using System.Linq;
using System.Web;
using Google.Protobuf;
using Tokenio.Exceptions;
using Tokenio.Proto.Common.SecurityProtos;
using Tokenio.Tpp.Utils;

namespace Tokenio.Tpp.TokenRequests {
    public class TokenRequestCallbackParameters {
        private static readonly string TOKEN_ID_FIELD = "tokenId";
        private static readonly string STATE_FIELD = "state";
        private static readonly string SIGNATURE_FIELD = "signature";

        /// <summary>
        /// Parses the token request callback URL's parameters. Extracts the state, the token ID, and
        /// the signature over(state | token ID).
        /// </summary>
        /// <returns>The TokenRequestCallbackParameters instance.</returns>
        /// <param name="url">URL.</param>
        public static TokenRequestCallbackParameters Create (string url) {
            var parameters = HttpUtility.ParseQueryString (Util.GetQueryString (url));

            if (!parameters.AllKeys.Contains (TOKEN_ID_FIELD) ||
                !parameters.AllKeys.Contains (STATE_FIELD) ||
                !parameters.AllKeys.Contains (SIGNATURE_FIELD)) {
                throw new InvalidTokenRequestQuery ();
            }

            return new TokenRequestCallbackParameters {
                TokenId = parameters.Get (TOKEN_ID_FIELD),
                    SerializedState = parameters.Get (STATE_FIELD),
                    Signature = JsonParser.Default.Parse<Signature> (parameters.Get (SIGNATURE_FIELD))
            };
        }

        public string TokenId { get; private set; }

        public string SerializedState { get; private set; }

        public Signature Signature { get; private set; }
    }
}