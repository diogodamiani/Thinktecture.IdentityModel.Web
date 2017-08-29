﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.ServiceModel.Security;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;

namespace Thinktecture.IdentityModel.Web
{
    public static class WSSecurity10Constants
    {
        public const string FragmentBaseAddress = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0";
        public const string Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string Prefix = "wsse";
        public const string Base64EncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
        public const string HexBinaryEncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary";
        public const string KerberosTokenType1510 = "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ1510";
        public const string KerberosTokenTypeGSS = "http://docs.oasis-open.org/wss/oasis-wss-kerberos-token-profile-1.1#GSS_Kerberosv5_AP_REQ";
        public const string TextEncodingType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text";
        public const string X509TokenType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3";
        public const string UPTokenPasswordTextValue = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";

        public static class Attributes
        {
            public const string ValueType = "ValueType";
            public const string EncodingType = "EncodingType";
            public const string URI = "URI";
            public const string Type = "Type";
        }

        public static class Elements
        {
            public const string BinarySecurityToken = "BinarySecurityToken";
            public const string Reference = "Reference";
            public const string KeyIdentifier = "KeyIdentifier";
            public const string SecurityTokenReference = "SecurityTokenReference";
            public const string UsernameToken = "UsernameToken";
            public const string Username = "Username";
            public const string Password = "Password";
            public const string Nonce = "Nonce";
            public const string Created = "Created";
        }

        public static class EncodingTypes
        {
            public const string Base64 = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary";
            public const string HexBinary = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#HexBinary";
            public const string Text = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Text";
        }

        public static class FaultCodes
        {
            public const string FailedAuthentication = "FailedAuthentication";
            public const string FailedCheck = "FailedCheck";
            public const string InvalidSecurity = "InvalidSecurity";
            public const string InvalidSecurityToken = "InvalidSecurityToken";
            public const string MessageExpired = "MessageExpired";
            public const string SecurityTokenUnavailable = "SecurityTokenUnavailable";
            public const string UnsupportedAlgorithm = "UnsupportedAlgorithm";
            public const string UnsupportedSecurityToken = "UnsupportedSecurityToken";
        }
    }

    public static class SimpleWebTokenConstants
    {
        public const string Audience = "Audience";
        public const string ExpiresOn = "ExpiresOn";
        public const string Id = "Id";
        public const string Issuer = "Issuer";
        public const string Signature = "HMACSHA256";
        public const string ValidFrom = "ValidFrom";
        public const string ValueTypeUri = "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0";
    }

    /// <summary>
    /// Handles SWT tokens.
    /// </summary>
    public class SwtSecurityTokenHandler : SecurityTokenHandler, IWebSecurityTokenHandler
    {
        // constants
        const string AudienceLabel = "Audience";
        const string ExpiresOnLabel = "ExpiresOn";
        const string IssuerLabel = "Issuer";
        const string Digest256Label = "HMACSHA256";

        public ClaimsPrincipal ValidateWebToken(string token)
        {
            var securityToken = ReadToken(token);
            return new ClaimsPrincipal(ValidateToken(securityToken));
        }

        public override string[] GetTokenTypeIdentifiers()
        {
            return new[] { "http://schemas.microsoft.com/ws/2010/07/identitymodel/tokens/SWT",
                           "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0" };
        }

        public override Type TokenType
        {
            get { return typeof(SimpleWebToken); }
        }

        public override bool CanReadToken(XmlReader reader)
        {
            return
                reader.IsStartElement(WSSecurity10Constants.Elements.BinarySecurityToken, WSSecurity10Constants.Namespace) &&
                reader.GetAttribute(WSSecurity10Constants.Attributes.ValueType) == "http://schemas.xmlsoap.org/ws/2009/11/swt-token-profile-1.0";
        }

        public override SecurityToken ReadToken(XmlReader reader)
        {
            if (!this.CanReadToken(reader))
            {
                throw new InvalidOperationException("Can't read token.");
            }

            var swtBuffer = Convert.FromBase64String(reader.ReadElementContentAsString());
            var swt = Encoding.Default.GetString(swtBuffer);

            return ReadToken(swt);
        }

