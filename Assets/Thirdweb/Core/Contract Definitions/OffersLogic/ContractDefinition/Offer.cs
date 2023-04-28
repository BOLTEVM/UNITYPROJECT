using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Thirdweb.Contracts.OffersLogic.ContractDefinition
{
    public partial class Offer : OfferBase { }

    public class OfferBase
    {
        [Parameter("uint256", "offerId", 1)]
        public virtual BigInteger OfferId { get; set; }

        [Parameter("address", "offeror", 2)]
        public virtual string Offeror { get; set; }

        [Parameter("address", "assetContract", 3)]
        public virtual string AssetContract { get; set; }

        [Parameter("uint256", "tokenId", 4)]
        public virtual BigInteger TokenId { get; set; }

        [Parameter("uint256", "quantity", 5)]
        public virtual BigInteger Quantity { get; set; }

        [Parameter("address", "currency", 6)]
        public virtual string Currency { get; set; }

        [Parameter("uint256", "totalPrice", 7)]
        public virtual BigInteger TotalPrice { get; set; }

        [Parameter("uint256", "expirationTimestamp", 8)]
        public virtual BigInteger ExpirationTimestamp { get; set; }

        [Parameter("uint8", "tokenType", 9)]
        public virtual byte TokenType { get; set; }

        [Parameter("uint8", "status", 10)]
        public virtual byte Status { get; set; }
    }
}
