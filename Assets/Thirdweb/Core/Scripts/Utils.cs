using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;
using System.IO;
using UnityEngine;
using WalletConnectSharp.Unity;

namespace Thirdweb
{
    public static class Utils
    {
        public const string AddressZero = "0x0000000000000000000000000000000000000000";
        public const string NativeTokenAddress = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        public const string NativeTokenAddressV2 = "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE";
        public const double DECIMALS_18 = 1000000000000000000;

        public static string[] ToJsonStringArray(params object[] args)
        {
            List<string> stringArgs = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                {
                    continue;
                }
                // if value type, convert to string otherwise serialize to json
                if (args[i].GetType().IsPrimitive || args[i] is string)
                {
                    stringArgs.Add(args[i].ToString());
                }
                else
                {
                    stringArgs.Add(Utils.ToJson(args[i]));
                }
            }
            return stringArgs.ToArray();
        }

        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static string ToBytes32HexString(byte[] bytes)
        {
            var hex = new StringBuilder(64);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return "0x" + hex.ToString().PadLeft(64, '0');
        }

        public static long UnixTimeNowMs()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        public static string ToWei(this string eth)
        {
            double ethDouble = 0;
            if (!double.TryParse(eth, out ethDouble))
                throw new ArgumentException("Invalid eth value.");
            BigInteger wei = (BigInteger)(ethDouble * DECIMALS_18);
            return wei.ToString();
        }

        public static string ToEth(this string wei, int decimalsToDisplay = 4)
        {
            return FormatERC20(wei, decimalsToDisplay);
        }

        public static string FormatERC20(this string wei, int decimalsToDisplay = 4, int decimals = 18)
        {
            BigInteger weiBigInt = 0;
            if (!BigInteger.TryParse(wei, out weiBigInt))
                throw new ArgumentException("Invalid wei value.");
            double eth = (double)weiBigInt / Math.Pow(10.0, decimals);
            string format = "#,0";
            if (decimalsToDisplay > 0)
                format += ".";
            for (int i = 0; i < decimalsToDisplay; i++)
                format += "#";
            return eth.ToString(format);
        }

        public static string ShortenAddress(this string address)
        {
            if (address.Length != 42)
                throw new ArgumentException("Invalid Address Length.");
            return $"{address.Substring(0, 5)}...{address.Substring(39)}";
        }

        public static bool IsWebGLBuild()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static string ReplaceIPFS(this string uri, string gateway = "https://gateway.ipfscdn.io/ipfs/")
        {
            if (uri.StartsWith("ipfs://"))
                return uri.Replace("ipfs://", gateway);
            else
                return uri;
        }

        public static TransactionResult ToTransactionResult(this Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt)
        {
            TransactionResult result = new TransactionResult();
            result.receipt.from = receipt.From;
            result.receipt.to = receipt.To;
            result.receipt.transactionIndex = int.Parse(receipt.TransactionIndex.ToString());
            result.receipt.gasUsed = receipt.GasUsed.ToString();
            result.receipt.blockHash = receipt.BlockHash;
            result.receipt.transactionHash = receipt.TransactionHash;
            result.id = receipt.Status.ToString();
            return result;
        }

        public async static Task<List<NFT>> ToNFTList(this List<TokenData721> tokenDataList)
        {
            List<NFT> allNfts = new List<NFT>();
            foreach (var tokenData in tokenDataList)
            {
                Contract c = ThirdwebManager.Instance.SDK.GetContract(tokenData.Contract);
                NFT nft = new NFT();
                nft.owner = tokenData.Owner;
                nft.type = "ERC721";
                nft.supply = (int)await c.ERC721.TotalCount();
                nft.quantityOwned = 1;
                string tokenURI = tokenData.Uri;
                nft.metadata = await ThirdwebManager.Instance.SDK.storage.DownloadText<NFTMetadata>(tokenURI);
                nft.metadata.image = nft.metadata.image.ReplaceIPFS();
                nft.metadata.id = tokenData.TokenId;
                nft.metadata.uri = tokenURI.ReplaceIPFS();
                allNfts.Add(nft);
            }
            return allNfts;
        }