        /// <summary>
        /// Reads a serialized token and converts it into a <see cref="SecurityToken"/>.
        /// </summary>
        /// <param name="rawToken">The token in serialized form.</param>
        /// <returns>The parsed form of the token.</returns>
        public SecurityToken ReadToken(string rawToken)
        {
            char parameterSeparator = '&';
            Uri audienceUri = null;
            string issuer = null;
            string signature = null;
            string unsignedString = null;
            string expires = null;

            if (string.IsNullOrEmpty(rawToken))
            {
                throw new ArgumentNullException("rawToken");
            }

            //
            // Find the last parameter. The signature must be last per SWT specification.
            //
            int lastSeparator = rawToken.LastIndexOf(parameterSeparator);

            // Check whether the last parameter is an hmac.
            //
            if (lastSeparator > 0)
            {
                string lastParamStart = parameterSeparator + Digest256Label + "=";
                string lastParam = rawToken.Substring(lastSeparator);

                // Strip the trailing hmac to obtain the original unsigned string for later hmac verification.
                // e.g. name1=value1&name2=value2&HMACSHA256=XXX123 -> name1=value1&name2=value2
                //
                if (lastParam.StartsWith(lastParamStart, StringComparison.Ordinal))
                {
                    unsignedString = rawToken.Substring(0, lastSeparator);
                }
            }
            else
            {
                throw new WebFaultException<string>("The Simple Web Token must have a signature at the end. The incoming token did not have a signature at the end of the token.", HttpStatusCode.BadRequest);
            }

            // Signature is a mandatory parameter, and it must be the last one.
            // If there's no trailing hmac, Return error.
            //
            if (unsignedString == null)
            {
                throw new WebFaultException<string>("The Simple Web Token must have a signature at the end. The incoming token did not have a signature at the end of the token.", HttpStatusCode.BadRequest);
            }

            // Create a collection of SWT claims
            //
            NameValueCollection rawClaims = ParseToken(rawToken);

            audienceUri = new Uri(rawClaims[AudienceLabel]);
            if (audienceUri != null)
            {
                rawClaims.Remove(AudienceLabel);
            }
            else
            {
                throw new WebFaultException<string>("Then incoming token does not have an AudienceUri.", HttpStatusCode.BadRequest);
            }

            expires = rawClaims[ExpiresOnLabel];
            if (expires != null)
            {
                rawClaims.Remove(ExpiresOnLabel);
            }
            else
            {
                throw new WebFaultException<string>("Then incoming token does not have an expiry time.", HttpStatusCode.BadRequest);
            }

            issuer = rawClaims[IssuerLabel];
            if (issuer != null)
            {
                rawClaims.Remove(IssuerLabel);
            }
            else
            {
                throw new WebFaultException<string>("Then incoming token does not have an Issuer", HttpStatusCode.BadRequest);
            }

            signature = rawClaims[Digest256Label];
            if (signature != null)
            {
                rawClaims.Remove(Digest256Label);
            }
            else
            {
                throw new WebFaultException<string>("Then incoming token does not have a signature", HttpStatusCode.BadRequest);
            }

            List<Claim> claims = DecodeClaims(issuer, rawClaims);

            SimpleWebToken swt = new SimpleWebToken(audienceUri, issuer, DecodeExpiry(expires), claims, signature, unsignedString, rawToken);
            return swt;
        }


        public override bool CanValidateToken
        {
            get { return true; }
        }

        public override bool CanWriteToken
        {
            get { return true; }
        }

        public override void WriteToken(XmlWriter writer, SecurityToken token)
        {
            var swt = token as SimpleWebToken;

            if (swt == null)
                throw new SecurityTokenException();

            // Wrap the token into a binary token for XML transport.
            writer.WriteStartElement(WSSecurity10Constants.Elements.BinarySecurityToken, WSSecurity10Constants.Namespace);
            writer.WriteAttributeString(WSSecurity10Constants.Attributes.ValueType, SimpleWebTokenConstants.ValueTypeUri);
            writer.WriteAttributeString(WSSecurity10Constants.Attributes.EncodingType, WSSecurity10Constants.EncodingTypes.Base64);
            writer.WriteValue(Convert.ToBase64String(Encoding.Default.GetBytes(WriteToken(token))));
            writer.WriteEndElement();
        }

        public string WriteToken(SecurityToken token)
        {
            var swt = token as SimpleWebToken;

            if (swt == null)
            {
                throw new InvalidOperationException("token");
            }

            var sb = new StringBuilder();

            CreateClaims(swt, sb);

            sb.AppendFormat("Issuer={0}&", HttpUtility.UrlEncode(swt.Issuer));
            sb.AppendFormat("Audience={0}&", HttpUtility.UrlEncode(swt.AudienceUri.AbsoluteUri));
            sb.AppendFormat("ExpiresOn={0:0}", swt.ValidTo.ToEpochTime());

            var unsignedToken = sb.ToString();

            // retrieve signing key
            var clause = new SwtSecurityKeyClause(swt.Issuer);
            var key = Configuration.IssuerTokenResolver.ResolveSecurityKey(clause) as InMemorySymmetricSecurityKey;

            if (key == null)
            {
                throw new InvalidOperationException("No signing key found");
            }

            var hmac = new HMACSHA256(key.GetSymmetricKey());
            var sig = hmac.ComputeHash(Encoding.ASCII.GetBytes(unsignedToken));

            var signedToken = String.Format("{0}&HMACSHA256={1}",
                unsignedToken,
                HttpUtility.UrlEncode(Convert.ToBase64String(sig)));

            return signedToken;
        }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            SimpleWebToken swt = token as SimpleWebToken;
            if (swt == null)
            {
                throw new WebFaultException<string>("The received token is of incorrect token type.Expected SimpleWebToken", HttpStatusCode.BadRequest);
            }

            // check issuer name registry for allowed issuers
            string issuerName = null;
            if (base.Configuration.IssuerNameRegistry != null)
            {
                issuerName = base.Configuration.IssuerNameRegistry.GetIssuerName(token);
                if (string.IsNullOrEmpty(issuerName))
                {
                    throw new SecurityTokenException("Invalid issuer");
                }
            }

