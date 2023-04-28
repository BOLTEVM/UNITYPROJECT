using System.Collections.Generic;

namespace Thirdweb
{
    [System.Serializable]
    public class QueryAllParams
    {
        public int start;
        public int count;

        public override string ToString()
        {
            return $"QueryAllParams:" + $"\n>start: {start}" + $"\n>count: {count}";
        }
    }

    // NFTs

    [System.Serializable]
    public struct NFTMetadata
    {
        public string id;
        public string uri;
        public string description;
        public string image;
        public string name;
        public string external_url;
        public object attributes;

        public override string ToString()
        {
            return $"NFTMetadata:"
                + $"\n>id: {id}"
                + $"\n>uri: {uri}"
                + $"\n>description: {description}"
                + $"\n>image: {image}"
                + $"\n>name: {name}"
                + $"\n>external_url: {external_url}"
                + $"\n>attributes: {attributes?.ToString()}";
        }
    }

    [System.Serializable]
    public struct NFTMetadataWithSupply
    {
        public NFTMetadata metadata;
        public int supply;

        public override string ToString()
        {
            return $"NFTMetadataWithSupply:" + $"\n>>>>>\n{metadata.ToString()}\n<<<<<" + $"\n>supply: {supply}";
        }
    }

    [System.Serializable]
    public struct NFT
    {
        public NFTMetadata metadata;
        public string owner;
        public string type;
        public int supply;
        public int quantityOwned; // only for ERC1155.GetOwned()

        public override string ToString()
        {
            return $"NFT:" + $"\n>>>>>\n{metadata.ToString()}\n<<<<<" + $"\n>owner: {owner}" + $"\n>type: {type}" + $"\n>supply: {supply}" + $"\n>quantityOwned: {quantityOwned}";
        }
    }

    // Tokens

    [System.Serializable]
    public struct Currency
    {
        public string name;
        public string symbol;
        public string decimals;

        public Currency(string name, string symbol, string decimals)
        {
            this.name = name;
            this.symbol = symbol;
            this.decimals = decimals;
        }

        public override string ToString()
        {
            return $"Currency:" + $"\n>name: {name}" + $"\n>symbol: {symbol}" + $"\n>decimals: {decimals}";
        }
    }

    [System.Serializable]
    public struct CurrencyValue
    {
        public string name;
        public string symbol;
        public string decimals;
        public string value;
        public string displayValue;

        public CurrencyValue(string name, string symbol, string decimals, string value, string displayValue)
        {
            this.name = name;
            this.symbol = symbol;
            this.decimals = decimals;
            this.value = value;
            this.displayValue = displayValue;
        }

        public override string ToString()
        {
            return $"CurrencyValue:" + $"\n>name: {name}" + $"\n>symbol: {symbol}" + $"\n>decimals: {decimals}" + $"\n>value: {value}" + $"\n>displayValue: {displayValue}";
        }
    }

    // Marketplace

#nullable enable

    [System.Serializable]
    public enum MarkteplaceStatus
    {
        CREATED,
        COMPLETED,
        CANCELLED
    }

    [System.Serializable]
    public class MarketplaceFilters
    {
        public int? count; // Number of listings to fetch
        public string? offeror; // Has offers from this address
        public string? seller; // Being sold by this address
        public int? start; // Start from this index (pagination)
        public string? tokenContract; // Only show NFTs from this collection
        public string? tokenId; // Only show NFTs with this ID
    }

    [System.Serializable]
    public struct DirectListing
    {
        public string? id; // The id of the listing.
        public string? creatorAddress; // The address of the creator of listing.
        public string? assetContractAddress; // The address of the asset being listed.
        public string? tokenId; // The ID of the token to list.
        public string? quantity; //  The quantity of tokens to include in the listing (always 1 for ERC721).
        public string? currencyContractAddress; // The address of the currency to accept for the listing.
        public CurrencyValue? currencyValuePerToken; // The `CurrencyValue` of the listing. Useful for displaying the price information.
        public string? pricePerToken; // The price to pay per unit of NFTs listed.
        public NFTMetadata? asset; // The asset being listed.
        public long? startTimeInSeconds; // The start time of the listing.
        public long? endTimeInSeconds; // The end time of the listing.
        public bool? isReservedListing; // Whether the listing is reserved to be bought from a specific set of buyers.
        public MarkteplaceStatus? status; // Whether the listing is CREATED, COMPLETED, or CANCELLED.