        public static List<Thirdweb.Contracts.Pack.ContractDefinition.Token> ToPackTokenList(this NewPackInput packContents)
        {
            List<Thirdweb.Contracts.Pack.ContractDefinition.Token> tokenList = new List<Contracts.Pack.ContractDefinition.Token>();
            // Add ERC20 Rewards
            foreach (var erc20Reward in packContents.erc20Rewards)
            {
                tokenList.Add(
                    new Thirdweb.Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc20Reward.contractAddress,
                        TokenType = 0,
                        TokenId = 0,
                        TotalAmount = BigInteger.Parse(erc20Reward.totalRewards.ToWei()),
                    }
                );
            }
            // Add ERC721 Rewards
            foreach (var erc721Reward in packContents.erc721Rewards)
            {
                tokenList.Add(
                    new Thirdweb.Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc721Reward.contractAddress,
                        TokenType = 1,
                        TokenId = BigInteger.Parse(erc721Reward.tokenId),
                        TotalAmount = 1,
                    }
                );
            }
            // Add ERC1155 Rewards
            foreach (var erc1155Reward in packContents.erc1155Rewards)
            {
                tokenList.Add(
                    new Thirdweb.Contracts.Pack.ContractDefinition.Token()
                    {
                        AssetContract = erc1155Reward.contractAddress,
                        TokenType = 2,
                        TokenId = BigInteger.Parse(erc1155Reward.tokenId),
                        TotalAmount = BigInteger.Parse(erc1155Reward.totalRewards),
                    }
                );
            }
            return tokenList;
        }

        public static List<BigInteger> ToPackRewardUnitsList(this PackContents packContents)
        {
            List<BigInteger> rewardUnits = new List<BigInteger>();
            // Add ERC20 Rewards
            foreach (var content in packContents.erc20Rewards)
            {
                rewardUnits.Add(BigInteger.Parse(content.quantityPerReward.ToWei()));
            }
            // Add ERC721 Rewards
            foreach (var content in packContents.erc721Rewards)
            {
                rewardUnits.Add(1);
            }
            // Add ERC1155 Rewards
            foreach (var content in packContents.erc1155Rewards)
            {
                rewardUnits.Add(BigInteger.Parse(content.quantityPerReward));
            }
            return rewardUnits;
        }

        public static long GetUnixTimeStampNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long GetUnixTimeStampIn10Years()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60 * 60 * 24 * 365 * 10;
        }

        public static byte[] HexStringToByteArray(this string hex)
        {
            return hex.HexToByteArray();
        }

        public static string ByteArrayToHexString(this byte[] hexBytes)
        {
            return hexBytes.ToHex(true);
        }

        public static BigInteger GetMaxUint256()
        {
            return BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935");
        }

        public async static Task<BigInteger> GetCurrentBlockTimeStamp()
        {
            var blockNumber = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            var block = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new Nethereum.Hex.HexTypes.HexBigInteger(blockNumber));
            return block.Timestamp.Value;
        }

        public static string GetDeviceIdentifier()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        public static bool HasStoredAccount()
        {
            return File.Exists(GetAccountPath());
        }

        public static string GetAccountPath()
        {
            return Application.persistentDataPath + "/account.json";
        }

        public static Account UnlockOrGenerateAccount(int chainId, string password = null, string privateKey = null)
        {
            password ??= GetDeviceIdentifier();

            var path = GetAccountPath();
            var keyStoreService = new Nethereum.KeyStore.KeyStoreScryptService();

            if (privateKey != null)
            {
                return new Account(privateKey, chainId);
            }
            else
            {
                if (File.Exists(path))
                {
                    try
                    {
                        var encryptedJson = File.ReadAllText(path);
                        var key = keyStoreService.DecryptKeyStoreFromJson(password, encryptedJson);
                        return new Account(key, chainId);
                    }
                    catch (System.Exception)
                    {
                        throw new UnityException("Incorrect Password!");
                    }
                }
                else
                {
                    var scryptParams = new Nethereum.KeyStore.Model.ScryptParams
                    {
                        Dklen = 32,
                        N = 262144,
                        R = 1,
                        P = 8
                    };
                    var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
                    var keyStore = keyStoreService.EncryptAndGenerateKeyStore(password, ecKey.GetPrivateKeyAsBytes(), ecKey.GetPublicAddress(), scryptParams);
                    var json = keyStoreService.SerializeKeyStoreToJson(keyStore);
                    File.WriteAllText(path, json);
                    return new Account(ecKey, chainId);
                }
            }
        }

        public static bool ActiveWalletConnectSession()
        {
            return WalletConnect.Instance != null && WalletConnect.Instance.Session != null && WalletConnect.Instance.Session.Connected;
        }

        public static string cidToIpfsUrl(this string cid, bool useGateway = false)
        {
            string ipfsRaw = $"ipfs://{cid}";
            return useGateway ? ipfsRaw.ReplaceIPFS() : ipfsRaw;
        }

        public async static Task<string> GetENSName(string address)
        {
            if (IsWebGLBuild())
            {
                return null;
            }
            else
            {
                var ensService = new Nethereum.Contracts.Standards.ENS.ENSService(
                    new Nethereum.Web3.Web3("https://ethereum.rpc.thirdweb.com/339d65590ba0fa79e4c8be0af33d64eda709e13652acb02c6be63f5a1fbef9c3").Eth,
                    "0x00000000000C2E074eC69A0dFb2997BA6C7d2e1e"
                );
                return await ensService.ReverseResolveAsync(address);
            }
        }

        public static string GetNativeTokenWrapper(int chainId)
        {
            switch (chainId)
            {
                case 1:
                    return "0xC02aaA39b223FE8D0A0e5C4F27eAD9083C756Cc2";
                case 4:
                    return "0xc778417E063141139Fce010982780140Aa0cD5Ab"; // rinkeby
                case 5:
                    return "0xB4FBF271143F4FBf7B91A5ded31805e42b2208d6"; // goerli
                case 137:
                    return "0x0d500B1d8E8eF31E21C99d1Db9A6444d3ADf1270";
                case 80001:
                    return "0x9c3C9283D3e44854697Cd22D3Faa240Cfb032889";
                case 43114:
                    return "0xB31f66AA3C1e785363F0875A1B74E27b85FD66c7";
                case 43113:
                    return "0xd00ae08403B9bbb9124bB305C09058E32C39A48c";
                case 250:
                    return "0x21be370D5312f44cB42ce377BC9b8a0cEF1A4C83";
                case 4002:
                    return "0xf1277d1Ed8AD466beddF92ef448A132661956621";
                case 10:
                    return "0x4200000000000000000000000000000000000006"; // optimism
                case 69:
                    return "0xbC6F6b680bc61e30dB47721c6D1c5cde19C1300d"; // optimism kovan
                case 420:
                    return "0x4200000000000000000000000000000000000006"; // optimism goerli
                case 42161:
                    return "0x82af49447d8a07e3bd95bd0d56f35241523fbab1"; // arbitrum
                case 421611:
                    return "0xEBbc3452Cc911591e4F18f3b36727Df45d6bd1f9"; // arbitrum rinkeby
                case 421613:
                    return "0xe39Ab88f8A4777030A534146A9Ca3B52bd5D43A3"; // arbitrum goerli
                case 56:
                    return "0xbb4CdB9CBd36B01bD1cBaEBF2De08d9173bc095c"; // binance mainnet
                case 97:
                    return "0xae13d989daC2f0dEbFf460aC112a837C89BAa7cd"; // binance testnet
                default:
                    throw new UnityException("Native Token Wrapper Unavailable For This Chain!");
            }
        }
    }
}
