using System.Threading.Tasks;
using System.Numerics;
using UnityEngine;
using Nethereum.Hex.HexTypes;

namespace Thirdweb
{
    /// <summary>
    /// Convenient wrapper to interact with any EVM contract
    /// </summary>
    public class Contract : Routable
    {
        public string chain;
        public string address;
        public string abi;

        /// <summary>
        /// Call any ERC20 supported functions
        /// </summary>
        public ERC20 ERC20;

        /// <summary>
        /// Call any ERC721 supported functions
        /// </summary>
        public ERC721 ERC721;

        /// <summary>
        /// Call any ERC1155 supported functions
        /// </summary>
        public ERC1155 ERC1155;

        /// <summary>
        /// Call any Marketplace supported functions
        /// </summary>
        public Marketplace marketplace;

        /// <summary>
        /// Call any Pack supported functions
        /// </summary>
        public Pack pack;

        /// <summary>
        /// Call any Contract Event functions
        /// </summary>
        public Events events;

        public Contract(string chain, string address, string abi = null) : base(abi != null ? $"{address}{Routable.subSeparator}{abi}" : address)
        {
            this.chain = chain;
            this.address = address;
            this.abi = abi;
            this.ERC20 = new ERC20(baseRoute, address);
            this.ERC721 = new ERC721(baseRoute, address);
            this.ERC1155 = new ERC1155(baseRoute, address);
            this.marketplace = new Marketplace(baseRoute, address);
            this.pack = new Pack(chain, address);
            this.events = new Events(baseRoute);
        }

        public async Task<CurrencyValue> GetBalance()
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<CurrencyValue>(getRoute("balance"), new string[] { });
            }
            else
            {
                BigInteger balance = await ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetBalance.SendRequestAsync(address);
                CurrencyValue cv = new CurrencyValue();
                cv.value = balance.ToString();
                cv.displayValue = balance.ToString().ToEth();
                return cv;
            }
        }

        /// <summary>
        /// Read data from a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The data deserialized to the given typed</returns>
        public async Task<T> Read<T>(string functionName, params object[] args)
        {
            if (Utils.IsWebGLBuild())
            {
                return await Bridge.InvokeRoute<T>(getRoute("call"), Utils.ToJsonStringArray(functionName, args));
            }
            else
            {
                if (this.abi == null)
                    throw new UnityException("You must pass an ABI for native platform custom calls");

                var contract = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContract(this.abi, this.address);
                var function = contract.GetFunction(functionName);
                return await function.CallAsync<T>(args);
            }
        }

        /// <summary>
        /// Execute a write transaction on a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The transaction receipt</returns>
        public Task<TransactionResult> Write(string functionName, params object[] args)
        {
            return Write(functionName, null, args);
        }

        /// <summary>
        /// Execute a write transaction on a contract
        /// </summary>
        /// <param name="functionName">The contract function name to call</param>
        /// <param name="transactionOverrides">Overrides to pass with the transaction</param>
        /// <param name="args">Optional function arguments. Structs and Lists will get serialized automatically</param>
        /// <returns>The transaction receipt</returns>
        public async Task<TransactionResult> Write(string functionName, TransactionRequest? transactionOverrides, params object[] args)
        {
            if (Utils.IsWebGLBuild())
            {
                args = args ?? new object[0];
                return await Bridge.InvokeRoute<TransactionResult>(getRoute("call"), Utils.ToJsonStringArray(functionName, args, transactionOverrides));
            }
            else
            {
                if (this.abi == null)
                    throw new UnityException("You must pass an ABI for native platform custom calls");

                var contract = ThirdwebManager.Instance.SDK.nativeSession.web3.Eth.GetContract(this.abi, this.address);

                var function = contract.GetFunction(functionName);

                var value = transactionOverrides?.value != null ? new HexBigInteger(BigInteger.Parse(transactionOverrides?.value)) : new HexBigInteger(0);

                var gas =
                    transactionOverrides?.gasLimit != null
                        ? new HexBigInteger(BigInteger.Parse(transactionOverrides?.gasLimit))
                        : await function.EstimateGasAsync(await ThirdwebManager.Instance.SDK.wallet.GetAddress(), null, value, args);

                var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                    from: transactionOverrides?.from ?? await ThirdwebManager.Instance.SDK.wallet.GetAddress(),
                    gas: gas,
                    value: value,
                    receiptRequestCancellationToken: null,
                    args
                );
                return receipt.ToTransactionResult();
            }
        }
    }
}