        public override string ToString()
        {
            return "DirectListing:\n"
                + $"id: {id?.ToString()}\n"
                + $"creatorAddress: {creatorAddress?.ToString()}\n"
                + $"assetContractAddress: {assetContractAddress?.ToString()}\n"
                + $"tokenId: {tokenId?.ToString()}\n"
                + $"quantity: {quantity?.ToString()}\n"
                + $"currencyContractAddress: {currencyContractAddress?.ToString()}\n"
                + $"currencyValuePerToken: {currencyValuePerToken?.ToString()}\n"
                + $"pricePerToken: {pricePerToken?.ToString()}\n"
                + $"asset: {asset?.ToString()}\n"
                + $"startTimeInSeconds: {startTimeInSeconds?.ToString()}\n"
                + $"endTimeInSeconds: {endTimeInSeconds?.ToString()}\n"
                + $"isReservedListing: {isReservedListing?.ToString()}\n"
                + $"status: {status?.ToString()}\n";
        }
    }

    [System.Serializable]
    public struct CreateListingInput
    {
        public string assetContractAddress; // Required - smart contract address of NFT to sell
        public string tokenId; // Required - token ID of the NFT to sell
        public string pricePerToken; // Required - price of each token in the listing
        public string? currencyContractAddress; // Optional - smart contract address of the currency to use for the listing
        public bool? isReservedListing; // Optional - whether or not the listing is reserved (only specific wallet addresses can buy)
        public string? quantity; // Optional - number of tokens to sell (1 for ERC721 NFTs)
        public long? startTimestamp; // Optional - when the listing should start (default is now)
        public long? endTimestamp; // Optional - when the listing should end (default is 7 days from now)
    }

    [System.Serializable]
    public struct Auction
    {
        public string? id; // The id of the auction listing
        public string? creatorAddress; // The wallet address of the creator of auction.
        public string? assetContractAddress; // The address of the asset being auctioned.
        public string? tokenId; // The ID of the token being auctioned.
        public string? quantity; // The quantity of tokens included in the auction.
        public string? currencyContractAddress; // The address of the currency to accept for the auction.
        public string? minimumBidAmount; // The minimum price that a bid must be in order to be accepted.
        public CurrencyValue? minimumBidCurrencyValue; // The `CurrencyValue` of the minimum bid amount. Useful for displaying the price information.
        public string? buyoutBidAmount; // The buyout price of the auction.
        public CurrencyValue? buyoutCurrencyValue; //  The `CurrencyValue` of the buyout price. Useful for displaying the price information.
        public int? timeBufferInSeconds; // This is a buffer e.g. x seconds.
        public int? bidBufferBps; // To be considered as a new winning bid, a bid must be at least x% greater than the previous bid.
        public long? startTimeInSeconds; // The start time of the auction.
        public long? endTimeInSeconds; // The end time of the auction.
        public NFTMetadata? asset; // The asset being auctioned.
        public MarkteplaceStatus? status; // Whether the listing is CREATED, COMPLETED, or CANCELLED.

        public override string ToString()
        {
            return "Auction:\n"
                + $"id: {id?.ToString()}\n"
                + $"creatorAddress: {creatorAddress?.ToString()}\n"
                + $"assetContractAddress: {assetContractAddress?.ToString()}\n"
                + $"tokenId: {tokenId?.ToString()}\n"
                + $"quantity: {quantity?.ToString()}\n"
                + $"currencyContractAddress: {currencyContractAddress?.ToString()}\n"
                + $"minimumBidAmount: {minimumBidAmount?.ToString()}\n"
                + $"minimumBidCurrencyValue: {minimumBidCurrencyValue?.ToString()}\n"
                + $"buyoutBidAmount: {buyoutBidAmount?.ToString()}\n"
                + $"buyoutCurrencyValue: {buyoutCurrencyValue?.ToString()}\n"
                + $"timeBufferInSeconds: {timeBufferInSeconds?.ToString()}\n"
                + $"bidBufferBps: {bidBufferBps?.ToString()}\n"
                + $"startTimeInSeconds: {startTimeInSeconds?.ToString()}\n"
                + $"endTimeInSeconds: {endTimeInSeconds?.ToString()}\n"
                + $"asset: {asset?.ToString()}\n"
                + $"status: {status?.ToString()}\n";
        }
    }

