using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using Nethereum.ENS.PublicResolver.ContractDefinition;

namespace Nethereum.ENS
{
    public partial class PublicResolverService
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.Web3 web3, PublicResolverDeployment publicResolverDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<PublicResolverDeployment>().SendRequestAndWaitForReceiptAsync(publicResolverDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.Web3 web3, PublicResolverDeployment publicResolverDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<PublicResolverDeployment>().SendRequestAsync(publicResolverDeployment);
        }

        public static async Task<PublicResolverService> DeployContractAndGetServiceAsync(Nethereum.Web3.Web3 web3, PublicResolverDeployment publicResolverDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, publicResolverDeployment, cancellationTokenSource).ConfigureAwait(false);
            return new PublicResolverService(web3, receipt.ContractAddress);
        }

        protected Nethereum.Web3.Web3 Web3 { get; }

        public ContractHandler ContractHandler { get; }

        public PublicResolverService(Nethereum.Web3.Web3 web3, string contractAddress)
        {
            Web3 = web3;
            ContractHandler = web3.Eth.GetContractHandler(contractAddress);
        }

        public Task<ABIOutputDTO> ABIQueryAsync(ABIFunction aBIFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<ABIFunction, ABIOutputDTO>(aBIFunction, blockParameter);
        }

        public Task<ABIOutputDTO> ABIQueryAsync(byte[] node, BigInteger contentTypes, BlockParameter blockParameter = null)
        {
            var aBIFunction = new ABIFunction();
            aBIFunction.Node = node;
            aBIFunction.ContentTypes = contentTypes;

            return ContractHandler.QueryDeserializingToObjectAsync<ABIFunction, ABIOutputDTO>(aBIFunction, blockParameter);
        }

        public Task<string> AddrQueryAsync(AddrFunction addrFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AddrFunction, string>(addrFunction, blockParameter);
        }


        public Task<string> AddrQueryAsync(byte[] node, BlockParameter blockParameter = null)
        {
            var addrFunction = new AddrFunction();
            addrFunction.Node = node;

            return ContractHandler.QueryAsync<AddrFunction, string>(addrFunction, blockParameter);
        }

        public Task<byte[]> AddrQueryAsync(AddrFunction2 addrFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AddrFunction2, byte[]>(addrFunction, blockParameter);
        }


        public Task<byte[]> AddrQueryAsync(byte[] node, BigInteger coinType, BlockParameter blockParameter = null)
        {
            var addrFunction = new AddrFunction2();
            addrFunction.Node = node;
            addrFunction.CoinType = coinType;

            return ContractHandler.QueryAsync<AddrFunction2, byte[]>(addrFunction, blockParameter);
        }

        public Task<bool> AuthorisationsQueryAsync(AuthorisationsFunction authorisationsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AuthorisationsFunction, bool>(authorisationsFunction, blockParameter);
        }


        public Task<bool> AuthorisationsQueryAsync(byte[] returnValue1, string returnValue2, string returnValue3, BlockParameter blockParameter = null)
        {
            var authorisationsFunction = new AuthorisationsFunction();
            authorisationsFunction.ReturnValue1 = returnValue1;
            authorisationsFunction.ReturnValue2 = returnValue2;
            authorisationsFunction.ReturnValue3 = returnValue3;

            return ContractHandler.QueryAsync<AuthorisationsFunction, bool>(authorisationsFunction, blockParameter);
        }

        public Task<string> ClearDNSZoneRequestAsync(ClearDNSZoneFunction clearDNSZoneFunction)
        {
            return ContractHandler.SendRequestAsync(clearDNSZoneFunction);
        }

        public Task<TransactionReceipt> ClearDNSZoneRequestAndWaitForReceiptAsync(ClearDNSZoneFunction clearDNSZoneFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(clearDNSZoneFunction, cancellationToken);
        }

        public Task<string> ClearDNSZoneRequestAsync(byte[] node)
        {
            var clearDNSZoneFunction = new ClearDNSZoneFunction();
            clearDNSZoneFunction.Node = node;

            return ContractHandler.SendRequestAsync(clearDNSZoneFunction);
        }

        public Task<TransactionReceipt> ClearDNSZoneRequestAndWaitForReceiptAsync(byte[] node, CancellationTokenSource cancellationToken = null)
        {
            var clearDNSZoneFunction = new ClearDNSZoneFunction();
            clearDNSZoneFunction.Node = node;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(clearDNSZoneFunction, cancellationToken);
        }

        public Task<byte[]> ContenthashQueryAsync(ContenthashFunction contenthashFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ContenthashFunction, byte[]>(contenthashFunction, blockParameter);
        }


        public Task<byte[]> ContenthashQueryAsync(byte[] node, BlockParameter blockParameter = null)
        {
            var contenthashFunction = new ContenthashFunction();
            contenthashFunction.Node = node;

            return ContractHandler.QueryAsync<ContenthashFunction, byte[]>(contenthashFunction, blockParameter);
        }

        public Task<byte[]> DnsRecordQueryAsync(DnsRecordFunction dnsRecordFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<DnsRecordFunction, byte[]>(dnsRecordFunction, blockParameter);
        }


        public Task<byte[]> DnsRecordQueryAsync(byte[] node, byte[] name, ushort resource, BlockParameter blockParameter = null)
        {
            var dnsRecordFunction = new DnsRecordFunction();
            dnsRecordFunction.Node = node;
            dnsRecordFunction.Name = name;
            dnsRecordFunction.Resource = resource;

            return ContractHandler.QueryAsync<DnsRecordFunction, byte[]>(dnsRecordFunction, blockParameter);
        }

        public Task<bool> HasDNSRecordsQueryAsync(HasDNSRecordsFunction hasDNSRecordsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<HasDNSRecordsFunction, bool>(hasDNSRecordsFunction, blockParameter);
        }


        public Task<bool> HasDNSRecordsQueryAsync(byte[] node, byte[] name, BlockParameter blockParameter = null)
        {
            var hasDNSRecordsFunction = new HasDNSRecordsFunction();
            hasDNSRecordsFunction.Node = node;
            hasDNSRecordsFunction.Name = name;

            return ContractHandler.QueryAsync<HasDNSRecordsFunction, bool>(hasDNSRecordsFunction, blockParameter);
        }

        public Task<string> InterfaceImplementerQueryAsync(InterfaceImplementerFunction interfaceImplementerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<InterfaceImplementerFunction, string>(interfaceImplementerFunction, blockParameter);
        }


        public Task<string> InterfaceImplementerQueryAsync(byte[] node, byte[] interfaceID, BlockParameter blockParameter = null)
        {
            var interfaceImplementerFunction = new InterfaceImplementerFunction();
            interfaceImplementerFunction.Node = node;
            interfaceImplementerFunction.InterfaceID = interfaceID;

            return ContractHandler.QueryAsync<InterfaceImplementerFunction, string>(interfaceImplementerFunction, blockParameter);
        }

        public Task<string> NameQueryAsync(NameFunction nameFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameter);
        }


        public Task<string> NameQueryAsync(byte[] node, BlockParameter blockParameter = null)
        {
            var nameFunction = new NameFunction();
            nameFunction.Node = node;

            return ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameter);
        }

        public Task<PubkeyOutputDTO> PubkeyQueryAsync(PubkeyFunction pubkeyFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<PubkeyFunction, PubkeyOutputDTO>(pubkeyFunction, blockParameter);
        }

        public Task<PubkeyOutputDTO> PubkeyQueryAsync(byte[] node, BlockParameter blockParameter = null)
        {
            var pubkeyFunction = new PubkeyFunction();
            pubkeyFunction.Node = node;

            return ContractHandler.QueryDeserializingToObjectAsync<PubkeyFunction, PubkeyOutputDTO>(pubkeyFunction, blockParameter);
        }

        public Task<string> SetABIRequestAsync(SetABIFunction setABIFunction)
        {
            return ContractHandler.SendRequestAsync(setABIFunction);
        }

        public Task<TransactionReceipt> SetABIRequestAndWaitForReceiptAsync(SetABIFunction setABIFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setABIFunction, cancellationToken);
        }

        public Task<string> SetABIRequestAsync(byte[] node, BigInteger contentType, byte[] data)
        {
            var setABIFunction = new SetABIFunction();
            setABIFunction.Node = node;
            setABIFunction.ContentType = contentType;
            setABIFunction.Data = data;

            return ContractHandler.SendRequestAsync(setABIFunction);
        }

        public Task<TransactionReceipt> SetABIRequestAndWaitForReceiptAsync(byte[] node, BigInteger contentType, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var setABIFunction = new SetABIFunction();
            setABIFunction.Node = node;
            setABIFunction.ContentType = contentType;
            setABIFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setABIFunction, cancellationToken);
        }

        public Task<string> SetAddrRequestAsync(SetAddrFunction setAddrFunction)
        {
            return ContractHandler.SendRequestAsync(setAddrFunction);
        }

        public Task<TransactionReceipt> SetAddrRequestAndWaitForReceiptAsync(SetAddrFunction setAddrFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAddrFunction, cancellationToken);
        }

        public Task<string> SetAddrRequestAsync(byte[] node, BigInteger coinType, byte[] a)
        {
            var setAddrFunction = new SetAddrFunction();
            setAddrFunction.Node = node;
            setAddrFunction.CoinType = coinType;
            setAddrFunction.A = a;

            return ContractHandler.SendRequestAsync(setAddrFunction);
        }

        public Task<TransactionReceipt> SetAddrRequestAndWaitForReceiptAsync(byte[] node, BigInteger coinType, byte[] a, CancellationTokenSource cancellationToken = null)
        {
            var setAddrFunction = new SetAddrFunction();
            setAddrFunction.Node = node;
            setAddrFunction.CoinType = coinType;
            setAddrFunction.A = a;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAddrFunction, cancellationToken);
        }

        public Task<string> SetAddrRequestAsync(SetAddrFunction2 setAddrFunction)
        {
            return ContractHandler.SendRequestAsync(setAddrFunction);
        }

        public Task<TransactionReceipt> SetAddrRequestAndWaitForReceiptAsync(SetAddrFunction2 setAddrFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAddrFunction, cancellationToken);
        }

        public Task<string> SetAddrRequestAsync(byte[] node, string a)
        {
            var setAddrFunction = new SetAddrFunction2();
            setAddrFunction.Node = node;
            setAddrFunction.A = a;

            return ContractHandler.SendRequestAsync(setAddrFunction);
        }

        public Task<TransactionReceipt> SetAddrRequestAndWaitForReceiptAsync(byte[] node, string a, CancellationTokenSource cancellationToken = null)
        {
            var setAddrFunction = new SetAddrFunction2();
            setAddrFunction.Node = node;
            setAddrFunction.A = a;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAddrFunction, cancellationToken);
        }

        public Task<string> SetAuthorisationRequestAsync(SetAuthorisationFunction setAuthorisationFunction)
        {
            return ContractHandler.SendRequestAsync(setAuthorisationFunction);
        }

        public Task<TransactionReceipt> SetAuthorisationRequestAndWaitForReceiptAsync(SetAuthorisationFunction setAuthorisationFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAuthorisationFunction, cancellationToken);
        }

        public Task<string> SetAuthorisationRequestAsync(byte[] node, string target, bool isAuthorised)
        {
            var setAuthorisationFunction = new SetAuthorisationFunction();
            setAuthorisationFunction.Node = node;
            setAuthorisationFunction.Target = target;
            setAuthorisationFunction.IsAuthorised = isAuthorised;

            return ContractHandler.SendRequestAsync(setAuthorisationFunction);
        }

        public Task<TransactionReceipt> SetAuthorisationRequestAndWaitForReceiptAsync(byte[] node, string target, bool isAuthorised, CancellationTokenSource cancellationToken = null)
        {
            var setAuthorisationFunction = new SetAuthorisationFunction();
            setAuthorisationFunction.Node = node;
            setAuthorisationFunction.Target = target;
            setAuthorisationFunction.IsAuthorised = isAuthorised;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setAuthorisationFunction, cancellationToken);
        }

        public Task<string> SetContenthashRequestAsync(SetContenthashFunction setContenthashFunction)
        {
            return ContractHandler.SendRequestAsync(setContenthashFunction);
        }

        public Task<TransactionReceipt> SetContenthashRequestAndWaitForReceiptAsync(SetContenthashFunction setContenthashFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContenthashFunction, cancellationToken);
        }

        public Task<string> SetContenthashRequestAsync(byte[] node, byte[] hash)
        {
            var setContenthashFunction = new SetContenthashFunction();
            setContenthashFunction.Node = node;
            setContenthashFunction.Hash = hash;

            return ContractHandler.SendRequestAsync(setContenthashFunction);
        }

        public Task<TransactionReceipt> SetContenthashRequestAndWaitForReceiptAsync(byte[] node, byte[] hash, CancellationTokenSource cancellationToken = null)
        {
            var setContenthashFunction = new SetContenthashFunction();
            setContenthashFunction.Node = node;
            setContenthashFunction.Hash = hash;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setContenthashFunction, cancellationToken);
        }

        public Task<string> SetDNSRecordsRequestAsync(SetDNSRecordsFunction setDNSRecordsFunction)
        {
            return ContractHandler.SendRequestAsync(setDNSRecordsFunction);
        }

        public Task<TransactionReceipt> SetDNSRecordsRequestAndWaitForReceiptAsync(SetDNSRecordsFunction setDNSRecordsFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setDNSRecordsFunction, cancellationToken);
        }

        public Task<string> SetDNSRecordsRequestAsync(byte[] node, byte[] data)
        {
            var setDNSRecordsFunction = new SetDNSRecordsFunction();
            setDNSRecordsFunction.Node = node;
            setDNSRecordsFunction.Data = data;

            return ContractHandler.SendRequestAsync(setDNSRecordsFunction);
        }

        public Task<TransactionReceipt> SetDNSRecordsRequestAndWaitForReceiptAsync(byte[] node, byte[] data, CancellationTokenSource cancellationToken = null)
        {
            var setDNSRecordsFunction = new SetDNSRecordsFunction();
            setDNSRecordsFunction.Node = node;
            setDNSRecordsFunction.Data = data;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setDNSRecordsFunction, cancellationToken);
        }

        public Task<string> SetInterfaceRequestAsync(SetInterfaceFunction setInterfaceFunction)
        {
            return ContractHandler.SendRequestAsync(setInterfaceFunction);
        }

        public Task<TransactionReceipt> SetInterfaceRequestAndWaitForReceiptAsync(SetInterfaceFunction setInterfaceFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setInterfaceFunction, cancellationToken);
        }

        public Task<string> SetInterfaceRequestAsync(byte[] node, byte[] interfaceID, string implementer)
        {
            var setInterfaceFunction = new SetInterfaceFunction();
            setInterfaceFunction.Node = node;
            setInterfaceFunction.InterfaceID = interfaceID;
            setInterfaceFunction.Implementer = implementer;

            return ContractHandler.SendRequestAsync(setInterfaceFunction);
        }

        public Task<TransactionReceipt> SetInterfaceRequestAndWaitForReceiptAsync(byte[] node, byte[] interfaceID, string implementer, CancellationTokenSource cancellationToken = null)
        {
            var setInterfaceFunction = new SetInterfaceFunction();
            setInterfaceFunction.Node = node;
            setInterfaceFunction.InterfaceID = interfaceID;
            setInterfaceFunction.Implementer = implementer;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setInterfaceFunction, cancellationToken);
        }

        public Task<string> SetNameRequestAsync(SetNameFunction setNameFunction)
        {
            return ContractHandler.SendRequestAsync(setNameFunction);
        }

        public Task<TransactionReceipt> SetNameRequestAndWaitForReceiptAsync(SetNameFunction setNameFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setNameFunction, cancellationToken);
        }

        public Task<string> SetNameRequestAsync(byte[] node, string name)
        {
            var setNameFunction = new SetNameFunction();
            setNameFunction.Node = node;
            setNameFunction.Name = name;

            return ContractHandler.SendRequestAsync(setNameFunction);
        }

        public Task<TransactionReceipt> SetNameRequestAndWaitForReceiptAsync(byte[] node, string name, CancellationTokenSource cancellationToken = null)
        {
            var setNameFunction = new SetNameFunction();
            setNameFunction.Node = node;
            setNameFunction.Name = name;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setNameFunction, cancellationToken);
        }

        public Task<string> SetPubkeyRequestAsync(SetPubkeyFunction setPubkeyFunction)
        {
            return ContractHandler.SendRequestAsync(setPubkeyFunction);
        }

        public Task<TransactionReceipt> SetPubkeyRequestAndWaitForReceiptAsync(SetPubkeyFunction setPubkeyFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPubkeyFunction, cancellationToken);
        }

        public Task<string> SetPubkeyRequestAsync(byte[] node, byte[] x, byte[] y)
        {
            var setPubkeyFunction = new SetPubkeyFunction();
            setPubkeyFunction.Node = node;
            setPubkeyFunction.X = x;
            setPubkeyFunction.Y = y;

            return ContractHandler.SendRequestAsync(setPubkeyFunction);
        }

        public Task<TransactionReceipt> SetPubkeyRequestAndWaitForReceiptAsync(byte[] node, byte[] x, byte[] y, CancellationTokenSource cancellationToken = null)
        {
            var setPubkeyFunction = new SetPubkeyFunction();
            setPubkeyFunction.Node = node;
            setPubkeyFunction.X = x;
            setPubkeyFunction.Y = y;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setPubkeyFunction, cancellationToken);
        }

        public Task<string> SetTextRequestAsync(SetTextFunction setTextFunction)
        {
            return ContractHandler.SendRequestAsync(setTextFunction);
        }

        public Task<TransactionReceipt> SetTextRequestAndWaitForReceiptAsync(SetTextFunction setTextFunction, CancellationTokenSource cancellationToken = null)
        {
            return ContractHandler.SendRequestAndWaitForReceiptAsync(setTextFunction, cancellationToken);
        }

        public Task<string> SetTextRequestAsync(byte[] node, string key, string value)
        {
            var setTextFunction = new SetTextFunction();
            setTextFunction.Node = node;
            setTextFunction.Key = key;
            setTextFunction.Value = value;

            return ContractHandler.SendRequestAsync(setTextFunction);
        }

        public Task<TransactionReceipt> SetTextRequestAndWaitForReceiptAsync(byte[] node, string key, string value, CancellationTokenSource cancellationToken = null)
        {
            var setTextFunction = new SetTextFunction();
            setTextFunction.Node = node;
            setTextFunction.Key = key;
            setTextFunction.Value = value;

            return ContractHandler.SendRequestAndWaitForReceiptAsync(setTextFunction, cancellationToken);
        }

        public Task<bool> SupportsInterfaceQueryAsync(SupportsInterfaceFunction supportsInterfaceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }


        public Task<bool> SupportsInterfaceQueryAsync(byte[] interfaceID, BlockParameter blockParameter = null)
        {
            var supportsInterfaceFunction = new SupportsInterfaceFunction();
            supportsInterfaceFunction.InterfaceID = interfaceID;

            return ContractHandler.QueryAsync<SupportsInterfaceFunction, bool>(supportsInterfaceFunction, blockParameter);
        }

        public Task<string> TextQueryAsync(TextFunction textFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<TextFunction, string>(textFunction, blockParameter);
        }


        public Task<string> TextQueryAsync(byte[] node, string key, BlockParameter blockParameter = null)
        {
            var textFunction = new TextFunction();
            textFunction.Node = node;
            textFunction.Key = key;

            return ContractHandler.QueryAsync<TextFunction, string>(textFunction, blockParameter);
        }
    }
}