            // retrieve signing key
            var clause = new SwtSecurityKeyClause(swt.Issuer);
            var securityKey = Configuration.IssuerTokenResolver.ResolveSecurityKey(clause) as InMemorySymmetricSecurityKey;

            if (securityKey == null)
            {
                throw new InvalidOperationException("No signing key found");
            }

            // check signature
            if (!swt.SignVerify(securityKey.GetSymmetricKey()))
            {
                throw new SecurityTokenValidationException("Signature verification of the incoming token failed.");
            }

            // check expiration
            if (DateTime.Compare(swt.ValidTo, DateTime.UtcNow) <= 0)
            {
                throw new SecurityTokenExpiredException("The incoming token has expired. Get a new access token from the Authorization Server.");
            }

            // check audience
            if (base.Configuration.AudienceRestriction.AudienceMode != System.IdentityModel.Selectors.AudienceUriMode.Never)
            {
                var allowedAudiences = base.Configuration.AudienceRestriction.AllowedAudienceUris;

                if (!allowedAudiences.Any(uri => uri == swt.AudienceUri))
                {
                    throw new AudienceUriValidationFailedException();
                }
            }

            var id = new ClaimsIdentity(swt.Claims);

            //foreach (var claim in swt.Claims)
            //{
            //	claim.Value.Split(',').ToList().ForEach(v => id.Claims.Add(new Claim(claim.ClaimType, v, ClaimValueTypes.String, issuerName)));
            //}

            //return new ClaimsIdentityCollection(new IClaimsIdentity[] { id });

            return new ReadOnlyCollection<ClaimsIdentity>(new[] { id });
        }



        private static void CreateClaims(SimpleWebToken swt, StringBuilder sb)
        {
            var claims = new Dictionary<string, string>();

            foreach (var claim in swt.Claims)
            {
                claims.Add(claim.Type, claim.Value);
            }

            foreach (var kv in claims)
            {
                sb.AppendFormat("{0}={1}&", HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value));
            }
        }

        public override SecurityKeyIdentifierClause CreateSecurityTokenReference(SecurityToken token, bool attached)
        {
            var swt = token as SimpleWebToken;
            if (swt == null)
                throw new SecurityTokenException("Expected SWT token.");

            return new KeyNameIdentifierClause(swt.Issuer);
        }


        /// <summary>
        /// Parses the token into a collection.
        /// </summary>
        /// <param name="encodedToken">The serialized token.</param>
        /// <returns>A colleciton of all name-value pairs from the token.</returns>
        NameValueCollection ParseToken(string encodedToken)
        {
            NameValueCollection claimCollection = new NameValueCollection();
            foreach (string nameValue in encodedToken.Split('&'))
            {
                string[] keyValueArray = nameValue.Split('=');

                if ((keyValueArray.Length != 2)
                   && !String.IsNullOrEmpty(keyValueArray[0]))
                {
                    // the signature may have multiple '=' in the end
                    throw new WebFaultException<string>("The received token is not correctly formed", HttpStatusCode.BadRequest);
                }

                if (String.IsNullOrEmpty(keyValueArray[1]))
                {
                    // ignore parameter with empty values
                    continue;
                }

                string key = HttpUtility.UrlDecode(keyValueArray[0].Trim());               // Names must be decoded for the claim type case
                string value = HttpUtility.UrlDecode(keyValueArray[1].Trim().Trim('"')); // remove any unwanted "
                claimCollection.Add(key, value);
            }

            return claimCollection;
        }

        /// <summary>Create <see cref="Claim"/> from the incoming token.
        /// </summary>
        /// <param name="issuer">The issuer of the token.</param>
        /// <param name="rawClaims">The name value pairs from the token.</param>        
        /// <returns>A list of Claims created from the token.</returns>
        protected List<Claim> DecodeClaims(string issuer, NameValueCollection rawClaims)
        {
            if (rawClaims == null)
            {
                throw new ArgumentNullException("rawClaims");
            }

            List<Claim> decodedClaims = new List<Claim>();

            foreach (string key in rawClaims.Keys)
            {
                if (string.IsNullOrEmpty(rawClaims[key]))
                {
                    throw new WebFaultException<string>("Claim value cannot be empty", HttpStatusCode.BadRequest);
                }

                decodedClaims.Add(new Claim(key, rawClaims[key], ClaimValueTypes.String, issuer));
            }

            return decodedClaims;
        }

        /// <summary>
        /// Convert the expiryTime to the <see cref="DateTime"/> format.
        /// </summary>
        /// <param name="expiry">The expiry time from the token.</param>
        /// <returns>The local expiry time of the token.</returns>
        protected DateTime DecodeExpiry(string expiry)
        {
            long totalSeconds = 0;
            if (!long.TryParse(expiry, out totalSeconds))
            {
                throw new WebFaultException<string>("The incoming token has an unexpected expiration time format", HttpStatusCode.BadRequest);
            }

            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(totalSeconds);
        }
    }
}