    [System.Serializable]
    public struct CreateAuctionInput
    {
        public string assetContractAddress; // Required - smart contract address of NFT to sell
        public string tokenId; // Required - token ID of the NFT to sell
        public string buyoutBidAmount; // Required - amount to buy the NFT and close the listing
        public string minimumBidAmount; // Required - Minimum amount that bids must be to placed
        public string? currencyContractAddress; // Optional - smart contract address of the currency to use for the listing
        public string? quantity; // Optional - number of tokens to sell (1 for ERC721 NFTs)
        public long? startTimestamp; // Optional - when the listing should start (default is now)
        public long? endTimestamp; // Optional - when the listing should end (default is 7 days from now)
        public string? bidBufferBps; // Optional - percentage the next bid must be higher than the current highest bid (default is contract-level bid buffer bps)
        public string? timeBufferInSeconds; // Optional - time in seconds that are added to the end time when a bid is placed (default is contract-level time buffer in seconds)
    }

    [System.Serializable]
    public struct Offer
    {
        public string? id; // The id of the offer.
        public string? offerorAddress; // The wallet address of the creator of offer.
        public string? assetContractAddress; // The address of the asset being offered on.
        public string? tokenId; // The ID of the token.
        public string? quantity; // The quantity of tokens offered to buy
        public string? currencyContractAddress; // The address of the currency offered for the NFTs.
        public CurrencyValue? currencyValue; // The `CurrencyValue` of the offer. Useful for displaying the price information.
        public string? totalPrice; // The total offer amount for the NFTs.
        public NFTMetadata? asset; // Metadata of the asset
        public long? endTimeInSeconds; // The end time of the offer.
        public MarkteplaceStatus? status; // Whether the listing is CREATED, COMPLETED, or CANCELLED.

        public override string ToString()
        {
            return "Offer:\n"
                + $"id: {id?.ToString()}\n"
                + $"offerorAddress: {offerorAddress?.ToString()}\n"
                + $"assetContractAddress: {assetContractAddress?.ToString()}\n"
                + $"tokenId: {tokenId?.ToString()}\n"
                + $"quantity: {quantity?.ToString()}\n"
                + $"currencyContractAddress: {currencyContractAddress?.ToString()}\n"
                + $"currencyValue: {currencyValue?.ToString()}\n"
                + $"totalPrice: {totalPrice?.ToString()}\n"
                + $"asset: {asset?.ToString()}\n"
                + $"endTimeInSeconds: {endTimeInSeconds?.ToString()}\n"
                + $"status: {status?.ToString()}\n";
        }
    }

    [System.Serializable]
    public struct MakeOfferInput
    {
        public string assetContractAddress; // Required - the contract address of the NFT to offer on
        public string tokenId; // Required - the token ID to offer on
        public string totalPrice; // Required - the price to offer in the currency specified
        public string? currencyContractAddress; // Optional - defaults to the native wrapped currency
        public long? endTimestamp; // Optional - Defaults to 10 years from now
        public string? quantity; // Optional - defaults to 1
    }

    [System.Serializable]
    public struct Bid
    {
        public string? auctionId; // The id of the auction.
        public string? bidderAddress; // The address of the buyer who made the offer.
        public string? currencyContractAddress; // The currency contract address of the offer token.
        public string? bidAmount; // The amount of coins offered per token.
        public CurrencyValue? bidAmountCurrencyValue;
    }

#nullable disable

    // Claim conditions

    [System.Serializable]
    public class ClaimConditions
    {
        public string availableSupply;
        public string currentMintSupply;
        public CurrencyValue currencyMetadata;
        public string currencyAddress;
        public string maxClaimableSupply;
        public string maxClaimablePerWallet;
        public string waitInSeconds;

        public override string ToString()
        {
            return $"ClaimConditions:"
                + $"\n>availableSupply: {availableSupply}"
                + $"\n>currentMintSupply: {currentMintSupply}"
                + $"\n>>>>>\n{currencyMetadata.ToString()}\n<<<<<"
                + $"\n>currencyAddress: {currencyAddress}"
                + $"\n>maxClaimableSupply: {maxClaimableSupply}"
                + $"\n>maxClaimablePerWallet: {maxClaimablePerWallet}"
                + $"\n>waitInSeconds: {waitInSeconds}";
        }
    }

    [System.Serializable]
    public class SnapshotEntry
    {
        public string address;
        public string maxClaimable;
        public string price;
        public string currencyAddress;

        public override string ToString()
        {
            return $"SnapshotEntry:" + $"\n>address: {address}" + $"\n>maxClaimable: {maxClaimable}" + $"\n>price: {price}" + $"\n>currencyAddress: {currencyAddress}";
        }
    }

    // Transactions

    [System.Serializable]
    public struct Receipt
    {
        public string from;
        public string to;
        public int transactionIndex;
        public string gasUsed;
        public string blockHash;
        public string transactionHash;

        public override string ToString()
        {
            return $"Receipt:"
                + $"\n>from: {from}"
                + $"\n>to: {to}"
                + $"\n>transactionIndex: {transactionIndex}"
                + $"\n>gasUsed: {gasUsed}"
                + $"\n>blockHash: {blockHash}"
                + $"\n>transactionHash: {transactionHash}";
        }
    }

    [System.Serializable]
    public class TransactionResult
    {
        public Receipt receipt;
        public string id;

        public bool isSuccessful()
        {
            return receipt.transactionHash != null;
        }

        public override string ToString()
        {
            return $"TransactionResult:" + $"\n{receipt.ToString()}" + $"\n>id: {id}";
        }
    }

    [System.Serializable]
    public struct TransactionRequest
    {
        public string from;
        public string to;
        public string data;
        public string value;
        public string gasLimit;
        public string gasPrice;

        public override string ToString()
        {
            return $"TransactionRequest:" + $"\n>from: {from}" + $"\n>to: {to}" + $"\n>data: {data}" + $"\n>value: {value}" + $"\n>gasLimit: {gasLimit}" + $"\n>gasPrice: {gasPrice}";
        }
    }

    [System.Serializable]
    public struct LoginPayload
    {
        public LoginPayloadData payload;
        public string signature;

        public override string ToString()
        {
            return $"LoginPayloadData:" + $"\n>>>>>\n{payload.ToString()}\n<<<<<" + $"\n>signature: {signature}";
        }
    }

    [System.Serializable]
    public class LoginPayloadData
    {
        public string type;
        public string domain;
        public string address;
        public string statement;
        public string uri;
        public string version;
        public string chain_id;
        public string nonce;
        public string issued_at;
        public string expiration_time;
        public string invalid_before;
        public List<string> resources;

        public LoginPayloadData()
        {
            type = "evm";
        }

        public override string ToString()
        {
            return $"LoginPayloadData:"
                + $"\n>type: {type}"
                + $"\n>domain: {domain}"
                + $"\n>address: {address}"
                + $"\n>statement: {statement}"
                + $"\n>uri: {uri}"
                + $"\n>version: {version}"
                + $"\n>chain_id: {chain_id}"
                + $"\n>nonce: {nonce}"
                + $"\n>issued_at: {issued_at}"
                + $"\n>expiration_time: {expiration_time}"
                + $"\n>invalid_before: {invalid_before}"
                + $"\n>resources: {resources}";
        }
    }

    [System.Serializable]
    public struct FundWalletOptions
    {
        public string appId;
        public string address;
        public int chainId;
        public List<string> assets;
    }

    // Events

    [System.Serializable]
    public class EventQueryOptions
    {
        public int? fromBlock;
        public string order; // "asc" or "desc"
        public int? toBlock;
        public Dictionary<string, object> filters;

        public EventQueryOptions(Dictionary<string, object> filters = null, int? fromBlock = null, int? toBlock = null, string order = null)
        {
            this.fromBlock = fromBlock;
            this.order = order;
            this.toBlock = toBlock;
            this.filters = filters;
        }
    }

    [System.Serializable]
    public struct ContractEvent<T>
    {
        public string eventName;
        public T data;
        public EventTransaction transaction;

        public override string ToString()
        {
            return $"ContractEvent:" + $"\n>eventName: {eventName}" + $"\n>data: {data.ToString()}" + $"\n{transaction.ToString()}";
        }
    }

    [System.Serializable]
    public struct EventTransaction
    {
        public int blockNumber;
        public string blockHash;
        public int transactionIndex;
        public bool removed;
        public string address;
        public string data;
        public List<string> topics;
        public string transactionHash;
        public int logIndex;
        public string @event;
        public string eventSignature;

        public override string ToString()
        {
            return $"EventTransaction:"
                + $"\n>blockNumber: {blockNumber}"
                + $"\n>blockHash: {blockHash}"
                + $"\n>transactionIndex: {transactionIndex}"
                + $"\n>removed: {removed}"
                + $"\n>address: {address}"
                + $"\n>data: {data}"
                + $"\n>topics: {topics}"
                + $"\n>transactionHash: {transactionHash}"
                + $"\n>logIndex: {logIndex}"
                + $"\n>event: {@event}"
                + $"\n>eventSignature: {eventSignature}";
        }
    }
}
